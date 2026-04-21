using KolayCAR.Broker.Domain.Models.Response;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Logo
{
    public class InvoiceListResponse : ServiceResponseBase
    {
        public List<Invoice> InvoiceList { get; set; }
    }

    public class Invoice
    {
        public string CustomerTitle { get; set; }
        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public int InvoiceID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string StockCode { get; set; }
        public string Mail { get; set; }
        public string CustomerCode { get; set; }
        public string Plate { get; set; }
        public string RowDescription { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string InvoiceNumber { get; set; }
        public string EInvoiceNumber { get; set; }
        public string Cancel { get; set; }
        public string Office { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal InvoiceTaxAmount { get; set; }
        public decimal InvoiceTotalAmount { get; set; }
        public string InvoiceDescription { get; set; }
        public int RowID { get; set; }
        public string ProcessType { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Discount { get; set; }
        public decimal RowAmount { get; set; }
        public decimal RowTaxAmount { get; set; }
        public decimal RowTotalAmount { get; set; }
        public string Description { get; set; }
        public string ContractCustomerCode { get; set; }
        public string TCNumber { get; set; }
        public string InvoiceStatus { get; set; }
        public string ContractCustomerTitle { get; set; }
        public string ContractTaxNumber { get; set; }
        public string ContractTaxOffice { get; set; }
        public string ContractCustomerName { get; set; }
        public string ContractCustomerSurname { get; set; }
        public string ContractCustomerCountry { get; set; }
        public string ContractCustomerCity { get; set; }
        public string ContractCustomerDistrict { get; set; }
        public string ContractCustomerAddress { get; set; }
        public string ContractCustomerTCNumber { get; set; }
        public DateTime? FinalizationDate { get; set; }
        public DateTime? CancellationDate { get; set; }
        public string ReservationNumber { get; set; }
        public int VendorId { get; set; }
        public string PickupLocation { get; set; }
        public int PickupLocationId { get; set; }
        public string ReturnLocation { get; set; }
        public int ReturnLocationId { get; set; }
        public int RentalDuration { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal? CancellationRefundAmount { get; set; }
        public decimal? VendorPrice { get; set; }
        public decimal? VendorProfitMarkup { get; set; }
        public int? VendorWorkingType { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsPaymentReceived { get; set; }
        public bool VendorCommissionInvoice { get; set; }
        public string TaxExceptionCode { get; set; }

        //public decimal VendorRentPrice { get; set; }
        //public int VendorRentalWorkingType { get; set; }
        //public decimal VendorProfitMatkup { get; set; }
        //public decimal VendorExtraPrice { get; set; }
        //public int VendorExtraWorkingType { get; set; }
        //public decimal VendorExtraProfitMarkup { get; set; }
        //public decimal VendorDropPrice { get; set; }
        //public int VendorDropWorkingType { get; set; }
        //public decimal VendorDropProfitMarkup { get; set; }

    }
}
