namespace KolayCAR.Broker.API.Models.PaymentDto
{
    public class PaymentControlDto
    {
        public string ReservationToken { get; set; }
        public string PaymentCode { get; set; }
        public string Status { get; set; }
        public string Auth { get; set; }
        public string Level { get; set; }
        public string TxnId { get; set; }
        public string Md { get; set; }
        public string MdText { get; set; }
        public bool? Successfully { get; set; }
        public int LanguageId { get; set; }
        public int CurrencyId { get; set; }
    }
}
