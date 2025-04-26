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
        // Password hashing configuration
        private const int HashWorkFactor = 12;
        private const HashType HashAlgorithm = HashType.SHA384;

        /// <summary>
        /// Registers a new user with master password
        /// </summary>
        public static bool RegisterUser(string username, string masterPassword, out string deviceSecret)
        {
            deviceSecret = GenerateDeviceSecret();

            try
            {
                string hashedMaster = EnhancedHashPassword(masterPassword, deviceSecret);

                using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                    INSERT INTO Users (Username, MasterKeyHash, DeviceSecret) 
                    VALUES (@username, @hash, @secret)";

                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@hash", hashedMaster);
                    command.Parameters.AddWithValue("@secret", deviceSecret);

                    return command.ExecuteNonQuery() == 1;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                deviceSecret = null;
                return false;
            }
        }

        public static bool VerifyUser(string username, string masterPassword, out string deviceSecret)
        {
            deviceSecret = null;

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                SELECT MasterKeyHash, DeviceSecret 
                FROM Users 
                WHERE Username = @username";

                command.Parameters.AddWithValue("@username", username);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string storedHash = reader.GetString(0);
                        deviceSecret = reader.GetString(1);
                        return EnhancedVerify(masterPassword, storedHash, deviceSecret);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Verifies master password and returns device secret if valid
        /// </summary>
        public static bool VerifyMasterPassword(string masterPassword, out string deviceSecret)
        {
            deviceSecret = null;

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MasterKeyHash, DeviceSecret FROM Secrets LIMIT 1";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string storedHash = reader.GetString(0);
                        deviceSecret = reader.GetString(1);

                        return EnhancedVerify(masterPassword, storedHash, deviceSecret);
                        // In VerifyMasterPassword(), add debug output:
                        MessageBox.Show($"Stored Hash: {storedHash} \n \"Device Secret: {deviceSecret}\"");
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Generates cryptographic key from master password + device secret
        /// </summary>
        public static byte[] DeriveMasterKey(string masterPassword, string deviceSecret)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(masterPassword),
                Encoding.UTF8.GetBytes(deviceSecret),
                100000,
                HashAlgorithmName.SHA384);

            return pbkdf2.GetBytes(32); // 256-bit key for AES
        }

        private static string GenerateDeviceSecret()
        {
            const string validChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // No confusing characters
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[16];
            rng.GetBytes(bytes);

            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(validChars[b % validChars.Length]);
            }
            return sb.ToString();
        }

        // Enhanced BCrypt hashing with combined secret
        private static string EnhancedHashPassword(string password, string secret)
        {
            // Step 1: Combine password + secret
            string combined = password + secret;

            // Step 2: Hash the combination with SHA384
            using var sha384 = SHA384.Create();
            byte[] combinedBytes = Encoding.UTF8.GetBytes(combined);
            byte[] shaHash = sha384.ComputeHash(combinedBytes);
            string base64Hash = Convert.ToBase64String(shaHash);

            // Step 3: Now hash the SHA384 result with BCrypt
            return BCrypt.Net.BCrypt.HashPassword(base64Hash, HashWorkFactor);
        }

        private static bool EnhancedVerify(string password, string hash, string secret)
        {
            // Step 1: Combine password + secret
            string combined = password + secret;

            // Step 2: Hash the combination with SHA384
            using var sha384 = SHA384.Create();
            byte[] combinedBytes = Encoding.UTF8.GetBytes(combined);
            byte[] shaHash = sha384.ComputeHash(combinedBytes);
            string base64Hash = Convert.ToBase64String(shaHash);

            // Step 3: Verify the BCrypt hash
            return BCrypt.Net.BCrypt.Verify(base64Hash, hash);
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
