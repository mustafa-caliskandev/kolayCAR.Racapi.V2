using KolayCAR.Broker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class PostReservationExtraListJsonConverter : JsonConverter<List<Extra>>
    {
        public override List<Extra> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            using var document = JsonDocument.ParseValue(ref reader);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
                throw new JsonException("Extras must be an array.");

            var extras = new List<Extra>();

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.Null)
                {
                    extras.Add(null);
                    continue;
                }

                var extra = DeserializeExtra(element, options);

                if (TryGetProperty(element, nameof(Extra.Price), options, out var priceElement))
                {
                    extra.RequestPrice = ReadPrice(priceElement);

                    if (extra.RequestPrice.HasValue)
                        extra.Price = extra.RequestPrice.Value;
                }

                extras.Add(extra);
            }

            return extras;
        }

        public override void Write(Utf8JsonWriter writer, List<Extra> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            if (value != null)
            {
                foreach (var extra in value)
                    JsonSerializer.Serialize(writer, extra, options);
            }

            writer.WriteEndArray();
        }

        private static Extra DeserializeExtra(JsonElement element, JsonSerializerOptions options)
        {
            using var stream = new MemoryStream();

            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();

                foreach (var property in element.EnumerateObject())
                {
                    if (IsProperty(property.Name, nameof(Extra.Price), options))
                        continue;

                    property.WriteTo(writer);
                }

                writer.WriteEndObject();
            }

            return JsonSerializer.Deserialize<Extra>(stream.ToArray(), options) ?? new Extra();
        }

        private static bool TryGetProperty(JsonElement element, string propertyName, JsonSerializerOptions options, out JsonElement property)
        {
            if (element.TryGetProperty(propertyName, out property))
                return true;

            var convertedPropertyName = options.PropertyNamingPolicy?.ConvertName(propertyName);

            if (!string.IsNullOrWhiteSpace(convertedPropertyName) &&
                element.TryGetProperty(convertedPropertyName, out property))
                return true;

            if (options.PropertyNameCaseInsensitive)
            {
                foreach (var jsonProperty in element.EnumerateObject())
                {
                    if (IsProperty(jsonProperty.Name, propertyName, options))
                    {
                        property = jsonProperty.Value;
                        return true;
                    }
                }
            }

            property = default;
            return false;
        }

        private static bool IsProperty(string jsonPropertyName, string propertyName, JsonSerializerOptions options)
        {
            if (string.Equals(jsonPropertyName, propertyName, StringComparison.Ordinal))
                return true;

            var convertedPropertyName = options.PropertyNamingPolicy?.ConvertName(propertyName);

            if (!string.IsNullOrWhiteSpace(convertedPropertyName) &&
                string.Equals(jsonPropertyName, convertedPropertyName, StringComparison.Ordinal))
                return true;

            return options.PropertyNameCaseInsensitive &&
                (string.Equals(jsonPropertyName, propertyName, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(jsonPropertyName, convertedPropertyName, StringComparison.OrdinalIgnoreCase));
        }

        private static float? ReadPrice(JsonElement priceElement)
        {
            if (priceElement.ValueKind == JsonValueKind.Null ||
                priceElement.ValueKind == JsonValueKind.Undefined)
                return null;

            if (priceElement.ValueKind == JsonValueKind.Number)
                return priceElement.GetSingle();

            if (priceElement.ValueKind == JsonValueKind.String)
            {
                var price = priceElement.GetString();

                if (string.IsNullOrWhiteSpace(price))
                    return null;

                if (float.TryParse(price, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariantPrice))
                    return invariantPrice;

                if (float.TryParse(price, NumberStyles.Float, CultureInfo.CurrentCulture, out var currentCulturePrice))
                    return currentCulturePrice;
            }

            throw new JsonException("Extra Price must be a number.");
        }
    }
}
