using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace KolayCAR.Broker.Domain.Models
{
    public class HttpResult<T> where T : class
    {
        public bool Success { get; set; }

        public string Message { get; set; }
        public string ServiceMessage { get; set; }

        public int ResultCode { get; set; }
        [JsonIgnore]
        public string Cookie { get; set; }
        [JsonIgnore]
        public string RawContent { get; set; }
        [JsonIgnore]
        public bool DeserializeSuccess { get; set; } = true;
        [JsonIgnore]
        public string DeserializeErrorMessage { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public T Data { get; set; }

        public static HttpResult<T> Result(T data, HttpStatusCode httpResultType, bool success, string message = null, ResultCodes resultCode = ResultCodes.Success, string cookie = "", string rawContent = "", bool deserializeSuccess = true, string deserializeErrorMessage = "", string serviceMessage = null) => new HttpResult<T>
        {
            Success = success,
            Message = message,
            ServiceMessage = serviceMessage,
            HttpStatusCode = httpResultType,
            Data = data,
            ResultCode = (int)resultCode,
            Cookie = cookie,
            RawContent = rawContent,
            DeserializeSuccess = deserializeSuccess,
            DeserializeErrorMessage = deserializeErrorMessage
        };

        public static HttpResult<T> Catch(string exception) => new HttpResult<T>
        {
            Success = false,
            Message = exception,
            ServiceMessage = exception,
            ResultCode = (int)ResultCodes.Error,
            HttpStatusCode = HttpStatusCode.InternalServerError,
            DeserializeSuccess = false,
            DeserializeErrorMessage = exception
        };

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
    }

    public enum ResultCodes
    {
        /*0*/
        Success,
        /*1*/
        Error,
        /*2*/
        VehicleNotAvailable,
        /*3*/
        VehiclePriceChange,
        /*4*/
        InvalidCouponCode,
        /*5*/
        Timeout,
        /*6*/
        NoFullCreditPermit
    }
}
