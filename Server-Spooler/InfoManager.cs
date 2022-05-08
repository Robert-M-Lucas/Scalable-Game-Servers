namespace ServerSpooler;

using System;

public static class InfoManager {
    public static void ShowInfo() {
        Console.WriteLine($"----------INFO START----------");
        Console.WriteLine($"Server Spooler:");
        Console.WriteLine($" | IP: {"127.0.0.1"}:{Program.config.ServerSpoolerPort}");
        Console.WriteLine($"Load Balancer [{Program.LoadBalancerResponseTime}ms]:");
        Console.WriteLine($" | IP: {"127.0.0.1"}:{Program.config.LoadBalancerPort}");
        Console.WriteLine($" | Queue Length: {Program.LoadBalancerQueueLen}");
        Console.WriteLine($"Lobby Servers [{Program.AllLobbiesResponseTime}ms]: ");
        foreach (LobbyData lobby in Program.LobbyServers) {
            Console.WriteLine($" | Lobby Server [{lobby.UID}] [{lobby.response_time}ms]: ");
            Console.WriteLine($" | | IP: {"127.0.0.1"}:{lobby.ip.iPort}");
            Console.WriteLine($" | | Fill Level: {lobby.FillLevel}");
        }
        Console.WriteLine($"----------INFO ENDS----------");
    }
}