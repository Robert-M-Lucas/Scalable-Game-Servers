namespace ServerSpooler;

using Shared;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

// Load balancer args:
// [Server Spooler IP] [Server Spooler Port] [Load Balancer Port]

public class LobbyData {
    public ByteIP ip;
    public int UID;
    public Process process;
    public Socket? socket;
    public uint FillLevel = 0;

    public long response_time = 0;

    public LobbyData(int uid, ByteIP _ip, Process _process) {
        UID = uid;
        ip = _ip;
        process = _process;
    }
}

public static class ServerStarter {
    public static int LobbyPortCounter = 10123;
    public static int LobbyUIDCounter = 0;
    public static Process? LoadBalancer;
    public static Process? Matchmaker;

    public static List<LobbyData> LobbyServers = new List<LobbyData>();

    public static void StartLoadBalancer() {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow = false;
        startInfo.UseShellExecute = true;
        startInfo.FileName = "..\\Load-Balancer\\Load-Balancer.exe";
        Program.logger.LogDebug("Load balancer start");
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;
        startInfo.Arguments = $"\"{Program.config.Version}\" {"127.0.0.1"} {Program.config.ServerSpoolerPort} {Program.config.LoadBalancerPort} {Program.config.MaxLobbyFill} {Program.config.MaxQueueLen}";

        LoadBalancer = Process.Start(startInfo);
    }

    public static bool StartLobby() {
        Program.logger.LogInfo($"Starting lobby {LobbyUIDCounter} on port {LobbyPortCounter}");

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow = false;
        startInfo.UseShellExecute = true;
        startInfo.FileName = "..\\Lobby-Server\\Lobby-Server.exe";
        startInfo.WindowStyle = ProcessWindowStyle.Normal;
        startInfo.Arguments = $"\"{Program.config.Version}\" {"127.0.0.1"} {Program.config.ServerSpoolerPort} {LobbyPortCounter} {Program.config.MaxLobbyFill} {LobbyUIDCounter}";

        Process? lobby_server = Process.Start(startInfo);
        if (lobby_server is null) { return false; }
        LobbyData new_lobby = new LobbyData(LobbyUIDCounter, ByteIP.StringToIP("127.0.0.1", (uint) LobbyPortCounter), lobby_server);

        Program.logger.LogDebug("Waiting for lobby response");
        try {
            new_lobby.socket = Listener.AcceptClient();
        }
        catch (ServerConnectTimeoutException) {
            Program.logger.LogError("Lobby server failed to connect in time, killing...");
            if (lobby_server is not null) { lobby_server.Kill(); }
            LobbyUIDCounter ++;
            LobbyPortCounter ++;
            return false;
        }

        Program.LobbyServers.Add(new_lobby);
        LobbyUIDCounter ++;
        LobbyPortCounter ++;
        return true;
    }

    public static void StopEmptyLobby() {
        LobbyData? to_remove = null;
        foreach (LobbyData lobby in Program.LobbyServers) {
            if (lobby.FillLevel == 0) {
                Console.WriteLine($"Stopping lobby server {lobby.UID} at port {lobby.ip.iPort}");
                if (lobby.socket is not null) { lobby.socket.Shutdown(SocketShutdown.Both); }
                Thread.Sleep(200);
                lobby.process.Kill();
                to_remove = lobby;
                break;
            }
        }
        if (to_remove is not null) { Program.LobbyServers.Remove(to_remove); }
    }

    public static void StartMatchmaker() {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow = false;
        startInfo.UseShellExecute = true;
        startInfo.FileName = "..\\Matchmaker\\Matchmaker.exe";
        Program.logger.LogDebug("Matchmaker start");
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;
        startInfo.Arguments = $"\"{Program.config.Version}\" {"127.0.0.1"} {Program.config.ServerSpoolerPort} {Program.config.MatchmakerPort} {2} {Program.config.MaxQueueLen}";

        Matchmaker = Process.Start(startInfo);
    }

    public static void Exit() {
        if (LoadBalancer is not null) { Program.logger.LogInfo("Killing Load Balancer"); LoadBalancer.Kill(); }
        if (Matchmaker is not null) { Program.logger.LogInfo("Killing Matchmaker"); Matchmaker.Kill(); }

        Console.WriteLine("Killing lobby servers");
        foreach (LobbyData lb in LobbyServers) {
            if (lb.socket is not null) { lb.socket.Shutdown(SocketShutdown.Both); }
            lb.process.Kill();
        }
    }
}