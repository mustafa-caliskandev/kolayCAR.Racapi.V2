using KolayCAR.Broker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace KolayCAR.Broker.API.Providers.Vonarent
{
    public static class SignatureHelper
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string CreateTimestamp()
            => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);

        public static IDictionary<string, object> CreateHeaders(
            Vendor vendor,
            string method,
            string path,
            IDictionary<string, object> query = null,
            object body = null,
            string bearerToken = null)
        {
            var timestamp = CreateTimestamp();
            var message = CreateMessage(method, path, timestamp, query, body);
            var signature = CreateSignature(vendor.SecretKey, message);

            var headers = new Dictionary<string, object>
            {
                { "x-api-key", vendor.ApiKey },
                { "x-timestamp", timestamp },
                { "x-signature", signature }
            };

            if (!string.IsNullOrWhiteSpace(bearerToken))
                headers.Add("Authorization", $"Bearer {bearerToken}");

            return headers;
        }

        public static string CreateMessage(
            string method,
            string path,
            string timestamp,
            IDictionary<string, object> query = null,
            object body = null)
        {
            var queryString = CreateQueryString(query);
            var bodyString = SerializeBody(body);

            return $"{method?.ToUpperInvariant()}|{path}|{timestamp}|{queryString}|{bodyString}";
        }

        public static string SerializeBody(object body)
            => body != null ? JsonSerializer.Serialize(body, JsonSerializerOptions) : string.Empty;

        public static string CreateQueryString(IDictionary<string, object> query)
        {
            if (query == null || query.Count == 0)
                return string.Empty;

            var parts = query
                .Where(kvp => kvp.Value != null)
                .Select(kvp => new KeyValuePair<string, string>(kvp.Key, ConvertToInvariantString(kvp.Value)))
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .Select(kvp => $"{kvp.Key}={kvp.Value}");

            return string.Join("&", parts);
        }

        public static string CreateSignature(string secret, string message)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("Vonarent SecretKey bos olamaz.");

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message ?? string.Empty));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static string ConvertToInvariantString(object value)
        {
            return value switch
            {
                null => string.Empty,
                DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
                DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
        }
    }
}
