using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Vanta_Safe.Services
{
    public static class EncryptDecryptService
    {
        public static string DecryptField(byte[] encryptedData, byte[] masterKey)
        {
            if (encryptedData == null || encryptedData.Length < 16)
                throw new ArgumentException("Invalid encrypted data");

            using (Aes aes = Aes.Create())
            {
                // Extract IV (first 16 bytes)
                byte[] iv = new byte[16];
                Array.Copy(encryptedData, 0, iv, 0, iv.Length);

                // Extract ciphertext (remaining bytes)
                byte[] ciphertext = new byte[encryptedData.Length - iv.Length];
                Array.Copy(encryptedData, iv.Length, ciphertext, 0, ciphertext.Length);

                // Decrypt
                aes.Key = masterKey;
                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream ms = new MemoryStream(ciphertext))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        // Decrypt with memory protection
        public static string DecryptSafely(byte[] encrypted, byte[] key)
        {
            byte[] plaintext = null;
            try
            {
                plaintext = Encoding.UTF8.GetBytes(DecryptField(encrypted, key));
                return Encoding.UTF8.GetString(plaintext);
            }
            finally
            {
                if (plaintext != null)
                    CryptographicOperations.ZeroMemory(plaintext);
            }
        }

        public static byte[] EncryptField(string plaintext, byte[] masterKey)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = masterKey;
                aes.GenerateIV(); // Unique IV per encryption

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cs))
                    {
                        writer.Write(plaintext);
                    }
                    return ms.ToArray(); // Returns IV + ciphertext
                }
            }
        }
    }
}
