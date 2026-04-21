namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class GetPaymentSettingsRequest
    {
        public int LanguageId { get; set; }
        public string BINNumber { get; set; }
        public bool AdvancePaymentActive { get; set; }
        public float PaymentAmount { get; set; }
    }
}
