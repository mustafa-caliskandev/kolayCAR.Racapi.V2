using System;
using System.Text;

namespace KolayCAR.Broker.Infrastructure.Helpers
{
    public class EncodingHelper
    {
        public static string Base64Encode(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    }
}
