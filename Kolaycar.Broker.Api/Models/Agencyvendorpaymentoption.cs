namespace KolayCAR.Broker.API.Models
{
    public partial class Agencyvendorpaymentoption
    {
        public int Id { get; set; }
        public int Agencyid { get; set; }
        public int Vendorid { get; set; }
        public bool Creditcardpaymentactive { get; set; }
        public bool Payallactive { get; set; }
        public bool Advancepaymentactive { get; set; }
        public bool Commissionfreepaymentactive { get; set; }
        public bool Paydeliveryactive { get; set; }
        public bool Payagencyactive { get; set; }
        public bool? Onewayamountdeliverypayment { get; set; }
        public bool? Additionalproductamountdeliverypayment { get; set; }
    }
}
