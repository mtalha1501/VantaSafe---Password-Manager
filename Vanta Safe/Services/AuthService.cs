using BCrypt.Net;
using Microsoft.Data.Sqlite;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;



namespace Vanta_Safe.Services
{

    // In AuthService.cs
    public static class AuthService
    {
        private const int HashWorkFactor = 12;
        private const int Pbkdf2Iterations = 100_000;  // For AES key derivation
        private const int AesKeySize = 256;            // AES-256


        public static bool RegisterUser(string username, string masterPassword, out string deviceSecret)
        {
            deviceSecret = GenerateDeviceSecret();
            string hashedMaster = BCrypt.Net.BCrypt.EnhancedHashPassword(
                masterPassword + deviceSecret,
                BCrypt.Net.HashType.SHA384,
                HashWorkFactor);

            try
            {
                using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                        INSERT INTO Users (Username, MasterKeyHash, DeviceSecret) 
                        VALUES (@username, @hash, @secret)";

                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@hash", hashedMaster);
                    cmd.Parameters.AddWithValue("@secret", deviceSecret);

                    return cmd.ExecuteNonQuery() == 1;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                deviceSecret = null;
                return false; // Username exists
            }
        }
        public static byte[] DeriveMasterKey(string masterPassword, string deviceSecret) // AES Key hai ye
        {
            byte[] salt = Encoding.UTF8.GetBytes(deviceSecret); // DeviceSecret as salt
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password: masterPassword + deviceSecret,
                salt: salt,
                iterations: Pbkdf2Iterations,
                hashAlgorithm: HashAlgorithmName.SHA384
            );
            return pbkdf2.GetBytes(AesKeySize / 8); // 32 bytes for AES-256
        }

        public static bool VerifyUser(string username, string masterPassword, out string deviceSecret)
        {
            deviceSecret = null;

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT MasterKeyHash, DeviceSecret FROM Users WHERE Username = @username";
                cmd.Parameters.AddWithValue("@username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string storedHash = reader.GetString(0);
                        deviceSecret = reader.GetString(1);
                        return BCrypt.Net.BCrypt.EnhancedVerify(
                            masterPassword + deviceSecret,
                            storedHash,
                            BCrypt.Net.HashType.SHA384);
                    }
                }
            }
            return false;
        }

        // Add these methods to AuthService.cs
        private static string HashMasterPassword(string password, string secret)
            => BCrypt.Net.BCrypt.EnhancedHashPassword(password + secret, BCrypt.Net.HashType.SHA384);

        public static bool VerifyMasterPassword(string password, string username, out string deviceSecret)
        {
            deviceSecret = null;

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT MasterKeyHash, DeviceSecret FROM Users WHERE Username = @username";
                cmd.Parameters.AddWithValue("@username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string storedHash = reader.GetString(0);
                        deviceSecret = reader.GetString(1);
                        return BCrypt.Net.BCrypt.EnhancedVerify(
                            password + deviceSecret,
                            storedHash,
                            BCrypt.Net.HashType.SHA384);
                    }
                }
            }
            return false;
        }

        private static string GenerateDeviceSecret()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[10];
            rng.GetBytes(bytes);

            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }
        // In AuthService.cs
        public static byte[] GenerateHash(string plaintext, byte[] masterKey)
        {
            using (var hmac = new HMACSHA256(masterKey)) // Keyed hash
            {
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                byte[] hash = hmac.ComputeHash(plaintextBytes);

                // Securely wipe plaintext bytes
                Array.Clear(plaintextBytes, 0, plaintextBytes.Length);
                return hash;
            }
        }
    }


    public static class PasswordValidator
    {
        public static (bool IsValid, string ErrorMessage) Validate(string password)
        {
            if (password.Length < 12)
                return (false, "Password must be at least 12 characters");

            if (!Regex.IsMatch(password, @"[0-9]"))
                return (false, "Password must contain at least one number");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return (false, "Password must contain at least one capital letter");

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
                return (false, "Password must contain at least one special character");

            return (true, string.Empty);
        }
    }
}
