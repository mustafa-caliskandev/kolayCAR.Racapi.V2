using Microsoft.AspNetCore.Html;
using System;
using System.Linq;
using System.Text;

namespace kolayCAR.Broker.AWS.Extensions
{
    internal static class StringExtensions
    {
        public static string ReplaceIfExists(this string text, string from, string to)
        {
            if (text.Contains(to))
            {
                return text.Replace(from, to);
            }
            return text;
        }

        public static string Crop(this string text, int lenght = 0)
        {
            var croppedText = lenght != 0 ?
                (text.Length > lenght ? text.Substring(0, lenght) : text) :
                (text);
            return croppedText;
        }

        public static string CropUntilLastSpace(this string text, int lenght = 0)
        {
            var croppedText = lenght != 0 ?
                (text.Length > lenght ? text.Substring(0, lenght) : text) :
                (text);

            var splittedText = croppedText.Split(" ");
            return string.Join(" ", splittedText.Take(splittedText.Count() - 1).ToArray());
        }

        public static string ToFirstLetterCapital(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            text = text.Trim();

            switch (text.Length)
            {
                case 0:
                    return "";
                case 1:
                    return char.ToUpper(text[0]).ToString();
                default:
                    {
                        var parts = text.Split(" ").Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        for (int i = 0; i < parts.Length; i++)
                        {
                            parts[i] = $"{char.ToUpper(parts[i][0]) + parts[i][1..].ToLower()}";
                        }
                        return string.Join(" ", parts);
                    }
            }
        }

        public static HtmlString ReplaceWith(this HtmlString text, string oldValue, string newValue)
        {
            var str = text.ToString();
            if (!str.Contains(oldValue))
                return text;
            else
            {
                str = str.Replace(oldValue, newValue);
                return new HtmlString(str);
            }
        }

        public static string ReplaceWith(this object text, string oldValue, string newValue)
        {
            var str = text.ToString();
            if (!str.Contains(oldValue))
                return str;
            else
            {
                return str.Replace(oldValue, newValue);
            }
        }

        public static int ToInt(this string text, int defaultValue = 0)
        {
            return int.TryParse(text, out var value) ? value : defaultValue;
        }

        public static decimal ToDecimal(this string text, decimal defaultValue = 0)
        {
            return decimal.TryParse(text, out var value) ? value : defaultValue;
        }

        public static float ToFloat(this string text, float defaultValue = 0)
        {
            return float.TryParse(text, out var value) ? value : defaultValue;
        }

        public static bool ToBool(this string text)
        {
            if (!bool.TryParse(text, out var value))
            {
                value = false;
            }
            return value;
        }

        public static string ToSecureText(this string text)
        {
            return $"{text[0]}****";
        }

        public static string ToEnglishLetters(this string text)
        {
            text = text.Replace("ü", "u");
            text = text.Replace("ı", "i");
            text = text.Replace("ö", "o");
            text = text.Replace("ü", "u");
            text = text.Replace("ş", "s");
            text = text.Replace("ğ", "g");
            text = text.Replace("ç", "c");
            text = text.Replace("Ü", "U");
            text = text.Replace("İ", "I");
            text = text.Replace("Ö", "O");
            text = text.Replace("Ü", "U");
            text = text.Replace("Ş", "S");
            text = text.Replace("Ğ", "G");
            text = text.Replace("Ç", "C");

            return text;
        }

        public static string ToUtf8(this string unicodeText)
        {
            var bytes = Encoding.UTF8.GetBytes(unicodeText);
            return new string(bytes.Select(b => (char)b).ToArray());
        }

        public static string Mask(this string text, int characterCount = 3, bool firstOpen = true)
        {
            if (string.IsNullOrEmpty(text) || text.Length < characterCount)
            {
                return "*****";
            }

            return firstOpen ?
                $"{text[..characterCount]}****" :
                $"****{text[^characterCount..]}";
        }

        public static string EmailMask(this string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    return "*****";
                }
                else if (text.Length < 4)
                {
                    return "*****";
                }

                var mailParts = text.Split('@');
                if (mailParts.Length < 2)
                {
                    return "*****";
                }
                return $"{mailParts[0][..3]}****@{mailParts[1]}";
            }
            catch (System.Exception)
            {
                return text;
            }
        }

        public static string PhoneNumberMask(this string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 6)
            {
                return "*****";
            }

            return $"{text[..3]}*****{text[^3..]}";
        }

        public static string CreditCardNumberMask(this string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 6)
            {
                return "*****";
            }

            return $"{text[..4]}*****{text[^4..]}";
        }

        public static string ToFormatPrice(this string value)
        {
            return string.Format("{0:#,##0,00}", value);
        }

        public static string ToStringNullSafe(this object value)
        {
            try
            {
                return (value ?? string.Empty).ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Unique bir string değer döner
        /// </summary>
        /// <param name="length">Verilen değer uzunluğunda data döner 0 verilirse tam uzunluk döner</param>
        /// <returns></returns>
        public static string CreateUniqueCode(this int length)
        {
            var g = Guid.NewGuid();
            var guidString = Convert.ToBase64String(g.ToByteArray());
            guidString = guidString.Replace("=", "");
            guidString = guidString.Replace("+", "");
            return length == 0
                ? guidString
                : (guidString.Length > length
                        ? guidString[..length]
                        : guidString);
        }
    }
}
