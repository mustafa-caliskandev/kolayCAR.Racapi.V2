namespace KolayCAR.Broker.API.Models.PaymentDto
{
    public class PaymentDto
    {
        public int languageId { get; set; }
        public int bankId { get; set; }
        public int currencyId { get; set; }
        public int bankVendorId { get; set; }
        public string customerMailAddress { get; set; }
        public string creditCardHolder { get; set; }
        public string creditCardNumber { get; set; }
        public int creditCardExpiredYear { get; set; }
        public int creditCardExpiredMonth { get; set; }
        public string securityCode { get; set; }
        public int installmentCount { get; set; }
        public string paymentAmount { get; set; }
        public string orderNo { get; set; }
        public string ipAddress { get; set; }
        public bool threeDPaymentActive { get; set; }
        public string status { get; set; }
        public string auth { get; set; }
        public string level { get; set; }
        public string txnid { get; set; }
        public string md { get; set; }
        public string pnOrInfo { get; set; }
    }
}
