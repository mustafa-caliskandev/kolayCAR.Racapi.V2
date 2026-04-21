using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KolayCARProvider = KolayCAR.Broker.API.Providers.KolayCAR;

namespace KolayCAR.Broker.API.Services
{
    public interface IInvoiceService
    {
        Task<ReservationInvoice> GetReservationInvoice(int id);
        Task<List<ReservationInvoice>> GetReservationInvoiceList(bool isOnlyActiveInvoice);
        Task<ServiceResponseBase> CreateReservationInvoice(string reservationNumber, string customerEmail);
        Task<ServiceResponseBase> CancelReservationInvoice(string reservationNumber, string customerEmail);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IReservationService _reservationService;
        private readonly BrokerContext _context;

        public InvoiceService(
            IConfigurationService configurationService,
            IReservationService reservationService,
            BrokerContext context)
        {
            _configurationService = configurationService;
            _reservationService = reservationService;
            _context = context;
        }

        public async Task<ServiceResponseBase> CreateReservationInvoice(string reservationNumber, string customerEmail)
        {
            var dbInvoice = await _context.Invoice.FirstOrDefaultAsync(x => x.Reservationnumber == reservationNumber && x.Customeremail == customerEmail);

            if (dbInvoice != null && !string.IsNullOrEmpty(dbInvoice.Accountinginvoicenumber))
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = dbInvoice.Map()
                };

            bool isFailedInvoice = dbInvoice != null && string.IsNullOrEmpty(dbInvoice.Accountinginvoicenumber);

            var invoice = await InitReservationInvoice(reservationNumber, customerEmail);

            if (invoice == null)
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "The relevant reservation was not found!"
                };

            var serviceResult = await PostInvoiceToExternalService(invoice);
            invoice = serviceResult.Data as ReservationInvoice;

            if (isFailedInvoice)
                invoice.Id = dbInvoice.Id;

            dbInvoice = invoice.Map();

            //Önceden başarısız oldu ve tekrar deneniyorsa update edilir. Önceden denenmediyse isnert edilir
            if (!isFailedInvoice)
            {
                await _context.Invoice.AddAsync(dbInvoice);
                await _context.SaveChangesAsync();
                invoice.Id = dbInvoice.Id;
            }
            else
            {
                _context.Entry(dbInvoice).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return serviceResult;
        }

        public async Task<ServiceResponseBase> CancelReservationInvoice(string reservationNumber, string customerEmail)
        {
            var dbInvoice = await _context.Invoice.FirstOrDefaultAsync(x => x.Reservationnumber == reservationNumber && x.Customeremail == customerEmail);

            if (dbInvoice == null)
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "Reservation invoice does not exist!"
                };

            var invoice = dbInvoice.Map();

