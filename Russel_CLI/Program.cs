using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using Russel_CLI.Extensions;

public class ApiResponse<T>
{
    [JsonProperty("is_success")]
    public bool IsSuccess { get; set; }

    [JsonProperty("data")]
    public T Data { get; set; }
}

public class SetRequest
{
    [JsonProperty("cluster")]
    public string cluster { get; set; }

    [JsonProperty("key")]
    public string key { get; set; }

    [JsonProperty("value")]
    public string value { get; set; }
}
public class ApiClient
{
    private readonly string _baseUrl;
    private readonly RestClient _client;

    public ApiClient(string baseUrl)
    {
        _baseUrl = baseUrl;
        _client = new RestClient(baseUrl);
    }

    public async Task Set(string cluster, string key, string value)
    {
        var url = "/api/set";
        var request = new RestRequest(url, Method.Post);

        var setRequest = new SetRequest
        {
            cluster = cluster,
            key = key,
            value = Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
        };
        var jsonBody = JsonConvert.SerializeObject(setRequest);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            Console.WriteLine($"Value [{value}] set on cluster [{cluster}]");
        }
        else
        {
            Console.WriteLine($"Error setting value: {response.ErrorMessage}");
        }
    }

    public async Task<string> Get(string cluster, string key)
    {
        try
        {
            var url = $"/api/get/{cluster}/{key}";
            var request = new RestRequest(url, Method.Get);

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<byte[]>>(response.Content);
                if (apiResponse.IsSuccess)
                {
                    return Encoding.UTF8.GetString(apiResponse.Data).DecodeBase64ToString();
                }
                else
                {
                    throw new Exception("Failed to get value");
                }
            }
            return response.ErrorMessage;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting value: {ex.Message}");
        }
    }

    public async Task Delete(string cluster, string key)
    {
        var url = $"/api/delete/{cluster}/{key}";
        var request = new RestRequest(url, Method.Delete);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            Console.WriteLine($"Deleted {key} from cluster [{cluster}]");
        }
        else
        {
            throw new Exception($"Error deleting value: {response.ErrorMessage}");
        }
    }

    public async Task<List<string>> GetKeysOfCluster(string clusterName)
    {
        var url = $"/api/get_keys/{clusterName}";
        var request = new RestRequest(url, Method.Get);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<string>>>(response.Content);
            return apiResponse.Data;
        }
        else
        {
            throw new Exception($"Error getting keys of  cluster: {response.ErrorMessage}");
        }
    }

    public async Task SetCluster(string cluster)
    {
        var url = $"/api/set_cluster/{cluster}";
        var request = new RestRequest(url, Method.Post);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            Console.WriteLine($"cluster [{cluster}] set");
        }
        else
        {
            Console.WriteLine($"Error set cluster: {response.ErrorMessage}");
        }
    }
    public async Task ClearCluster(string cluster)
    {
        var url = $"/api/clear_cluster/{cluster}";
        var request = new RestRequest(url, Method.Delete);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            Console.WriteLine($"Cleared cluster [{cluster}]");
        }
        else
        {
            throw new Exception($"Error clearing cluster: {response.ErrorMessage}");
        }
    }

    public async Task ClearAll()
    {
        var url = $"/api/clear_all";
        var request = new RestRequest(url, Method.Delete);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            Console.WriteLine("Cleared all clusters");
        }
        else
        {
            throw new Exception($"Error clearing all clusters: {response.ErrorMessage}");
        }
    }

    public async Task<List<string>> GetAllClusters()
    {
        var url = $"/api/get_clusters";
        var request = new RestRequest(url, Method.Get);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            try
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<string>>>(response.Content);
                return apiResponse.Data;
            }
            catch
            {
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(response.Content);
                if (!string.IsNullOrEmpty(apiResponse.Data))
                {
                    return new List<string>() { apiResponse.Data };
                }
                else
                {
                    return new List<string>() { "no cluster found" };
                }
            }
        }
        else
        {
            throw new Exception($"Error getting clusters: {response.ErrorMessage}");
        }
    }


    public async Task<ushort> GetPort()
    {
        var url = $"/api/port";
        var request = new RestRequest(url, Method.Get);

        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ushort>>(response.Content);
            return apiResponse.Data;
        }
        else
        {
            throw new Exception($"Error getting port: {response.ErrorMessage}");
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var client = new ApiClient("http://127.0.0.1:6060");
        await CommandHandler.HandleCommand(client);
    }
}


public static class CommandHandler
{
    public static async Task HandleCommand(ApiClient client)
    {
        while (true)
        {
            PrintPrompt();

            var input = Console.ReadLine().Trim();
            var parts = input.Split(new[] { ' ' }, 4);
            if (parts.Length == 0)
            {
                continue;
            }

            switch (parts[0])
            {
                case "set" when parts.Length == 4:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        var value = parts[3];
                        await client.Set(cluster, key, value);
                        break;
                    }
                case "-v":
                    {
                        Console.WriteLine($"{AppName} version {Version}");
                        break;
                    }
                case "set_cluster" when parts.Length == 2:
                    {
                        var cluster = parts[1];
                        await client.SetCluster(cluster); 
                        break;
                    }
                case "keys*" when parts.Length == 2:
                    {
                        var cluster = parts[1];
                        var keys = await client.GetKeysOfCluster(cluster);
                        Console.WriteLine($"Keys in cluster [{cluster}]: {string.Join(", ", keys)}");
                        break;
                    }
                case "get" when parts.Length == 3:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        var value = await client.Get(cluster, key);
                        Console.WriteLine($"{value}");
                        break;
                    }
                case "delete" when parts.Length == 3:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        await client.Delete(cluster, key);
                        Console.WriteLine($"Deleted {key} from cluster [{cluster}]");
                        break;
                    }
                case "clear_cluster" when parts.Length == 2:
                    {
                        var cluster = parts[1];
                        await client.ClearCluster(cluster);
                        Console.WriteLine($"Cleared cluster [{cluster}]");
                        break;
                    }
                case "cluster*":
                    {
                        var clusters = await client.GetAllClusters();
                        Console.WriteLine($"Clusters are: {string.Join(", ", clusters)}");
                        break;
                    }
                case "help":
                    {
                        PrintHelp();
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Invalid command. Use 'help' to see available commands.");
                        break;
                    }
            }
        }
    }

    private static void PrintPrompt()
    {
        Console.Write("> ");
    }
    private static void PrintHelp()
    {
        Console.WriteLine("Commands:");
        Console.WriteLine("set [cluster name] [key] [value] - Set value for key in cluster");
        Console.WriteLine("set_cluster [cluster name] - Set a new cluster");
        Console.WriteLine("get [cluster name] [key] - Get value for key in cluster");
        Console.WriteLine("delete [cluster name] [key] - Delete key from cluster");
        Console.WriteLine("clear_cluster [cluster name] - Clear all keys in cluster");
        Console.WriteLine("cluster* - Get list of all clusters");
        Console.WriteLine("-v - Show version");
        Console.WriteLine("help - Show this help message");
    }

    private const string AppName = "MyApp";
    private const string Version = "1.0.0";
}
