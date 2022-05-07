namespace LoadBalancer;

using System;
using System.Net;
using System.Net.Sockets;
using Shared;

public static class Program{
    public static int Port = -1;

    public static string SpoolerIP = "";
    public static int SpoolerPort = -1;

    public static string Version = "";
    public static int MaxQueueLen = 1000;
    public static int MaxLobbyFill = 0;

    public static Socket? SpoolerSocket;

    public static SpoolerInterface? spoolerInterface;

    public static bool exit = false;

    public static Server? server;

     public static List<Tuple<ByteIP, uint>> LobbyServers = new List<Tuple<ByteIP, uint>>();

    public static void Main(string[] args) {
        if (args.Length < 6) { Console.WriteLine("Args must be: [Version] [Server Spooler IP] [Server Spooler Port] [Load Balancer Port] [Max lobby fill] [Max queue length]"); return; }

        Version = args[0];
        SpoolerIP = args[1];
        if (!int.TryParse(args[2], out SpoolerPort)) { Console.WriteLine("Spooler port incorrectly formatted"); return; }
        if (!int.TryParse(args[3], out Port)) { Console.WriteLine("Port incorrectly formatted"); return; }
        if (!int.TryParse(args[4], out MaxLobbyFill)) { Console.WriteLine("Max lobby fill incorrectly formatted"); return; }
        if (!int.TryParse(args[5], out MaxQueueLen)) { Console.WriteLine("Max queue fill incorrectly formatted"); return; }

        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);

        try {
            spoolerInterface = new SpoolerInterface(SpoolerIP, SpoolerPort);
        }
        catch (Exception e) {
            Console.WriteLine("Error connecting to spooler");
            Console.WriteLine(e);
            return;
        }

        Console.WriteLine("Press Ctrl-C to exit");

        server = new Server();
        server.Start();

        while (!exit) {
            if (server.Players.Count > 0) {
                Tuple<ByteIP, uint>[] LobbyServersCopy = new Tuple<ByteIP, uint>[LobbyServers.Count];
                LobbyServers.CopyTo(LobbyServersCopy);

                int best_lobby_server = -1;
                for (int i = 0; i < LobbyServersCopy.Length; i++){
                    if ((int) LobbyServersCopy[i].Item2 < MaxLobbyFill) {
                        if (best_lobby_server == -1) { best_lobby_server = i; continue; }
                        if (LobbyServersCopy[best_lobby_server].Item2 < LobbyServersCopy[i].Item2) { best_lobby_server = i;}
                    }
                }

                if (best_lobby_server == -1) { 
                    Console.WriteLine("No available lobby servers to transfer client to");
                    Thread.Sleep(200);
                    continue;
                }
                server.TransferClient(LobbyServersCopy[best_lobby_server].Item1);
                // TODO: Make server count update on player transfer
            }

            Thread.Sleep(10);
        }
    }

    static void exitHandler(object? sender, ConsoleCancelEventArgs args) {
        Console.WriteLine("Escape key pressed");
        args.Cancel = true;
        exit = true;
        Exit();
    }

    public static void Exit() {
        Console.WriteLine("Shutting down server");
        if (!(server is null)) {server.Stop();}
        Console.WriteLine("Shutting down environment");
        Environment.Exit(0);
    }
}