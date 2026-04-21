using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models
{
    public class PaymentSetting
    {
        public int Id { get; set; }
        public int BankId { get; set; }

        public DateTime? LogDate { get; set; }

        public string ReservationToken { get; set; }
        public string BinNumber { get; set; }
        public string PaymentAmount { get; set; }
        public string CustomerInfo { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string ProvisionNumber { get; set; }

        public bool? AdvencedPaymentActive { get; set; }

        public virtual Bank Bank { get; set; }
        public virtual ICollection<Installment> Installments { get; set; }
    }
}
