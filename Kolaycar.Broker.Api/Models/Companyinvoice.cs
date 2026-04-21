namespace KolayCAR.Broker.API.Models
{
    public partial class Companyinvoice
    {
        public int Recid { get; set; }
        public string Companytitle { get; set; }
        public string Companyaddress { get; set; }
        public string Companypostbox { get; set; }
        public string County { get; set; }
        public string City { get; set; }
        public string Companytaxno { get; set; }
        public string Companytaxoffice { get; set; }
        public string Companyphone { get; set; }
        public string Companyfax { get; set; }
        public string Companyemail { get; set; }
        public string Companywebsite { get; set; }
        public int? Taxrate { get; set; }
        public string Invoicehtml { get; set; }
    }
}
