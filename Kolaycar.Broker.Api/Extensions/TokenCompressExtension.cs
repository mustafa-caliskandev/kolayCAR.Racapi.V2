using System.Text;
using System;
using System.IO;
using System.IO.Compression;

namespace KolayCAR.Broker.API.Extensions
{
    public static class TokenCompressExtension
    {
        public static string Encode(string input)
        {
            var bytesToEncode = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytesToEncode).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public static string Decode(string encoded)
        {
            string s = encoded.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            var decodedBytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(decodedBytes);
        }

        public static string Compress(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    gzip.Write(tokenBytes, 0, tokenBytes.Length);
                }
                return Convert.ToBase64String(output.ToArray());
            }
        }

        public static string Decompress(string compressedToken)
        {
            var compressedBytes = Convert.FromBase64String(compressedToken);
            using (var input = new MemoryStream(compressedBytes))
            {
                using (var gzip = new GZipStream(input, CompressionMode.Decompress))
                {
                    using (var output = new MemoryStream())
                    {
                        gzip.CopyTo(output);
                        return Encoding.UTF8.GetString(output.ToArray());
                    }
                }
            }
        }
    }
}
