using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models
{

    public class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<T>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            if (token.Type == JTokenType.Array)
            {
                // JSON alanı liste olarak gelmişse
                return token.ToObject<List<T>>();
            }
            else
            {
                // JSON alanı tek bir nesne olarak gelmişse
                return new List<T> { token.ToObject<T>() };
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = value as List<T>;
            if (list.Count == 1)
            {
                // Eğer tek bir eleman varsa nesne olarak yaz
                serializer.Serialize(writer, list[0]);
            }
            else
            {
                // Eğer birden fazla eleman varsa liste olarak yaz
                serializer.Serialize(writer, list);
            }
        }
    }

}
