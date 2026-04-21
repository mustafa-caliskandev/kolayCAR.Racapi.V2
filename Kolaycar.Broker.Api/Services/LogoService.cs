using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Logo;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Invoice = KolayCAR.Broker.Domain.Models.Logo.Invoice;

namespace KolayCAR.Broker.API.Services
{
    public interface ILogoService
    {
        Task<PaymentListResponse> GetPaymentList(GetPaymentListRequest getPaymentListRequest);
        Task<InvoiceListResponse> GetInvoiceList(GetInvoiceListRequest getInvoiceListRequest);
        Task<List<VendorListResponse>> GetVendors();
    }

    public class LogoService : ILogoService
    {
        private readonly BrokerContext _context;
        private readonly AppSettings _appSettings;
        private readonly IConfigurationService _configurationService;
        public LogoService(BrokerContext context, IConfigurationService configurationService, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _configurationService = configurationService;
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Fatura servisi
        /// </summary>
        /// <param name="getInvoiceListRequest"></param>
        /// <returns></returns>
        public async Task<InvoiceListResponse> GetInvoiceList(GetInvoiceListRequest getInvoiceListRequest)
        {
            if (getInvoiceListRequest == null || string.IsNullOrEmpty(getInvoiceListRequest.StartDate) || string.IsNullOrEmpty(getInvoiceListRequest.EndDate) || !getInvoiceListRequest.StartDate.IsDateTime() || !getInvoiceListRequest.EndDate.IsDateTime())
            {
                return new InvoiceListResponse
                {
                    Success = false,
                    Message = "Sorgu parametreleri uygun formatta değil!"
                };
            }

            try
            {
                var InvoiceList = new List<Invoice>();

                #region Dbden verilerin çekilmesi işlemi
                var resultPayments = await _context.Payments.FromSqlRaw("EXEC GETINVOICELIST @startDate = {0}, @endDate = {1}, @agencyId = {2}",
                                getInvoiceListRequest.StartDate.ToDateTimeNullSafe(),
                                getInvoiceListRequest.EndDate.ToDateTimeNullSafe(),
                                string.IsNullOrEmpty(getInvoiceListRequest.BranchID) || getInvoiceListRequest.BranchID == "0" ?
                                    0 :
                                    getInvoiceListRequest.BranchID.ToIntNullSafe()).ToListAsync();
                #endregion

                if (resultPayments != null && resultPayments.Count > 0)
                {
                    int i = 1;
                    foreach (var item in resultPayments)
                    {
                        i = 1;

                        #region Ek ürün veya tek yön faturalarını parametre ile ayrı isteme opsiyoneli eklendiği için fatura toplam tutarı ona göre hesaplamak gerekiyor.
                        var invoiceAmount = getInvoiceListRequest.ShowAdditionalProductInvoices.ToBoolNullSafe() && getInvoiceListRequest.ShowDropInvoices.ToBoolNullSafe() ?
                                                    item.Amount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/ :
                                                    getInvoiceListRequest.ShowDropInvoices.ToBoolNullSafe() ?
                                                        item.RentAmount.ToDecimalNullSafe() + item.DropAmount.ToDecimalNullSafe() :
                                                        getInvoiceListRequest.ShowAdditionalProductInvoices.ToBoolNullSafe() ?
                                                            item.RentAmount.ToDecimalNullSafe() + item.ExtraAmount.ToDecimalNullSafe() :
                                                            item.RentAmount.ToDecimalNullSafe();
                        #endregion

                        if (item.ResStatusId != -4)
                        {
                            #region Kiralama faturası
                            InvoiceList.Add(GetRentInvoice(item, i, invoiceAmount));//kiralama faturasının listeye eklenmesi 
                            #endregion

                            #region Tek yön ücreti faturası
                            i++;
                            if (getInvoiceListRequest.ShowDropInvoices && item.DropAmount > 0)
                            {
                                InvoiceList.Add(GetOneWayInvoice(item, i, invoiceAmount));
                                i++;
                            }
                            #endregion

                            #region Ek ürünler fatuası
                            if (getInvoiceListRequest.ShowAdditionalProductInvoices && item.ExtraAmount > 0)
                            {
                                var resExtras = await _context.Reservationextra.Where(x => x.Resno == item.ReservationNumber).ToListAsync();

                                if (resExtras.Count > 0)
                                {
                                    foreach (var extraItem in resExtras)
                                    {
                                        InvoiceList.Add(GetAdditionalProductInvoice(extraItem, item, i, invoiceAmount));
                                        i++;
                                    }
                                }
                            }
                            #endregion

                            #region Kupon indirimi varsa gönderilecek kalem
                            if (item.CouponDiscount.ToDecimalNullSafe() > 0)
                            {
                                InvoiceList.Add(GetCouponInvoice(item, i, invoiceAmount));
                                i++;
                            }
                            #endregion

                            #region Vade farkı varsa gönderilecek kalem
                            if ((item.InstallmentCommissionAmount ?? 0) > 0)
                            {
                                InvoiceList.Add(GetInterestRateDifferenceInvoice(item, i, invoiceAmount));
                                i++;
                            }
                            #endregion

                            #region Servis ücreti faturası 'Not: Bu bölüm iptal edildi.'
                            //if (item.ServiceCharge.ToDecimalNullSafe() > 0)
                            //{
                            //    InvoiceList.Add(await GetServiceCharge(item, i));
                            //    i++;
                            //}
                            #endregion
                        }
                        #region İptal durumu için fatura oluşuyordu. Bu durum iptal edildi.
                        //else
                        //{
                        //    #region İptal faturası
                        //    InvoiceList.Add(GetCancelInvoice(item, i));
                        //    i++;
                        //    #endregion
                        //} 
                        #endregion
                    }
                }

                return new InvoiceListResponse
                {
                    Success = true,
                    InvoiceList = InvoiceList,
                    Message = resultPayments != null && resultPayments.Count > 0 ? "" : "Belirtilen tarih aralığında veri bulunamadı."
                };
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@GetInvoiceListError}", ex.Message);
                return new InvoiceListResponse
                {
                    Success = false,
                    Message = "Bir hata oluştu. Lütfen yetkili ile iletişime geçiniz!"
                };
            }
        }

