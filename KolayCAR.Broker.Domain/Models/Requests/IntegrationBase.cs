namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class IntegrationBase
    {
        public VendorTypes VendorType { get; set; }
        public string ApiKey { get; set; }
        public string ApiPassword { get; set; }
        public string ApiClientId { get; set; }
        public string SecretKey { get; set; }
    }
}
