namespace LobbyServer;

using System;
using Shared;

public static class Program {
    public static string SpoolerIP = "";
    public static int SpoolerPort = -1;

    public static int Port = -1;
    public static int MaxLobbyFill = -1;

    public const int MaxClientConnectionDataSendTime = 5000;

    public static string version = "";

    public static SILobbyServer? spoolerInterface;

    public static DatabaseInterface? databaseInterface;

    public static bool exit = false;

    public static uint fill_level;

    public static Server? server = null;

    static int LobbyServerID = -1;

    static int DatabaseServerPort = -1;

    public static void Main(string[] args) {
        // [version] [spooler ip] [spooler port] [lobby port] [max lobby fill] [lobby id]
        if (args.Length < 7) { Console.WriteLine("Not enough arguments"); return; }
        version = args[0];
        SpoolerIP = args[1];
        if (!int.TryParse(args[2], out SpoolerPort)) { Console.WriteLine("Spooler port incorrectly formatted"); return; }
        if (!int.TryParse(args[3], out Port)) { Console.WriteLine("Port incorrectly formatted"); return; }
        if (!int.TryParse(args[4], out MaxLobbyFill)) { Console.WriteLine("Max lobby fill incorrectly formatted"); return; }
        if (!int.TryParse(args[5], out LobbyServerID)) { Console.WriteLine("Lobby server ID incorrectly formatted"); return; }
        if (!int.TryParse(args[6], out DatabaseServerPort)) { Console.WriteLine("Database server port incorrectly formatted"); return; }

        Logger.InitialiseLogger("Lobby-Server-" + LobbyServerID, false);

        Console.Title = $"Lobby Server [{LobbyServerID}]";
        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);
        
        try {
            spoolerInterface = new SILobbyServer(SpoolerIP, SpoolerPort, ServerSpoolerExit);
        }
        catch (Exception e) {
            Logger.LogError("Error connecting to spooler");
            Logger.LogError(e);
            return;
        }

        try {
            databaseInterface = new DatabaseInterface("127.0.0.1", DatabaseServerPort);
        }
        catch (Exception e) {
            Logger.LogError("Error connecting to database");
            Logger.LogError(e);
            return;
        }

        Console.WriteLine("Press Ctrl-C to exit");
        try {
            server = new Server();
            server.Start();
        }
        catch (Exception e) {
            Logger.LogError(e);
            Exit();
        }
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

    public static void Exit(string reason = "") {
        Logger.LogInfo("Shutting down server");
        server?.Stop();
        databaseInterface?.Shutdown();
        Logger.LogWarning("Shutting down environment and Logger");
        Logger.CleanUp();
        Environment.Exit(0);
    }
}