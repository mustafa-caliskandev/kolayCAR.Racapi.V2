namespace KolayCAR.Broker.Domain.Models.Response
{
    public class ServiceResponseBase
    {
        public ServiceResponseBase()
        {

        }
        public ServiceResponseBase(object data, bool success, string message = "", string serviceMessage = "", string serviceCode = "", object data2 = null)
        {
            this.Data = data;
            this.Data2 = data2;
            this.Message = message;
            this.Success = success;
            this.ServiceMessage = serviceMessage;
            this.ServiceCode = serviceCode;
        }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceMessage { get; set; }
        public object Data { get; set; }
        public object Data2 { get; set; }
        public bool IsTimeOut { get; set; }
    }
}
