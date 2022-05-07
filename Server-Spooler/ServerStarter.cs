namespace ServerSpooler;

using System.Diagnostics;

// Load balancer args:
// [Server Spooler IP] [Server Spooler Port] [Load Balancer Port]

public static class ServerStarter{
    public static Process? LoadBalancer;

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

    public static void Exit() {
        if (!(LoadBalancer is null)) { Console.WriteLine("Killing load balancer"); LoadBalancer.Kill(); }
    }
}