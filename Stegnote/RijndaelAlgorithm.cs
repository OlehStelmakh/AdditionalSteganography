using System;
using System.Security.Cryptography;
using System.IO;
namespace Stegnote
{
    public class RijndaelAlgorithm
    {

        private static byte[] key = new byte[32] {10, 100, 10, 100, 10, 100, 10, 100, 10, 100, 10, 100, 10, 100, 10, 100, 10, 100,
                    10, 100,10, 100,10, 100,10, 100,10, 100,10, 100,10, 100};
        private static byte[] IV = new byte[16] { 10, 100, 32, 122, 33, 41, 44, 65, 128, 42, 95, 100, 25, 10, 100, 10 };

        public static byte[] Encrypt(string textForEncrypt)
        {
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                //TODO SAVE KEY AND IV to file

                byte[] encrypted = EncryptStringToBytes(textForEncrypt, key, IV);
                return encrypted;
            }
        }

        public static string Decrypt(byte[] textForDecrypt)
        {
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                //TODO READ KEY AND IV
                string decrypted = DecryptStringFromBytes(textForDecrypt, key, IV);
                return decrypted;
            }
        }

        private static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {

            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
