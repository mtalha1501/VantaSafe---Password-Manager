using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Vanta_Safe.Services
{
    public static class DPAPIService
    {
        public static string KeyPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VantaSafe", "Keys", "db_key.secure"
        );
        private static readonly IDataProtector Protector;

        static DPAPIService()
        {
            var provider = DataProtectionProvider.Create("VantaSafe");
            Protector = provider.CreateProtector("SQLCipherKey");
        }

        public static void SaveEncryptedKey(byte[] key)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(KeyPath)!);
            byte[] encryptedKey = Protector.Protect(key);
            File.WriteAllBytes(KeyPath, encryptedKey);
        }

        public static byte[] LoadDecryptedKey()
        {
            byte[] encryptedKey = File.ReadAllBytes(KeyPath);
            return Protector.Unprotect(encryptedKey);
        }

        
    }
}