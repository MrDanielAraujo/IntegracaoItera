using IntegracaoItera.Configuration;
using IntegracaoItera.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Configurando o Sirilog..
Log.Logger = new LoggerConfiguration()
    .MinimumLevel
    .Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Host.UseSerilog();


builder.Services.AddAuthentication()
    .AddJwtBearer("Local", options =>
    {
        var secret = builder.Configuration["SecretKey"];
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    })
    .AddJwtBearer("UsuarioSenha", options =>
    {
        var key = Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("SecretKey"));
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = "roles",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    })
    .AddMicrosoftIdentityWebApi(options =>
    {
        string mensagem = string.Empty;

        builder.Configuration.Bind("AzureAD", options);

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = async ctx =>
            {
                mensagem = "OnMessageReceived:\n";
                ctx.Request.Headers.TryGetValue("Authorization", out var bearerToken);
                if (StringValues.IsNullOrEmpty(bearerToken) || bearerToken.Count == 0)
                    bearerToken = "nenhum Bearer token enviado\n";
                mensagem += $"Authorization Header enviado: {bearerToken}\n";

                await Task.CompletedTask;
            },
            OnTokenValidated = async ctx =>
            {
                ctx.Request.Headers.TryGetValue("Authorization", out var bearerToken);
                string[] allowedClientApps =
                {
                     /* lista de clientid permitidos */
                     builder.Configuration["AzureAd:ClientId"]
                };
                if (ctx?.Principal?.Claims != null)
                {
                    string? clientappId = ctx?.Principal?.Claims?.FirstOrDefault(x => x.Type == "azp" || x.Type == "appid")?.Value;
                }
                await Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                // Token inválido ou EXPIRADO => 401
                ctx.NoResult();
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Response.ContentType = "application/json";

                var msg = ctx.Exception is SecurityTokenExpiredException
                    ? "{\"error\":\"token_expired\"}"
                    : "{\"error\":\"invalid_token\"}";

                return ctx.Response.WriteAsync(msg);
            },
        };
    }, options =>
    {
        builder.Configuration.Bind("AzureAD", options);
    });

// cria as políticas de acesso, com as roles requeridas, seguindo o princípio do menor privilégio, no serviço de autorização
builder.Services.AddAuthorization(options =>
{
    // Torna a policy padrão (pega qualquer [Authorize] sem nome de policy)
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes("Bearer")
        .AddRequirements(new UserExistsRequirement())
        .Build();

    options.AddPolicy("Local", new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes("Local")
        .RequireRole("Acionista")
        .Build());

    options.AddPolicy("TI.Only", policy => policy.RequireRole("IntraCred.TI.Usuario"));
});

// Registra o Serviço de verificação de policy padrão, com tratativas no banco de dados
builder.Services.AddScoped<IAuthorizationHandler, UserExistsHandler>();

// Add services to the container.
builder.EnvInitializer();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    {
        // descrição da API no padrão OpenAPI
        string ambientName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? "Desenvolvimento" : "Produção";
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = $"Integração Itera Core API (.NET 8.0) - Ambiente de {ambientName}",
            Description = "Back-end monolito, escrito em .NET 8.0 para rodar em Linux 🐧 e Windows 🪟, responsável pela validação dos documentos de balanço.",
            Version = "v1"
        });

        // expõe os comentários XML das controllers        
        // options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"), includeControllerXmlComments: true);

        // expõe o esquema de autenticação
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme()
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                Implicit = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(builder.Configuration["AzureAd:Instance"] + builder.Configuration["AzureAd:TenantId"] + "/oauth2/v2.0/authorize"),
                    TokenUrl = new Uri(builder.Configuration["AzureAd:Instance"] + builder.Configuration["AzureAd:TenantId"] + "/oauth2/v2.0/token"),
                    Scopes = new Dictionary<string, string>
                    {
                                { builder.Configuration["AzureAd:Scopes"], "Intracred.All" }
                    }
                }
            }
        });

        // requer globalmente autenticação para acessar a API
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                },
                new[]
                {
                    builder.Configuration["AzureAd:Scopes"]
                }
            }
        });
    }

);

builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        string ambientName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? "Desenvolvimento" : "Produção";

        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.DocumentTitle = $"Swagger UI - {ambientName}";
        options.RoutePrefix = string.Empty;
        options.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
        options.OAuthUsePkce();
        options.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
