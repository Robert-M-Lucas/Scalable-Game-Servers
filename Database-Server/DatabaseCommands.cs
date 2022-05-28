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

public class ConnectionReader {
    public SqliteConnection connection;
    public SqliteDataReader reader;

    public ConnectionReader (SqliteConnection _connection, SqliteDataReader _reader) {
        connection = _connection;
        reader = _reader;
    }

    public void Close() {
        reader.Close();
        connection.Close();
        connection.Dispose();
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

    public static ConnectionReader ExecuteQuery (string commandText, params CommandArgsPair[] commandArgs) {
        SqliteDataReader reader;

        SqliteConnection connection = new SqliteConnection($"Data Source={dbFileName}");

        connection.Open();

        SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;

        foreach (CommandArgsPair argsPair in commandArgs) {
            command.Parameters.AddWithValue(argsPair.toReplace, argsPair.replaceWith);
        }

        reader = command.ExecuteReader();

        return new ConnectionReader(connection, reader);
    }

    public static string GetValueFromDictionary (string key) {
        ConnectionReader conn_reader = ExecuteQuery(@"SELECT Value FROM Dictionary WHERE Key = $key",
            new CommandArgsPair("$key", key));
        
        conn_reader.reader.Read();
        string val = conn_reader.reader.GetString(0);
        conn_reader.Close();
        return val;
    }

    public static void UpdateValueFromDictionary (string key, string new_val) {
        ExecuteNonQuery(@"UPDATE Dictionary SET Value = $value WHERE Key = $key",
            new CommandArgsPair("$key", key), new CommandArgsPair("$value", new_val));
    }

    public static void CreateTables() {
        DatabaseCommands.ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS Players (
                PlayerID INT PRIMARY KEY,
                PlayerName STRING,
                PlayerPassword STRING,
                CurrencyAmount INT
            )
        ");

        DatabaseCommands.ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS Dictionary (
                Key STRING PRIMARY KEY,
                Value STRING
            )
        ");

        DatabaseCommands.ExecuteNonQuery(@"
        INSERT OR IGNORE INTO Dictionary VALUES ('PlayerIDCounter', '1')
        ");

    }

    public static DatabasePlayer GetOrAddPlayer(string username, string password) {
        ConnectionReader conn_reader = DatabaseCommands.ExecuteQuery(@"
        SELECT PlayerID, PlayerPassword, CurrencyAmount FROM Players WHERE PlayerName = $playerName
        ", new CommandArgsPair("$playerName", username));

        DatabasePlayer? to_return;

        if (conn_reader.reader.HasRows) { // Player exists
            conn_reader.reader.Read();
            if (password != conn_reader.reader.GetString(1)) {
                Logger.LogInfo($"Player {username} tried to log in with password [{password}] when actual password is [{conn_reader.reader.GetString(1)}]");
                to_return = new DatabasePlayer("", "", -1, 0);
            }
            else {
                int id = conn_reader.reader.GetInt32(0);
                int currency_amount = conn_reader.reader.GetInt32(2);
                to_return = new DatabasePlayer(username, password, id, currency_amount);
                Logger.LogInfo($"Player {to_return} retrieved from database");
            }
        }
        else { // Player doesn't exist
            int id = int.Parse(GetValueFromDictionary("PlayerIDCounter"));
            UpdateValueFromDictionary("PlayerIDCounter", (id+1).ToString());
            DatabaseCommands.ExecuteNonQuery(@"
            INSERT INTO Players VALUES ($id, $username, $password, 0)
            ", new CommandArgsPair("$id", id),
            new CommandArgsPair("$username", username),
            new CommandArgsPair("$password", password));

            to_return = new DatabasePlayer(username, password, id, 0);

            Logger.LogImportant($"Player {to_return} created");
        }

        conn_reader.Close();
        return to_return;
    }
}