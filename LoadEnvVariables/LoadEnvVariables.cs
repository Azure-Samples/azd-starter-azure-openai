using DotNetEnv;
using Newtonsoft.Json.Linq;

namespace LoadEnvVariables;

public class AzureEnvManager
{
    private const string ConfigFilePath = "../../.azure/config.json";

    public string GetDefaultEnvironment()
    {
        if (!File.Exists(ConfigFilePath))
        {
            throw new FileNotFoundException($"The config file was not found: {ConfigFilePath}");
        }

        var jsonData = File.ReadAllText(ConfigFilePath);
        var config = JObject.Parse(jsonData);
        var defaultEnvironment = config["defaultEnvironment"]?.ToString();

        if (string.IsNullOrEmpty(defaultEnvironment))
        {
            throw new InvalidOperationException("defaultEnvironment is not set in the config file.");
        }

        return defaultEnvironment;
    }

    public string GetEnvFilePath()
    {
        var defaultEnvironment = GetDefaultEnvironment();
        var envFilePath = Path.Combine("../../.azure", defaultEnvironment, ".env");

        if (!File.Exists(envFilePath))
        {
            throw new FileNotFoundException($"The .env file was not found: {envFilePath}");
        }

        return envFilePath;
    }

    public void LoadEnvVariables()
    {
        var envFilePath = GetEnvFilePath();
        Env.Load(envFilePath);
    }
}

