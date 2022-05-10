namespace ServerSpooler;

using Shared;
using System.Net;
using System.Diagnostics;

public static class Program {
    # region Constants
    public const int MaxServerConnectTime = 20000;
    public const int GracefulShutdownTime = 1000;
    public const long ServerInfoInterval = 2000;
    public const int interval = 50;
    # endregion

    public static Logger logger = new Logger("Server-Spooler", true);

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
        logger.debug_logged = config.Debug;
        // Example lobby server
        // LobbyServers.Add(new LobbyData(0, ByteIP.StringToIP("127.0.0.1", 123), new Process()));

        // if (args.Length < 1) { Program.logger.LogInfo("No config.json path, exitting"); return; }
        // config_path = args[0];
        config_path = "config.json";
        
        try { 
            config = Config.GetConfig(config_path);
        }
        catch (BadConfigFormatException) { Program.logger.LogError("Incorrect formatting of config.json"); Exit(); }
        
        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);

        Timer t = new Timer();

        Program.logger.LogInfo("Starting load balancer");
        ServerStarter.StartLoadBalancer();
        Program.logger.LogInfo("Waiting for load balancer response");
        t.Reset();
        try { Listener.LoadBalancerSocket = Listener.AcceptClient(); }
        catch (ServerConnectTimeoutException) { Program.logger.LogError("Load balancer didn't connect in required time, exitting"); Exit(); return; }
        Program.logger.LogInfo($"Connected in {t.GetMsAndReset()}ms");
        // TODO: Same for matchmaker


        Console.WriteLine("Press Ctrl-C to exit");
        
        Timer t_full = new Timer();
        Timer info_refresh = new Timer();

        try {
            while (!exit) {
                t.Reset();
                Program.logger.LogDebug("Communicating with Load Balancer");
                Transciever.LoadBalancerTranscieve();
                LoadBalancerResponseTime = t.GetMsAndReset();
                
                // Transciever.MatchmakerTranscieve

                Program.logger.LogDebug("Communicating with Lobbies");
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
                
                if (info_refresh.GetMs() > ServerInfoInterval) {
                    InfoManager.ShowInfo();
                    info_refresh.Restart();
                }

                long update_len = t_full.GetMs();
                if (interval - update_len > 0) { Thread.Sleep(interval - (int) update_len); }
                t_full.Reset();
            }
        }
        catch (Exception e) {
            logger.LogError("Error in main transcieve loop");
            logger.LogError(e.ToString());
            Exit();
        }
    }

    static void exitHandler(object? sender, ConsoleCancelEventArgs args) {
        Program.logger.LogInfo("Escape key pressed");
        args.Cancel = true;
        exit = true;
        Exit();
    }

    public static void Exit() {
        logger.LogInfo("Shutting down network");
        Listener.Exit();

        logger.LogInfo("Giving time for graceful shutdown");
        Thread.Sleep(GracefulShutdownTime);

        logger.LogInfo("Terminating processes");

        ServerStarter.Exit();

        logger.CleanUp();

        Console.WriteLine("Exiting");

        Environment.Exit(0);
    }
}
