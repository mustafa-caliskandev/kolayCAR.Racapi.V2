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

        public int ResultCode { get; set; }
        [JsonIgnore]
        public string Cookie { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public T Data { get; set; }

        public static HttpResult<T> Result(T data, HttpStatusCode httpResultType, bool success, string message = null, ResultCodes resultCode = ResultCodes.Success, string cookie = "") => new HttpResult<T>
        {
            Success = success,
            Message = message,
            HttpStatusCode = httpResultType,
            Data = data,
            ResultCode = (int)resultCode,
            Cookie = cookie
        };

        public static HttpResult<T> Catch(string exception) => new HttpResult<T>
        {
            Success = false,
            Message = exception,
            ResultCode = (int)ResultCodes.Error,
            HttpStatusCode = HttpStatusCode.InternalServerError
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
