using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanta_Safe.Services
{
    public static class DatabaseService
    {
        // Permanent path in your project's DB folder
        private static readonly string _dbPath = Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName,
            "DB",
            "Vault.db");

        public static string ConnectionString => $"Data Source={_dbPath}";

        public static void Initialize()
        {
            // Create DB folder if missing
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);

            // Create database file if missing
            if (!File.Exists(_dbPath))
            {
                File.Create(_dbPath).Close(); // Creates empty file

                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                // Create tables
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
    CREATE TABLE IF NOT EXISTS Users (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Username TEXT NOT NULL UNIQUE,
        MasterKeyHash TEXT NOT NULL,
        DeviceSecret TEXT NOT NULL
    );
    
    CREATE TABLE IF NOT EXISTS Entries (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        UserId INTEGER NOT NULL,
        SiteName TEXT NOT NULL,
        EncryptedPassword TEXT NOT NULL,
        FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
    )";
                cmd.ExecuteNonQuery();
            }
        }
    }

}
