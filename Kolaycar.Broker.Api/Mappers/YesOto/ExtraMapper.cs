using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Responses.YesOto;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Kolaycar.Broker.Api.Mappers.YesOto
{
    public static class ExtraMapper
    {
        public static List<Extra> Map(this List<YesOtoAdditionalService> services)
        {
            return services?
                .Where(service => service != null && !string.IsNullOrWhiteSpace(service.id))
                .Select((service, index) => service.Map(index))
                .ToList() ?? new List<Extra>();
        }

        private static Extra Map(this YesOtoAdditionalService service, int index)
        {
            var extraCode = service.id;
            var extraName = FirstText(service.text, service.name, service.title, ToText(service.serviceType), service.description, extraCode);
            var price = GetPrice(service);
            var rentalType = GetRentalType(service);

            return new Extra
            {
                ExtraId = ToStableId($"{extraCode}|{index}"),
                ExtraCode = extraCode,
                ApiExtraCode = extraCode,
                ExtraName = extraName,
                ExtraDescription = service.description,
                Price = price,
                ApiPrice = price,
                DefaultPrice = price,
                UnitPrice = price,
                Piece = service.count > 0 ? service.count : 1,
                ExtraRentalType = rentalType,
                ExtraType = GetExtraType(service),
                ExtraQuantityIncreasable = service.maxCount > 1 || service.isQuantityIncreasable || service.quantityIncreasable,
                IsRequired = service.isRequired,
                VendorExtraExists = true,
                CurrencyCode = CurrencyTypes.TRY.ToString()
            };
        }

        private static float GetPrice(YesOtoAdditionalService service)
        {
            var totalPrice = FirstPositive(
                service.discountedTotalPrice,
                service.totalPrice,
                service.discountedGrandTotal,
                service.grandTotal,
                GetFloat(service, "discountedTotalPrice"),
                GetFloat(service, "totalPrice"),
                GetFloat(service, "discountedGrandTotal"),
                GetFloat(service, "grandTotal"));

            if (totalPrice > 0)
                return totalPrice;

            var dailyPrice = FirstPositive(
                service.discountedPricePerDay,
                service.pricePerDay,
                service.discountedDailyPrice,
                service.dailyPrice,
                GetFloat(service, "discountedPricePerDay"),
                GetFloat(service, "pricePerDay"),
                GetFloat(service, "discountedDailyPrice"),
                GetFloat(service, "dailyPrice"));

            if (dailyPrice > 0)
                return dailyPrice;

            return FirstPositive(
                service.discountedPrice,
                service.price,
                GetFloat(service, "discountedPrice"),
                GetFloat(service, "price"),
                GetFloat(service, "salePrice"),
                GetFloat(service, "amount"),
                GetFloat(service, "value"));
        }

        private static ExtraRentalTypes GetRentalType(YesOtoAdditionalService service)
        {
            if (FirstPositive(
                    service.discountedTotalPrice,
                    service.totalPrice,
                    service.discountedGrandTotal,
                    service.grandTotal,
                    GetFloat(service, "discountedTotalPrice"),
                    GetFloat(service, "totalPrice"),
                    GetFloat(service, "discountedGrandTotal"),
                    GetFloat(service, "grandTotal")) > 0)
                return ExtraRentalTypes.PerRental;

            if (FirstPositive(
                    service.discountedPricePerDay,
                    service.pricePerDay,
                    service.discountedDailyPrice,
                    service.dailyPrice,
                    GetFloat(service, "discountedPricePerDay"),
                    GetFloat(service, "pricePerDay"),
                    GetFloat(service, "discountedDailyPrice"),
                    GetFloat(service, "dailyPrice")) > 0)
                return ExtraRentalTypes.Daily;

            if (service.isDaily == true || service.perDay == true)
                return ExtraRentalTypes.Daily;

            var calculationType = $"{ToText(service.priceCalculationType)} {GetString(service, "priceCalculationType")} {GetString(service, "calculationType")} {GetString(service, "rentalType")}".ToLowerInvariant();

            return calculationType.Contains("day") ||
                   calculationType.Contains("daily") ||
                   calculationType.Contains("gun") ||
                   calculationType.Contains("gün") ||
                   calculationType.Contains("per_day") ||
                   calculationType.Contains("depended_on_duration")
                ? ExtraRentalTypes.Daily
                : ExtraRentalTypes.PerRental;
        }

        private static AdditionalProductTypes GetExtraType(YesOtoAdditionalService service)
        {
            var typeText = $"{ToText(service.serviceType)} {GetString(service, "serviceType")} {service.name} {service.text} {service.description}".ToLowerInvariant();

            return typeText.Contains("sigorta") ||
                   typeText.Contains("insurance") ||
                   typeText.Contains("coverage") ||
                   typeText.Contains("paket") ||
                   typeText.Contains("package")
                ? AdditionalProductTypes.Insurance
                : AdditionalProductTypes.Extra;
        }

        private static string ToText(object value)
        {
            if (value == null)
                return null;

            if (value is string text)
                return FirstText(text);

            if (value is JValue jValue)
                return jValue.Value?.ToString();

            if (value is JObject jObject)
                return FirstText(
                    jObject.Value<string>("text"),
                    jObject.Value<string>("name"),
                    jObject.Value<string>("value"),
                    jObject.Value<string>("code"),
                    jObject.Value<string>("id"));

            var serializedValue = value.ToString();

            if (!string.IsNullOrWhiteSpace(serializedValue) && serializedValue.TrimStart().StartsWith("{"))
            {
                try
                {
                    var json = JObject.Parse(serializedValue);
                    return FirstText(
                        json.Value<string>("text"),
                        json.Value<string>("name"),
                        json.Value<string>("value"),
                        json.Value<string>("code"),
                        json.Value<string>("id"));
                }
                catch
                {
                    return FirstText(serializedValue);
                }
            }

            return FirstText(serializedValue);
        }

        private static float GetFloat(YesOtoAdditionalService service, string key)
        {
            var token = GetToken(service, key);

            if (token == null || token.Type == JTokenType.Null)
                return 0;

            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                return token.Value<float>();

            return float.TryParse(token.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantValue)
                ? invariantValue
                : float.TryParse(token.ToString(), out var value) ? value : 0;
        }

        private static string GetString(YesOtoAdditionalService service, string key)
        {
            return GetToken(service, key)?.ToString();
        }

        private static JToken GetToken(YesOtoAdditionalService service, string key)
        {
            return service?.AdditionalData?
                .FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
                .Value;
        }

        private static string FirstText(params string[] values)
        {
            return values?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
        }

        private static float FirstPositive(params float[] values)
        {
            return values.FirstOrDefault(value => value > 0);
        }

        private static int ToStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            unchecked
            {
                uint hash = 2166136261;

                foreach (var character in value)
                {
                    hash ^= character;
                    hash *= 16777619;
                }

                return (int)(hash & 0x7fffffff);
            }
        }
    }
}
