namespace DatabaseServer;

using Shared;

using Microsoft.Data.Sqlite;
using System;

public static class Program {
    public static ConfigObj config;

    public static void Main(string[] args) {
        Logger.InitialiseLogger("Database-Server", true);

        string config_path = "config.json";
        
        try {
            config = Config.GetConfig(config_path);
        }
        catch (BadConfigFormatException) { Logger.LogError("Incorrect formatting of config.json"); Exit(); }

        Logger.LogInfo("Creating table");
        DatabaseCommands.CreateTables();

        Server server = new Server("127.0.0.1", config.DatabaseServerPort);
        server.Start();

        Console.ReadLine();

        server.Stop();

        Logger.LogInfo("Done");
        Logger.CleanUp();
    }

    public static void Exit() {

    }
}
