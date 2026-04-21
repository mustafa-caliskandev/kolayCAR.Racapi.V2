using System.Globalization;

namespace KolayCAR.Broker.API.Extensions
{
    public static class CurrencyExtensions
    {
        /// <summary>
        /// Aldığı float nesnesini 0,## formatında döner.
        /// </summary>
        /// <param name="model">object tipinde herhangi bir değer</param>
        /// <returns>Json String</returns>
        public static string ToCurrencyString(this float amount, string currencySymbol, bool symbolAfterAmount = true, int scale = 2)
        {
            return symbolAfterAmount
                ? $"{amount.ToString($"N{scale}", new CultureInfo("tr-TR"))} {currencySymbol}"
                : $"{currencySymbol} {amount.ToString($"N{scale}", new CultureInfo("tr-TR"))}";
        }
        public static string ToCurrencyString(this float amount, int scale = 2)
        {
            return $"{amount.ToString($"N{scale}", new CultureInfo("tr-TR"))}";
        }

        /// <summary>
        /// Aldığı float? nesnesini 0,## formatında döner.
        /// </summary>
        /// <param name="model">object tipinde herhangi bir değer</param>
        /// <returns>Json String</returns>
        public static string ToCurrencyString(this float? amount, string currencySymbol, bool symbolAfterAmount = true, int scale = 2)
        {
            if (float.TryParse(amount.ToString(), out var curNotNull))
            {
                return symbolAfterAmount
                    ? $"{curNotNull.ToString($"N{scale}")} {currencySymbol}"
                    : $"{currencySymbol} {curNotNull.ToString($"N{scale}")}";
            }
            return symbolAfterAmount
                ? $"{0.ToString($"N{scale}")} {currencySymbol}"
                : $"{currencySymbol} {0.ToString($"N{scale}")}"; ;
        }

        public static string ToCurrencyString(this float? amount, int scale = 2)
        {
            if (float.TryParse(amount.ToString(), out var curNotNull))
            {
                return $"{curNotNull.ToString($"N{scale}")}";
            }

            return $"{0.ToString($"N{scale}")}";
        }

        /// <summary>
        /// Aldığı decimal nesnesini 0,## formatında döner.
        /// </summary>
        /// <param name="model">object tipinde herhangi bir değer</param>
        /// <returns>Json String</returns>
        public static string ToCurrencyString(this decimal amount, string currencySymbol, bool symbolAfterAmount = true, int scale = 2)
        {
            return symbolAfterAmount
                ? $"{amount.ToString($"N{scale}", new CultureInfo("tr-TR"))} {currencySymbol}"
                : $"{currencySymbol} {amount.ToString($"N{scale}", new CultureInfo("tr-TR"))}";
        }

        public static string ToCurrencyString(this decimal amount, int scale = 2)
        {
            return amount.ToString($"N{scale}", new CultureInfo("tr-TR"));
        }

        /// <summary>
        /// Aldığı decimal? nesnesini 0,## formatında döner.
        /// </summary>
        /// <param name="model">object tipinde herhangi bir değer</param>
        /// <returns>Json String</returns>
        public static string ToCurrencyString(this decimal? amount, string currencySymbol, bool symbolAfterAmount = true, int scale = 2)
        {
            if (decimal.TryParse(amount.ToString(), out var decNotNull))
            {
                return symbolAfterAmount
                    ? $"{decNotNull.ToString($"N{scale}", new CultureInfo("tr-TR"))} {currencySymbol}"
                    : $"{currencySymbol} {decNotNull.ToString($"N{scale}", new CultureInfo("tr-TR"))}";
            }
            return symbolAfterAmount
                ? $"{0.ToString($"N{scale}", new CultureInfo("tr-TR"))} {currencySymbol}"
                : $"{currencySymbol} {0.ToString($"N{scale}", new CultureInfo("tr-TR"))}";
        }

        public static string ToCurrencyString(this decimal? amount, int scale = 2)
        {
            if (decimal.TryParse(amount.ToString(), out var decNotNull))
            {
                return $"{decNotNull.ToString($"N{scale}", new CultureInfo("tr-TR"))}";
            }
            return $"{0.ToString($"N{scale}", new CultureInfo("tr-TR"))}";
        }

        /// <summary>
        /// Aldığı decimal nesnesini Tam sayı formatında döner.
        /// </summary>
        /// <param name="model">object tipinde herhangi bir değer</param>
        /// <returns>Json String</returns>
        public static string ToCurrencyIntString(this decimal amount, string currencySymbol, bool symbolAfterAmount = true)
        {
            return symbolAfterAmount
                ? $"{amount:N0} {currencySymbol}"
                : $"{currencySymbol} {amount:N0}";
        }

        public static string ToCurrencyIntString(this decimal amount)
        {
            return $"{amount:N0}";
        }

        /// <summary>
        /// Aldığı decimal? nesnesini Tam Sayı formatında döner.
        /// </summary>
        /// <param name="currency">decimal? tipinde bir değer</param>
        /// <returns>String</returns>
        public static string ToCurrencyIntString(this decimal? amount, string currencySymbol, bool symbolAfterAmount = true)
        {
            if (decimal.TryParse(amount.ToString(), out var intNotNull))
            {
                return symbolAfterAmount
                    ? $"{intNotNull:N0} {currencySymbol}"
                    : $"{currencySymbol} {intNotNull:N0}";
            }
            return symbolAfterAmount
                ? $"{0:N0} {currencySymbol}"
                : $"{currencySymbol} {0:N0}";
        }

        public static string ToCurrencyString(this decimal? amount, string currencySymbol, bool symbolAfterAmount = true, int scale = 2, CultureInfo cultureInfo = null)
        {
            cultureInfo ??= CultureInfo.CurrentCulture;

            if (amount.HasValue)
            {
                string formattedAmount = amount.Value.ToString("N" + scale, cultureInfo);

                return symbolAfterAmount
                    ? $"{formattedAmount} {currencySymbol}"
                    : $"{currencySymbol} {formattedAmount}";
            }
            else
            {
                string zeroAmount = 0.ToString("N" + scale, cultureInfo);

                return symbolAfterAmount
                    ? $"{zeroAmount} {currencySymbol}"
                    : $"{currencySymbol} {zeroAmount}";
            }
        }
    }
}
