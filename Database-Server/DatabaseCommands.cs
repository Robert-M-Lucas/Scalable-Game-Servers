namespace DatabaseServer;

using Microsoft.Data.Sqlite;

using Shared;

public class CommandArgsPair {
    public string toReplace;
    public object replaceWith;

    public CommandArgsPair(string to_replace, object replace_with) {
        toReplace = to_replace;
        replaceWith = replace_with;
    }
}

public static class DatabaseCommands {
    public const string dbFileName = "Main.db";

    public static void ExecuteNonQuery (string commandText, params CommandArgsPair[] commandArgs) {
        Logger.LogInfo("1");
        Console.WriteLine("1");
        using (var connection = new SqliteConnection($"Data Source={dbFileName}"))
        {
            Logger.LogInfo("2");
            Console.WriteLine("2");
            connection.Open();
            Logger.LogInfo("3");
            Console.WriteLine("3");
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            foreach (CommandArgsPair argsPair in commandArgs) {
                command.Parameters.AddWithValue(argsPair.toReplace, argsPair.replaceWith);
            }
            Logger.LogInfo("4");
            Console.WriteLine("4");
            command.ExecuteNonQuery();
            Logger.LogInfo("5");
            Console.WriteLine("5");
            connection.Close();
            connection.Dispose();
            Logger.LogInfo("6");
            Console.WriteLine("6");
        }
        Logger.LogInfo("7");
        Console.WriteLine("7");
    }

    public static SqliteDataReader ExecuteQuery (string commandText, params CommandArgsPair[] commandArgs) {
        SqliteDataReader reader;

        using (var connection = new SqliteConnection($"Data Source={dbFileName}"))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            foreach (CommandArgsPair argsPair in commandArgs) {
                command.Parameters.AddWithValue(argsPair.toReplace, argsPair.replaceWith);
            }

            reader = command.ExecuteReader();

            connection.Close();
        }

        return reader;
    }
}