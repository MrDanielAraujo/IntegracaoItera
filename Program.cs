using IntegracaoItera.Configuration;
using IntegracaoItera.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Serilog;
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
builder.Services.AddSwaggerGen();
builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
