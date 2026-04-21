using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
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
    public interface ILogoServiceV2
    {
        Task<InvoiceListResponse> GetInvoices(GetInvoiceListRequest getInvoiceListRequest);
        Task<PaymentListResponse> GetPayments(GetPaymentListRequest getPaymentListRequest);
        Task<List<VendorListResponse>> GetVendors();
    }

    public class LogoServiceV2 : ILogoServiceV2
    {
        #region Tanımlamalar & Constructor

        private enum StockCodes
        {
            VendorAllowance = -1,
            OneWay = -2,
            Extras = -3,
            CouponDiscount = -4,
            BankInstallmentCommission = -5,
            BrokerAllowance = -6,
            BankCommission = -7,
            InstallmentFee = -8
        }

        private readonly IConfigurationService _configurationService;
        private readonly AppSettings _appSettings;
        private readonly BrokerContext _context;
        private readonly IVendorService _vendorService;
        private readonly ILocationService _locationService;
        public LogoServiceV2(IConfigurationService configurationService, IOptions<AppSettings> appSettings, BrokerContext context, IVendorService vendorService, ILocationService locationService)
        {
            _configurationService = configurationService;
            _appSettings = appSettings.Value;
            _context = context;
            _vendorService = vendorService;
            _locationService = locationService;
        }
        #endregion

        #region Fatura İşlemleri
        public async Task<InvoiceListResponse> GetInvoices(GetInvoiceListRequest getInvoiceListRequest)
        {
            #region Parametre hatası
            if (getInvoiceListRequest == null || string.IsNullOrEmpty(getInvoiceListRequest.StartDate) || string.IsNullOrEmpty(getInvoiceListRequest.EndDate) || !getInvoiceListRequest.StartDate.IsDateTime() || !getInvoiceListRequest.EndDate.IsDateTime())
            {
                return new InvoiceListResponse
                {
                    Success = false,
                    Message = "Sorgu parametreleri uygun formatta değil!"
                };
            }
            #endregion

            try
            {
                var invoiceList = new List<Invoice>();

                #region Veri tabanından datalar alınıyor
                var startDate = getInvoiceListRequest.StartDate.ToDateTimeNullSafe();
                var endDate = getInvoiceListRequest.EndDate.ToDateTimeNullSafe();
                var agencyId = getInvoiceListRequest.BranchID.ToIntNullSafe();

                var logoReservations = await _context.LogoReservations
                    .FromSqlRaw("EXEC GETINVOICELINES @startDate = {0}, @endDate = {1}, @agencyId = {2}", startDate, endDate, agencyId)
                    .ToListAsync();
                #endregion

                #region Veri bulunamadı
                if (logoReservations == null || logoReservations.Count <= 0)
                    return new InvoiceListResponse
                    {
                        Success = true,
                        InvoiceList = invoiceList,
                        Message = "Belirtilen tarih aralığında veri bulunamadı."
                    };
                #endregion

                foreach (var logoReservation in logoReservations)
                {
                    var location = await _locationService.GetLocationById(logoReservation.PickupLocationId);
                    var countryId = location?.Countryid ?? 190;
                    var vendor = await _vendorService.GetVendorById(logoReservation.VendorId);
                    if (logoReservation.ReservationStatusId == (getInvoiceListRequest.OnlyCanceledReservations ? -4 : -1) && logoReservation.DailyPrice.ToDecimalNullSafe() > 0)
                    {
                        var rowId = 1;

                        var rentalAmount = logoReservation.DailyPrice.ToDecimalNullSafe() * logoReservation.RentalDuration.ToIntNullSafe();
                        var rentalAmountWithoutTax = await CalculateAmountWithoutTax(rentalAmount, logoReservation.ReturnDate, countryId);

                        var invoiceTotalAmount = logoReservation.TotalPaymentPrice; // Ödemesi alınan tutar fatura toplamı
                        var invoiceAmount = await CalculateAmountWithoutTax(invoiceTotalAmount, logoReservation.ReturnDate, countryId); // Faturanın KDV hariç tutarı
                        var invoiceTaxAmount = invoiceTotalAmount - invoiceAmount; // Fatura KDV tutarı

                        decimal
                            bankCommissionAmount = 0M, // -5
                            brokerAllowance = 0M, // -6
                            brokerAllowanceWithoutTax = 0M, // -6
                            vendorAllowance = 0M, // -1
                            vendorAllowanceWithoutTax = 0M, // -1
                            couponDiscountAmount = logoReservation.CouponDiscountAmount.ToDecimalNullSafe(), // -4
                            couponDiscountPercent = 0M;

                        if (logoReservation.InstallmentCount == 0)
                        {
                            #region Tek çekim senaryosu

                            //bankCommissionAmount = CalculateBankCommissionAmount(invoiceTotalAmount,logoReservation.InstallmentCommission.ToDecimalNullSafe());
                            bankCommissionAmount = logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe();

                            brokerAllowance = CalculateBrokerAllowance(
                                logoReservation.TotalPaymentPrice - bankCommissionAmount,
                                logoReservation.CouponDiscountType,
                                logoReservation.CouponDiscountValue.ToDecimalNullSafe(),
                                logoReservation.RentalWorkingType,
                                logoReservation.ProfitMarkupRental.ToDecimalNullSafe());
                            brokerAllowanceWithoutTax = await CalculateAmountWithoutTax(brokerAllowance, logoReservation.ReturnDate, countryId);

                            vendorAllowance = rentalAmount - brokerAllowance - bankCommissionAmount;
                            vendorAllowanceWithoutTax = await CalculateAmountWithoutTax(vendorAllowance, logoReservation.ReturnDate, countryId);

                            couponDiscountPercent = (couponDiscountAmount / vendorAllowance) * 100;

                            #endregion
                        }
                        else
                        {
                            #region Taksitli çekim senaryosu

                            bankCommissionAmount = logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe();
                            vendorAllowance = CalculateVendorAllowance(rentalAmount, logoReservation.ProfitMarkupRental.ToDecimalNullSafe(),
                                logoReservation.RentalWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType.ToIntNullSafe(), logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe());
                            vendorAllowanceWithoutTax =
                                await CalculateAmountWithoutTax(vendorAllowance, logoReservation.ReturnDate, countryId);
                            brokerAllowance = rentalAmount - vendorAllowance;
                            brokerAllowanceWithoutTax = rentalAmountWithoutTax - vendorAllowanceWithoutTax;

                            couponDiscountPercent = (couponDiscountAmount / vendorAllowance) * 100;

                            #endregion
                        }

                        decimal
                            vendorAllowanceUnitPrice = vendorAllowance,
                            vendorAllowanceLineTotalAmount = vendorAllowance,
                            vendorCouponDiscountPercent = couponDiscountPercent,
                            brokerAllowanceUnitPrice = brokerAllowance,
                            brokerAllowanceLineTotalAmount = brokerAllowance,
                            brokerCouponDiscountPercent = 0M;

                        #region -1 => Tedarikçi hakediş satırı

                        var vendorAllowanceLine = VendorAllowance(
                            rowId: rowId,
                            couponDiscountPercent: vendorCouponDiscountPercent,
                            invoiceAmount: invoiceAmount,
                            invoiceTaxAmount: invoiceTaxAmount,
                            invoiceTotalAmount: invoiceTotalAmount,
                            taxRate: await GetTaxRate(logoReservation.ReturnDate, countryId),
                            unitPrice: vendorAllowanceWithoutTax,
                            lineAmount: vendorAllowanceWithoutTax,
                            lineTaxAmount: vendorAllowanceLineTotalAmount - vendorAllowanceWithoutTax,
                            lineTotalAmount: vendorAllowanceLineTotalAmount,
                            logoReservation: logoReservation);
                        if (logoReservation.TotalPrice.ToDecimalNullSafe() == 0 || vendorAllowanceLine.TaxRate <= 0)
                        {
                            vendorAllowanceLine.TaxExceptionCode = "350";
                        }
                        invoiceList.Add(vendorAllowanceLine);
                        #endregion 

                        #region -2 => Tek yön ücreti satırı
                        if (getInvoiceListRequest.ShowDropInvoices && logoReservation.DropAmount > 0)
                        {
                            rowId++;
                            var oneWayLine = await OneWayLine(logoReservation, rowId, countryId);
                            if (logoReservation.TotalPrice.ToDecimalNullSafe() == 0 || oneWayLine.TaxRate <= 0)
                            {
                                oneWayLine.TaxExceptionCode = "350";
                            }
                            invoiceList.Add(oneWayLine);
                        }
                        #endregion

                        #region -3 => Ek ürünler satırı
                        if (getInvoiceListRequest.ShowAdditionalProductInvoices && logoReservation.ExtraTotal > 0)
                        {
                            var resExtras = await _context.Reservationextra.Where(x => x.Resno == logoReservation.ReservationNo).ToListAsync();

                            if (resExtras.Count > 0)
                            {
                                foreach (var extraItem in resExtras)
                                {
                                    rowId++;
                                    var additionalProductLine =
                                        await AdditionalProductLine(extraItem, logoReservation, rowId, countryId);
                                    if (logoReservation.TotalPrice.ToDecimalNullSafe() == 0 || additionalProductLine.TaxRate <= 0)
                                    {
                                        additionalProductLine.TaxExceptionCode = "350";
                                    }
                                    invoiceList.Add(additionalProductLine);
                                }
                            }
                        }
                        #endregion

                        #region -4 => Kupon indirimi satırı (Satır kapatıldı -1 satırında gönderilecek - 28.11.2023 Toplantısı)
                        //if (logoReservation.CouponDiscountAmount.ToDecimalNullSafe() > 0)
                        //{
                        //    rowId++;
                        //    invoiceList.Add(CouponLine(logoReservation, rowId));
                        //}
                        #endregion

                        #region -5 => Vade farkı satırı
                        if ((logoReservation.InstallmentCount ?? 0) > 0)
                        {
                            rowId++;
                            var installmentCommitionLine = InstallmentCommissionLine(logoReservation, rowId);
                            if (logoReservation.TotalPrice.ToDecimalNullSafe() == 0 || installmentCommitionLine.TaxRate <= 0)
                            {
                                installmentCommitionLine.TaxExceptionCode = "350";
                            }
                            invoiceList.Add(installmentCommitionLine);
                        }
                        #endregion

                        #region -6 => Broker hakediş satırı
                        rowId++;
                        var brokerAllowanceLineAdd = BrokerAllowance(
                            rowId,
                            brokerCouponDiscountPercent,
                            invoiceAmount,
                            invoiceTaxAmount,
                            invoiceTotalAmount,
                            await GetTaxRate(logoReservation.ReturnDate, countryId),
                            brokerAllowanceWithoutTax,
                            brokerAllowanceWithoutTax,
                            brokerAllowanceLineTotalAmount - brokerAllowanceWithoutTax,
                            brokerAllowanceLineTotalAmount,
                            logoReservation
                        );
                        if (logoReservation.TotalPrice.ToDecimalNullSafe() == 0 || brokerAllowanceLineAdd.TaxRate <= 0)
                        {
                            brokerAllowanceLineAdd.TaxExceptionCode = "350";
                        }
                        invoiceList.Add(brokerAllowanceLineAdd);
                        #endregion

                        #region -7 => Banka komisyonu satırı
                        if ((logoReservation.InstallmentCount ?? 0) == 0)
                        {
                            rowId++;
                            var bankCommissionLine = BankCommissionLine(logoReservation, rowId);
                            if (logoReservation.TotalPrice.ToDecimalNullSafe() == 0 || bankCommissionLine.TaxRate <= 0)
                            {
                                bankCommissionLine.TaxExceptionCode = "350";
                            }
                            invoiceList.Add(bankCommissionLine);
                        }
                        #endregion

                        #region -8 => Banka taksit ücreti satırı
                        rowId++;
                        var installmentFeeLine = await InstallmentFeeLine(logoReservation, rowId, countryId);

                        if (logoReservation.InstallmentCount == 0 || installmentFeeLine.RowAmount <= decimal.Zero || installmentFeeLine.TaxRate <= 0)
                            installmentFeeLine.TaxExceptionCode = "350";

                        invoiceList.Add(installmentFeeLine);
                        #endregion
                    }
                }

                return new InvoiceListResponse
                {
                    Success = true,
                    InvoiceList = invoiceList,
                    Message = !logoReservations.Any() ? "Belirtilen tarih aralığında veri bulunamadı." : ""
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

        private Invoice VendorAllowance(
            int rowId,
            decimal couponDiscountPercent,
            decimal invoiceAmount,
            decimal invoiceTaxAmount,
            decimal invoiceTotalAmount,
            decimal taxRate,
            decimal unitPrice,
            decimal lineAmount,
            decimal lineTaxAmount,
            decimal lineTotalAmount,
            LogoReservation logoReservation)
        {
            return new Invoice()
            {
                RowID = rowId,

                InvoiceID = logoReservation.ReservationId.ToIntNullSafe(),
                InvoiceDate = logoReservation.ReservationDate,
                InvoiceNumber = logoReservation.ReservationId.ToString(),

                StockCode = ((int)StockCodes.VendorAllowance).ToString(),
                RowDescription = "Kiralama",
                Quantity = 1,
                Unit = "Adet",
                Discount = Math.Round(couponDiscountPercent.ToDecimalNullSafe(), 4),
                InvoiceAmount = Math.Round(invoiceAmount, 4),
                InvoiceTaxAmount = Math.Round(invoiceTaxAmount, 4),
                InvoiceTotalAmount = Math.Round(invoiceTotalAmount, 4),

                TaxRate = Math.Round(taxRate, 4),
                UnitPrice = Math.Round(unitPrice, 4),
                RowAmount = Math.Round(lineAmount, 4),
                RowTaxAmount = Math.Round(lineTaxAmount, 4),
                RowTotalAmount = Math.Round(lineTotalAmount, 4),

                ReservationNumber = logoReservation.ReservationNo,
                RentalDuration = logoReservation.RentalDuration.ToIntNullSafe(),
                PickupLocation = logoReservation.PickupLocation,
                PickupLocationId = logoReservation.PickupLocationId.ToIntNullSafe(),
                PickupDate = logoReservation.PickupDate,
                ReturnLocation = logoReservation.ReturnLocation,
                ReturnLocationId = logoReservation.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = logoReservation.ReturnDate,
                IsCancelled = logoReservation.ReservationStatusId == -4,

                TCNumber = logoReservation.IdentityNumber,
                CustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                Mail = logoReservation.CustomerEmail,
                TaxOffice = logoReservation.TaxOffice,
                TaxNumber = logoReservation.TaxNumber,
                Address = logoReservation.CustomerAddress,
                City = logoReservation.City,
                District = logoReservation.District,
                Office = logoReservation.TaxOffice,
                Description = logoReservation.CustomerNote,
                ContractCustomerTitle = logoReservation.CustomerTitle,
                ContractTaxNumber = logoReservation.TaxNumber,
                ContractTaxOffice = logoReservation.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(logoReservation.Country) ? logoReservation.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(logoReservation.City) ? logoReservation.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(logoReservation.District) ? logoReservation.District : "Ataşehir",
                ContractCustomerAddress = logoReservation.CustomerAddress,
                ContractCustomerTCNumber = logoReservation.IdentityNumber,

                //Tedarikçi kar - komisyon ve hesaplama
                VendorId = logoReservation.VendorId.ToIntNullSafe(),
                VendorPrice = CalculateVendorAllowance(logoReservation.TotalPrice.ToDecimalNullSafe(), logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), logoReservation.RentalWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType, logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe()),
                VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), 2),
                IsPaymentReceived = true,

                VendorCommissionInvoice = logoReservation.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }

        private async Task<Invoice> OneWayLine(LogoReservation logoReservation, int rowId, int countryId)
        {
            var invoiceAmount = logoReservation.TotalPaymentPrice;

            return new Invoice
            {
                StockCode = ((int)StockCodes.OneWay).ToString(),
                RowDescription = "Tek yön",
                UnitPrice = Math.Round(logoReservation.DropAmount.ToDecimalNullSafe() / (1 + await GetTaxRate(logoReservation.ReturnDate, countryId) / 100), 4),
                RowAmount = Math.Round(logoReservation.DropAmount.ToDecimalNullSafe() / (1 + await GetTaxRate(logoReservation.ReturnDate, countryId) / 100), 4),
                RowTaxAmount = Math.Round((logoReservation.DropAmount.ToDecimalNullSafe() / (1 + await GetTaxRate(logoReservation.ReturnDate, countryId) / 100)) * (await GetTaxRate(logoReservation.ReturnDate, countryId) / 100)/*(decimal)0.18*/, 4),
                RowTotalAmount = Math.Round(logoReservation.DropAmount.ToDecimalNullSafe(), 4),
                Discount = 0,

                InvoiceAmount = Math.Round(invoiceAmount / (1 + await GetTaxRate(logoReservation.ReturnDate, countryId) / 100), 4),
                InvoiceTaxAmount = Math.Round(invoiceAmount * (await GetTaxRate(logoReservation.ReturnDate, countryId) / 100)/*(decimal)0.18*/, 4),
                InvoiceTotalAmount = Math.Round(invoiceAmount, 4),

                //Tedarikçi kar - komisyon ve hesaplama
                VendorPrice = CalculateVendorAllowance(logoReservation.DropAmount.ToDecimalNullSafe(), logoReservation.ProfitMarkupOneWay.ToDecimalNullSafe(), logoReservation.OneWayWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType, logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe()),
                VendorWorkingType = logoReservation.OneWayWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(logoReservation.ProfitMarkupOneWay.ToDecimalNullSafe(), 2),
                IsPaymentReceived = !logoReservation.OneWayFeePayOnDelivery.ToBoolNullSafe(),

                InvoiceDate = logoReservation.ReservationDate,
                InvoiceID = logoReservation.ReservationId.ToIntNullSafe(),
                ReservationNumber = logoReservation.ReservationNo,
                CustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                CustomerTitle = logoReservation.CustomerTitle,
                Mail = logoReservation.CustomerEmail,
                CustomerCode = "",
                Plate = "",
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = logoReservation.TaxOffice,
                TaxNumber = logoReservation.TaxNumber,
                Address = logoReservation.CustomerAddress,
                City = logoReservation.City,
                District = logoReservation.District,
                InvoiceNumber = logoReservation.ReservationId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = logoReservation.TaxOffice,
                InvoiceDescription = "",
                RowID = rowId,
                ProcessType = "",
                TaxRate = (await GetTaxRate(logoReservation.ReturnDate, countryId))/*18*/,
                Description = logoReservation.CustomerNote,
                ContractCustomerCode = "",
                TCNumber = logoReservation.IdentityNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = logoReservation.CustomerTitle,
                ContractTaxNumber = logoReservation.TaxNumber,
                ContractTaxOffice = logoReservation.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(logoReservation.Country) ? logoReservation.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(logoReservation.City) ? logoReservation.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(logoReservation.District) ? logoReservation.District : "Ataşehir",
                ContractCustomerAddress = logoReservation.CustomerAddress,
                ContractCustomerTCNumber = logoReservation.IdentityNumber,
                PickupLocation = logoReservation.PickupLocation,
                PickupLocationId = logoReservation.PickupLocationId.ToIntNullSafe(),
                PickupDate = logoReservation.PickupDate,
                ReturnLocation = logoReservation.ReturnLocation,
                ReturnLocationId = logoReservation.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = logoReservation.ReturnDate,
                VendorId = logoReservation.VendorId.ToIntNullSafe(),
                RentalDuration = logoReservation.RentalDuration.ToIntNullSafe(),
                IsCancelled = logoReservation.ReservationStatusId == -4,
                VendorCommissionInvoice = logoReservation.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }

        private async Task<Invoice> AdditionalProductLine(Reservationextra reservationExtra, LogoReservation logoReservation, int rowId, int countryId)
        {
            var invoiceAmount = logoReservation.TotalPaymentPrice;
            return new Invoice
            {
                StockCode = reservationExtra.Extraid.ToString(),//Ek ürün
                RowDescription = reservationExtra.Extraname,
                UnitPrice = Math.Round(reservationExtra.Amount.ToDecimalNullSafe() / (1 + await GetTaxRate(logoReservation.ReturnDate, countryId) / 100), 4),
                RowAmount = Math.Round(reservationExtra.Amount.ToDecimalNullSafe() / (1 + await GetTaxRate(logoReservation.ReturnDate, countryId) / 100), 4),
                RowTaxAmount = Math.Round((reservationExtra.Amount.ToDecimalNullSafe() / (1 + await GetTaxRate(logoReservation.ReturnDate, countryId) / 100)) * (await GetTaxRate(logoReservation.ReturnDate, countryId) / 100)/*(decimal)0.18*/, 4),
                RowTotalAmount = Math.Round(reservationExtra.Amount.ToDecimalNullSafe(), 4),
                Discount = 0,

                //Tedarikçi kar - komisyon ve hesaplama
                VendorPrice = CalculateVendorAllowance(reservationExtra.Amount.ToDecimalNullSafe(), logoReservation.ProfitMarkupAdditionalProducts.ToDecimalNullSafe(), logoReservation.AdditionalWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType, logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe()),
                VendorWorkingType = logoReservation.AdditionalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(logoReservation.ProfitMarkupAdditionalProducts.ToDecimalNullSafe(), 4),
                IsPaymentReceived = !logoReservation.ExtraPricePayOnDelivery.ToBoolNullSafe(),

                InvoiceAmount = Math.Round(invoiceAmount / (1 + await GetTaxRate(logoReservation.ReturnDate, countryId) / 100), 4),
                InvoiceTaxAmount = Math.Round(invoiceAmount * (await GetTaxRate(logoReservation.ReturnDate, countryId) / 100)/*(decimal)0.18*/, 4),
                InvoiceTotalAmount = Math.Round(invoiceAmount, 4),

                RowID = rowId,
                InvoiceDate = logoReservation.ReservationDate,
                InvoiceID = logoReservation.ReservationId.ToIntNullSafe(),
                ReservationNumber = logoReservation.ReservationNo,
                CustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                CustomerTitle = logoReservation.CustomerTitle,
                Mail = logoReservation.CustomerEmail,
                CustomerCode = "",
                Plate = "",
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = logoReservation.TaxOffice,
                TaxNumber = logoReservation.TaxNumber,
                Address = logoReservation.CustomerAddress,
                City = logoReservation.City,
                District = logoReservation.District,
                InvoiceNumber = logoReservation.ReservationId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = logoReservation.TaxOffice,
                InvoiceDescription = "",
                ProcessType = "",
                TaxRate = (await GetTaxRate(logoReservation.ReturnDate, countryId))/*18*/,
                Description = logoReservation.CustomerNote,
                ContractCustomerCode = "",
                TCNumber = logoReservation.IdentityNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = logoReservation.CustomerTitle,
                ContractTaxNumber = logoReservation.TaxNumber,
                ContractTaxOffice = logoReservation.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(logoReservation.Country) ? logoReservation.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(logoReservation.City) ? logoReservation.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(logoReservation.District) ? logoReservation.District : "Ataşehir",
                ContractCustomerAddress = logoReservation.CustomerAddress,
                ContractCustomerTCNumber = logoReservation.IdentityNumber,
                PickupLocation = logoReservation.PickupLocation,
                PickupLocationId = logoReservation.PickupLocationId.ToIntNullSafe(),
                PickupDate = logoReservation.PickupDate,
                ReturnLocation = logoReservation.ReturnLocation,
                ReturnLocationId = logoReservation.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = logoReservation.ReturnDate,
                VendorId = logoReservation.VendorId.ToIntNullSafe(),
                RentalDuration = logoReservation.RentalDuration.ToIntNullSafe(),
                IsCancelled = logoReservation.ReservationStatusId == -4,
                VendorCommissionInvoice = logoReservation.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }

        private async Task<Invoice> CouponLine(LogoReservation logoReservation, int rowId, Location location)
        {
            return new Invoice()
            {
                InvoiceDate = logoReservation.ReservationDate,
                InvoiceID = logoReservation.ReservationId.ToIntNullSafe(),
                ReservationNumber = logoReservation.ReservationNo,
                CustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                CustomerTitle = logoReservation.CustomerTitle,
                StockCode = ((int)StockCodes.CouponDiscount).ToString(),
                Mail = logoReservation.CustomerEmail,
                CustomerCode = "",
                Plate = "",
                RowDescription = "Kupon indirimi",//Kalemlere göre değişecek
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = logoReservation.TaxOffice,
                TaxNumber = logoReservation.TaxNumber,
                Address = logoReservation.CustomerAddress,
                City = logoReservation.City,
                District = logoReservation.District,
                InvoiceNumber = logoReservation.ReservationId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = logoReservation.TaxOffice,
                InvoiceDescription = "",
                RowID = rowId,
                ProcessType = "",

                Discount = Math.Round(0M, 4),//Math.Round(item.CouponDiscount.ToDecimalNullSafe(), 4),
                InvoiceAmount = Math.Round(0M, 4),//Math.Round(invoiceAmount, 4),
                InvoiceTaxAmount = Math.Round(0M, 4),//Math.Round(invoiceAmount * (GetTaxRate(item.ReturnDate) / 100)/*(decimal)0.18*/, 4),
                InvoiceTotalAmount = Math.Round(0M, 4),//Math.Round(invoiceAmount, 4),

                UnitPrice = Math.Round(logoReservation.CouponDiscountAmount.ToDecimalNullSafe() / (1 + await GetTaxRate(logoReservation.ReturnDate, location.Countryid) / 100), 4),
                TaxRate = await GetTaxRate(logoReservation.ReturnDate, location.Countryid),
                RowAmount = Math.Round(logoReservation.CouponDiscountAmount.ToDecimalNullSafe() / (1 + await GetTaxRate(logoReservation.ReturnDate, location.Countryid) / 100), 4),
                RowTaxAmount = Math.Round((logoReservation.CouponDiscountAmount.ToDecimalNullSafe() / (1 + await GetTaxRate(logoReservation.ReturnDate, location.Countryid) / 100)) * (await GetTaxRate(logoReservation.ReturnDate, location.Countryid) / 100)/*(decimal)0.18*/, 4),
                RowTotalAmount = Math.Round(logoReservation.CouponDiscountAmount.ToDecimalNullSafe(), 4) * -1,

                //Tedarikçi kar - komisyon ve hesaplama
                VendorPrice = CalculateVendorAllowance(logoReservation.TotalPrice.ToDecimalNullSafe(), logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), logoReservation.RentalWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType, logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe()),
                VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), 2),
                IsPaymentReceived = true,

                Description = logoReservation.CustomerNote,
                ContractCustomerCode = "",
                TCNumber = logoReservation.IdentityNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = logoReservation.CustomerTitle,
                ContractTaxNumber = logoReservation.TaxNumber,
                ContractTaxOffice = logoReservation.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(logoReservation.Country) ? logoReservation.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(logoReservation.City) ? logoReservation.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(logoReservation.District) ? logoReservation.District : "Ataşehir",
                ContractCustomerAddress = logoReservation.CustomerAddress,
                ContractCustomerTCNumber = logoReservation.IdentityNumber,
                PickupLocation = logoReservation.PickupLocation,
                PickupLocationId = logoReservation.PickupLocationId.ToIntNullSafe(),
                PickupDate = logoReservation.PickupDate,
                ReturnLocation = logoReservation.ReturnLocation,
                ReturnLocationId = logoReservation.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = logoReservation.ReturnDate,
                VendorId = logoReservation.VendorId.ToIntNullSafe(),
                RentalDuration = logoReservation.RentalDuration.ToIntNullSafe(),
                IsCancelled = logoReservation.ReservationStatusId == -4,
                VendorCommissionInvoice = logoReservation.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }

        /// <summary>
        /// Tek çekim komisyon satırı
        /// </summary>
        /// <param name="logoReservation"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        private Invoice BankCommissionLine(LogoReservation logoReservation, int rowId)
        {
            var invoiceAmount = logoReservation.TotalPaymentPrice;
            return new Invoice()
            {
                InvoiceDate = logoReservation.ReservationDate,
                InvoiceID = logoReservation.ReservationId.ToIntNullSafe(),
                ReservationNumber = logoReservation.ReservationNo,
                CustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                CustomerTitle = logoReservation.CustomerTitle,
                StockCode = ((int)StockCodes.BankCommission).ToString(),
                Mail = logoReservation.CustomerEmail,
                CustomerCode = "",
                Plate = "",
                RowDescription = "Araç kiralama banka komisyonu",//Kalemlere göre değişecek
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = logoReservation.TaxOffice,
                TaxNumber = logoReservation.TaxNumber,
                Address = logoReservation.CustomerAddress,
                City = logoReservation.City,
                District = logoReservation.District,
                InvoiceNumber = logoReservation.ReservationId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = logoReservation.TaxOffice,
                InvoiceDescription = "",
                RowID = rowId,
                ProcessType = "",

                Discount = Math.Round(0M, 4),
                InvoiceAmount = Math.Round(invoiceAmount / (decimal)(1.2), 4),
                InvoiceTaxAmount = Math.Round(invoiceAmount * (decimal)(0.2), 4),
                InvoiceTotalAmount = Math.Round(invoiceAmount, 4),

                UnitPrice = Math.Round(logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe(), 4),
                TaxRate = 20,
                RowAmount = Math.Round(logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe(), 4),
                RowTaxAmount = Math.Round(logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe() * (decimal)(0.2), 4),
                RowTotalAmount = Math.Round(logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe() * (decimal)(1.2), 4),
                TaxExceptionCode = null,

                //Tedarikçi kar - komisyon ve hesaplama
                VendorPrice = 0,
                VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(0M, 4), // Gönderilmesi durumunda komisyonun bir kısmını tedarikçi karşılar anlamına geliyormuş (26.10.2023 - Fatih Bey)
                IsPaymentReceived = true,

                Description = logoReservation.CustomerNote,
                ContractCustomerCode = "",
                TCNumber = logoReservation.IdentityNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = logoReservation.CustomerTitle,
                ContractTaxNumber = logoReservation.TaxNumber,
                ContractTaxOffice = logoReservation.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(logoReservation.Country) ? logoReservation.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(logoReservation.City) ? logoReservation.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(logoReservation.District) ? logoReservation.District : "Ataşehir",
                ContractCustomerAddress = logoReservation.CustomerAddress,
                ContractCustomerTCNumber = logoReservation.IdentityNumber,
                PickupLocation = logoReservation.PickupLocation,
                PickupLocationId = logoReservation.PickupLocationId.ToIntNullSafe(),
                PickupDate = logoReservation.PickupDate,
                ReturnLocation = logoReservation.ReturnLocation,
                ReturnLocationId = logoReservation.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = logoReservation.ReturnDate,
                VendorId = logoReservation.VendorId.ToIntNullSafe(),
                RentalDuration = logoReservation.RentalDuration.ToIntNullSafe(),
                IsCancelled = logoReservation.ReservationStatusId == -4,
                VendorCommissionInvoice = logoReservation.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }
        private async Task<Invoice> InstallmentFeeLine(LogoReservation logoReservation, int rowId, int countryId)
        {
            return new Invoice()
            {
                InvoiceDate = logoReservation.ReservationDate,
                InvoiceID = logoReservation.ReservationId.ToIntNullSafe(),
                ReservationNumber = logoReservation.ReservationNo,
                CustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                CustomerTitle = logoReservation.CustomerTitle,
                StockCode = ((int)StockCodes.InstallmentFee).ToString(),
                Mail = logoReservation.CustomerEmail,
                CustomerCode = "",
                Plate = "",
                RowDescription = "Araç kiralama taksit komisyonu",
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = logoReservation.TaxOffice,
                TaxNumber = logoReservation.TaxNumber,
                Address = logoReservation.CustomerAddress,
                City = logoReservation.City,
                District = logoReservation.District,
                InvoiceNumber = logoReservation.ReservationId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = logoReservation.TaxOffice,
                InvoiceDescription = "",
                RowID = rowId,
                ProcessType = "",

                Discount = Math.Round(0M, 4),
                InvoiceAmount = Math.Round(0M, 4),
                InvoiceTaxAmount = Math.Round(0M, 4),
                InvoiceTotalAmount = Math.Round(0M, 4),

                UnitPrice = Math.Round(logoReservation.InstallmentFee.ToDecimalNullSafe(), 4),
                TaxRate = Math.Round(await GetTaxRate(logoReservation.ReturnDate, countryId), 4),
                RowAmount = Math.Round(logoReservation.InstallmentFee.ToDecimalNullSafe(), 4),
                RowTaxAmount = Math.Round((logoReservation.InstallmentFee.ToDecimalNullSafe() / (1 + await GetTaxRate(logoReservation.ReturnDate, countryId) / 100)) * (await GetTaxRate(logoReservation.ReturnDate, countryId) / 100)/*(decimal)0.18*/, 4),
                RowTotalAmount = Math.Round(logoReservation.InstallmentFee.ToDecimalNullSafe(), 4),
                TaxExceptionCode = null,

                //Tedarikçi kar - komisyon ve hesaplama
                VendorPrice = 0,
                VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(0M, 4),
                IsPaymentReceived = true,
                Description = logoReservation.CustomerNote,
                ContractCustomerCode = "",
                TCNumber = logoReservation.IdentityNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = logoReservation.CustomerTitle,
                ContractTaxNumber = logoReservation.TaxNumber,
                ContractTaxOffice = logoReservation.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(logoReservation.Country) ? logoReservation.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(logoReservation.City) ? logoReservation.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(logoReservation.District) ? logoReservation.District : "Ataşehir",
                ContractCustomerAddress = logoReservation.CustomerAddress,
                ContractCustomerTCNumber = logoReservation.IdentityNumber,
                PickupLocation = logoReservation.PickupLocation,
                PickupLocationId = logoReservation.PickupLocationId.ToIntNullSafe(),
                PickupDate = logoReservation.PickupDate,
                ReturnLocation = logoReservation.ReturnLocation,
                ReturnLocationId = logoReservation.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = logoReservation.ReturnDate,
                VendorId = logoReservation.VendorId.ToIntNullSafe(),
                RentalDuration = logoReservation.RentalDuration.ToIntNullSafe(),
                IsCancelled = logoReservation.ReservationStatusId == -4,
                VendorCommissionInvoice = logoReservation.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }

        /// <summary>
        /// Taksitli işlem banka komisyonu işlemi
        /// </summary>
        /// <param name="logoReservation"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        private Invoice InstallmentCommissionLine(LogoReservation logoReservation, int rowId)
        {
            return new Invoice()
            {
                InvoiceDate = logoReservation.ReservationDate,
                InvoiceID = logoReservation.ReservationId.ToIntNullSafe(),
                ReservationNumber = logoReservation.ReservationNo,
                CustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                CustomerTitle = logoReservation.CustomerTitle,
                StockCode = ((int)StockCodes.BankInstallmentCommission).ToString(),
                Mail = logoReservation.CustomerEmail,
                CustomerCode = "",
                Plate = "",
                RowDescription = "Araç kiralama banka komisyonu",//Kalemlere göre değişecek
                Quantity = 1,
                Unit = "Adet",
                TaxOffice = logoReservation.TaxOffice,
                TaxNumber = logoReservation.TaxNumber,
                Address = logoReservation.CustomerAddress,
                City = logoReservation.City,
                District = logoReservation.District,
                InvoiceNumber = logoReservation.ReservationId.ToString(),
                EInvoiceNumber = "",
                Cancel = "",
                Office = logoReservation.TaxOffice,
                InvoiceDescription = "",
                RowID = rowId,
                ProcessType = "",

                Discount = Math.Round(0M, 4),
                InvoiceAmount = Math.Round(0M, 4),
                InvoiceTaxAmount = Math.Round(0M, 4),
                InvoiceTotalAmount = Math.Round(0M, 4),

                UnitPrice = Math.Round(logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe(), 4),
                TaxRate = 0,
                RowAmount = Math.Round(logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe(), 4),
                RowTaxAmount = 0,
                RowTotalAmount = Math.Round(logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe(), 4),
                TaxExceptionCode = "350",

                //Tedarikçi kar - komisyon ve hesaplama
                VendorPrice = 0,
                VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(0M, 4), // Gönderilmesi durumunda komisyonun bir kısmını tedarikçi karşılar anlamına geliyormuş (26.10.2023 - Fatih Bey)
                IsPaymentReceived = true,

                Description = logoReservation.CustomerNote,
                ContractCustomerCode = "",
                TCNumber = logoReservation.IdentityNumber,
                InvoiceStatus = "",
                ContractCustomerTitle = logoReservation.CustomerTitle,
                ContractTaxNumber = logoReservation.TaxNumber,
                ContractTaxOffice = logoReservation.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(logoReservation.Country) ? logoReservation.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(logoReservation.City) ? logoReservation.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(logoReservation.District) ? logoReservation.District : "Ataşehir",
                ContractCustomerAddress = logoReservation.CustomerAddress,
                ContractCustomerTCNumber = logoReservation.IdentityNumber,
                PickupLocation = logoReservation.PickupLocation,
                PickupLocationId = logoReservation.PickupLocationId.ToIntNullSafe(),
                PickupDate = logoReservation.PickupDate,
                ReturnLocation = logoReservation.ReturnLocation,
                ReturnLocationId = logoReservation.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = logoReservation.ReturnDate,
                VendorId = logoReservation.VendorId.ToIntNullSafe(),
                RentalDuration = logoReservation.RentalDuration.ToIntNullSafe(),
                IsCancelled = logoReservation.ReservationStatusId == -4,
                VendorCommissionInvoice = logoReservation.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }

        private Invoice BrokerAllowance(
            int rowId,
            decimal couponDiscountPercent,
            decimal invoiceAmount,
            decimal invoiceTaxAmount,
            decimal invoiceTotalAmount,
            decimal taxRate,
            decimal unitPrice,
            decimal lineAmount,
            decimal lineTaxAmount,
            decimal lineTotalAmount,
            LogoReservation logoReservation)
        {
            return new Invoice()
            {
                RowID = rowId,

                InvoiceID = logoReservation.ReservationId.ToIntNullSafe(),
                InvoiceDate = logoReservation.ReservationDate,
                InvoiceNumber = logoReservation.ReservationId.ToString(),

                StockCode = ((int)StockCodes.BrokerAllowance).ToString(),
                RowDescription = "AKŞ Hakediş",
                Quantity = 1,
                Unit = "Adet",
                Discount = Math.Round(couponDiscountPercent.ToDecimalNullSafe(), 4),
                InvoiceAmount = Math.Round(invoiceAmount, 4),
                InvoiceTaxAmount = Math.Round(invoiceTaxAmount, 4),
                InvoiceTotalAmount = Math.Round(invoiceTotalAmount, 4),

                TaxRate = Math.Round(taxRate, 4),
                UnitPrice = Math.Round(unitPrice, 4),
                RowAmount = Math.Round(lineAmount, 4),
                RowTaxAmount = Math.Round(lineTaxAmount, 4),
                RowTotalAmount = Math.Round(lineTotalAmount, 4),

                ReservationNumber = logoReservation.ReservationNo,
                RentalDuration = logoReservation.RentalDuration.ToIntNullSafe(),
                PickupLocation = logoReservation.PickupLocation,
                PickupLocationId = logoReservation.PickupLocationId.ToIntNullSafe(),
                PickupDate = logoReservation.PickupDate,
                ReturnLocation = logoReservation.ReturnLocation,
                ReturnLocationId = logoReservation.ReturnLocationId.ToIntNullSafe(),
                ReturnDate = logoReservation.ReturnDate,
                IsCancelled = logoReservation.ReservationStatusId == -4,

                TCNumber = logoReservation.IdentityNumber,
                CustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                CustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                Mail = logoReservation.CustomerEmail,
                TaxOffice = logoReservation.TaxOffice,
                TaxNumber = logoReservation.TaxNumber,
                Address = logoReservation.CustomerAddress,
                City = logoReservation.City,
                District = logoReservation.District,
                Office = logoReservation.TaxOffice,
                Description = logoReservation.CustomerNote,
                ContractCustomerTitle = logoReservation.CustomerTitle,
                ContractTaxNumber = logoReservation.TaxNumber,
                ContractTaxOffice = logoReservation.TaxOffice,
                ContractCustomerName = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? logoReservation.CustomerTitle : logoReservation.CustomerName,
                ContractCustomerSurname = !string.IsNullOrEmpty(logoReservation.CustomerTitle) ? "" : logoReservation.CustomerSurname,
                ContractCustomerCountry = !string.IsNullOrEmpty(logoReservation.Country) ? logoReservation.Country : "Türkiye",
                ContractCustomerCity = !string.IsNullOrEmpty(logoReservation.City) ? logoReservation.City : "İstanbul",
                ContractCustomerDistrict = !string.IsNullOrEmpty(logoReservation.District) ? logoReservation.District : "Ataşehir",
                ContractCustomerAddress = logoReservation.CustomerAddress,
                ContractCustomerTCNumber = logoReservation.IdentityNumber,

                //Tedarikçi kar - komisyon ve hesaplama
                VendorId = logoReservation.VendorId.ToIntNullSafe(),
                VendorPrice = CalculateVendorAllowance(logoReservation.TotalPrice.ToDecimalNullSafe(), logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), logoReservation.RentalWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType, logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe()),
                VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe(),
                VendorProfitMarkup = Math.Round(logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), 2),
                IsPaymentReceived = true,

                VendorCommissionInvoice = logoReservation.VendorCommissionInvoice.ToBoolNullSafe()
            };
        }
        #endregion

        #region Tahsilat İşlemleri
        public Task<PaymentListResponse> GetPayments(GetPaymentListRequest getPaymentListRequest)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Tedarikçiler
        public Task<List<VendorListResponse>> GetVendors()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Fonksiyonlar

        /// <summary>
        /// Verilen tarihe göre ilgili KDV oranını döner
        /// </summary>
        /// <param name="invoiceDateTime"></param>
        /// <returns></returns>
        private async Task<decimal> GetTaxRate(DateTime? invoiceDateTime, int countryId)
        {
            var taxRate = await _locationService.GetTaxRateByCountryId(countryId);
            //var defaultTaxRate = await _configurationService.GetConfigurationValueByFieldName<decimal>("DefaultCountryTaxRate");
            if (taxRate != null)
            {
                if (invoiceDateTime < new DateTime(2023, 7, 10))
                    return 18;

                return taxRate.TaxRate.ToDecimalNullSafe();
            }
            return invoiceDateTime < new DateTime(2023, 7, 10) ? 18 : 20;
        }

        /// <summary>
        /// Verilen tutarın banka komisyon tutarını döner
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="invoiceDateTime"></param>
        /// <returns></returns>
        private decimal CalculateBankCommissionAmount(decimal amount, decimal bankCommission)
        {
            return (amount.ToDecimalNullSafe() * (bankCommission.ToDecimalNullSafe() / 100));
        }

        /// <summary>
        /// Verilen tutarın KDV tutarını döner
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="invoiceDateTime"></param>
        /// <returns></returns>
        private async Task<decimal> CalculateTaxAmount(decimal amount, DateTime? invoiceDateTime, int countryId)
        {
            return (amount.ToDecimalNullSafe() * (await GetTaxRate(invoiceDateTime, countryId) / 100));
        }

        /// <summary>
        /// Verilen tutarın KDV hariç halini döner
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="invoiceDateTime"></param>
        /// <returns></returns>
        private async Task<decimal> CalculateAmountWithoutTax(decimal amount, DateTime? invoiceDateTime, int countryId)
        {
            return (amount.ToDecimalNullSafe() / (1 + await GetTaxRate(invoiceDateTime, countryId) / 100));
        }

        /// <summary>
        /// Tedarikçi çalışma şekline göre broker hakediş tutarını hesaplar
        /// </summary>
        /// <param name="price"></param>
        /// <param name="profitMarkup"></param>
        /// <param name="workingType"></param>
        /// <returns></returns>
        private decimal CalculateBrokerAllowance(
            decimal rentalAmount,
            int? couponType,
            decimal couponAmount,
            int workingType,
            decimal profitMarkup)
        {
            return
                workingType == 0
                    ? Math.Round(rentalAmount * (profitMarkup / 100), 4)
                    : rentalAmount - Math.Round(((rentalAmount * 100) / (100 + profitMarkup)), 4);
        }

        /// <summary>
        /// Tedarikçi çalışma şekline göre tutar hesaplar
        /// </summary>
        /// <param name="price"></param>
        /// <param name="profitMarkup"></param>
        /// <param name="workingType"></param>
        /// <returns></returns>
        private decimal CalculateVendorAllowance(decimal price, decimal profitMarkup, int workingType, int? couponType, decimal couponVendorDiscount)
        {
            var vendorAllowance =
                workingType == 1
                    ? Math.Round(price * ((100 - profitMarkup) / 100), 4)
                    : Math.Round((price * 100) / (100 + profitMarkup), 4);
            return couponType == 0
                ? vendorAllowance - (vendorAllowance * (couponVendorDiscount / 100))
                : vendorAllowance - couponVendorDiscount.ToDecimalNullSafe();
        }
        #endregion
    }
}
