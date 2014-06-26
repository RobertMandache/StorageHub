using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace StorageHub.Utility
{
    public class Encryption
    {
        // 128bit(16byte)IV and Key        
        private const string AesKey = @"5TGB&YHN7UJM(IK<";

        /// <summary>
        /// AES Encryption
        /// </summary>
        public static string Encrypt(string clearText)
        {
            if (clearText == null) return null;
            // AesCryptoServiceProvider
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 128;            
            
            aes.GenerateIV();
            aes.Key = Encoding.UTF8.GetBytes(AesKey);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Convert string to byte array
            byte[] src = Encoding.Unicode.GetBytes(clearText);

            // encryption
            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

                // Convert byte array to Base64 strings                
                return Convert.ToBase64String(aes.IV) + Convert.ToBase64String(dest);
            }
        }

        /// <summary>
        /// AES decryption
        /// </summary>
        public  static string Decrypt(string cipherText)
        {
            if (cipherText == null) return null;
            // AesCryptoServiceProvider
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            string AesIV = cipherText.Substring(0, 24);
            aes.IV = Convert.FromBase64String(AesIV);
            aes.Key = Encoding.UTF8.GetBytes(AesKey);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Convert Base64 strings to byte array
            string actualCipherText = cipherText.Substring(24);
            byte[] src = System.Convert.FromBase64String(actualCipherText);

            // decryption
            using (ICryptoTransform decrypt = aes.CreateDecryptor())
            {
                byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                return Encoding.Unicode.GetString(dest);
            }
        }
    }
}