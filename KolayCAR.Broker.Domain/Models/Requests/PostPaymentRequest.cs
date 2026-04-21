namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class PostPaymentRequest
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
        public float PaymentAmount { get; set; }
        public string OrderNo { get; set; }
        public string IpAddress { get; set; }
        public bool ThreeDPaymentActive { get; set; }
        public string Status { get; set; }
        public string Auth { get; set; }
        public string Level { get; set; }
        public string Txnid { get; set; }
        public string Md { get; set; }
        public string PnOrInfo { get; set; }
    }

    public class PostPaymentRequestV2
    {
        public int LanguageId { get; set; }
        public int CurrencyId { get; set; }
        public int BankId { get; set; }
        public int BankVendorId { get; set; }
        public string CustomerMailAddress { get; set; }
        public string CreditCardHolder { get; set; }
        public string CreditCardNumber { get; set; }
        public string CreditCardExpiredYear { get; set; }
        public string CreditCardExpiredMonth { get; set; }
        public string SecurityCode { get; set; }
        public int InstallmentCount { get; set; }
        public float PaymentAmount { get; set; }
        public string OrderNo { get; set; }
        public string IpAddress { get; set; }
        public bool ThreeDPaymentActive { get; set; }
        public string Status { get; set; }
        public string Auth { get; set; }
        public string Level { get; set; }
        public string Txnid { get; set; }
        public string Md { get; set; }
        public string PnOrInfo { get; set; }
    }
}
