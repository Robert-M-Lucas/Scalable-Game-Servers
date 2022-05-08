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

    public LobbyData(int uid, ByteIP _ip, Process _process) {
        UID = uid;
        ip = _ip;
        process = _process;
    }
}

public static class ServerStarter {
    public static Process? LoadBalancer;

    public static List<LobbyData> LobbyServers = new List<LobbyData>();

    public static void StartLoadBalancer() {
        // Code for starting load balancer would go here
        // Console.WriteLine(".Build\\Load-Balancer\\Load-Balancer.exe \"" + Program.config_path + "\"");
        // Process.Start(".Build\\Load-Balancer\\Load-Balancer.exe \"" + Program.config_path + "\"");

        // Use ProcessStartInfo class
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow = false;
        startInfo.UseShellExecute = true;
        startInfo.FileName = ".Build\\Load-Balancer\\Load-Balancer.exe";
        startInfo.WindowStyle = ProcessWindowStyle.Normal;
        startInfo.Arguments = $"\"{Program.config.Version}\" {"127.0.0.1"} {Program.config.ServerSpoolerPort} {Program.config.LoadBalancerPort} {Program.config.MaxLobbyFill} {Program.config.MaxQueueLen}";

        LoadBalancer = Process.Start(startInfo);
    }

    public static void StartLobby() {

    }

    public static void Exit() {
        if (!(LoadBalancer is null)) { Console.WriteLine("Killing load balancer"); LoadBalancer.Kill(); }
        Console.WriteLine("Killing lobby servers");
        foreach (LobbyData lb in LobbyServers) {
            if (!(lb.socket is null)) { lb.socket.Shutdown(SocketShutdown.Both); }
            lb.process.Kill();
        }
    }
}