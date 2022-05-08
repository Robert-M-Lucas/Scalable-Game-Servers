namespace LobbyServer;
using System;

public static class Program {
    public static string SpoolerIP = "";
    public static int SpoolerPort = -1;

    public static int Port = -1;
    public static int MaxLobbyFill = -1;

    public static string version = "";

    public static List<Client> Players = new List<Client>();

    public static SILobbyServer? spoolerInterface;

    public static bool exit = false;

    public static uint fill_level;

    public static void Main(string[] args) {
        // [version] [spooler ip] [spooler port] [lobby port] [max lobby fill]
        if (args.Length < 5) { Console.WriteLine("Not enough arguments"); return; }
        version = args[0];
        SpoolerIP = args[1];
        if (!int.TryParse(args[2], out SpoolerPort)) { Console.WriteLine("Spooler port incorrectly formatted"); return; }
        if (!int.TryParse(args[3], out Port)) { Console.WriteLine("Port incorrectly formatted"); return; }
        if (!int.TryParse(args[4], out MaxLobbyFill)) { Console.WriteLine("Max lobby fill incorrectly formatted"); return; }

        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);
        
        try {
            spoolerInterface = new SILobbyServer(SpoolerIP, SpoolerPort);
        }
        catch (Exception e) {
            Console.WriteLine("Error connecting to spooler");
            Console.WriteLine(e);
            return;
        }

        Console.WriteLine("Press Ctrl-C to exit");
        Console.ReadLine();
    }

    static void exitHandler(object? sender, ConsoleCancelEventArgs args) {
        Console.WriteLine("Escape key pressed");
        args.Cancel = true;
        exit = true;
        Exit();
    }

    public static void Exit() {
        Console.WriteLine("Shutting down environment");
        Environment.Exit(0);
    }
}