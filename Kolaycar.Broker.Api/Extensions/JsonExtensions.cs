using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KolayCAR.Broker.API.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Aldığı objeyi Json nesnesi haline getirir.
        /// </summary>
        /// <param name="model">object tipinde herhangi bir değer</param>
        /// <returns>Json String</returns>
        public static string ModelToJson(this object model)
        {
            return JsonConvert.SerializeObject(model);
        }

        /// <summary>
        /// Json'dan objeye dönüşüm işlemi gerçekleştirir.
        /// </summary>
        /// <typeparam name="TEntity">Dönüştürülmesi istenen nesne</typeparam>
        /// <param name="json">Json String</param>
        /// <returns>TEntity tipinde nesne</returns>
        public static TEntity FromJson<TEntity>(this string json)
        {
            return JsonConvert.DeserializeObject<TEntity>(json);
        }

        /// <summary>
        /// JSON text'inin geçerli olup olmadığını kotrol eder.
        /// </summary>
        /// <param name="json"> JSON String </param>
        /// <returns> Geçerli formatta ise true, değil ise false döner. </returns>
        public static bool ValidateJson(this string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            bool response;
            try
            {
                JToken.Parse(json);
                response = true;
            }
            catch (JsonReaderException)
            {
                response = false;
            }
            return response;
        }
    }
}
