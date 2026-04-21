namespace KolayCAR.Broker.Domain.Models.Response
{
    public class PostThreeDSecureControlResponse
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public string Content { get; set; }
        public string APIKey { get; set; }
        public string EncryptedAPIPassword { get; set; }
        public string EncryptedVendorId { get; set; }
        public string EncryptedType { get; set; }
        public string Token { get; set; }
    }
}
