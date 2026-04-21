using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers
{
    public static class InvoiceMapper
    {
        public static CommonModels.ReservationInvoice Map(this Invoice invoice) =>
            invoice != null ? new CommonModels.ReservationInvoice
            {
                Id = invoice.Id,
                AccountingInvoiceNumber = invoice.Accountinginvoicenumber,
                InvoiceDate = invoice.Invoicedate,
                CancelDate = invoice.Canceldate,
                InvoiceType = (CommonModels.InvoiceTypes)invoice.Invoicetype,
                InvoiceStatusType = (CommonModels.InvoiceStatusTypes)invoice.Status,
                ReservationNumber = invoice.Reservationnumber,
                CustomerType = (CommonModels.CustomerTypes)invoice.Customertype,
                CustomerName = invoice.Customername,
                CustomerSurname = invoice.Customersurname,
                CompanyTitle = invoice.Companytitle,
                CustomerEmail = invoice.Customeremail,
                CustomerPhoneNumber = invoice.Customerphonenumber,
                CustomerIdentityNumber = invoice.Customeridentitynumber,
                TaxOffice = invoice.Taxoffice,
                TaxNumber = invoice.Taxnumber,
                ServiceName = invoice.Servicename,
                Piece = invoice.Piece,
                CurrencyType = (CommonModels.CurrencyTypes)(invoice.Currencyid - 1),
                UnitPriceExcludingVat = invoice.Unitpriceexcludingvat.ToFloatNullSafe(),
                PriceExcludingVat = invoice.Priceexcludingvat.ToFloatNullSafe(),
                TotalPriceIncludingVat = invoice.Totalpriceincludingvat.ToFloatNullSafe(),
                TotalVat = invoice.Totalvat.ToFloatNullSafe(),
                VatRate = invoice.Vatrate,
                CountryId = invoice.Countryid,
                CityId = invoice.Cityid,
                DistrictId = invoice.Districtid,
                Address = invoice.Address,
                ExchangeRate = invoice.Exchangerate.ToFloatNullSafe(),
                CreateInvoiceServiceResponse = invoice.Createinvoiceserviceresponse,
                CancelInvoiceServiceResponse = invoice.Cancelinvoiceserviceresponse
            }
            : null;

        public static Invoice Map(this CommonModels.ReservationInvoice invoice) =>
            invoice != null ? new Invoice
            {
                Id = invoice.Id,
                Accountinginvoicenumber = invoice.AccountingInvoiceNumber,
                Invoicedate = invoice.InvoiceDate,
                Canceldate = invoice.CancelDate,
                Invoicetype = (int)invoice.InvoiceType,
                Status = (int)invoice.InvoiceStatusType,
                Reservationnumber = invoice.ReservationNumber,
                Customertype = (int)invoice.CustomerType,
                Customername = invoice.CustomerName,
                Customersurname = invoice.CustomerSurname,
                Companytitle = invoice.CompanyTitle,
                Customeremail = invoice.CustomerEmail,
                Customerphonenumber = invoice.CustomerPhoneNumber,
                Customeridentitynumber = invoice.CustomerIdentityNumber,
                Taxoffice = invoice.TaxOffice,
                Taxnumber = invoice.TaxNumber,
                Servicename = invoice.ServiceName,
                Piece = invoice.Piece,
                Currencyid = (int)invoice.CurrencyType + 1,
                Unitpriceexcludingvat = invoice.UnitPriceExcludingVat.ToDecimalNullSafe(),
                Priceexcludingvat = invoice.PriceExcludingVat.ToDecimalNullSafe(),
                Totalpriceincludingvat = invoice.TotalPriceIncludingVat.ToDecimalNullSafe(),
                Totalvat = invoice.TotalVat.ToDecimalNullSafe(),
                Vatrate = invoice.VatRate,
                Countryid = invoice.CountryId,
                Cityid = invoice.CityId,
                Districtid = invoice.DistrictId,
                Address = invoice.Address,
                Exchangerate = invoice.ExchangeRate.ToDecimalNullSafe(),
                Createinvoiceserviceresponse = invoice.CreateInvoiceServiceResponse,
                Cancelinvoiceserviceresponse = invoice.CancelInvoiceServiceResponse
            }
            : null;

        public static List<CommonModels.ReservationInvoice> Map(this List<Invoice> invoices)
        {
            var _invoices = new List<CommonModels.ReservationInvoice>();

            if (invoices != null && invoices.Count != 0)
                foreach (var invoice in invoices)
                    _invoices.Add(invoice.Map());

            return _invoices;
        }
    }
}
