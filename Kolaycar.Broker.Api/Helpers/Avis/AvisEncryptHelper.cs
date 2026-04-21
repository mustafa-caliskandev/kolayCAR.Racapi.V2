using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace KolayCAR.Broker.API.Helpers.Avis
{
    public class AvisEncryptHelper
    {
        //private static string secretKey  = "gGPs,:+Ka6AUwPE+yM[Gpf=aW&{[U*";
        //private static string secretKey  = "*mV80ZOjU]hm!1XC0Ih^Th_Mc1tQdS";

        public static bool IsAuthenticated(string message, string signature, string secretKey)
        {
            if (string.IsNullOrEmpty(secretKey))
                return false;

            var verifiedHash = HashHMACHex(message, secretKey);
            if (signature != null && signature.Equals(verifiedHash))
                return true;

            return false;
        }

        public static string HashHMACHex(string message, string secretKey)
        {
            byte[] hash = HashHMAC(Encoding.UTF8.GetBytes(secretKey),
            StringEncode(message));
            return HashEncode(hash);
        }
        private static byte[] HashHMAC(byte[] key, byte[] message)
        {
            var hash = new HMACSHA256(key);
            return hash.ComputeHash(message);
        }
        private static byte[] StringEncode(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
        private static string HashEncode(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        private bool TryParse(string input_date, string formatCulture, out DateTime output_date)
        {
            return DateTime.TryParse(input_date, new CultureInfo(formatCulture), DateTimeStyles.None, out output_date);
        }

        internal DateTime ParseValidDateTime(string input_date)
        {
            string _format = "yyyy-MM-ddTHH:mm:ss"; //Z 
            string outt = "";
            if (TryParse(input_date, "tr-TR", out DateTime output_date))
            {
                outt = output_date.ToString(_format);
            }
            return Convert.ToDateTime(outt);
        }
    }
}
