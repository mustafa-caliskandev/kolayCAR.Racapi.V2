namespace KolayCAR.Broker.API.Models.PaymentDto
{
    public class StartPaymentRequestDto
    {
        public int LanguageId { get; set; }
        public int CurrencyId { get; set; }
        public int BankId { get; set; }
        public int BankVendorId { get; set; }
        public string CustomerMailAddress { get; set; }
        public string CreditCardHolder { get; set; }
        public string CreditCardNumber { get; set; }
        public int CreditCardExpiredYear { get; set; }
        public int CreditCardExpiredMonth { get; set; }
        public string SecurityCode { get; set; }
        public int InstallmentCount { get; set; }
        public string PaymentAmount { get; set; }
        public string OrderNo { get; set; }
        public string IpAddress { get; set; }
        public string CallbackUrl { get; set; }
    }
}
