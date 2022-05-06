namespace ServerSpooler;

using Shared;
using System.Net;

public static class Program{
    public static int ServerSpoolerPort = -1;
    public static int LoadBalancerPort = -1;

    public static string Version = "";

    public static string config_path = "";

    public const int MaxServerConnectTime = 20;

    public static List<Tuple<ByteIP, uint>> LobbyServers = new List<Tuple<ByteIP, uint>>();

    public static bool exit = false;

    public static void Main(string[] args){
        if (args.Length < 1) { Console.WriteLine("No config.json path, exitting"); return; }
        config_path = args[0];
        
        try { 
            ConfigObj config = Config.GetConfig(config_path); 
            ServerSpoolerPort = config.ServerSpoolerPort;
            LoadBalancerPort = config.LoadBalancerPort;
            Version = config.Version;
        }
        catch (BadConfigFormatException) { Console.WriteLine("Incorrect formatting of config.json"); Exit(); }
        
        

        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);

        Console.WriteLine("Starting load balancer");
        ServerStarter.StartLoadBalancer();
        Console.WriteLine("Waiting for load balancer response");
        try { Listener.LoadBalancerSocket = Listener.AcceptClient(); }
        catch (ServerConnectTimeoutException) { Console.WriteLine("Load balancer didn't connect in required time, exitting"); Exit(); return; }
        // Same for matchmaker

        Console.WriteLine("Press Ctrl-C to exit");
        Console.ReadLine();
        /*
        while (!exit){

        }
        */
    }

    static void exitHandler(object? sender, ConsoleCancelEventArgs args)
    {
        Console.WriteLine("Escape key pressed");
        args.Cancel = true;
        exit = true;
        Exit();
    }

    public static void Exit() {
        Console.WriteLine("Shutting down network");
        Listener.Exit();
        Thread.Sleep(100);

        Console.WriteLine("Terminating processes");

        ServerStarter.Exit();

        Console.WriteLine("Exiting");

        Environment.Exit(0);
    }
}
