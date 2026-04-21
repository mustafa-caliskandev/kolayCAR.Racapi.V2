using System;

namespace KolayCAR.Broker.API.Models
{
    public class ReservationPaymentDetail
    {
        public int Id { get; set; }
        public int ReservationDetailId { get; set; }

        public string PaymentCode { get; set; }
        public string CreditCardNumber { get; set; }
        public string CreditCardOwnerName { get; set; }
        public string ExpireDate { get; set; }
        public string Cvc { get; set; }

        public string PaymentResultCode { get; set; }
        public string PaymentResultMessage { get; set; }
        public string BankResultMessage { get; set; }
        public string ProvisionNumber { get; set; }

        public int ExpireMonth { get; set; }
        public int ExpireYear { get; set; }

        public int? InstallmentCount { get; set; }
        public decimal? InstallmentCommissionAmount { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal PaidAmount { get; set; }

        public bool? AdvencedPayment { get; set; }
        public bool? AdditionalProductPricePoa { get; set; }
        public bool? OneWayFeePoa { get; set; }

        public bool? Refunded { get; set; }
        public int? Returner { get; set; }
        public DateTime? RefundDate { get; set; }

        public virtual ReservationDetail ReservationDetail { get; set; }
    }
}
