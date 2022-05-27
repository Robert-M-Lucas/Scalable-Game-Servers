namespace DatabaseServer;

using Shared;

using Microsoft.Data.Sqlite;

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
        using (var connection = new SqliteConnection($"Data Source={dbFileName}"))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();
            command.CommandText = commandText;

            foreach (CommandArgsPair argsPair in commandArgs) {
                command.Parameters.AddWithValue(argsPair.toReplace, argsPair.replaceWith);
            }

            command.ExecuteNonQuery();
            connection.Close();
            connection.Dispose();
        }
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