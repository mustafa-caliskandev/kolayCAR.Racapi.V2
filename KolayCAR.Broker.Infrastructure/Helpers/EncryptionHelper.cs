using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KolayCAR.Broker.Infrastructure.Helpers
{
    public static class EncryptionHelper
    {
        private static readonly string password = ".web!758558";

        public static string Encrypt(string str)
        {
            try
            {
                byte[] encryptedByteArray = System.Text.Encoding.Unicode.GetBytes(str);
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                byte[] encryptedData = Encrypt(encryptedByteArray, pdb.GetBytes(32), pdb.GetBytes(16));
                return Convert.ToBase64String(encryptedData);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string Decrypt(string str)
        {
            try
            {
                byte[] encryptedByteArray = Convert.FromBase64String(str);
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                byte[] decryptedData = Decrypt(encryptedByteArray, pdb.GetBytes(32), pdb.GetBytes(16));
                return System.Text.Encoding.Unicode.GetString(decryptedData);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static byte[] Encrypt(byte[] unencryptedData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(unencryptedData, 0, unencryptedData.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        private static byte[] Decrypt(byte[] encryptedData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(encryptedData, 0, encryptedData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        public static string Hash(string text, EncryptionType type)
        {
            byte[] hashData;
            switch (type)
            {
                case EncryptionType.Md5:
                    MD5 md5 = MD5.Create();
                    hashData = md5.ComputeHash(Encoding.Default.GetBytes(text));
                    break;
                case EncryptionType.SHA1:
                    SHA1 sha1 = new SHA1CryptoServiceProvider();
                    hashData = sha1.ComputeHash(Encoding.Default.GetBytes(text));
                    break;
                default:
                    return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            foreach (var t in hashData)
                builder.Append(t.ToString("x2"));

            return builder.ToString();
        }

        public static bool Validate(string text, string hashedData, EncryptionType type)
        {
            return string.CompareOrdinal(Hash(text, type), hashedData) == 0;
        }

        public static string EncryptAES256(string plainText)
        {
            Aes encryptor = Aes.Create();
            encryptor.Mode = CipherMode.CBC;

            //encryptor.KeySize = 256;
            //encryptor.BlockSize = 128;
            //encryptor.Padding = PaddingMode.Zeros;
            // Set key and IV

            byte[] key =
            {
                0xD4, 0x4E, 0xDA, 0x90, 0xDD, 0xCF, 0xA9, 0x02,
                0x16, 0xBD, 0xAC, 0x55, 0x9D, 0x70, 0x4D, 0x33,
                0x6D, 0x37, 0x1A, 0x3D, 0xA9, 0x43, 0xE9, 0x35,
                0x31, 0x59, 0x64, 0xD2, 0x7C, 0xC9, 0x1D, 0x0C
            };

            byte[] iv =
            {
                0x7B, 0x13, 0xE1, 0xA1, 0x78, 0x61, 0x35, 0x64,
                0x01, 0xA3, 0xC1, 0x5F, 0x4F, 0x05, 0x25, 0xC3
            };

            encryptor.Key = key;
            encryptor.IV = iv;
            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();

            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();
            string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);
            return cipherText;
        }

        public static string DecryptAES256(string cipherText)
        {
            Aes encryptor = Aes.Create();
            encryptor.Mode = CipherMode.CBC;
            //encryptor.KeySize = 256;
            //encryptor.BlockSize = 128;
            //encryptor.Padding = PaddingMode.Zeros;
            // Set key and IV

            byte[] key =
            {
                0xD4, 0x4E, 0xDA, 0x90, 0xDD, 0xCF, 0xA9, 0x02,
                0x16, 0xBD, 0xAC, 0x55, 0x9D, 0x70, 0x4D, 0x33,
                0x6D, 0x37, 0x1A, 0x3D, 0xA9, 0x43, 0xE9, 0x35,
                0x31, 0x59, 0x64, 0xD2, 0x7C, 0xC9, 0x1D, 0x0C
            };

            byte[] iv =
            {
                0x7B, 0x13, 0xE1, 0xA1, 0x78, 0x61, 0x35, 0x64,
                0x01, 0xA3, 0xC1, 0x5F, 0x4F, 0x05, 0x25, 0xC3
            };

            encryptor.Key = key;
            encryptor.IV = iv;

            MemoryStream memoryStream = new MemoryStream();
            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();

            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);

            string plainText = string.Empty;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] plainBytes = memoryStream.ToArray();
                plainText = Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }
            return plainText;
        }
    }

    public enum EncryptionType
    {
        Md5,
        SHA1
    }
}
