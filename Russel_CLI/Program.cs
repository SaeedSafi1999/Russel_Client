using Russel_CLI.Helpers;
using Microsoft.Extensions.Configuration;
using Russel_CLI.Settings;
using System.Text.Json;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configJson = await File.ReadAllTextAsync("CLI_Config.json");
        var config = JsonSerializer.Deserialize<Config>(configJson);
        var port = config.ApiSettings.Port;
        var client = new ApiClient($"http://127.0.0.1:{port}");
        await CommandHandler.HandleCommand(client);
    }
}
