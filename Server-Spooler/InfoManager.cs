namespace ServerSpooler;

using System;
using Shared;

public static class InfoManager {
    public static void ShowInfo(long full_time_taken) {
        Logger.LogImportant($"----------INFO START----------");
        Logger.LogInfo($"Full update / target update interval =  [{full_time_taken}/{Program.UpdateInterval}ms]");
        Logger.LogInfo($"Starting Lobby Servers took [{Program.LobbyServerStartTime}ms]");
        Logger.LogInfo($"Stopping Lobby Servers took [{Program.LobbyServerStopTime}ms]");
        Logger.LogInfo($"Starting Game Servers took [{Program.GameServerStartTime}ms]");
        Logger.LogInfo($"Starting Game Servers took [{Program.GameServerStopTime}ms]");
        Logger.LogInfo($"");

        Logger.LogInfo($"Server Spooler:");
        Logger.LogInfo($" | >IP: {"127.0.0.1"}:{Program.config.ServerSpoolerPort}");

        Logger.LogInfo($"Load Balancer [{Program.LoadBalancerResponseTime}ms]:");
        Logger.LogInfo($" | >IP: {"127.0.0.1"}:{Program.config.LoadBalancerPort}");
        Logger.LogInfo($" | >Queue Length: {Program.LoadBalancerQueueLen}");

        Logger.LogInfo($"Lobby Servers [{Program.AllLobbiesResponseTime}ms]: ");
        foreach (LobbyData lobby in Program.LobbyServers) {
            Logger.LogInfo($" | Lobby Server [{lobby.UID}] [{lobby.response_time}ms]: ");
            Logger.LogInfo($" | | >IP: {"127.0.0.1"}:{lobby.ip.iPort}");
            Logger.LogInfo($" | | >Fill Level: {lobby.FillLevel}/{Program.config.MaxLobbyFill}");
        }

        Logger.LogInfo($"Matchmaker [{Program.MatchmakerResponseTime}ms]:");
        Logger.LogInfo($" | >IP: {"127.0.0.1"}:{Program.config.MatchmakerPort}");
        Logger.LogInfo($" | >Queue Length: {Program.MatchmakerQueueLen}");

        Logger.LogInfo($"Game Servers [{Program.AllGameServersResponseTime}ms]: ");
        foreach (GameServerData gameServers in Program.GameServers) {
            Logger.LogInfo($" | Game Server [{gameServers.UID}] [{gameServers.response_time}ms]: ");
            Logger.LogInfo($" | | >IP: {"127.0.0.1"}:{gameServers.ip.iPort}");
            Logger.LogInfo($" | | >Fill Level: {gameServers.FillLevel}/{2}");
        }

        Logger.LogInfo($"----------INFO ENDS----------");
    }
}