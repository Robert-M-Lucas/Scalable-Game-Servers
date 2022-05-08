namespace ServerSpooler;

using Shared;
using System.Net;
using System.Diagnostics;

public static class Program {
    # region Constants
    public const int MaxServerConnectTime = 20000;
    public const int GracefulShutdownTime = 1000;
    # endregion

    public static ConfigObj config;
    public static string config_path = "";


    public static List<LobbyData> LobbyServers = new List<LobbyData>();
    public static List<Tuple<ByteIP, uint>> GameServers = new List<Tuple<ByteIP, uint>>();

    public static uint LoadBalancerQueueLen = 0;

    public static bool exit = false;

    # region ResponseTimes
    
    public static long LoadBalancerResponseTime = 0;
    public static long AllLobbiesResponseTime = 0;
    public static long MatchmakerResponseTime = 0;

    # endregion

    public static void Main(string[] args) {
        // Example lobby server
        // LobbyServers.Add(new LobbyData(0, ByteIP.StringToIP("127.0.0.1", 123), new Process()));

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
        // TODO: Same for matchmaker


        Console.WriteLine("Press Ctrl-C to exit");

        try {
            while (!exit) {
                t.Reset();
                Console.WriteLine("Communicating with Load Balancer");
                Transciever.LoadBalancerTranscieve();
                LoadBalancerResponseTime = t.GetMsAndReset();
                
                // Transciever.MatchmakerTranscieve

                Console.WriteLine("Communicating with Lobbies");
                Transciever.LobbyServersTranscieve();
                AllLobbiesResponseTime = t.GetMsAndReset();


                int empty_lobby_severs = 0;
                foreach (LobbyData lobby_server in LobbyServers) {
                    if (lobby_server.FillLevel == 0) {
                        empty_lobby_severs++;
                    }
                }

                while (empty_lobby_severs < config.MinEmptyLobbies) {
                    ServerStarter.StartLobby();
                    empty_lobby_severs++;
                }

                while (empty_lobby_severs > config.MaxEmptyLobbies) {
                    ServerStarter.StopEmptyLobby();
                    empty_lobby_severs--;
                }
                
                InfoManager.ShowInfo();
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
