namespace KolayCAR.Broker.API.Models.PaymentDto
{
    public class PaymentResultUpdateDto
    {
        public string ReservationToken { get; set; }
        public string PaymentResultCode { get; set; }
        public string PaymentResultMessage { get; set; }
        public string BankResultMessage { get; set; }
        public string ProvisionNumber { get; set; }
    }
}
