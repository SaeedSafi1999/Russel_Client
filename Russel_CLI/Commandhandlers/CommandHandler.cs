using Russel_CLI.Helpers;

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
