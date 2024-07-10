using DotNetEnv;
using Newtonsoft.Json.Linq;

namespace AzdLibrary;

public static class AzdEnvironment
{
    private const string AzureFolderPath = "../../.azure";

    public static string GetDefaultEnvironment()
    {
        var configFilePath = Path.Combine(AzureFolderPath, "config.json");

        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException($"The config file was not found: {configFilePath}");
        }

        var jsonData = File.ReadAllText(configFilePath);
        var config = JObject.Parse(jsonData);
        var defaultEnvironment = config["defaultEnvironment"]?.ToString();

        if (string.IsNullOrEmpty(defaultEnvironment))
        {
            throw new InvalidOperationException("defaultEnvironment is not set in the config file.");
        }

        return defaultEnvironment;
    }

    public static string GetEnvFilePath()
    {
        var defaultEnvironment = GetDefaultEnvironment();
        var envFilePath = Path.Combine(AzureFolderPath, defaultEnvironment, ".env");

        if (!File.Exists(envFilePath))
        {
            throw new FileNotFoundException($"The .env file was not found: {envFilePath}");
        }

        return envFilePath;
    }

    public static void LoadEnvVariables()
    {
        var envFilePath = GetEnvFilePath();
        Env.Load(envFilePath);
    }
}

