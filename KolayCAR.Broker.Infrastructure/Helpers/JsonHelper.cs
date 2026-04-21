using Newtonsoft.Json;
using System;
using System.IO;

namespace KolayCAR.Broker.Infrastructure.Helpers
{
    public class JsonHelper
    {
        public static T ReadFromJsonFile<T>(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                }
            }
            catch (Exception)
            {
                return default(T);
            }

        }
    }
}
