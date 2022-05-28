namespace DatabaseServer;

using Shared;

using Microsoft.Data.Sqlite;
using System;

public static class Program {
    public static void Main(string[] args) {
        Logger.InitialiseLogger("Database-Server", true);

        Logger.LogInfo("Creating table");
        DatabaseCommands.CreateTables();

        Console.WriteLine(DatabaseCommands.GetValueFromDictionary("PlayerIDCounter"));

        DatabaseCommands.GetOrAddPlayer("playerOne", "password1");
        DatabaseCommands.GetOrAddPlayer("playerTwo", "password");

        Server server = new Server("127.0.0.1", 11111);
        server.Start();

        Console.ReadLine();

        server.Stop();

        Logger.LogInfo("Done");
        Logger.CleanUp();
    }
}