        /// <summary>
        /// Tahislat servisi
        /// </summary>
        /// <param name="getPaymentListRequest"></param>
        /// <returns></returns>
        public async Task<PaymentListResponse> GetPaymentList(GetPaymentListRequest getPaymentListRequest)
        {
            if (getPaymentListRequest == null || string.IsNullOrEmpty(getPaymentListRequest.StartDate) || string.IsNullOrEmpty(getPaymentListRequest.EndDate) || !getPaymentListRequest.StartDate.IsDateTime() || !getPaymentListRequest.EndDate.IsDateTime())
            {
                return new PaymentListResponse
                {
                    Success = false,
                    Message = "Sorgu parametreleri uygun formatta değil!",
                };
            }

            try
            {
                #region Dbden verilerin çekilmesi işlemi
                var result = await _context.Payments.FromSqlRaw("EXEC GETPAYMENTLIST @startDate = {0}, @endDate = {1}, @agencyId = {2}, @canceledOnly = {3}",
                                    getPaymentListRequest.StartDate.ToDateTimeNullSafe(),
                                    getPaymentListRequest.EndDate.ToDateTimeNullSafe(),
                                    string.IsNullOrEmpty(getPaymentListRequest.BranchID) ||
                                                    getPaymentListRequest.BranchID == "0" ? 0 :
                                                    getPaymentListRequest.BranchID.ToIntNullSafe(),
                                    getPaymentListRequest.CancelledOnly).ToListAsync();
                #endregion

                return new PaymentListResponse
                {
                    Success = true,
                    PaymentList = result.Count > 0 ? Map(result) : new List<Domain.Models.Logo.Payment>(),
                    Message = result != null && result.Count > 0 ? "" : "Belirtilen tarih aralığında veri bulunamadı."
                };
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@GetPaymentListError}", ex.Message);
                return new PaymentListResponse
                {
                    Success = false,
                    Message = "Bir hata oluştu. Lütfen yetkili ile iletişime geçiniz!"
                };
            }
        }

        /// <summary>
        /// Tedarikçi listesi
        /// </summary>
        /// <returns>VendorListResponse</returns>
        public async Task<List<VendorListResponse>> GetVendors()
        {
            var resultVendors = await _context.Vendor.ToListAsync();

            return Map(resultVendors);
        }

