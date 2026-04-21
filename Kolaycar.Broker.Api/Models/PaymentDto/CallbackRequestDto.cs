namespace KolayCAR.Broker.API.Models.PaymentDto
{
    public class CallbackRequestDto
    {
        public string PaymentCode { get; set; }
        public string ReservationToken { get; set; }
        public int LanguageId { get; set; }
        public int CurrencyId { get; set; }
        public string MdText { get; set; }
        public bool? Successfully { get; set; } = false;
    }
}
