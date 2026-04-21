namespace KolayCAR.Broker.Domain.Models.Response
{
    public class PostPaymentResponseBase
    {
        public bool Success { get; set; }
        public PostPaymentResponse PostPaymentResponse { get; set; }
    }
    public class PostPaymentResponse
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public PaymentResult PaymentResult { get; set; }
        public string ProvisionNo { get; set; }
    }

    public class PaymentResult
    {
        public int BankId { get; set; }
        public string ProvisionNumber { get; set; }
        public float PaymentAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string AlertErrorCode { get; set; }
        public string LogResultNumber { get; set; }
        public string LogErrorCode { get; set; }
        public string OrderNumber { get; set; }
    }
}
