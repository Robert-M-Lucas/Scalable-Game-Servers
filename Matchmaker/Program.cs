﻿namespace Matchmaker;
using System;
using Shared;

public static class Program{
    public static int Port = -1;

    public static string SpoolerIP = "";
    public static int SpoolerPort = -1;

    public static string version = "";
    public static int MaxQueueLen = 1000;
    public static int MaxGameFill = 0;

    public static SIMatchmaker? spoolerInterface;

    public static bool exit = false;

    public static Server? server;

    public static List<Tuple<ByteIP, uint>> GameServers = new List<Tuple<ByteIP, uint>>();

    public static Logger logger = new Logger("Load-Balancer", false);

    public static void Main(string[] args) {
        if (args.Length < 6) { logger.LogError("Args must be: [Version] [Server Spooler IP] [Server Spooler Port] [Matchmaker Port] [Max Game fill] [Max queue length]"); Exit(); return; }

        version = args[0];
        SpoolerIP = args[1];
        if (!int.TryParse(args[2], out SpoolerPort)) { logger.LogInfo("Spooler port incorrectly formatted"); Exit(); return; }
        if (!int.TryParse(args[3], out Port)) { logger.LogInfo("Port incorrectly formatted"); Exit(); return; }
        if (!int.TryParse(args[4], out MaxGameFill)) { logger.LogInfo("Max lobby fill incorrectly formatted"); Exit(); return; }
        if (!int.TryParse(args[5], out MaxQueueLen)) { logger.LogInfo("Max queue fill incorrectly formatted"); Exit(); return; }

        Console.Title = "Matchmaker";
        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);

        try {
            spoolerInterface = new SIMatchmaker(SpoolerIP, SpoolerPort, logger);
        }
        catch (Exception e) {
            logger.LogError("Error connecting to spooler");
            logger.LogError(e.ToString());
            return;
        }

        Console.WriteLine("Press Ctrl-C to exit");

        server = new Server();
        server.Start();

        while (!exit) {
            if (server.Players.Count > 0) {
                // Temp server list to not have to worry about updates from spooler
                Tuple<ByteIP, uint>[] GameServersCopy = new Tuple<ByteIP, uint>[GameServers.Count];
                GameServers.CopyTo(GameServersCopy);
                logger.LogInfo(GameServersCopy.Length);

                int best_lobby_server = -1;
                for (int i = 0; i < GameServersCopy.Length; i++){
                    if ((int) GameServersCopy[i].Item2 < MaxGameFill) {
                        if (best_lobby_server == -1) { best_lobby_server = i; continue; }
                        if (GameServersCopy[best_lobby_server].Item2 < GameServersCopy[i].Item2) { best_lobby_server = i; }
                    }
                }

                if (best_lobby_server == -1) { 
                    logger.LogWarning("No available game servers to transfer client to");
                    Thread.Sleep(200);
                    continue;
                }
                server.TransferClient(GameServersCopy[best_lobby_server].Item1);
                // TODO: Make server count update on player transfer
            }

            Thread.Sleep(10);
        }
    }

    static void exitHandler(object? sender, ConsoleCancelEventArgs args) {
        logger.LogInfo("Escape key pressed");
        args.Cancel = true;
        exit = true;
        Exit();
    }

    public static void Exit() {
        logger.LogInfo("Shutting down server");
        if (server is not null) {server.Stop();}
        logger.LogInfo("Shutting down logger and environment");
        logger.CleanUp();
        Environment.Exit(0);
    }
}