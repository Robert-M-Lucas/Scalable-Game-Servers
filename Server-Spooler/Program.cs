namespace ServerSpooler;

using Shared;
using System.Net;

public static class Program {
    # region Constants
    public const int MaxServerConnectTime = 20000;
    public const int GracefulShutdownTime = 1000;
    # endregion

    public static ConfigObj config;
    public static string config_path = "";


    public static List<Tuple<ByteIP, uint>> LobbyServers = new List<Tuple<ByteIP, uint>>();
    public static List<Tuple<ByteIP, uint>> GameServers = new List<Tuple<ByteIP, uint>>();

    public static bool exit = false;

    public static void Main(string[] args){
        // Example lobby server
        LobbyServers.Add(new Tuple<ByteIP, uint>(ByteIP.StringToIP("210.222.111.001", 8108), 201));

        // if (args.Length < 1) { Console.WriteLine("No config.json path, exitting"); return; }
        // config_path = args[0];
        config_path = "config.json";
        
        try { 
            config = Config.GetConfig(config_path);
        }
        catch (BadConfigFormatException) { Console.WriteLine("Incorrect formatting of config.json"); Exit(); }
        
        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);

        Timer t = new Timer();

        Console.WriteLine("Starting load balancer");
        ServerStarter.StartLoadBalancer();
        Console.WriteLine("Waiting for load balancer response");
        t.Reset();
        try { Listener.LoadBalancerSocket = Listener.AcceptClient(); }
        catch (ServerConnectTimeoutException) { Console.WriteLine("Load balancer didn't connect in required time, exitting"); Exit(); return; }
        Console.WriteLine($"Connected in {t.GetMsAndReset()}ms");
        // Same for matchmaker


        Console.WriteLine("Press Ctrl-C to exit");

        try {
            while (!exit){
                t.Reset();
                Console.WriteLine("Communicating with Load Balancer");
                Transciever.LoadBalancerTranscieve();
                Console.WriteLine($"Done in {t.GetMsAndReset()}ms");
                Thread.Sleep(1000);
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
            Exit();
        }
    }

    static void exitHandler(object? sender, ConsoleCancelEventArgs args) {
        Console.WriteLine("Escape key pressed");
        args.Cancel = true;
        exit = true;
        Exit();
    }

    public static void Exit() {
        Console.WriteLine("Shutting down network");
        Listener.Exit();

        Console.WriteLine("Giving time for graceful shutdown");
        Thread.Sleep(GracefulShutdownTime);

        Console.WriteLine("Terminating processes");

        ServerStarter.Exit();

        Console.WriteLine("Exiting");

        Environment.Exit(0);
    }
}
