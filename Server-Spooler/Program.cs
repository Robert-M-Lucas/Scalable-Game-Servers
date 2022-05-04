namespace ServerSpooler;

using Shared;
using System.Net;

public static class Program{
    public static string ServerSpoolerIP = "";
    public static int ServerSpoolerPort = -1;

    public static string config_path = "";

    public const int MaxServerConnectTime = 20;

    public static bool exit = false;

    public static void Main(string[] args){
        if (args.Length < 1) { Console.WriteLine("No config.json path, exitting"); return; }
        config_path = args[0];
        try { // Do all config handling in here
            ConfigObj? config = Config.GetConfig(config_path);
            if (config is null) {throw new NullReferenceException();}
            if (config.Addresses is null) {throw new NullReferenceException();}
            if (config.Addresses.ServerSpooler is null) {throw new NullReferenceException();}
            if (config.Addresses.ServerSpooler.IP is null) {throw new NullReferenceException();}
            if (config.Addresses.ServerSpooler.Port is null) {throw new NullReferenceException();}

            ServerSpoolerIP = config.Addresses.ServerSpooler.IP;
            ServerSpoolerPort = (int) config.Addresses.ServerSpooler.Port;
        }
        catch (NullReferenceException n) { Console.WriteLine("Null reference exception, probable incorrect formatting of config.json"); Console.WriteLine(n); return; }
        catch (Exception e) { Console.WriteLine("Unhandled exception: " + e); Console.WriteLine("Probable incorrect formatting of config.json"); return; }

        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);

        Console.WriteLine("Starting load balancer");
        ServerStarter.StartLoadBalancer();
        Console.WriteLine("Waiting for load balancer response");
        Listener.LoadBalancerSocket = Listener.AcceptClient();
        // Same for matchmaker

        Console.WriteLine("Press Ctrl-C to exit");

        while (!exit){

        }
    }

    static void exitHandler(object? sender, ConsoleCancelEventArgs args)
    {
        Console.WriteLine("Escape key pressed");
        args.Cancel = true;
        exit = true;
    }
}
