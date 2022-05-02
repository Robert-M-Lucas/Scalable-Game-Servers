namespace Shared;

using System.Text.Json;


public class ConfigTextAddress{
    public string? IP { get; set; }
    public int? Port { get; set; }
}

public class ConfigAddresses{
    public ConfigTextAddress? Load_Balancer { get; set; }
    public ConfigTextAddress? Matchmaker { get; set; }
}

public class ConfigBalancer{
    public int? MinEmptyServers { get; set; }
    public int? MaxEmptyServers { get; set; }
    public int? MaxQueueLen { get; set; }
}

public class ConfigObj{
    public ConfigAddresses? Addresses { get; set; }
    public ConfigBalancer? Load_Balancer { get; set; }
    public ConfigBalancer? Matchmaker { get; set; }
}

public static class Config{
    public static ConfigObj? GetConfig(string config_path){
        return JsonSerializer.Deserialize<ConfigObj>(File.ReadAllText(config_path));
    }
}