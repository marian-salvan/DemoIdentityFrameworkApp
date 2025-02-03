using System.Security.Cryptography;
using System.Text;

namespace DemoBackend.Helpers
{
    public static class EncryptionHelper
    {
        private static readonly string Bas64EncryptionKey = "I3PKTwfP1UkX4BOOBnifO/Ye6YEa7E0tDkp4QSQpRD4="; // Use a secure key
        private static readonly string Base64EncryptionIV = "VU/3R0D9fZtvKf9zr6mNZw=="; // Use a secure key

        public static string Encrypt(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using Aes aes = Aes.Create();
            aes.Key = Convert.FromBase64String(Bas64EncryptionKey);
            aes.IV = Convert.FromBase64String(Base64EncryptionIV);

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.Close();
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            using Aes aes = Aes.Create();
            aes.Key = Convert.FromBase64String(Bas64EncryptionKey);
            aes.IV = Convert.FromBase64String(Base64EncryptionIV);

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                cs.Close();
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
