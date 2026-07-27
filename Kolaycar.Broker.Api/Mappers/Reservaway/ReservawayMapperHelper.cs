using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response.Reservaway;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Reservaway
{
    public static class ReservawayMapperHelper
    {
        private const string PlanSeparator = "|";
        private static readonly HashSet<string> SellableProductTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BSC"
        };
        private static readonly HashSet<string> SellablePaymentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PPFC",
            "PPNC"
        };

        public static string CreatePlanReference(ReservawayPlanDefinition planDefinition)
        {
            if (planDefinition == null)
                return string.Empty;

            return string.Join(PlanSeparator, new[]
            {
                planDefinition.product_type_id.ToString(),
                planDefinition.payment_type_id.ToString(),
                planDefinition.product_type_name.ToStringNullSafe(),
                planDefinition.payment_type_name.ToStringNullSafe()
            });
        }

        public static ReservawayPlanReference ParsePlanReference(string value)
        {
            var parts = value.ToStringNullSafe().Split(PlanSeparator);
            return new ReservawayPlanReference
            {
                ProductTypeId = parts.Length > 0 ? parts[0].ToIntNullSafe() : 0,
                PaymentTypeId = parts.Length > 1 ? parts[1].ToIntNullSafe() : 0,
                ProductTypeName = parts.Length > 2 ? parts[2] : string.Empty,
                PaymentTypeName = parts.Length > 3 ? parts[3] : string.Empty
            };
        }

        public static ReservawayPlanDefinition GetBasePlanDefinition(
            ReservawayPrices prices,
            List<ReservawayTypeItem> productTypes = null,
            List<ReservawayTypeItem> paymentTypes = null)
        {
            if (prices?.rate_codes == null)
                return null;

            ReservawayPlanDefinition selectedPlan = null;
            var selectedPrice = float.MaxValue;

            foreach (var productRateCode in prices.rate_codes)
            {
                if (!SellableProductTypes.Contains(productRateCode.Key.ToStringNullSafe()) || productRateCode.Value == null)
                    continue;

                foreach (var paymentRateCode in productRateCode.Value)
                {
                    if (!SellablePaymentTypes.Contains(paymentRateCode.Key.ToStringNullSafe()))
                        continue;

                    var comparablePrice = GetComparablePrice(paymentRateCode.Value);
                    if (selectedPlan != null && comparablePrice >= selectedPrice)
                        continue;

                    selectedPrice = comparablePrice;
                    selectedPlan = new ReservawayPlanDefinition
                    {
                        product_type_name = productRateCode.Key.ToStringNullSafe(),
                        payment_type_name = paymentRateCode.Key.ToStringNullSafe(),
                        product_type_id = FindTypeId(productTypes, productRateCode.Key),
                        payment_type_id = FindTypeId(paymentTypes, paymentRateCode.Key)
                    };
                }
            }

            return selectedPlan;
        }

        public static bool IsSellablePlan(ReservawayPlanReference planReference)
            => planReference != null
               && SellableProductTypes.Contains(planReference.ProductTypeName.ToStringNullSafe())
               && SellablePaymentTypes.Contains(planReference.PaymentTypeName.ToStringNullSafe());

        public static bool HasSellablePlan(ReservawayPrices prices)
            => GetBasePlanDefinition(prices) != null;

        private static float GetComparablePrice(ReservawayRatePrice ratePrice)
        {
            if (ratePrice == null)
                return float.MaxValue;

            if (ratePrice.total_price > 0)
                return ratePrice.total_price;

            if (ratePrice.daily_price > 0)
                return ratePrice.daily_price;

            return float.MaxValue;
        }

        public static ReservawayRatePrice GetRatePrice(ReservawayPrices prices, string productTypeName, string paymentTypeName)
        {
            if (prices?.rate_codes == null || string.IsNullOrWhiteSpace(productTypeName))
                return null;

            if (!TryGetRateCodePrices(prices, productTypeName, out var paymentTypes) || paymentTypes == null)
                return null;

            if (!string.IsNullOrWhiteSpace(paymentTypeName)
                && TryGetPaymentRate(paymentTypes, paymentTypeName, out var exactRate))
                return exactRate;

            if (!string.IsNullOrWhiteSpace(paymentTypeName))
                return null;

            return paymentTypes.Values.FirstOrDefault();
        }

        private static bool TryGetRateCodePrices(ReservawayPrices prices, string productTypeName, out Dictionary<string, ReservawayRatePrice> paymentTypes)
        {
            paymentTypes = null;

            if (prices?.rate_codes == null)
                return false;

            if (prices.rate_codes.TryGetValue(productTypeName, out paymentTypes))
                return true;

            var matchingKey = prices.rate_codes.Keys.FirstOrDefault(x => x.Equals(productTypeName, StringComparison.OrdinalIgnoreCase));
            return !string.IsNullOrWhiteSpace(matchingKey) && prices.rate_codes.TryGetValue(matchingKey, out paymentTypes);
        }

        private static bool TryGetPaymentRate(Dictionary<string, ReservawayRatePrice> paymentTypes, string paymentTypeName, out ReservawayRatePrice ratePrice)
        {
            ratePrice = null;

            if (paymentTypes.TryGetValue(paymentTypeName, out ratePrice))
                return true;

            var matchingKey = paymentTypes.Keys.FirstOrDefault(x => x.Equals(paymentTypeName, StringComparison.OrdinalIgnoreCase));
            return !string.IsNullOrWhiteSpace(matchingKey) && paymentTypes.TryGetValue(matchingKey, out ratePrice);
        }

        private static int FindTypeId(List<ReservawayTypeItem> types, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return 0;

            var item = types?.FirstOrDefault(x => x.key.ToStringNullSafe().Equals(key, StringComparison.OrdinalIgnoreCase))
                ?? types?.FirstOrDefault(x => x.description.ToStringNullSafe().Equals(key, StringComparison.OrdinalIgnoreCase));

            if (item?.id > 0)
                return item.id;

            return 0;
        }

        public static CurrencyTypes GetCurrencyType(string currencyCode, CurrencyTypes fallbackCurrencyType)
            => Enum.TryParse(currencyCode, true, out CurrencyTypes currencyType) ? currencyType : fallbackCurrencyType;

        public static string GetLocale(string languageCode)
        {
            var normalized = languageCode.ToStringNullSafe().Trim().ToUpperInvariant();
            return normalized switch
            {
                "TR" => "tr-TR",
                "DE" => "de-DE",
                "FR" => "fr-FR",
                "ES" => "es-ES",
                "IT" => "it-IT",
                "NL" => "nl-NL",
                _ => "en-GB"
            };
        }

        public static string NormalizeBaseUrl(string apiBaseUrl)
            => string.IsNullOrWhiteSpace(apiBaseUrl)
                ? "https://api.reservaway.com/"
                : apiBaseUrl.TrimEnd('/') + "/";
    }

    public class ReservawayPlanReference
    {
        public int ProductTypeId { get; set; }
        public int PaymentTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public string PaymentTypeName { get; set; }
    }
}
