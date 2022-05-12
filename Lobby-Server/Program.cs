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

    public static bool exit = false;

    public static uint fill_level;

    public static Logger? _logger = null;

    public static Server? server = null;

    public static Logger logger {
    get {
        if (_logger is null) {throw new NullReferenceException();}
        return _logger;
    }}

    static int LobbyServerID = -1;

    public static void Main(string[] args) {
        // [version] [spooler ip] [spooler port] [lobby port] [max lobby fill] [lobby id]
        if (args.Length < 6) { Console.WriteLine("Not enough arguments"); return; }
        version = args[0];
        SpoolerIP = args[1];
        if (!int.TryParse(args[2], out SpoolerPort)) { Console.WriteLine("Spooler port incorrectly formatted"); return; }
        if (!int.TryParse(args[3], out Port)) { Console.WriteLine("Port incorrectly formatted"); return; }
        if (!int.TryParse(args[4], out MaxLobbyFill)) { Console.WriteLine("Max lobby fill incorrectly formatted"); return; }
        if (!int.TryParse(args[5], out LobbyServerID)) { Console.WriteLine("Lobby server ID incorrectly formatted"); return; }

        _logger =  new Logger("Lobby-Server-" + LobbyServerID, false);

        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);
        
        try {
            spoolerInterface = new SILobbyServer(SpoolerIP, SpoolerPort, logger);
        }
        catch (Exception e) {
            logger.LogError("Error connecting to spooler");
            logger.LogError(e);
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

    public static void Exit() {
        if (!(server is null)) {
            logger.LogInfo("Shutting down server");
            server.Stop();
        }
        
        logger.LogWarning("Shutting down environment and logger");
        logger.CleanUp();
        Environment.Exit(0);
    }
}