        /// <summary>
        /// Tahsilat servisi için modelleme
        /// </summary>
        /// <param name="paymentDtos"></param>
        /// <returns>Payment</returns>
        private List<KolayCAR.Broker.Domain.Models.Logo.Payment> Map(List<PaymentDto> paymentDtos)
        {
            var _paymentList = new List<KolayCAR.Broker.Domain.Models.Logo.Payment>();

            foreach (var item in paymentDtos)
            {
                _paymentList.Add(new Domain.Models.Logo.Payment
                {
                    PaymentDate = item.PaymentDate,
                    PaymentID = item.ResId.ToLongNullSafe(),
                    PaymentRefNo = item.PaymentRefNo,
                    ContractNumber = item.ContractNumber,
                    CustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                    CustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                    TCNumber = item.TCNumber,
                    Address = item.CustomerAddress,
                    Mail = item.Mail,
                    Country = !string.IsNullOrEmpty(item.Country) ? item.Country : "Türkiye",
                    City = !string.IsNullOrEmpty(item.City) ? item.City : "İstanbul",
                    District = !string.IsNullOrEmpty(item.District) ? item.District : "Ataşehir",
                    CustomerTitle = item.CustomerTitle,
                    TaxNumber = item.TaxNumber,
                    TaxOffice = item.TaxOffice,
                    ReservationNumber = item.ReservationNumber,
                    Amount = Math.Round(item.Amount.ToDecimalNullSafe(), 4),
                    TaxAmount = Math.Round(item.Amount.ToDecimalNullSafe() * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/),
                    Bank = item.Bank,
                    BankAccountCode = item.BankAccountCode,
                    BankAccountingCode = item.BankAccountingCode == item.BankAccountCode ? "102.03.028" : item.BankAccountingCode,
                    CreditCard = item.CreditCard,
                    CreditCardBank = item.CreditCardBank,
                    InstallmentCount = item.InstallmentCount.ToIntNullSafe(),
                    VendorId = item.VendorId.ToIntNullSafe(),
                    Branch = item.Branch,
                    Currency = item.Currency.ToIntNullSafe(),
                    //RefundAmount = item.CurrentResStatus == -4 ? Math.Round(item.CancellationRefundAmount.ToDecimalNullSafe(), 4) : 0,
                    IsCancel = item.CurrentResStatus == -4
                });

                // İptal edildi((?))
                if (item.CurrentResStatus == -4)
                {
                    var cancellationAmount =
                        item.CancellationRefundAmount.ToDecimalNullSafe() > 0 ?
                            item.CancellationRefundAmount.ToDecimalNullSafe() :
                            item.Amount.ToDecimalNullSafe();
                    _paymentList.Add(new Domain.Models.Logo.Payment
                    {
                        PaymentDate = item.CancelDate.ToDateTimeNullSafe(),
                        PaymentID = (item.ResId.ToLongNullSafe() * 10) + 1,
                        PaymentRefNo = item.PaymentRefNo,
                        ContractNumber = item.ContractNumber,
                        CustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                        CustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                        TCNumber = item.TCNumber,
                        Address = item.CustomerAddress,
                        Mail = item.Mail,
                        Country = !string.IsNullOrEmpty(item.Country) ? item.Country : "Türkiye",
                        City = !string.IsNullOrEmpty(item.City) ? item.City : "İstanbul",
                        District = !string.IsNullOrEmpty(item.District) ? item.District : "Ataşehir",
                        CustomerTitle = item.CustomerTitle,
                        TaxNumber = item.TaxNumber,
                        TaxOffice = item.TaxOffice,
                        ReservationNumber = item.ReservationNumber,
                        Amount = Math.Round(cancellationAmount, 4) * -1,
                        TaxAmount = Math.Round(cancellationAmount * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/) * -1,
                        Bank = item.Bank,
                        BankAccountCode = item.BankAccountCode,
                        BankAccountingCode = item.BankAccountingCode == item.BankAccountCode ? "102.03.028" : item.BankAccountingCode,
                        CreditCard = item.CreditCard,
                        CreditCardBank = item.CreditCardBank,
                        InstallmentCount = item.InstallmentCount.ToIntNullSafe(),
                        VendorId = item.VendorId.ToIntNullSafe(),
                        Branch = item.Branch,
                        Currency = item.Currency.ToIntNullSafe(),
                        IsCancel = item.CurrentResStatus == -4
                    });
                }

                //Kısmi iade varsa
                if (item.TLAmount.ToDecimalNullSafe() > 0)
                {
                    _paymentList.Add(new Domain.Models.Logo.Payment
                    {
                        PaymentDate = item.CancelDate.ToDateTimeNullSafe(),
                        PaymentID = item.ResId.ToLongNullSafe(),
                        PaymentRefNo = item.PaymentRefNo,
                        ContractNumber = item.ContractNumber,
                        CustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                        CustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                        TCNumber = item.TCNumber,
                        Address = item.CustomerAddress,
                        Mail = item.Mail,
                        Country = !string.IsNullOrEmpty(item.Country) ? item.Country : "Türkiye",
                        City = !string.IsNullOrEmpty(item.City) ? item.City : "İstanbul",
                        District = !string.IsNullOrEmpty(item.District) ? item.District : "Ataşehir",
                        CustomerTitle = item.CustomerTitle,
                        TaxNumber = item.TaxNumber,
                        TaxOffice = item.TaxOffice,
                        ReservationNumber = item.ReservationNumber,
                        Amount = Math.Round(item.TLAmount.ToDecimalNullSafe(), 4) * -1,
                        TaxAmount = Math.Round(item.TLAmount.ToDecimalNullSafe() * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/) * -1,
                        Bank = item.Bank,
                        BankAccountCode = item.BankAccountCode,
                        BankAccountingCode = item.BankAccountingCode == item.BankAccountCode ? "102.03.028" : item.BankAccountingCode,
                        CreditCard = item.CreditCard,
                        CreditCardBank = item.CreditCardBank,
                        InstallmentCount = item.InstallmentCount.ToIntNullSafe(),
                        VendorId = item.VendorId.ToIntNullSafe(),
                        Branch = item.Branch,
                        Currency = item.Currency.ToIntNullSafe(),
                        IsCancel = item.CurrentResStatus == -4
                    });
                }
            }

            return _paymentList.Where(pl => pl.Amount > 0).ToList();
        }

