using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KolayCAR.Broker.Domain.Models
{
    public class CreditTypeCodeJsonConverter : JsonConverter<CreditType>
    {
        public override CreditType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt16(out var value))
                return (CreditType)value;

            if (reader.TokenType != JsonTokenType.String)
                return CreditType.Non;

            return reader.GetString()?.Trim().ToUpperInvariant() switch
            {
                "FC" => CreditType.FullCredit,
                "LC" => CreditType.LimitedCredit,
                "NM" => CreditType.Non,
                "FULLCREDIT" => CreditType.FullCredit,
                "LIMITEDCREDIT" => CreditType.LimitedCredit,
                "NON" => CreditType.Non,
                var raw when short.TryParse(raw, out var numericValue) => (CreditType)numericValue,
                _ => CreditType.Non
            };
        }

        public override void Write(Utf8JsonWriter writer, CreditType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value switch
            {
                CreditType.FullCredit => "FC",
                CreditType.LimitedCredit => "LC",
                _ => "NM"
            });
        }
    }

    public class CreditTypeNewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter<CreditType>
    {
        public override CreditType ReadJson(
            Newtonsoft.Json.JsonReader reader,
            Type objectType,
            CreditType existingValue,
            bool hasExistingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.Value == null)
                return CreditType.Non;

            return reader.Value.ToString()?.Trim().ToUpperInvariant() switch
            {
                "FC" => CreditType.FullCredit,
                "LC" => CreditType.LimitedCredit,
                "NM" => CreditType.Non,
                "FULLCREDIT" => CreditType.FullCredit,
                "LIMITEDCREDIT" => CreditType.LimitedCredit,
                "NON" => CreditType.Non,
                var raw when short.TryParse(raw, out var numericValue) => (CreditType)numericValue,
                _ => CreditType.Non
            };
        }

        public override void WriteJson(
            Newtonsoft.Json.JsonWriter writer,
            CreditType value,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WriteValue(value switch
            {
                CreditType.FullCredit => "FC",
                CreditType.LimitedCredit => "LC",
                _ => "NM"
            });
        }
    }
}
