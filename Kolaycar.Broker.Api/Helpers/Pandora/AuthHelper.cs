using System;
using System.Security.Cryptography;
using System.Text;

namespace KolayCAR.Broker.API.Helpers.Pandora
{
    public class AuthHelper
    {
        public static string GenerateSignature(string username, string password, string clientId, string salt, string secret)
        {
            string compositeKey = username + salt + secret + password + salt + secret + clientId;
            HashAlgorithm hashAlgorithm = new SHA512CryptoServiceProvider();
            byte[] byteValue = Encoding.UTF8.GetBytes(compositeKey);
            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);
            return Convert.ToBase64String(byteHash);
        }
    }
}
