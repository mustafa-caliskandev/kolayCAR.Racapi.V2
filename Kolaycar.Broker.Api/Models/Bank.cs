
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models
{
    public class Bank
    {
        public int Id { get; set; }
        public int BankId { get; set; }
        public int BankVendorId { get; set; }

        public string BankName { get; set; }
        public string BankDefinition { get; set; }
        public string Account { get; set; }
        public string IBAN { get; set; }
        public string ReturnColumnName { get; set; }

        public bool InstallmentActive { get; set; }
        public bool ThreeDPaymentActive { get; set; }
        public bool ThreeDPaymentRequired { get; set; }
        public bool AmexActive { get; set; }

        public virtual ICollection<PaymentSetting> PaymentSettings { get; set; }
        public virtual ICollection<Payment3dSecure> Payment3DSecures { get; set; }
    }
}
