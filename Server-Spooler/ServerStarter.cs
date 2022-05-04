namespace ServerSpooler;

using System.Diagnostics;

public static class ServerStarter{
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
        startInfo.Arguments = "\"" + Program.config_path + "\"";

        Process.Start(startInfo);
    }
}