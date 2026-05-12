using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KolayCAR.Broker.Infrastructure.Helpers
{
    public static class VendorResponseMessageHelper
    {
        private static readonly string[] MessageKeys =
        {
            "message",
            "userMessage",
            "errorMessage",
            "error_description",
            "error",
            "detail",
            "title",
            "description",
            "reason"
        };

        public static string ExtractMessage(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            var trimmedContent = content.Trim();

            try
            {
                var token = JToken.Parse(trimmedContent);
                return ExtractMessage(token);
            }
            catch
            {
                return trimmedContent;
            }
        }

        public static bool IsErrorResponse(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            try
            {
                var token = JToken.Parse(content.Trim());
                return IsErrorResponse(token);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsDefaultMappedResponse<T>(string content, T data) where T : class
        {
            if (string.IsNullOrWhiteSpace(content) || data == null || !IsDefaultRootObject(data))
                return false;

            try
            {
                var token = JToken.Parse(content.Trim());
                if (token is not JObject obj)
                    return false;

                var propertyNames = typeof(T)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
                    .Select(x => x.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                return !obj.Properties().Any(x => propertyNames.Contains(x.Name));
            }
            catch
            {
                return false;
            }
        }

        private static string ExtractMessage(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return string.Empty;

            if (token is JObject obj)
            {
                foreach (var key in MessageKeys)
                {
                    var property = obj.Properties()
                        .FirstOrDefault(x => string.Equals(x.Name, key, StringComparison.OrdinalIgnoreCase));

                    var value = ConvertTokenToText(property?.Value);
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }

                foreach (var property in obj.Properties())
                {
                    var nestedValue = ExtractMessage(property.Value);
                    if (!string.IsNullOrWhiteSpace(nestedValue))
                        return nestedValue;
                }

                return string.Empty;
            }

            if (token is JArray array)
            {
                var messages = array
                    .Select(ConvertTokenToText)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (messages.Count > 0)
                    return string.Join(" | ", messages);

                foreach (var item in array)
                {
                    var nestedValue = ExtractMessage(item);
                    if (!string.IsNullOrWhiteSpace(nestedValue))
                        return nestedValue;
                }

                return string.Empty;
            }

            return ConvertTokenToText(token);
        }

        private static string ConvertTokenToText(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return string.Empty;

            if (token is JValue value)
                return value.ToString();

            if (token is JArray array)
            {
                return string.Join(" | ",
                    array.Select(ConvertTokenToText)
                        .Where(x => !string.IsNullOrWhiteSpace(x)));
            }

            if (token is JObject obj)
            {
                var messages = new List<string>();

                foreach (var property in obj.Properties()
                             .Where(x => MessageKeys.Any(key => string.Equals(key, x.Name, StringComparison.OrdinalIgnoreCase))))
                {
                    var value2 = ConvertTokenToText(property.Value);
                    if (!string.IsNullOrWhiteSpace(value2))
                        messages.Add(value2);
                }

                return messages.Count > 0 ? string.Join(" | ", messages.Distinct()) : string.Empty;
            }

            return string.Empty;
        }

        private static bool IsErrorResponse(JToken token)
        {
            if (token is not JObject obj)
                return false;

            if (obj.Properties().Any(x =>
                    string.Equals(x.Name, "code", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Name, "errorCode", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Name, "errors", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Name, "details", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Name, "error", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            var successProperty = obj.Properties()
                .FirstOrDefault(x => string.Equals(x.Name, "success", StringComparison.OrdinalIgnoreCase));

            if (successProperty?.Value.Type == JTokenType.Boolean && successProperty.Value.Value<bool>() == false)
                return true;

            var statusProperty = obj.Properties()
                .FirstOrDefault(x => string.Equals(x.Name, "status", StringComparison.OrdinalIgnoreCase));

            if (statusProperty?.Value.Type == JTokenType.String)
            {
                var statusValue = statusProperty.Value.ToString();
                if (!string.IsNullOrWhiteSpace(statusValue) &&
                    (statusValue.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                     statusValue.Contains("fail", StringComparison.OrdinalIgnoreCase) ||
                     statusValue.Contains("invalid", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDefaultRootObject(object instance)
        {
            if (instance == null)
                return true;

            var type = instance.GetType();

            if (type == typeof(string))
                return string.IsNullOrWhiteSpace(instance.ToString());

            if (type.IsValueType)
                return instance.Equals(Activator.CreateInstance(type));

            if (instance is IEnumerable enumerable && type != typeof(string))
                return !enumerable.Cast<object>().Any();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
                .ToList();

            if (properties.Count == 0)
                return false;

            return properties.All(property =>
            {
                var value = property.GetValue(instance);

                if (value == null)
                    return true;

                var propertyType = property.PropertyType;

                if (propertyType == typeof(string))
                    return string.IsNullOrWhiteSpace(value.ToString());

                if (propertyType.IsValueType)
                    return value.Equals(Activator.CreateInstance(propertyType));

                if (value is IEnumerable items && propertyType != typeof(string))
                    return !items.Cast<object>().Any();

                return false;
            });
        }
    }
}
