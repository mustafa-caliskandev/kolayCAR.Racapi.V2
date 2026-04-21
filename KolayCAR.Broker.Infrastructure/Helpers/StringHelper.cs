using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace KolayCAR.Broker.Infrastructure.Helpers
{
    public class StringHelper
    {
        public static string LastCharacterClear(string text, string lastCharacter)
        {
            string value = "";
            if (!string.IsNullOrEmpty(text))
            {
                if (text.Contains(lastCharacter))
                {
                    value = text.Substring(0, text.LastIndexOf(lastCharacter));
                }
            }
            return value;
        }

        public static string GetTwoCharacterDateItem(string item) => !string.IsNullOrEmpty(item) ? item.Length == 1 ? $"0{item}" : item : string.Empty;

        public static string GetTwoCharacterDateItem(int item) => !string.IsNullOrEmpty(item.ToStringNullSafe()) ? item.ToStringNullSafe().Length == 1 ? $"0{item.ToStringNullSafe()}" : item.ToStringNullSafe() : string.Empty;

        public static PostReservationRequest MaskCreditCard(PostReservationRequest postReservationRequest)
        {
            if (postReservationRequest != null)
            {
                if (!string.IsNullOrEmpty(postReservationRequest.CreditCardNumber?.Trim()))
                {
                    var reg = new Regex(@"(?<=\d{4}\d{2})\d{2}\d{4}(?=\d{4})|(?<=\d{4}( |-)\d{2})\d{2}\1\d{4}(?=\1\d{4})");
                    postReservationRequest.CreditCardNumber = reg.Replace(postReservationRequest.CreditCardNumber, new MatchEvaluator((m) => new String('*', m.Length)));
                }

                if (postReservationRequest.ExpiredYear != null)
                    postReservationRequest.ExpiredYear = 0;

                if (postReservationRequest.ExpiredMonth != null)
                    postReservationRequest.ExpiredMonth = 0;

                if (!string.IsNullOrEmpty(postReservationRequest.SecurityCode?.Trim()))
                    postReservationRequest.SecurityCode = "***";
            }

            return postReservationRequest;
        }

        public static bool IsOnlyIntCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            bool result = true;

            foreach (var x in text.ToList())
            {
                if (!int.TryParse(x.ToString(), out int value))
                    result = false;

                if (!result)
                    return false;
            }

            return true;
        }

        public static string SpecialProfitMarkup (float convertedProfitMarkup , ProfitMarkup profitMarkup, CurrencyTypes requestCurrencyType)
        {
            return "Markup = " + (profitMarkup.Type == 1 ? "+" : "-") + convertedProfitMarkup
                        + (profitMarkup.MarkupType == 1
                            ? convertedProfitMarkup != profitMarkup.MarkupValue
                                ? " " + (CurrencyTypes)(requestCurrencyType) + $" ({profitMarkup.MarkupValue} {(CurrencyTypes)(profitMarkup.CurrencyId - 1)})"
                                : " " + (CurrencyTypes)(profitMarkup.CurrencyId - 1)
                            : "%")
                        + "  MarkupID = " + profitMarkup.Id + "  MarkupName = " + profitMarkup.Name;
        }
        public static string ToTurkishCharacterEscapeUpperCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string normalized = input
                .Replace('ç', 'c')
                .Replace('Ç', 'C')
                .Replace('ğ', 'g')
                .Replace('Ğ', 'G')
                .Replace('ı', 'i')
                .Replace('İ', 'I')
                .Replace('ö', 'o')
                .Replace('Ö', 'O')
                .Replace('ş', 's')
                .Replace('Ş', 'S')
                .Replace('ü', 'u')
                .Replace('Ü', 'U');

            return normalized.ToUpper(new System.Globalization.CultureInfo("en-US", false));
        }
    }
}
