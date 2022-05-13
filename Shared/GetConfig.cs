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
    
    public bool? Debug { get; set; }
    public string? Version { get; set; }

    public int? ServerSpoolerPort { get; set; }
    public int? MatchmakerPort { get; set; }
    public int? LoadBalancerPort { get; set; }

    public int? MaxQueueLen { get; set; }
    public int? MaxLobbyFill { get; set; }

    public int? MaxEmptyLobbies { get; set; }
    public int? MinEmptyLobbies { get; set; }
    
    public int? MaxEmptyGameServers { get; set; }
    public int? MinEmptyGameServers { get; set; }

}

public struct ConfigObj {
    public bool Debug;
    public string Version;

    public int ServerSpoolerPort;
    public int MatchmakerPort;
    public int LoadBalancerPort;

    public int MaxQueueLen;
    public int MaxLobbyFill;

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
        NullableConfigObj? null_config = null;
        try {
            null_config = JsonSerializer.Deserialize<NullableConfigObj>(File.ReadAllText(config_path));
        }
        catch (System.IO.IOException e) {
            Console.WriteLine(e);
            throw new BadConfigFormatException(e.ToString());
        }

        if (null_config?.Version is null) {throw new BadConfigFormatException("Version");}
        if (null_config.ServerSpoolerPort is null) {throw new BadConfigFormatException("ServerSpoolerPort");}
        if (null_config.MatchmakerPort is null) {throw new BadConfigFormatException("MatchmakerPort");}
        if (null_config.LoadBalancerPort is null) {throw new BadConfigFormatException("LoadBalancerProt");}
        if (null_config.MaxLobbyFill is null) {throw new BadConfigFormatException("MaxLobbyFill");}
        if (null_config.MaxQueueLen is null) {throw new BadConfigFormatException("MaxQueueLen");}
        if (null_config.MaxEmptyGameServers is null) {throw new BadConfigFormatException("MaxEmptyGameServers");}
        if (null_config.MinEmptyGameServers is null) {throw new BadConfigFormatException("MinEmptyGameServers");}
        if (null_config.MaxEmptyLobbies is null) {throw new BadConfigFormatException("MaxEmptyLobbies");}
        if (null_config.MinEmptyLobbies is null) {throw new BadConfigFormatException("MinEmptyLobbies");}
        if (null_config.Debug is null) {throw new BadConfigFormatException("Debug");}

        ConfigObj config = new ConfigObj();
        config.Version = null_config.Version; config.ServerSpoolerPort = (int) null_config.ServerSpoolerPort; config.MatchmakerPort = (int) null_config.MatchmakerPort;
        config.LoadBalancerPort = (int) null_config.LoadBalancerPort; config.MaxEmptyGameServers = (int) null_config.MaxEmptyGameServers; config.MinEmptyGameServers = (int) null_config.MinEmptyGameServers;
        config.MaxEmptyLobbies = (int) null_config.MaxEmptyLobbies; config.MinEmptyLobbies = (int) null_config.MinEmptyLobbies;
        config.MaxLobbyFill = (int) null_config.MaxLobbyFill; config.MaxQueueLen = (int) null_config.MaxQueueLen; config.Debug = (bool) null_config.Debug;

        if (config.MaxEmptyLobbies < config.MinEmptyLobbies) { throw new BadConfigFormatException("MaxEmptyLobbies < MinEmptyLobbies"); }
        if (config.MaxEmptyGameServers < config.MinEmptyGameServers) { throw new BadConfigFormatException("MaxEmptyGameServers < MinEmptyGameServers"); }

        return config;
    }
}