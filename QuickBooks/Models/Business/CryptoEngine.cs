using System;
using System.IO;
using System.Security.Cryptography;

namespace QuickBooks.Models.Business
{
    public class CryptoEngine : ICryptoEngine
    {
        private readonly string _securityKey;

        public CryptoEngine(string securityKey)
        {
            _securityKey = securityKey;
        }

        public string Decrypt(string encryptText)
        {
            var tripleDes = new TripleDESCryptoServiceProvider();
            tripleDes.Key = TruncateHash(_securityKey, tripleDes.KeySize / 8);
            tripleDes.IV = TruncateHash(string.Empty, tripleDes.BlockSize / 8);
            // Convert the encrypted text string to a byte array.
            var encryptedBytes = Convert.FromBase64String(encryptText);
            // Create the stream.
            var ms = new MemoryStream();
            // Create the decoder to write to the stream.
            var decStream = new CryptoStream(ms, tripleDes.CreateDecryptor(), CryptoStreamMode.Write);
            // Use the crypto stream to write the byte array to the stream.
            decStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            decStream.FlushFinalBlock();
            // Convert the plaintext stream to a string.
            return System.Text.Encoding.Unicode.GetString(ms.ToArray());
        }
        
        private static byte[] TruncateHash(string key, int length)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            // Hash the key.
            byte[] keyBytes = System.Text.Encoding.Unicode.GetBytes(key);
            byte[] hash = sha1.ComputeHash(keyBytes);
            // Truncate or pad the hash.
            Array.Resize(ref hash, length);
            return hash;
        }
     
        public string Encrypt(string plainText)
        {
            var tripleDes = new TripleDESCryptoServiceProvider();
            tripleDes.Key = TruncateHash(_securityKey, tripleDes.KeySize / 8);
            tripleDes.IV = TruncateHash(string.Empty, tripleDes.BlockSize / 8);
            // Convert the plaintext string to a byte array.
            var plaintextBytes = System.Text.Encoding.Unicode.GetBytes(plainText);
            // Create the stream.
            var ms = new MemoryStream();
            // Create the encoder to write to the stream.
            var encStream = new CryptoStream(ms, tripleDes.CreateEncryptor(), CryptoStreamMode.Write);
            // Use the crypto stream to write the byte array to the stream.
            encStream.Write(plaintextBytes, 0, plaintextBytes.Length);
            encStream.FlushFinalBlock();
            // Convert the encrypted stream to a printable string.
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
