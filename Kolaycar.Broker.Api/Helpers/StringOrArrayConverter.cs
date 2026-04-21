using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KolayCAR.Broker.API.Helpers
{
    public class StringOrArrayConverter : JsonConverter<List<string>>
    {
        public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new List<string> { reader.GetString() };
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                return JsonSerializer.Deserialize<List<string>>(ref reader, options);
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
        {
            if (value.Count == 1)
            {
                writer.WriteStringValue(value[0]);
            }
            else
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }
    }

}
