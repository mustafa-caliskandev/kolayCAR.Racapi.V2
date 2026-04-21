using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Rentws;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static KolayCAR.Rentws.ServiceSoapClient;

namespace KolayCAR.Broker.API.Providers.KolayCAR
{
    public class InvoiceProvider : IInvoiceProvider
    {
        private readonly ServiceSoapClient _kolayCARRentService;

        public InvoiceProvider()
        {
            _kolayCARRentService = new ServiceSoapClient(EndpointConfiguration.ServiceSoap);
        }

        public async Task<ServiceResponseBase> SendCancelReservationInvoice(Configurations configurations, ReservationInvoice invoice)
        {
            var cancelInvoiceResponse = await _kolayCARRentService.POST_INVOICE_CANCELAsync(
                new POST_INVOICE_CANCELRequest(
                    new POST_INVOICE_CANCELRequestBody
                    {
                        APIKEY = configurations.KolayCARRentwsAPIKey,
                        APIPASSWORD = configurations.KolayCARRentwsAPIPassword,
                        MUHASEBEFATURANO = invoice.AccountingInvoiceNumber
                    }
                )
            );

            Serilog.Log.Fatal("{@KolayCARPostCancelInvoiceResponseObject}", cancelInvoiceResponse.Body.POST_INVOICE_CANCELResult);
            invoice.CancelInvoiceServiceResponse = cancelInvoiceResponse.Body.POST_INVOICE_CANCELResult;

            var invoiceResponseObject = JsonConvert.DeserializeObject<KolayCARInvoiceResponse.PostInvoiceResponse>(cancelInvoiceResponse.Body.POST_INVOICE_CANCELResult);

            if (invoiceResponseObject != null && invoiceResponseObject.Success)
            {
                invoice.InvoiceStatusType = InvoiceStatusTypes.Cancelled;
                invoice.CancelDate = DateTime.Now;

                return new ServiceResponseBase
                {
                    Success = true,
                    ServiceMessage = invoiceResponseObject.Message,
                    Data = invoice
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "KolayCAR servisinde problem meydana geldi!",
                Data = invoice
            };
        }

        public async Task<ServiceResponseBase> SendReservationInvoice(Configurations configurations, ReservationInvoice invoice)
        {
            var postInvoiceResponse = await _kolayCARRentService.POST_INVOICEAsync(
                new POST_INVOICERequest(
                    new POST_INVOICERequestBody
                    {
                        APIKEY = configurations.KolayCARRentwsAPIKey,
                        APIPASSWORD = configurations.KolayCARRentwsAPIPassword,
                        DOVIZID = (int)invoice.CurrencyType + 1,
                        KUR = invoice.ExchangeRate.ToDecimalNullSafe(),
                        MUSTERITIP = (int)invoice.CustomerType,
                        CARIAD = invoice.CustomerName,
                        CARISOYAD = invoice.CustomerSurname,
                        CARIUNVAN = invoice.CustomerType == CustomerTypes.Individual ? $"{invoice.CustomerName} {invoice.CustomerSurname}" : invoice.CompanyTitle,
                        TCVKN = invoice.CustomerType == CustomerTypes.Individual ?
                            !string.IsNullOrEmpty(invoice.CustomerIdentityNumber) ? invoice.CustomerIdentityNumber : "11111111111" :
                            !string.IsNullOrEmpty(invoice.TaxNumber) ? invoice.TaxNumber : "nodata",
                        TELEFON = invoice.CustomerPhoneNumber,
                        MAIL = invoice.CustomerEmail,
                        ADRES = invoice.Address,
                        VERGIDAIRESI = invoice.TaxOffice,
                        ULKEID = 0,
                        SEHIRID = 0,
                        ILCEID = 0,
                        KALEMBILGILERI = invoice.ServiceName
                    }
                )
            );

            Serilog.Log.Fatal("{@KolayCARPostInvoiceResponseObject}", postInvoiceResponse.Body.POST_INVOICEResult);
            invoice.CreateInvoiceServiceResponse = postInvoiceResponse.Body.POST_INVOICEResult;

            var invoiceResponseObject = JsonConvert.DeserializeObject<KolayCARInvoiceResponse.PostInvoiceResponse>(postInvoiceResponse.Body.POST_INVOICEResult);

            if (invoiceResponseObject != null && invoiceResponseObject.Success && !string.IsNullOrEmpty(invoiceResponseObject.MuhasebeFaturaNo))
            {
                invoice.AccountingInvoiceNumber = invoiceResponseObject.MuhasebeFaturaNo;
                invoice.InvoiceStatusType = InvoiceStatusTypes.Sent;
                invoice.InvoiceType = invoiceResponseObject.FaturaTipi == "E-arşiv" ? InvoiceTypes.EArchive : InvoiceTypes.EInvoice;

                return new ServiceResponseBase
                {
                    Success = true,
                    ServiceMessage = invoiceResponseObject.Message,
                    Data = invoice
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "KolayCAR servisinde problem meydana geldi!",
                Data = invoice
            };
        }
    }
}
