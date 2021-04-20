using Infrastructure;
using Microsoft.Data.Sqlite;
using System;

namespace DBImport
{
    public class Z21_New_Import
    {


        public Z21_New_Import(Database db, string path)
        {
            if (db is null || string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Arguments cant be null!");

            using (var connection = new SqliteConnection($"Data Source={path}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @" SELECT name FROM user WHERE id = $id    ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(0);

                        Console.WriteLine($"Hello, {name}!");
                    }
                }
            }

        }
    }
}
