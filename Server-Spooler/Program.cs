namespace ServerSpooler;

using Shared;
using System.Net;
using System.Diagnostics;

public static class Program {
    # region Constants
    public const int MaxServerConnectTime = 20000;
    public const int GracefulShutdownTime = 1000;
    public const long ServerInfoInterval = 2000;
    public const int UpdateInterval = 100;
    # endregion

    public static ConfigObj config;
    public static string config_path = "";

    public static List<LobbyData> LobbyServers = new List<LobbyData>();
    public static List<GameServerData> GameServers = new List<GameServerData>();

    public static uint LoadBalancerQueueLen = 0;
    public static uint MatchmakerQueueLen = 0;

    public static bool exit = false;

    # region ResponseTimes
    
    public static long LoadBalancerResponseTime = 0;
    public static long AllLobbiesResponseTime = 0;
    public static long MatchmakerResponseTime = 0;
    public static long AllGameServersResponseTime = 0;
    public static long LobbyServerStartTime = 0;
    public static long LobbyServerStopTime = 0;
    public static long GameServerStartTime = 0;
    public static long GameServerStopTime = 0;

    # endregion

    public static void Main(string[] args) {
        Logger.InitialiseLogger("Server-Spooler", true);
        // Example lobby server
        // LobbyServers.Add(new LobbyData(0, ByteIP.StringToIP("127.0.0.1", 123), new Process()));

        // if (args.Length < 1) { Logger.LogInfo("No config.json path, exitting"); return; }
        // config_path = args[0];
        config_path = "config.json";
        
        try {
            config = Config.GetConfig(config_path);
            Logger.debug_printed = config.Debug;
        }
        catch (BadConfigFormatException) { Logger.LogError("Incorrect formatting of config.json"); Exit(); }
        
        Console.Title = "Server Spooler";
        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);

        Timer t = new Timer();

        Logger.LogInfo("Starting Load Balancer");
        ServerStarter.StartLoadBalancer();
        Logger.LogInfo("Waiting for Load Balancer response");
        t.Restart();
        try { Listener.LoadBalancerSocket = Listener.AcceptClient(); }
        catch (ServerConnectTimeoutException) { Logger.LogError("Load Balancer didn't connect in required time, exitting"); Exit(); return; }
        Logger.LogInfo($"Connected in {t.GetMsAndRestart()}ms");
        
        Logger.LogInfo("Starting Matchmaker");
        ServerStarter.StartMatchmaker();
        Logger.LogInfo("Waiting for Matchmaker response");
        t.Restart();
        try { Listener.MatchmakerSocket = Listener.AcceptClient(); }
        catch (ServerConnectTimeoutException) { Logger.LogError("Matchmaker didn't connect in required time, exitting"); Exit(); return; }
        Logger.LogInfo($"Connected in {t.GetMsAndRestart()}ms");


        Console.WriteLine("Press Ctrl-C to exit");
        
        Timer t_full = new Timer();
        Timer info_refresh = new Timer();

        try {
            while (!exit) {
                t.Restart();
                Logger.LogDebug("Communicating with Load Balancer");
                Transciever.LoadBalancerTranscieve();
                LoadBalancerResponseTime = t.GetMsAndRestart();
                
                Logger.LogDebug("Communicating with Matchmaker");
                Transciever.MatchmakerTranscieve();
                MatchmakerResponseTime = t.GetMsAndRestart();

                Logger.LogDebug("Communicating with Lobbies");
                Transciever.LobbyServersTranscieve();
                AllLobbiesResponseTime = t.GetMsAndRestart();

                Logger.LogDebug("Communicating with Game Servers");
                Transciever.GameServersTranscieve();
                AllGameServersResponseTime = t.GetMsAndRestart();


                int empty_lobby_severs = 0;
                foreach (LobbyData lobby_server in LobbyServers) {
                    if (lobby_server.FillLevel == 0) {
                        empty_lobby_severs++;
                    }
                }

                t.Restart();
                while (empty_lobby_severs < config.MinEmptyLobbies) {
                    ServerStarter.StartLobby();
                    empty_lobby_severs++;
                }
                LobbyServerStartTime = t.GetMsAndRestart();

                t.Restart();
                while (empty_lobby_severs > config.MaxEmptyLobbies) {
                    ServerStarter.StopEmptyLobby();
                    empty_lobby_severs--;
                }
                LobbyServerStopTime = t.GetMsAndRestart();

                int empty_game_severs = 0;
                foreach (GameServerData game_server in GameServers) {
                    if (game_server.FillLevel == 0) {
                        empty_game_severs++;
                    }
                }

                t.Restart();
                while (empty_game_severs < config.MinEmptyGameServers) {
                    ServerStarter.StartGameServer();
                    empty_game_severs++;
                }
                GameServerStartTime = t.GetMsAndRestart();

                t.Restart();
                while (empty_game_severs > config.MaxEmptyGameServers) {
                    ServerStarter.StopEmptyGameServer();
                    empty_game_severs--;
                }
                GameServerStopTime = t.GetMsAndRestart();
                
                if (info_refresh.GetMs() > ServerInfoInterval) {
                    InfoManager.ShowInfo(t_full.GetMs());
                    info_refresh.Restart();
                }

                long update_len = t_full.GetMs();
                if (UpdateInterval - update_len > 0) { Thread.Sleep(UpdateInterval - (int) update_len); }
                else { Logger.LogWarning($"Update [{update_len}ms] took longer than update interval [{UpdateInterval}ms]"); }
                t_full.Restart();
            }
        }
        catch (Exception e) {
            Logger.LogError("Error in main transcieve loop");
            Logger.LogError(e.ToString());
            Exit();
        }
    }

    static void exitHandler(object? sender, ConsoleCancelEventArgs args) {
        Logger.LogInfo("Escape key pressed");
        args.Cancel = true;
        exit = true;
        Exit();
    }

    public static void Exit() {
        Logger.LogInfo("Shutting down network");
        Listener.Exit();

        Logger.LogInfo("Giving time for graceful shutdown");
        Thread.Sleep(GracefulShutdownTime);

        Logger.LogInfo("Terminating processes");

        ServerStarter.Exit();

        Logger.CleanUp();

        Console.WriteLine("Exiting");

        Environment.Exit(0);
    }
}