            if (invoice.InvoiceStatusType == InvoiceStatusTypes.Cancelled)
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = invoice
                };

            var serviceResult = await PostCancelInvoiceToExternalService(invoice);

            dbInvoice = (serviceResult.Data as ReservationInvoice).Map();

            _context.Entry(dbInvoice).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return serviceResult;
        }

        private async Task<ServiceResponseBase> PostInvoiceToExternalService(ReservationInvoice invoice)
        {
            Serilog.Log.Fatal("{@PostInvoiceObject}", invoice);

            var configurations = await _configurationService.GetConfigurations();
            if (configurations != null)
            {
                var externalInvoiceType = configurations.ExternalInvoiceType;

                IInvoiceProvider invoiceProvider;

                switch (externalInvoiceType)
                {
                    default:
                    case ExternalInvoiceType.None:
                        {
                            return new ServiceResponseBase
                            {
                                Success = false,
                                Message = "No invoice service defined!",
                                ServiceCode = null,
                                ServiceMessage = null
                            };
                        }
                    case ExternalInvoiceType.KolayCAR:
                        {
                            invoiceProvider = new KolayCARProvider.InvoiceProvider();
                            break;
                        }
                }

                var result = await invoiceProvider.SendReservationInvoice(configurations, invoice);

                Serilog.Log.Fatal("{@PostInvoiceResponse}", result);

                return result;
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "An unexpected error occurred while transferring the invoice!",
                ServiceCode = null,
                ServiceMessage = null
            };
        }

        private async Task<ServiceResponseBase> PostCancelInvoiceToExternalService(ReservationInvoice invoice)
        {
            Serilog.Log.Fatal("{@PostCancelInvoiceObject}", invoice);

            var configurations = await _configurationService.GetConfigurations();
            if (configurations != null)
            {
                var externalInvoiceType = configurations.ExternalInvoiceType;

                IInvoiceProvider invoiceProvider;

                switch (externalInvoiceType)
                {
                    default:
                    case ExternalInvoiceType.None:
                        {
                            return new ServiceResponseBase
                            {
                                Success = false,
                                Message = "No invoice service defined!",
                                ServiceCode = null,
                                ServiceMessage = null
                            };
                        }
                    case ExternalInvoiceType.KolayCAR:
                        {
                            invoiceProvider = new KolayCARProvider.InvoiceProvider();
                            break;
                        }
                }

                var result = await invoiceProvider.SendCancelReservationInvoice(configurations, invoice);

                Serilog.Log.Fatal("{@PostCancelInvoiceResponse}", result);

                return result;
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "An unexpected error occurred while transferring the invoice!",
                ServiceCode = null,
                ServiceMessage = null
            };
        }

        private async Task<ReservationInvoice> InitReservationInvoice(string reservationNumber, string customerEmail)
        {
            if (!string.IsNullOrEmpty(reservationNumber) && !string.IsNullOrEmpty(customerEmail))
            {
                var configurations = await _configurationService.GetConfigurations();

                if (configurations != null && configurations.ExternalInvoiceType != ExternalInvoiceType.None)
                {
                    var reservation = await _reservationService.GetReservation(new GetReservationsRequest
                    {
                        CustomerEmail = customerEmail,
                        ReservationNumber = reservationNumber
                    });

                    var dbExchangeRates = await _context.Exchangerates.ToListAsync();
                    var exchangeRates = dbExchangeRates.Map();

                    if (reservation != null &&
                        reservation.ReservationStatusType != ReservationStatusTypes.Cancelled &&
                        exchangeRates != null)
                    {
                        int vatRate = 18;
                        int piece = 1;

                        return new ReservationInvoice
                        {
                            InvoiceDate = DateTime.Now,
                            InvoiceType = InvoiceTypes.NotSet,
                            InvoiceStatusType = InvoiceStatusTypes.NotSent,
                            ReservationNumber = reservation.ReservationNumber,
                            CustomerType = !string.IsNullOrEmpty(reservation.CustomerTitle) ? CustomerTypes.Corporate : CustomerTypes.Individual,
                            CustomerName = reservation.CustomerName,
                            CustomerSurname = reservation.CustomerSurname,
                            CompanyTitle = reservation.CustomerTitle,
                            CustomerEmail = reservation.CustomerMail,
                            CustomerPhoneNumber = reservation.CustomerPhone,
                            CustomerIdentityNumber = reservation.CustomerIdentityNumber,
                            TaxOffice = reservation.CustomerTaxOffice,
                            TaxNumber = reservation.CustomerTaxNumber,
                            ServiceName = $"1~{reservation.PaidAmount}~{vatRate}",
                            Piece = piece,
                            CurrencyType = reservation.CurrencyType,
                            UnitPriceExcludingVat = CalculationHelper.SubtractVATFromPrice(reservation.PaidAmount, vatRate),
                            PriceExcludingVat = CalculationHelper.SubtractVATFromPrice(reservation.PaidAmount, vatRate) * piece,
                            TotalPriceIncludingVat = reservation.PaidAmount,
                            TotalVat = CalculationHelper.CalculateVATAmount(reservation.PaidAmount, vatRate),
                            VatRate = vatRate,
                            CountryId = 190,
                            CityId = 34,
                            DistrictId = 994,
                            Address = reservation.CustomerAddress,
                            ExchangeRate = exchangeRates.FirstOrDefault(x => x.CurrencyType == reservation.CurrencyType).ExchangeRate
                        };
                    }
                }
            }

            return null;
        }

        public async Task<ReservationInvoice> GetReservationInvoice(int id)
        {
            var dbInvoice = await _context.Invoice.FirstOrDefaultAsync(x => x.Id == id);

            return dbInvoice?.Map();
        }

        public async Task<List<ReservationInvoice>> GetReservationInvoiceList(bool isOnlyActiveInvoice)
        {
            var dbInvoices = !isOnlyActiveInvoice ?
                await _context.Invoice.ToListAsync() :
                await _context.Invoice.Where(x => x.Status != (int)InvoiceStatusTypes.Cancelled).ToListAsync();

            return dbInvoices?.Map();
        }
    }
}
