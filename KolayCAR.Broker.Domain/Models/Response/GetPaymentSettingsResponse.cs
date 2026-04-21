using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class GetPaymentSettingsResponse
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public BankInfo Bank { get; set; }
        public List<InstallmentInfo> Installments { get; set; }
    }

    public class BankInfo
    {
        public int BankId { get; set; }
        public string BankName { get; set; }
        public int BankVendorId { get; set; }
        public string BankDefinition { get; set; }
        public bool InstallmentActive { get; set; }
        public bool ThreeDPaymentActive { get; set; }
        public bool ThreeDPaymentRequired { get; set; }
        public bool AmexActive { get; set; }
        public string Account { get; set; }
        public string IBAN { get; set; }
    }

    public class InstallmentInfo
    {
        public int InstallmentCount { get; set; }
        public float InstallmentTotalAmount { get; set; }
        public float InstallmentAmount { get; set; }
        public string Comment { get; set; }
        public float Percent { get; set; }
    }
}
