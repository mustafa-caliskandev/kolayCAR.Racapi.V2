using Newtonsoft.Json;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace KolayCAR.Broker.Infrastructure.Extensions
{
    public static class ObjectExtensions
    {
        private static readonly Regex sWhitespace = new Regex(@"\s+");
        public static string ToStringNullSafe(this object value) => (value ?? string.Empty).ToString();

        public static string TrimNullSafe(this object value) => (value ?? string.Empty).ToString().Trim();

        public static bool IsNullOrEmpty(this object obj) => obj != null ? (!string.IsNullOrEmpty(obj.ToString().Trim()) ? false : true) : true;

        public static int ToIntNullSafe(this object obj)
        {
            if (obj == null)
                return 0;

            int.TryParse(obj.ToString().Trim(), out int result);
            return result;
        }

        public static int? ToIntNullAvailable(this object obj)
        {
            if (obj == null)
                return null;

            int.TryParse(obj.ToString().Trim(), out int result);
            return result;
        }

        public static long ToLongNullSafe(this object obj)
        {
            if (obj == null)
                return 0;

            long.TryParse(obj.ToString().Trim(), out long result);
            return result;
        }

        public static decimal ToDecimalNullSafe(this object obj)
        {
            if (obj == null)
                return 0;

            decimal.TryParse(obj.ToString().Trim().Replace(".", ","), out decimal result);
            return result;
        }

        public static decimal? ToDecimalNullAvailable(this object obj)
        {
            if (obj == null)
                return null;

            decimal.TryParse(obj.ToString().Trim().Replace(".", ","), out decimal result);
            return result;
        }

        public static float ToFloatNullSafe(this object obj)
        {
            if (obj == null)
                return 0;

            float.TryParse(obj.ToString().Trim().Replace(".", ","), out float result);
            return result;
        }

        public static float? ToFloatNullAvailable(this object obj)
        {
            if (obj == null)
                return null;

            float.TryParse(obj.ToString().Trim().Replace(".", ","), out float result);
            return result;
        }

        public static bool ToBoolNullSafe(this object obj)
        {
            if (obj == null)
                return false;

            bool.TryParse(obj.ToString().Trim(), out bool result);
            return result;
        }

        public static DateTime ToDateTimeNullSafe(this object obj)
        {
            if (obj == null)
                return default(DateTime);

            DateTime.TryParse(obj.ToString().Trim(), out DateTime result);
            return result;
        }
        public static DateTime NullSafeToDateTime(this object obj)
        {
            DateTime result;
            if (obj == null)
                return default(DateTime);
            DateTime.TryParse(obj.ToString().Trim(), out result);
            return result;
        }

        public static bool IsDateTime(this object obj)
        {
            DateTime result;
            return obj != null ? (DateTime.TryParse(obj.ToString().Trim(), out result)) : false;
        }

        public static string LongToHex(this long obj) => obj.ToLongNullSafe() != 0 ? obj.ToLongNullSafe().ToString("X") : "";

        public static long HexToLong(this string obj) => !obj.IsNullOrEmpty() ? long.Parse(obj, System.Globalization.NumberStyles.HexNumber) : 0;

        public static T ToEnum<T>(this string enumStr) => (T)Enum.Parse(typeof(T), enumStr);
        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public static string GetEightOrSixCharactersSafe(this string obj)
        {
            if (obj != null)
            {
                if (obj.Length > 7)
                {
                    return obj.Substring(0, 8);
                }
                else if (obj.Length > 6)
                {
                    return obj.Substring(0, 6);
                }
                else
                {
                    return obj;
                }
            }
            else
            {
                return obj;
            }
        }
        public static string MaskCreditCardNumber(this string str)
        {
            if (IsNullOrEmpty(str)) return str;
            return str.Length > 15 ? string.Format("{0}******{1}", str.Substring(0, 6), str.Substring(str.Length - 4)) : str;
        }
        public static string ReplaceWhitespace(this string input, string replacement)
        {
            if (!string.IsNullOrEmpty(replacement))
            {
                return sWhitespace.Replace(input, replacement);
            }

            return "";
        }

        public static string ToMaskEmail(this string str)
        {
            if (IsNullOrEmpty(str))
                return str;

            return Regex.Replace(str, @"(?<=[\w]{2})[\w\-._\+%]*(?=[\w]{0}@)", m => new string('*', m.Length));
        }
        public static string ToMaskPhoneNumber(this string str)
        {
            if (IsNullOrEmpty(str)) return str;

            return string.Format("{0}******{1}", str.Substring(0, 5), str.Substring(str.Length - 2));
        }
        public static string ToMaskPersonalNumber(this string str)
        {
            if (IsNullOrEmpty(str)) return str;
            return string.Format("{0}******{1}", str.Substring(0, 5), str.Substring(str.Length));
        }

        public static string ToJson(this object model) => JsonConvert.SerializeObject(model ?? string.Empty);
    }
}
