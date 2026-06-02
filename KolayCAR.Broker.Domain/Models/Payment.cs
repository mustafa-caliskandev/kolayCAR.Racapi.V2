namespace KolayCAR.Broker.Domain.Models
{
    public class Payment
    {
        public PaymentTypes? PaymentType { get; set; }
        public bool ExtraPricePayToDelivery { get; set; }
        public bool OneWayFeePayToDelivery { get; set; }
        public int? InstallmentCount { get; set; }
        public bool? ThreeDPaymentActive { get; set; } = false;
        public string ThreeDStatus { get; set; }
        public string ThreeDAuth { get; set; }
        public string ThreeDLevel { get; set; }
        public string ThreeDTxnId { get; set; }
        public string ThreeDMd { get; set; }
        public string ThreeDPnOrInfo { get; set; }
        public string PaymentCode { get; set; }
        public int? BankId { get; set; }
        public int? BankVendorId { get; set; }
        public string CreditCardHolder { get; set; }
        public string CreditCardNumber { get; set; }
        public int? ExpiredYear { get; set; }
        public int? ExpiredMonth { get; set; }
        public string SecurityCode { get; set; }
        public string CreditCardBank { get; set; }
        public string ProvisionNumber { get; set; }
        public bool? CreditCardPaymentTypeActive { get; set; } = false;
        public bool? AdvancePaymentTypeActive { get; set; } = false;
        public string Bank { get; set; }
        public string BankAccountCode { get; set; }
        public string BankAccountingCode { get; set; }
        public decimal? InstallmentCommissionAmount { get; set; }
        public float? InstallmentFee { get; set; }
        public bool AdvancedPaymentWithoutPayment { get; set; }
    }
}
