using dotenv.net;

namespace IntegracaoItera.Configuration;

public static class EnvSetting
{
    public static void EnvInitializer(this WebApplicationBuilder builder)
    {
        var env = builder.Environment;

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

        void LoadEnvIfExists(string file)
        {
            var path = Path.Combine(env.ContentRootPath, file);
            if (File.Exists(path))
            {
                DotEnv.Load(new DotEnvOptions(
                    envFilePaths: [path],
                    probeForEnv: false,
                    overwriteExistingVars: false
                ));
            }
        }

        LoadEnvIfExists(".env");
        LoadEnvIfExists($".env.{env.EnvironmentName.ToLower()}");

        builder.Configuration.AddEnvironmentVariables();
    }
}
