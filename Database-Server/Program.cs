namespace DatabaseServer;

using Shared;

using Microsoft.Data.Sqlite;
using System;

public static class Program {
    public static void Main(string[] args) {
        Logger.InitialiseLogger("Database-Server", true);

        Logger.LogInfo("Creating table");
        DatabaseCommands.ExecuteNonQuery(
            @"
            CREATE TABLE IF NOT EXISTS Players (
                PlayerID INT PRIMARY KEY
            )
            "
        );

        Logger.LogInfo("Done");
        Logger.CleanUp();
    }
}
