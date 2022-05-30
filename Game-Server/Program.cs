namespace GameServer;

using System;
using Shared;

public static class Program {
    public static string SpoolerIP = "";
    public static int SpoolerPort = -1;

    public static int Port = -1;
    public static int MaxGameServerFill = -1;

    public const int MaxClientConnectionDataSendTime = 5000;

    public static string version = "";

    public static SIGameServer? spoolerInterface;

    public static bool exit = false;

    public static uint fill_level;

    public static Server? server = null;

    static int GameServerID = -1;

    public static void Main(string[] args) {
        // [version] [spooler ip] [spooler port] [game server port] [max game server fill] [lobby id]
        if (args.Length < 6) { Console.WriteLine("Not enough arguments"); return; }
        version = args[0];
        SpoolerIP = args[1];
        if (!int.TryParse(args[2], out SpoolerPort)) { Console.WriteLine("Spooler port incorrectly formatted"); return; }
        if (!int.TryParse(args[3], out Port)) { Console.WriteLine("Port incorrectly formatted"); return; }
        if (!int.TryParse(args[4], out MaxGameServerFill)) { Console.WriteLine("Max game fill incorrectly formatted"); return; }
        if (!int.TryParse(args[5], out GameServerID)) { Console.WriteLine("Game server ID incorrectly formatted"); return; }

        Logger.InitialiseLogger("Game-Server-" + GameServerID, false);

        Console.Title = $"Game Server [{GameServerID}]";
        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);
        
        try {
            spoolerInterface = new SIGameServer(SpoolerIP, SpoolerPort, ServerSpoolerExit);
        }
        catch (Exception e) {
            Logger.LogError("Error connecting to spooler");
            Logger.LogError(e);
            return;
        }

        Console.WriteLine("Press Ctrl-C to exit");
        server = new Server();
        server.Start();
        // Console.ReadLine();
    }

    static void exitHandler(object? sender, ConsoleCancelEventArgs args) {
        Console.WriteLine("Escape key pressed");
        args.Cancel = true;
        exit = true;
        Exit();
    }

    public static void ServerSpoolerExit(string reason, string? error = null) {
        Logger.LogError($"Exitting due to Server Spooler. Reason: {reason}");
        if (error is not null) {
            Logger.LogError($"Precise error: {error}");
        }
        Exit();
    }

    public static void Exit() {
        if (server is not null) {
            Logger.LogInfo("Shutting down server");
            server.Stop();
        }
        
        Logger.LogWarning("Shutting down environment and Logger");
        Logger.CleanUp();
        Environment.Exit(0);
    }
}