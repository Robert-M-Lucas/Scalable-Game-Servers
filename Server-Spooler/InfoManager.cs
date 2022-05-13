namespace ServerSpooler;

using System;

public static class InfoManager {
    public static void ShowInfo() {
        Program.logger.LogImportant($"----------INFO START----------");
        Program.logger.LogInfo($"Server Spooler:");
        Program.logger.LogInfo($" | >IP: {"127.0.0.1"}:{Program.config.ServerSpoolerPort}");
        Program.logger.LogInfo($"Load Balancer [{Program.LoadBalancerResponseTime}ms]:");
        Program.logger.LogInfo($" | >IP: {"127.0.0.1"}:{Program.config.LoadBalancerPort}");
        Program.logger.LogInfo($" | >Queue Length: {Program.LoadBalancerQueueLen}");
        Program.logger.LogInfo($"Lobby Servers [{Program.AllLobbiesResponseTime}ms]: ");
        foreach (LobbyData lobby in Program.LobbyServers) {
            Program.logger.LogInfo($" | Lobby Server [{lobby.UID}] [{lobby.response_time}ms]: ");
            Program.logger.LogInfo($" | | >IP: {"127.0.0.1"}:{lobby.ip.iPort}");
            Program.logger.LogInfo($" | | >Fill Level: {lobby.FillLevel}/{Program.config.MaxLobbyFill}");
        }
        Program.logger.LogInfo($"----------INFO ENDS----------");
    }
}