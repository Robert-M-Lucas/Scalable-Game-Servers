namespace DatabaseServer;

using Shared;

using Microsoft.Data.Sqlite;
using System;

public static class Program {
    public static ConfigObj config;

    public static Server? server;

    public static void Main(string[] args) {
        Logger.InitialiseLogger("Database-Server", true);

        string config_path = "config.json";
        
        try {
            config = Config.GetConfig(config_path);
            Logger.debug_logged = config.Debug;
        }
        catch (BadConfigFormatException) { Logger.LogError("Incorrect formatting of config.json"); Exit(); }

        Console.Title = "Database Server";
        Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);

        DatabaseCommands.CreateTables();

        server = new Server("127.0.0.1", config.DatabaseServerPort);
        server.Start();

        Console.WriteLine("Press Ctrl+C to exit");
    }

    static void exitHandler(object? sender, ConsoleCancelEventArgs args) {
        Logger.LogInfo("Escape key pressed");
        args.Cancel = true;
        Exit();
    }

    public static void Exit() {
        Logger.LogWarning("Database shutting down");
        server?.Stop();
        Logger.CleanUp();
        Environment.Exit(0);
    }
}
