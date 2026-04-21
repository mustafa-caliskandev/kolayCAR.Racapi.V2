using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Invoice
    {
        public int Id { get; set; }
        public string Accountinginvoicenumber { get; set; }
        public DateTime Invoicedate { get; set; }
        public int Invoicetype { get; set; }
        public int Status { get; set; }
        public string Reservationnumber { get; set; }
        public string Customername { get; set; }
        public string Customersurname { get; set; }
        public string Companytitle { get; set; }
        public string Customeremail { get; set; }
        public string Customerphonenumber { get; set; }
        public string Customeridentitynumber { get; set; }
        public string Taxoffice { get; set; }
        public string Taxnumber { get; set; }
        public string Servicename { get; set; }
        public int Piece { get; set; }
        public int Currencyid { get; set; }
        public decimal Unitpriceexcludingvat { get; set; }
        public decimal Priceexcludingvat { get; set; }
        public decimal Totalpriceincludingvat { get; set; }
        public decimal Totalvat { get; set; }
        public int Vatrate { get; set; }
        public int Countryid { get; set; }
        public int Cityid { get; set; }
        public int Districtid { get; set; }
        public string Address { get; set; }
        public decimal Exchangerate { get; set; }
        public int Customertype { get; set; }
        public string Createinvoiceserviceresponse { get; set; }
        public string Cancelinvoiceserviceresponse { get; set; }
        public DateTime? Canceldate { get; set; }
    }
}
