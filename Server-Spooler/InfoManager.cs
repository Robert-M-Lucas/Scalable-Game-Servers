namespace ServerSpooler;

using System;

public static class InfoManager {
    public static void ShowInfo(long full_time_taken) {
        Program.logger.LogImportant($"----------INFO START----------");
        Program.logger.LogInfo($"Full update took [{full_time_taken}ms]");
        Program.logger.LogInfo($"Starting Lobby Servers took [{Program.LobbyServerStartTime}ms]");
        Program.logger.LogInfo($"Stopping Lobby Servers took [{Program.LobbyServerStopTime}ms]");
        Program.logger.LogInfo($"Starting Game Servers took [{Program.GameServerStartTime}ms]");
        Program.logger.LogInfo($"Starting Game Servers took [{Program.GameServerStopTime}ms]");
        Program.logger.LogInfo($"");

        Program.logger.LogInfo($"Server Spooler:");
        Program.logger.LogInfo($" | >IP: {"127.0.0.1"}:{Program.config.ServerSpoolerPort}");
        Program.logger.LogInfo($" |");

        Program.logger.LogInfo($"Load Balancer [{Program.LoadBalancerResponseTime}ms]:");
        Program.logger.LogInfo($" | >IP: {"127.0.0.1"}:{Program.config.LoadBalancerPort}");
        Program.logger.LogInfo($" | >Queue Length: {Program.LoadBalancerQueueLen}");
        Program.logger.LogInfo($" |");

        Program.logger.LogInfo($"Lobby Servers [{Program.AllLobbiesResponseTime}ms]: ");
        foreach (LobbyData lobby in Program.LobbyServers) {
            Program.logger.LogInfo($" | Lobby Server [{lobby.UID}] [{lobby.response_time}ms]: ");
            Program.logger.LogInfo($" | | >IP: {"127.0.0.1"}:{lobby.ip.iPort}");
            Program.logger.LogInfo($" | | >Fill Level: {lobby.FillLevel}/{Program.config.MaxLobbyFill}");
        }
        Program.logger.LogInfo($" |");

        Program.logger.LogInfo($"Matchmaker [{Program.MatchmakerResponseTime}ms]:");
        Program.logger.LogInfo($" | >IP: {"127.0.0.1"}:{Program.config.MatchmakerPort}");
        Program.logger.LogInfo($" | >Queue Length: {Program.MatchmakerQueueLen}");
        Program.logger.LogInfo($" |");

        Program.logger.LogInfo($"Game Servers [{Program.AllGameServersResponseTime}ms]: ");
        foreach (GameServerData gameServers in Program.GameServers) {
            Program.logger.LogInfo($" | Game Server [{gameServers.UID}] [{gameServers.response_time}ms]: ");
            Program.logger.LogInfo($" | | >IP: {"127.0.0.1"}:{gameServers.ip.iPort}");
            Program.logger.LogInfo($" | | >Fill Level: {gameServers.FillLevel}/{2}");
        }

        Program.logger.LogInfo($"----------INFO ENDS----------");
    }
}