using System;

namespace KolayCAR.Broker.Domain.Models
{
    public class ReservationInvoice
    {
        public int Id { get; set; }
        public string AccountingInvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public InvoiceTypes InvoiceType { get; set; }
        public InvoiceStatusTypes InvoiceStatusType { get; set; }
        public string ReservationNumber { get; set; }
        public CustomerTypes CustomerType { get; set; }
        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public string CompanyTitle { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerIdentityNumber { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
        public string Address { get; set; }
        public string ServiceName { get; set; }
        public int DistrictId { get; set; }
        public int CityId { get; set; }
        public int CountryId { get; set; }
        public int Piece { get; set; }
        public CurrencyTypes CurrencyType { get; set; }
        public float ExchangeRate { get; set; }
        public float UnitPriceExcludingVat { get; set; }
        public float PriceExcludingVat { get; set; }
        public float TotalPriceIncludingVat { get; set; }
        public float TotalVat { get; set; }
        public int VatRate { get; set; }
        public string CreateInvoiceServiceResponse { get; set; }
        public string CancelInvoiceServiceResponse { get; set; }
    }

    public enum InvoiceTypes
    {
        NotSet,
        EArchive,
        EInvoice
    }

    public enum InvoiceStatusTypes
    {
        NotSent,
        Sent,
        Cancelled
    }

    public enum CustomerTypes
    {
        Individual,
        Corporate
    }
}
