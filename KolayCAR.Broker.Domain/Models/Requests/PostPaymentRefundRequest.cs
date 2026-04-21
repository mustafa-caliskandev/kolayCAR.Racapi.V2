namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class PostPaymentRefundRequest
    {
        public string LanguageCode { get; set; }
        public string CurrencyCode { get; set; }
        public float RefundAmount { get; set; }
        public string OrderNumber { get; set; }
        public string IpAddress { get; set; }
        public PaymentRefundTypes PaymentRefundType { get; set; }
        public string ReservationNumber { get; set; }
    }

    public enum PaymentRefundTypes
    {
        Cancel = 2,
        Refund = 3
    }
}
