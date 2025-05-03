using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Microsoft.Data.Sqlite;
using Vanta_Safe.Models;



namespace Vanta_Safe.Services
{
    public static class DatabaseService
    {
        private static string _dbPath = Path.Combine("DB", "Vault.db");
        private static byte[] _dbKey; // DPAPI-managed key

        public static string ConnectionString => $"Data Source={_dbPath};";

        public static void Initialize()
        {
            try
            {
                // 1. Initialize SQLCipher
                Directory.CreateDirectory("DB");

                if (!File.Exists(DPAPIService.KeyPath))
                {
                    _dbKey = new byte[32];
                    RandomNumberGenerator.Fill(_dbKey);
                    DPAPIService.SaveEncryptedKey(_dbKey);
                }
                else
                {
                    _dbKey = DPAPIService.LoadDecryptedKey();
                }

                // DB initialization
                using (var conn = new SqliteConnection($"Data Source={_dbPath}"))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = $"PRAGMA key = '{Convert.ToHexString(_dbKey)}';";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"
    PRAGMA foreign_keys = ON;
    CREATE TABLE IF NOT EXISTS Users (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Username TEXT NOT NULL UNIQUE,
        MasterKeyHash TEXT NOT NULL,
        DeviceSecret TEXT NOT NULL,
        FailedAttempts INTEGER DEFAULT 0,
        EncryptedMasterKey BLOB DEFAULT NULL
    );
    CREATE TABLE IF NOT EXISTS Credentials (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        UserId INTEGER NOT NULL,
        EncryptedSiteName BLOB NOT NULL,
        EncryptedSiteUrl BLOB NOT NULL,
        EncryptedUsername BLOB NOT NULL,
        EncryptedPassword BLOB NOT NULL,
        UsernameHash BLOB NOT NULL DEFAULT X'00',
        SiteUrlHash BLOB NOT NULL DEFAULT X'00',
        FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    ";

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization failed: {ex.Message}");
                Application.Current.Shutdown();
            }
        }
    }
}