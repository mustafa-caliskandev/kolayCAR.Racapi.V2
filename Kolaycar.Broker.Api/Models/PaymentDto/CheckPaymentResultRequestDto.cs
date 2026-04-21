namespace KolayCAR.Broker.API.Models.PaymentDto
{
    public class CheckPaymentResultRequestDto
    {
        public string PaymentCode { get; set; }
        public string ReservationToken { get; set; }
    }
}