        /// <summary>
        /// Tedarikçi listesi
        /// </summary>
        /// <param name="vendors"></param>
        /// <returns>List<VendorListResponse></returns>
        private static List<VendorListResponse> Map(List<KolayCAR.Broker.API.Models.Vendor> vendors)
        {
            var vendorList = new List<VendorListResponse>();

            foreach (var item in vendors)
            {
                vendorList.Add(new VendorListResponse
                {
                    VendorId = item.Vendorid,
                    VendorName = item.Vendorname,
                    BankName = item.BankName,
                    BankBranchCode = item.BankBranchCode,
                    IBAN = item.IBAN,
                    AccountNumber = item.AccountNumber,
                    Address = item.Address,
                    Country = !string.IsNullOrEmpty(item.Country) ? item.Country : "Türkiye",
                    City = !string.IsNullOrEmpty(item.City) ? item.City : "İstanbul",
                    District = !string.IsNullOrEmpty(item.District) ? item.District : "Ataşehir",
                    Email = item.Email,
                    InvoiceOwner = item.InvoiceOwner == 1 ? "O" : item.InvoiceOwner == 2 ? "T" : "",
                    PersonalNumber = item.PersonalNumber.ReplaceWhitespace(string.Empty),
                    PhoneNumber = item.Phone,
                    TaxNumber = item.TaxNumber,
                    TaxOffice = item.TaxOffice,
                    Dbs = item.Dbs,
                    CompanyTitle = item.Companytitle
                });
            }

            return vendorList;
        }

        private static decimal CalculateVendorPrices(decimal price, decimal profitMarkup, int workingType)
        {
            return workingType == 1 ? Math.Round(price * ((100 - profitMarkup) / 100), 4) : Math.Round(price * (100 / (100 + profitMarkup)), 4);
        }

