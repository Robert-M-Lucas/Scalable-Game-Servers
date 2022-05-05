namespace Shared;

using System.Text.Json;
using System.Text.Json.Serialization;

/*
{
    "Version": "1.0",

    "ServerSpoolerPort": 5105,
    "MatchmakerPort": 6106,
    "LoadBalancerPort": 7107,

    "MaxEmptyLobbies": 10,
    "MinEmptyLobbies": 2,

    "MaxEmptyGameServers": 15,
    "MinEmptyGameServers": 7
}
*/

public class NullableConfigObj{
    public string? Version { get; set; }

    public int? ServerSpoolerPort { get; set; }
    public int? MatchmakerPort { get; set; }
    public int? LoadBalancerPort { get; set; }

    public int? MaxEmptyLobbies { get; set; }
    public int? MinEmptyLobbies { get; set; }
    
    public int? MaxEmptyGameServers { get; set; }
    public int? MinEmptyGameServers { get; set; }

}

public struct ConfigObj {
    public string Version;

    public int ServerSpoolerPort;
    public int MatchmakerPort;
    public int LoadBalancerPort;

    public int MaxEmptyLobbies;
    public int MinEmptyLobbies;
    
    public int MaxEmptyGameServers;
    public int MinEmptyGameServers;
}

public class BadConfigFormatException : Exception
{
    public BadConfigFormatException() { }

    public BadConfigFormatException(string message) : base(message) { }

    public BadConfigFormatException(string message, Exception inner) : base(message, inner) { }
}

public static class Config{
    public static ConfigObj GetConfig(string config_path){
        NullableConfigObj? null_config = JsonSerializer.Deserialize<NullableConfigObj>(File.ReadAllText(config_path));

        if (null_config is null) {throw new BadConfigFormatException();}
        if (null_config.Version is null) {throw new BadConfigFormatException();}
        if (null_config.ServerSpoolerPort is null) {throw new BadConfigFormatException();}
        if (null_config.MatchmakerPort is null) {throw new BadConfigFormatException();}
        if (null_config.LoadBalancerPort is null) {throw new BadConfigFormatException();}
        if (null_config.MaxEmptyGameServers is null) {throw new BadConfigFormatException();}
        if (null_config.MinEmptyGameServers is null) {throw new BadConfigFormatException();}
        if (null_config.MaxEmptyLobbies is null) {throw new BadConfigFormatException();}
        if (null_config.MinEmptyLobbies is null) {throw new BadConfigFormatException();}

        ConfigObj config = new ConfigObj();
        config.Version = null_config.Version; config.ServerSpoolerPort = (int) null_config.ServerSpoolerPort; config.MatchmakerPort = (int) null_config.MatchmakerPort;
        config.LoadBalancerPort = (int) null_config.LoadBalancerPort; config.MaxEmptyGameServers = (int) null_config.MaxEmptyGameServers; config.MinEmptyGameServers = (int) null_config.MinEmptyGameServers;
        config.MaxEmptyLobbies = (int) null_config.MaxEmptyLobbies; config.MinEmptyLobbies = (int) null_config.MinEmptyLobbies;

        return config;
    }
}