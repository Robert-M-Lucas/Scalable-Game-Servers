namespace Shared;

using System.Text.Json;

public class ConfigObj{
    public string? MatchmakerIP;
}

public static class Config{
    public static ConfigObj? GetConfig(string config_path){
        return JsonSerializer.Deserialize<ConfigObj>(File.ReadAllText(config_path));
    }
}