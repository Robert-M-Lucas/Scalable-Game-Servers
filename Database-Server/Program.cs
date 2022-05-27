namespace DatabaseServer;

using Microsoft.Data.Sqlite;
using System;

public static class Program {
    public static void Main(string[] args) {
        int id = 1;
        using (var connection = new SqliteConnection("Data Source=hello.db"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE ExampleTable (
                    fieldone INT,
                    fieldtwo STRING
                )
            ";
            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();

            // using (var reader = command.ExecuteReader())
            // {
            //     while (reader.Read())
            //     {
            //         var name = reader.GetString(0);

            //         Console.WriteLine($"Hello, {name}!");
            //     }
            // }
        }
    }
}