        private static Invoice GetCouponInvoice(PaymentDto item, int i, decimal invoiceAmount)
        {
            return new Invoice()
            {
                InvoiceDate = item.PaymentDate,
                InvoiceID = item.ContractNumber.ToIntNullSafe(),
                ReservationNumber = item.ReservationNumber,
                CustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                CustomerTitle = item.CustomerTitle,
                StockCode = "-4",//1: Kiralama, 2: Ek ürün, 3: Tek yön, 4: Kupon
                Mail = item.Mail,
                CustomerCode = "",
                Plate = "",
                RowDescription = "Kupon indirimi",//Kalemlere göre değişecek
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = item.TaxOffice,
                TaxNumber = item.TaxNumber,
                Address = item.CustomerAddress,
                City = item.City,
                District = item.District,
                InvoiceNumber = item.ResId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = item.TaxOffice,
                InvoiceDescription = "",
                RowID = i,
                ProcessType = "",

                Discount = Math.Round(0M, 4),//Math.Round(item.CouponDiscount.ToDecimalNullSafe(), 4),
                InvoiceAmount = Math.Round(0M, 4),//Math.Round(invoiceAmount, 4),
                InvoiceTaxAmount = Math.Round(0M, 4),//Math.Round(invoiceAmount * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/, 4),
                InvoiceTotalAmount = Math.Round(0M, 4),//Math.Round(invoiceAmount, 4),

                UnitPrice = Math.Round(item.CouponDiscount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                TaxRate = (GetTaxRate(item.ReturnDate) / 100)/*18*/,
                RowAmount = Math.Round(item.CouponDiscount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                RowTaxAmount = Math.Round((item.CouponDiscount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/) * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/, 4),
                RowTotalAmount = Math.Round(item.CouponDiscount.ToDecimalNullSafe(), 4) * -1,

                //Tedarikçi kar - komisyon ve hesaplama
                VendorPrice = CalculateVendorPrices(item.RentAmount.ToDecimalNullSafe(), item.ProfitMarkupRental.ToDecimalNullSafe(), item.RentalWorkingType.ToIntNullSafe()),
                VendorWorkingType = item.RentalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(item.ProfitMarkupRental.ToDecimalNullSafe(), 2),
                IsPaymentReceived = true,

                Description = item.Description,
                ContractCustomerCode = "",
                TCNumber = item.TCNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = item.CustomerTitle,
                ContractTaxNumber = item.TaxNumber,
                ContractTaxOffice = item.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(item.Country) ? item.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(item.City) ? item.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(item.District) ? item.District : "Ataşehir",
                ContractCustomerAddress = item.CustomerAddress,
                ContractCustomerTCNumber = item.TCNumber,
                PickupLocation = item.PickupLocation,
                PickupLocationId = item.PickupLocationId.ToIntNullSafe(),
                PickupDate = item.PickupDate,
                ReturnLocation = item.ReturnLocation,
                ReturnLocationId = item.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = item.ReturnDate,
                VendorId = item.VendorId.ToIntNullSafe(),
                RentalDuration = item.RentalDuration.ToIntNullSafe(),
                IsCancelled = item.CurrentResStatus == -4,
                VendorCommissionInvoice = item.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="i"></param>
        /// <param name="invoiceAmount"> KDV hariç kiralama tutarı</param>
        /// <returns></returns>
        private static Invoice GetRentInvoice(PaymentDto item, int i, decimal invoiceAmount)
        {
            var installmentCommissionAmount = Math.Round(item.InstallmentCommissionAmount.ToDecimalNullSafe(), 4);
            // KDV hariç vade farkının düşülmüş hali
            invoiceAmount =
                item.InstallmentCount == 0
                    ? Math.Round(invoiceAmount - CalculateAmountWithoutTax(installmentCommissionAmount, item.ReturnDate), 4) // Tek çekimde komisyon bedeli fatura tutarından düşülüyor
                    : Math.Round(invoiceAmount, 4);
            var invoiceTaxAmount = Math.Round(invoiceAmount * (GetTaxRate(item.ReturnDate) / 100), 4);
            var invoiceTotalAmount = invoiceAmount + invoiceTaxAmount;

            var unitPrice =
                item.InstallmentCount == 0
                    ? Math.Round(CalculateAmountWithoutTax(item.RentAmount.ToDecimalNullSafe(), item.ReturnDate) - CalculateAmountWithoutTax(installmentCommissionAmount, item.ReturnDate), 4)
                    : Math.Round(CalculateAmountWithoutTax(item.RentAmount.ToDecimalNullSafe(), item.ReturnDate), 4);

            return new Invoice()
            {
                InvoiceDate = item.PaymentDate,
                InvoiceID = item.ContractNumber.ToIntNullSafe(),
                ReservationNumber = item.ReservationNumber,
                CustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                CustomerTitle = item.CustomerTitle,
                StockCode = "-1",//1: Kiralama, 2: Ek ürün, 3: Tek yön
                Mail = item.Mail,
                CustomerCode = "",
                Plate = "",
                RowDescription = "Kiralama",//Kalemlere göre değişecek
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = item.TaxOffice,
                TaxNumber = item.TaxNumber,
                Address = item.CustomerAddress,
                City = item.City,
                District = item.District,
                InvoiceNumber = item.ResId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = item.TaxOffice,
                InvoiceDescription = "",
                RowID = i,
                ProcessType = "",

                Discount = Math.Round(item.CouponDiscount.ToDecimalNullSafe(), 4),
                InvoiceAmount = Math.Round(invoiceAmount, 4),
                InvoiceTaxAmount = Math.Round(invoiceTaxAmount, 4),
                InvoiceTotalAmount = Math.Round(invoiceTotalAmount, 4),

                UnitPrice = unitPrice,
                TaxRate = (GetTaxRate(item.ReturnDate)) /*18*/,
                RowAmount = unitPrice,
                RowTaxAmount = Math.Round((item.RentAmount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)) * (GetTaxRate(item.ReturnDate) / 100), 4),
                RowTotalAmount = Math.Round(item.RentAmount.ToDecimalNullSafe(), 4),

                //Tedarikçi kar - komisyon ve hesaplama
                VendorPrice = CalculateVendorPrices(item.RentAmount.ToDecimalNullSafe(), item.ProfitMarkupRental.ToDecimalNullSafe(), item.RentalWorkingType.ToIntNullSafe()),
                VendorWorkingType = item.RentalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(item.ProfitMarkupRental.ToDecimalNullSafe(), 2),
                IsPaymentReceived = true,

                Description = item.Description,
                ContractCustomerCode = "",
                TCNumber = item.TCNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = item.CustomerTitle,
                ContractTaxNumber = item.TaxNumber,
                ContractTaxOffice = item.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(item.Country) ? item.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(item.City) ? item.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(item.District) ? item.District : "Ataşehir",
                ContractCustomerAddress = item.CustomerAddress,
                ContractCustomerTCNumber = item.TCNumber,
                PickupLocation = item.PickupLocation,
                PickupLocationId = item.PickupLocationId.ToIntNullSafe(),
                PickupDate = item.PickupDate,
                ReturnLocation = item.ReturnLocation,
                ReturnLocationId = item.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = item.ReturnDate,
                VendorId = item.VendorId.ToIntNullSafe(),
                RentalDuration = item.RentalDuration.ToIntNullSafe(),
                IsCancelled = item.CurrentResStatus == -4,
                VendorCommissionInvoice = item.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }

        private static Invoice GetAdditionalProductInvoice(Reservationextra extraItem, PaymentDto item, int i, decimal invoiceAmount)
        {
            return new Invoice
            {
                StockCode = extraItem.Extraid.ToString(),//Ek ürün
                RowDescription = extraItem.Extraname,
                UnitPrice = Math.Round(extraItem.Amount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                RowAmount = Math.Round(extraItem.Amount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                RowTaxAmount = Math.Round((extraItem.Amount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/) * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/, 4),
                RowTotalAmount = Math.Round(extraItem.Amount.ToDecimalNullSafe(), 4),
                Discount = 0,

                //Tedarikçi kar - komisyon ve hesaplama
                VendorPrice = CalculateVendorPrices(extraItem.Amount.ToDecimalNullSafe(), item.ProfitMatkupAdditionalProducts.ToDecimalNullSafe(), item.AdditionalWorkingType.ToIntNullSafe()),
                VendorWorkingType = item.AdditionalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(item.ProfitMatkupAdditionalProducts.ToDecimalNullSafe(), 4),
                IsPaymentReceived = !item.ExtraPricePayOnDelivery.ToBoolNullSafe(),

                InvoiceAmount = Math.Round(invoiceAmount / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                InvoiceTaxAmount = Math.Round(invoiceAmount * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/, 4),
                InvoiceTotalAmount = Math.Round(invoiceAmount, 4),

                RowID = i,
                InvoiceDate = item.PaymentDate,
                InvoiceID = item.ContractNumber.ToIntNullSafe(),
                ReservationNumber = item.ReservationNumber,
                CustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                CustomerTitle = item.CustomerTitle,
                Mail = item.Mail,
                CustomerCode = "",
                Plate = "",
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = item.TaxOffice,
                TaxNumber = item.TaxNumber,
                Address = item.CustomerAddress,
                City = item.City,
                District = item.District,
                InvoiceNumber = item.ResId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = item.TaxOffice,
                InvoiceDescription = "",
                ProcessType = "",
                TaxRate = (GetTaxRate(item.ReturnDate))/*18*/,
                Description = item.Description,
                ContractCustomerCode = "",
                TCNumber = item.TCNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = item.CustomerTitle,
                ContractTaxNumber = item.TaxNumber,
                ContractTaxOffice = item.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(item.Country) ? item.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(item.City) ? item.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(item.District) ? item.District : "Ataşehir",
                ContractCustomerAddress = item.CustomerAddress,
                ContractCustomerTCNumber = item.TCNumber,
                PickupLocation = item.PickupLocation,
                PickupLocationId = item.PickupLocationId.ToIntNullSafe(),
                PickupDate = item.PickupDate,
                ReturnLocation = item.ReturnLocation,
                ReturnLocationId = item.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = item.ReturnDate,
                VendorId = item.VendorId.ToIntNullSafe(),
                RentalDuration = item.RentalDuration.ToIntNullSafe(),
                IsCancelled = item.CurrentResStatus == -4,
                VendorCommissionInvoice = item.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }

        private static Invoice GetOneWayInvoice(PaymentDto item, int i, decimal invoiceAmount)
        {
            return new Invoice
            {
                StockCode = "-2",//Tek yön
                RowDescription = "Tek yön",
                UnitPrice = Math.Round(item.DropAmount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                RowAmount = Math.Round(item.DropAmount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                RowTaxAmount = Math.Round((item.DropAmount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/) * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/, 4),
                RowTotalAmount = Math.Round(item.DropAmount.ToDecimalNullSafe(), 4),
                Discount = 0,

                InvoiceAmount = Math.Round(invoiceAmount / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                InvoiceTaxAmount = Math.Round(invoiceAmount * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/, 4),
                InvoiceTotalAmount = Math.Round(invoiceAmount, 4),

                //Tedarikçi kar - komisyon ve hesaplama
                VendorPrice = CalculateVendorPrices(item.DropAmount.ToDecimalNullSafe(), item.ProfitMatkupOneWay.ToDecimalNullSafe(), item.OneWayWorkingType.ToIntNullSafe()),
                VendorWorkingType = item.OneWayWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(item.ProfitMatkupOneWay.ToDecimalNullSafe(), 2),
                IsPaymentReceived = !item.OneWayFeePayOnDelivery.ToBoolNullSafe(),

                InvoiceDate = item.PaymentDate,
                InvoiceID = item.ContractNumber.ToIntNullSafe(),
                ReservationNumber = item.ReservationNumber,
                CustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                CustomerTitle = item.CustomerTitle,
                Mail = item.Mail,
                CustomerCode = "",
                Plate = "",
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = item.TaxOffice,
                TaxNumber = item.TaxNumber,
                Address = item.CustomerAddress,
                City = item.City,
                District = item.District,
                InvoiceNumber = item.ResId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = item.TaxOffice,
                InvoiceDescription = "",
                RowID = i,
                ProcessType = "",
                TaxRate = (GetTaxRate(item.ReturnDate))/*18*/,
                Description = item.Description,
                ContractCustomerCode = "",
                TCNumber = item.TCNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = item.CustomerTitle,
                ContractTaxNumber = item.TaxNumber,
                ContractTaxOffice = item.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(item.Country) ? item.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(item.City) ? item.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(item.District) ? item.District : "Ataşehir",
                ContractCustomerAddress = item.CustomerAddress,
                ContractCustomerTCNumber = item.TCNumber,
                PickupLocation = item.PickupLocation,
                PickupLocationId = item.PickupLocationId.ToIntNullSafe(),
                PickupDate = item.PickupDate,
                ReturnLocation = item.ReturnLocation,
                ReturnLocationId = item.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = item.ReturnDate,
                VendorId = item.VendorId.ToIntNullSafe(),
                RentalDuration = item.RentalDuration.ToIntNullSafe(),
                IsCancelled = item.CurrentResStatus == -4,
                VendorCommissionInvoice = item.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }

        private static Invoice GetInterestRateDifferenceInvoice(PaymentDto item, int i, decimal invoiceAmount)
        {
            return new Invoice()
            {
                InvoiceDate = item.PaymentDate,
                InvoiceID = item.ContractNumber.ToIntNullSafe(),
                ReservationNumber = item.ReservationNumber,
                CustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                CustomerTitle = item.CustomerTitle,
                StockCode = "-5",//1: Kiralama, 2: Ek ürün, 3: Tek yön, 4: Kupon, 5: Vade Farkı
                Mail = item.Mail,
                CustomerCode = "",
                Plate = "",
                RowDescription = "Araç kiralama banka komisyonu",//Kalemlere göre değişecek
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = item.TaxOffice,
                TaxNumber = item.TaxNumber,
                Address = item.CustomerAddress,
                City = item.City,
                District = item.District,
                InvoiceNumber = item.ResId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = item.TaxOffice,
                InvoiceDescription = "",
                RowID = i,
                ProcessType = "",

                Discount = Math.Round(0M, 4),
                InvoiceAmount = Math.Round(0M, 4),
                InvoiceTaxAmount = Math.Round(0M, 4),
                InvoiceTotalAmount = Math.Round(0M, 4),

                UnitPrice = Math.Round(item.InstallmentCommissionAmount.ToDecimalNullSafe(), 4),
                TaxRate = 0,
                RowAmount = Math.Round(item.InstallmentCommissionAmount.ToDecimalNullSafe(), 4),
                RowTaxAmount = 0,
                RowTotalAmount = Math.Round(item.InstallmentCommissionAmount.ToDecimalNullSafe(), 4),
                TaxExceptionCode = "350",

                //Tedarikçi kar - komisyon ve hesaplama
                VendorPrice = 0,
                VendorWorkingType = item.RentalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(0M, 4), // Gönderilmesi durumunda komisyonun bir kısmını tedarikçi karşılar anlamına geliyormuş (26.10.2023 - Fatih Bey)
                IsPaymentReceived = true,

                Description = item.Description,
                ContractCustomerCode = "",
                TCNumber = item.TCNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = item.CustomerTitle,
                ContractTaxNumber = item.TaxNumber,
                ContractTaxOffice = item.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(item.Country) ? item.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(item.City) ? item.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(item.District) ? item.District : "Ataşehir",
                ContractCustomerAddress = item.CustomerAddress,
                ContractCustomerTCNumber = item.TCNumber,
                PickupLocation = item.PickupLocation,
                PickupLocationId = item.PickupLocationId.ToIntNullSafe(),
                PickupDate = item.PickupDate,
                ReturnLocation = item.ReturnLocation,
                ReturnLocationId = item.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = item.ReturnDate,
                VendorId = item.VendorId.ToIntNullSafe(),
                RentalDuration = item.RentalDuration.ToIntNullSafe(),
                IsCancelled = item.CurrentResStatus == -4,
                VendorCommissionInvoice = item.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }

        private static Invoice GetServiceCharge(PaymentDto item, int i)
        {
            return new Invoice
            {
                StockCode = "-3",//Servis ücreti
                RowDescription = "Servis ücreti",
                UnitPrice = Math.Round(item.ServiceCharge.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                RowAmount = Math.Round(item.ServiceCharge.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                RowTaxAmount = Math.Round(item.ServiceCharge.ToDecimalNullSafe() * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/, 4),
                RowTotalAmount = Math.Round(item.ServiceCharge.ToDecimalNullSafe(), 4),
                Discount = 0,
                VendorPrice = 0,
                VendorWorkingType = null,
                VendorProfitMarkup = null,
                IsPaymentReceived = true,


                InvoiceDate = item.PaymentDate,
                InvoiceID = item.ContractNumber.ToIntNullSafe(),
                ReservationNumber = item.ReservationNumber,
                CustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                CustomerTitle = item.CustomerTitle,
                Mail = item.Mail,
                CustomerCode = "",
                Plate = "",
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = item.TaxOffice,
                TaxNumber = item.TaxNumber,
                Address = item.CustomerAddress,
                City = item.City,
                District = item.District,
                InvoiceNumber = item.ResId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = item.TaxOffice,
                InvoiceDescription = "",
                RowID = i,
                ProcessType = "",
                InvoiceAmount = Math.Round(item.Amount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                InvoiceTaxAmount = Math.Round(item.Amount.ToDecimalNullSafe() * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/, 4),
                InvoiceTotalAmount = Math.Round(item.Amount.ToDecimalNullSafe(), 4),
                TaxRate = (GetTaxRate(item.ReturnDate))/*18*/,
                Description = item.Description,
                ContractCustomerCode = "",
                TCNumber = item.TCNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = item.CustomerTitle,
                ContractTaxNumber = item.TaxNumber,
                ContractTaxOffice = item.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(item.Country) ? item.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(item.City) ? item.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(item.District) ? item.District : "Ataşehir",
                ContractCustomerAddress = item.CustomerAddress,
                ContractCustomerTCNumber = item.TCNumber,
                PickupLocation = item.PickupLocation,
                PickupLocationId = item.PickupLocationId.ToIntNullSafe(),
                PickupDate = item.PickupDate,
                ReturnLocation = item.ReturnLocation,
                ReturnLocationId = item.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = item.ReturnDate,
                VendorId = item.VendorId.ToIntNullSafe(),
                RentalDuration = item.RentalDuration.ToIntNullSafe(),
                IsCancelled = item.CurrentResStatus == -4
            };
        }

        private static Invoice GetCancelInvoice(PaymentDto item, int i)
        {
            return new Invoice
            {
                Cancel = "iptal/iade",
                CancellationDate = item.PaymentDate,
                CancellationRefundAmount = Math.Round(item.CancellationRefundAmount.ToDecimalNullSafe(), 4),
                IsPaymentReceived = true,

                InvoiceDate = item.PaymentDate,
                InvoiceID = item.ContractNumber.ToIntNullSafe(),
                ReservationNumber = item.ReservationNumber,
                CustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                CustomerTitle = item.CustomerTitle,
                StockCode = "-1",//1: Kiralama, 2: Ek ürün, 3: Tek yön
                Mail = item.Mail,
                CustomerCode = "",
                Plate = "",
                RowDescription = "Kiralama",//Kalemlere göre değişecek
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = item.TaxOffice,
                TaxNumber = item.TaxNumber,
                Address = item.CustomerAddress,
                City = item.City,
                District = item.District,
                InvoiceNumber = item.ResId.ToString(),
                EInvoiceNumber = "",
                Office = item.TaxOffice,
                InvoiceDescription = "",
                RowID = i,
                ProcessType = "",

                Discount = Math.Round(item.CouponDiscount.ToDecimalNullSafe(), 4),
                InvoiceAmount = Math.Round(item.Amount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                InvoiceTaxAmount = Math.Round(item.Amount.ToDecimalNullSafe() * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/, 4),
                InvoiceTotalAmount = Math.Round(item.Amount.ToDecimalNullSafe(), 4),

                UnitPrice = Math.Round(item.RentAmount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                TaxRate = (GetTaxRate(item.ReturnDate))/*18*/,
                RowAmount = Math.Round(item.RentAmount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/, 4),
                RowTaxAmount = Math.Round((item.RentAmount.ToDecimalNullSafe() / (1 + GetTaxRate(item.ReturnDate) / 100)/*(decimal)1.18*/) * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/, 4),
                RowTotalAmount = Math.Round(item.RentAmount.ToDecimalNullSafe(), 4),

                Description = item.Description,
                ContractCustomerCode = "",
                TCNumber = item.TCNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = item.CustomerTitle,
                ContractTaxNumber = item.TaxNumber,
                ContractTaxOffice = item.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(item.CustomerTitle) ? item.CustomerTitle : item.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(item.CustomerTitle) ? "" : item.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(item.Country) ? item.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(item.City) ? item.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(item.District) ? item.District : "Ataşehir",
                ContractCustomerAddress = item.CustomerAddress,
                ContractCustomerTCNumber = item.TCNumber,
                PickupLocation = item.PickupLocation,
                PickupLocationId = item.PickupLocationId.ToIntNullSafe(),
                PickupDate = item.PickupDate,
                ReturnLocation = item.ReturnLocation,
                ReturnLocationId = item.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = item.ReturnDate,
                VendorId = item.VendorId.ToIntNullSafe(),
                RentalDuration = item.RentalDuration.ToIntNullSafe(),

                VendorPrice = CalculateVendorPrices(item.RentAmount.ToDecimalNullSafe(), item.ProfitMarkupRental.ToDecimalNullSafe(), item.RentalWorkingType.ToIntNullSafe()),
                VendorWorkingType = item.RentalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(item.ProfitMarkupRental.ToDecimalNullSafe(), 2),
                IsCancelled = item.CurrentResStatus == -4
            };
        }

        /// <summary>
        /// Verilen tarihe göre ilgili KDV oranını döner
        /// </summary>
        /// <param name="invoiceDateTime"></param>
        /// <returns></returns>
        private static decimal GetTaxRate(DateTime? invoiceDateTime)
        {
            return invoiceDateTime < new DateTime(2023, 7, 10) ? 18 : 20;
        }

        private static decimal CalculateAmountWithoutTax(decimal amount, DateTime? invoiceDateTime)
        {
            //return ((amount * 100) / (100 + GetTaxRate(invoiceDateTime)));
            return (amount.ToDecimalNullSafe() / (1 + GetTaxRate(invoiceDateTime) / 100));
        }
    }
}
