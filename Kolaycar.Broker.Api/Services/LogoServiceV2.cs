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

        private const int DefaultCountryId = 190;

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
            if (getInvoiceListRequest == null
                || string.IsNullOrEmpty(getInvoiceListRequest.StartDate)
                || string.IsNullOrEmpty(getInvoiceListRequest.EndDate)
                || !getInvoiceListRequest.StartDate.IsDateTime()
                || !getInvoiceListRequest.EndDate.IsDateTime())
            {
                return new InvoiceListResponse { Success = false, Message = "Sorgu parametreleri uygun formatta değil!" };
            }

            try
            {
                var startDate = getInvoiceListRequest.StartDate.ToDateTimeNullSafe();
                var endDate = getInvoiceListRequest.EndDate.ToDateTimeNullSafe();
                var agencyId = getInvoiceListRequest.BranchID.ToIntNullSafe();

                var logoReservations = await _context.LogoReservations
                    .FromSqlRaw("EXEC GETINVOICELINES @startDate = {0}, @endDate = {1}, @agencyId = {2}", startDate, endDate, agencyId)
                    .ToListAsync();

                if (logoReservations == null || logoReservations.Count <= 0)
                    return new InvoiceListResponse { Success = true, InvoiceList = new List<Invoice>(), Message = "Belirtilen tarih aralığında veri bulunamadı." };

                var invoiceList = new List<Invoice>();

                foreach (var logoReservation in logoReservations)
                {
                    var location = await _locationService.GetLocationById(logoReservation.PickupLocationId);
                    var countryId = location?.Countryid ?? DefaultCountryId;

                    if (logoReservation.ReservationStatusId == (getInvoiceListRequest.OnlyCanceledReservations ? -4 : -1)
                        && logoReservation.DailyPrice.ToDecimalNullSafe() > 0)
                    {
                        var lines = await BuildInvoiceLinesForReservation(logoReservation, getInvoiceListRequest, countryId);
                        invoiceList.AddRange(lines);
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
                return new InvoiceListResponse { Success = false, Message = "Bir hata oluştu. Lütfen yetkili ile iletişime geçiniz!" };
            }
        }

        private async Task<List<Invoice>> BuildInvoiceLinesForReservation(
            LogoReservation logoReservation,
            GetInvoiceListRequest request,
            int countryId)
        {
            var lines = new List<Invoice>();
            var rowId = 1;

            if (logoReservation.HasRefundableCoupon ?? false)
            {
                logoReservation.TotalPaymentPrice += (logoReservation.CouponDiscountAmount.ToDecimalNullSafe() - logoReservation.CouponBonusAmount.ToDecimalNullSafe());
                logoReservation.TotalPrice += (logoReservation.CouponDiscountAmount.ToDecimalNullSafe() - logoReservation.CouponBonusAmount.ToDecimalNullSafe());
            }

            var taxRate = await GetTaxRate(logoReservation.ReturnDate, countryId);
            var rentalAmount = logoReservation.DailyPrice.ToDecimalNullSafe() * logoReservation.RentalDuration.ToIntNullSafe();
            var rentalAmountWithoutTax = rentalAmount / (1 + taxRate / 100);
            var invoiceTotalAmount = logoReservation.TotalPaymentPrice;
            var invoiceAmount = invoiceTotalAmount / (1 + taxRate / 100);
            var invoiceTaxAmount = invoiceTotalAmount - invoiceAmount;
            var couponDiscountAmount = logoReservation.HasRefundableCoupon ?? false ?
                                       logoReservation.CouponBonusAmount.ToDecimalNullSafe() :
                                       logoReservation.CouponDiscountAmount.ToDecimalNullSafe();

            decimal bankCommissionAmount, brokerAllowance, brokerAllowanceWithoutTax,
                    vendorAllowance, vendorAllowanceWithoutTax, couponDiscountPercent;

            if (logoReservation.InstallmentCount == 0)
            {
                // Tek çekim senaryosu
                bankCommissionAmount = logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe();
                brokerAllowance = !(logoReservation.VendorCommissionInvoice ?? false) ? CalculateBrokerAllowance(
                    logoReservation.TotalPaymentPrice - bankCommissionAmount,
                    logoReservation.RentalWorkingType,
                    logoReservation.ProfitMarkupRental.ToDecimalNullSafe()) : decimal.Zero;
                brokerAllowanceWithoutTax = brokerAllowance / (1 + taxRate / 100);
                vendorAllowance = rentalAmount - brokerAllowance - bankCommissionAmount;
                vendorAllowanceWithoutTax = vendorAllowance / (1 + taxRate / 100);
                couponDiscountPercent = (couponDiscountAmount / vendorAllowance) * 100;
            }
            else
            {
                // Taksitli çekim senaryosu
                bankCommissionAmount = logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe();
                vendorAllowance = CalculateVendorAllowance(rentalAmount, logoReservation.ProfitMarkupRental.ToDecimalNullSafe(),
                    logoReservation.RentalWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType.ToIntNullSafe(), logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe());
                vendorAllowanceWithoutTax = vendorAllowance / (1 + taxRate / 100);
                brokerAllowance = rentalAmount - vendorAllowance;
                brokerAllowanceWithoutTax = rentalAmountWithoutTax - vendorAllowanceWithoutTax;
                couponDiscountPercent = (couponDiscountAmount / vendorAllowance) * 100;
            }

            // -1 => Tedarikçi hakediş satırı
            var vendorAllowanceLine = VendorAllowance(rowId, couponDiscountPercent, invoiceAmount, invoiceTaxAmount, invoiceTotalAmount,
                taxRate, vendorAllowanceWithoutTax, vendorAllowanceWithoutTax, vendorAllowance - vendorAllowanceWithoutTax, vendorAllowance, logoReservation);
            ApplyTaxExceptionIfNeeded(vendorAllowanceLine, logoReservation);
            lines.Add(vendorAllowanceLine);

            // -2 => Tek yön ücreti satırı
            if (request.ShowDropInvoices && logoReservation.DropAmount > 0)
            {
                rowId++;
                var oneWayLine = OneWayLine(logoReservation, rowId, taxRate);
                ApplyTaxExceptionIfNeeded(oneWayLine, logoReservation);
                lines.Add(oneWayLine);
            }

            // -3 => Ek ürünler satırı
            if (request.ShowAdditionalProductInvoices && logoReservation.ExtraTotal > 0)
            {
                var resExtras = await _context.Reservationextra.Where(x => x.Resno == logoReservation.ReservationNo).ToListAsync();
                foreach (var extraItem in resExtras)
                {
                    rowId++;
                    var additionalProductLine = AdditionalProductLine(extraItem, logoReservation, rowId, taxRate);
                    ApplyTaxExceptionIfNeeded(additionalProductLine, logoReservation);
                    lines.Add(additionalProductLine);
                }
            }

            // -4 => Kupon indirimi satırı (Satır kapatıldı -1 satırında gönderilecek - 28.11.2023 Toplantısı)

            // -5 => Vade farkı satırı
            if ((logoReservation.InstallmentCount ?? 0) > 0)
            {
                rowId++;
                var installmentCommissionLine = InstallmentCommissionLine(logoReservation, rowId, taxRate);
                ApplyTaxExceptionIfNeeded(installmentCommissionLine, logoReservation);
                lines.Add(installmentCommissionLine);
            }

            // -6 => Broker hakediş satırı
            if (!(logoReservation.VendorCommissionInvoice ?? false))
            {
                rowId++;
                var brokerAllowanceLineTotalAmount = brokerAllowance;
                var brokerAllowanceLine = BrokerAllowance(rowId, 0M, invoiceAmount, invoiceTaxAmount, invoiceTotalAmount,
                    taxRate, brokerAllowanceWithoutTax, brokerAllowanceWithoutTax, brokerAllowanceLineTotalAmount - brokerAllowanceWithoutTax, brokerAllowanceLineTotalAmount, logoReservation);
                ApplyTaxExceptionIfNeeded(brokerAllowanceLine, logoReservation);
                lines.Add(brokerAllowanceLine);
            }

            // -7 => Banka komisyonu satırı
            if ((logoReservation.InstallmentCount ?? 0) == 0)
            {
                rowId++;
                var bankCommissionLine = BankCommissionLine(logoReservation, rowId);
                ApplyTaxExceptionIfNeeded(bankCommissionLine, logoReservation);
                lines.Add(bankCommissionLine);
            }

            // -8 => Banka taksit ücreti satırı
            rowId++;
            var installmentFeeLine = InstallmentFeeLine(logoReservation, rowId, taxRate);
            if (logoReservation.InstallmentCount == 0 || installmentFeeLine.RowAmount <= decimal.Zero || installmentFeeLine.TaxRate <= 0)
                installmentFeeLine.TaxExceptionCode = "350";
            lines.Add(installmentFeeLine);

            return lines;
        }

        private static void ApplyTaxExceptionIfNeeded(Invoice invoice, LogoReservation logoReservation)
        {
            if (logoReservation.TotalPrice.ToDecimalNullSafe() == 0 || invoice.TaxRate <= 0)
                invoice.TaxExceptionCode = "350";
        }

        private static Invoice MapCommonInvoiceFields(Invoice invoice, LogoReservation res)
        {
            var hasTitle = !string.IsNullOrEmpty(res.CustomerTitle);
            invoice.InvoiceDate = res.ReservationDate;
            invoice.InvoiceID = res.ReservationId.ToIntNullSafe();
            invoice.InvoiceNumber = res.ReservationId.ToString();
            invoice.ReservationNumber = res.ReservationNo;
            invoice.RentalDuration = res.RentalDuration.ToIntNullSafe();
            invoice.Quantity = 1;
            invoice.Unit = "Adet";
            invoice.CustomerTitle = res.CustomerTitle;
            invoice.CustomerName = hasTitle ? res.CustomerTitle : res.CustomerName;
            invoice.CustomerSurname = hasTitle ? "" : res.CustomerSurname;
            invoice.CustomerCode = "";
            invoice.TCNumber = res.IdentityNumber;
            invoice.Mail = res.CustomerEmail;
            invoice.TaxOffice = res.TaxOffice;
            invoice.TaxNumber = res.TaxNumber;
            invoice.Address = res.CustomerAddress;
            invoice.City = res.City;
            invoice.District = res.District;
            invoice.Office = res.TaxOffice;
            invoice.Plate = "";
            invoice.EInvoiceNumber = "";
            invoice.Cancel = "";
            invoice.InvoiceDescription = "";
            invoice.InvoiceStatus = "";
            invoice.ProcessType = "";
            invoice.Description = res.CustomerNote;
            invoice.ContractCustomerCode = "";
            invoice.ContractCustomerTitle = res.CustomerTitle;
            invoice.ContractTaxNumber = res.TaxNumber;
            invoice.ContractTaxOffice = res.TaxOffice;
            invoice.ContractCustomerName = hasTitle ? res.CustomerTitle : res.CustomerName;
            invoice.ContractCustomerSurname = hasTitle ? "" : res.CustomerSurname;
            invoice.ContractCustomerCountry = !string.IsNullOrEmpty(res.Country) ? res.Country : "Türkiye";
            invoice.ContractCustomerCity = !string.IsNullOrEmpty(res.City) ? res.City : "Belirtilmemiş";
            invoice.ContractCustomerDistrict = !string.IsNullOrEmpty(res.District) ? res.District : "Belirtilmemiş";
            invoice.ContractCustomerAddress = res.CustomerAddress;
            invoice.ContractCustomerTCNumber = res.IdentityNumber;
            invoice.PickupLocation = res.PickupLocation;
            invoice.PickupLocationId = res.PickupLocationId.ToIntNullSafe();
            invoice.PickupDate = res.PickupDate;
            invoice.ReturnLocation = res.ReturnLocation;
            invoice.ReturnLocationId = res.ReturnLocationId.ToIntNullSafe();
            invoice.ReturnDate = res.ReturnDate;
            invoice.VendorId = res.VendorId.ToIntNullSafe();
            invoice.IsCancelled = res.ReservationStatusId == -4;
            invoice.VendorCommissionInvoice = res.VendorCommissionInvoice.ToBoolNullSafe();
            return invoice;
        }

        private Invoice VendorAllowance(
            int rowId, decimal couponDiscountPercent,
            decimal invoiceAmount, decimal invoiceTaxAmount, decimal invoiceTotalAmount,
            decimal taxRate, decimal unitPrice, decimal lineAmount, decimal lineTaxAmount, decimal lineTotalAmount,
            LogoReservation logoReservation)
        {
            var invoice = MapCommonInvoiceFields(new Invoice(), logoReservation);
            invoice.RowID = rowId;
            invoice.StockCode = ((int)StockCodes.VendorAllowance).ToString();
            invoice.RowDescription = "Kiralama";
            invoice.Discount = Math.Round(couponDiscountPercent.ToDecimalNullSafe(), 4);
            invoice.InvoiceAmount = Math.Round(invoiceAmount, 4);
            invoice.InvoiceTaxAmount = Math.Round(invoiceTaxAmount, 4);
            invoice.InvoiceTotalAmount = Math.Round(invoiceTotalAmount, 4);
            invoice.TaxRate = Math.Round(taxRate, 4);
            invoice.UnitPrice = Math.Round(unitPrice, 4);
            invoice.RowAmount = Math.Round(lineAmount, 4);
            invoice.RowTaxAmount = Math.Round(lineTaxAmount, 4);
            invoice.RowTotalAmount = Math.Round(lineTotalAmount, 4);
            invoice.VendorPrice = CalculateVendorAllowance(logoReservation.TotalPrice.ToDecimalNullSafe(), logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), logoReservation.RentalWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType, logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe());
            invoice.VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe();
            invoice.VendorProfitMarkup = Math.Round(logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), 2);
            invoice.IsPaymentReceived = true;
            return invoice;
        }

        private Invoice OneWayLine(LogoReservation logoReservation, int rowId, decimal taxRate)
        {
            var dropAmount = logoReservation.DropAmount.ToDecimalNullSafe();
            var dropAmountWithoutTax = dropAmount / (1 + taxRate / 100);
            var invoiceAmount = logoReservation.TotalPaymentPrice;

            var invoice = MapCommonInvoiceFields(new Invoice(), logoReservation);
            invoice.RowID = rowId;
            invoice.StockCode = ((int)StockCodes.OneWay).ToString();
            invoice.RowDescription = "Tek yön";
            invoice.Discount = 0;
            invoice.TaxRate = taxRate;
            invoice.UnitPrice = Math.Round(dropAmountWithoutTax, 4);
            invoice.RowAmount = Math.Round(dropAmountWithoutTax, 4);
            invoice.RowTaxAmount = Math.Round(dropAmountWithoutTax * (taxRate / 100), 4);
            invoice.RowTotalAmount = Math.Round(dropAmount, 4);
            invoice.InvoiceAmount = Math.Round(invoiceAmount / (1 + taxRate / 100), 4);
            invoice.InvoiceTaxAmount = Math.Round(invoiceAmount * (taxRate / 100), 4);
            invoice.InvoiceTotalAmount = Math.Round(invoiceAmount, 4);
            invoice.VendorPrice = CalculateVendorAllowance(dropAmount, logoReservation.ProfitMarkupOneWay.ToDecimalNullSafe(), logoReservation.OneWayWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType, logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe());
            invoice.VendorWorkingType = logoReservation.OneWayWorkingType.ToIntNullSafe();
            invoice.VendorProfitMarkup = Math.Round(logoReservation.ProfitMarkupOneWay.ToDecimalNullSafe(), 2);
            invoice.IsPaymentReceived = !logoReservation.OneWayFeePayOnDelivery.ToBoolNullSafe();
            return invoice;
        }

        private Invoice AdditionalProductLine(Reservationextra reservationExtra, LogoReservation logoReservation, int rowId, decimal taxRate)
        {
            var extraAmount = reservationExtra.Amount.ToDecimalNullSafe();
            var extraAmountWithoutTax = extraAmount / (1 + taxRate / 100);
            var invoiceAmount = logoReservation.TotalPaymentPrice;

            var invoice = MapCommonInvoiceFields(new Invoice(), logoReservation);
            invoice.RowID = rowId;
            invoice.StockCode = reservationExtra.Extraid.ToString();
            invoice.RowDescription = reservationExtra.Extraname;
            invoice.Discount = 0;
            invoice.TaxRate = taxRate;
            invoice.UnitPrice = Math.Round(extraAmountWithoutTax, 4);
            invoice.RowAmount = Math.Round(extraAmountWithoutTax, 4);
            invoice.RowTaxAmount = Math.Round(extraAmountWithoutTax * (taxRate / 100), 4);
            invoice.RowTotalAmount = Math.Round(extraAmount, 4);
            invoice.InvoiceAmount = Math.Round(invoiceAmount / (1 + taxRate / 100), 4);
            invoice.InvoiceTaxAmount = Math.Round(invoiceAmount * (taxRate / 100), 4);
            invoice.InvoiceTotalAmount = Math.Round(invoiceAmount, 4);
            invoice.VendorPrice = CalculateVendorAllowance(extraAmount, logoReservation.ProfitMarkupAdditionalProducts.ToDecimalNullSafe(), logoReservation.AdditionalWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType, logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe());
            invoice.VendorWorkingType = logoReservation.AdditionalWorkingType.ToIntNullSafe();
            invoice.VendorProfitMarkup = Math.Round(logoReservation.ProfitMarkupAdditionalProducts.ToDecimalNullSafe(), 4);
            invoice.IsPaymentReceived = !logoReservation.ExtraPricePayOnDelivery.ToBoolNullSafe();
            return invoice;
        }

        private async Task<Invoice> CouponLine(LogoReservation logoReservation, int rowId, Location location)
        {
            var taxRate = await GetTaxRate(logoReservation.ReturnDate, location.Countryid);
            var couponAmount = logoReservation.CouponDiscountAmount.ToDecimalNullSafe();
            var couponAmountWithoutTax = couponAmount / (1 + taxRate / 100);

            var invoice = MapCommonInvoiceFields(new Invoice(), logoReservation);
            invoice.RowID = rowId;
            invoice.StockCode = ((int)StockCodes.CouponDiscount).ToString();
            invoice.RowDescription = "Kupon indirimi";
            invoice.Discount = 0M;
            invoice.InvoiceAmount = 0M;
            invoice.InvoiceTaxAmount = 0M;
            invoice.InvoiceTotalAmount = 0M;
            invoice.TaxRate = taxRate;
            invoice.UnitPrice = Math.Round(couponAmountWithoutTax, 4);
            invoice.RowAmount = Math.Round(couponAmountWithoutTax, 4);
            invoice.RowTaxAmount = Math.Round(couponAmountWithoutTax * (taxRate / 100), 4);
            invoice.RowTotalAmount = Math.Round(couponAmount, 4) * -1;
            invoice.VendorPrice = CalculateVendorAllowance(logoReservation.TotalPrice.ToDecimalNullSafe(), logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), logoReservation.RentalWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType, logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe());
            invoice.VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe();
            invoice.VendorProfitMarkup = Math.Round(logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), 2);
            invoice.IsPaymentReceived = true;
            return invoice;
        }

        /// <summary>
        /// Tek çekim komisyon satırı
        /// </summary>
        private Invoice BankCommissionLine(LogoReservation logoReservation, int rowId)
        {
            var invoiceAmount = logoReservation.TotalPaymentPrice;
            var commissionAmount = logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe();
            var amount = Math.Round(commissionAmount / (decimal)1.2, 4);
            var tax = Math.Round(commissionAmount - amount, 4);

            var invoice = MapCommonInvoiceFields(new Invoice(), logoReservation);
            invoice.RowID = rowId;
            invoice.StockCode = ((int)StockCodes.BankCommission).ToString();
            invoice.RowDescription = "Araç kiralama banka komisyonu";
            invoice.Discount = 0M;
            invoice.InvoiceAmount = Math.Round(invoiceAmount / (decimal)1.2, 4);
            invoice.InvoiceTaxAmount = Math.Round(invoiceAmount * (decimal)0.2, 4);
            invoice.InvoiceTotalAmount = Math.Round(invoiceAmount, 4);
            invoice.TaxRate = 20;
            invoice.UnitPrice = amount;
            invoice.RowAmount = amount;
            invoice.RowTaxAmount = tax;
            invoice.RowTotalAmount = Math.Round(commissionAmount, 4);
            invoice.TaxExceptionCode = null;
            invoice.VendorPrice = 0;
            invoice.VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe();
            invoice.VendorProfitMarkup = 0M; // Gönderilmesi durumunda komisyonun bir kısmını tedarikçi karşılar anlamına geliyormuş (26.10.2023 - Fatih Bey)
            invoice.IsPaymentReceived = true;
            return invoice;
        }

        private Invoice InstallmentFeeLine(LogoReservation logoReservation, int rowId, decimal taxRate)
        {
            var installmentFee = logoReservation.InstallmentFee.ToDecimalNullSafe();
            var amount = Math.Round(installmentFee / (decimal)1.2, 4);
            var tax = Math.Round(installmentFee - amount, 4);

            var invoice = MapCommonInvoiceFields(new Invoice(), logoReservation);
            invoice.RowID = rowId;
            invoice.StockCode = ((int)StockCodes.InstallmentFee).ToString();
            invoice.RowDescription = "Araç kiralama taksit komisyonu";
            invoice.Discount = 0M;
            invoice.InvoiceAmount = 0M;
            invoice.InvoiceTaxAmount = 0M;
            invoice.InvoiceTotalAmount = 0M;
            invoice.TaxRate = Math.Round(taxRate, 4);
            invoice.UnitPrice = amount;
            invoice.RowAmount = amount;
            invoice.RowTaxAmount = tax;
            invoice.RowTotalAmount = Math.Round(installmentFee, 4);
            invoice.TaxExceptionCode = null;
            invoice.VendorPrice = 0;
            invoice.VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe();
            invoice.VendorProfitMarkup = 0M;
            invoice.IsPaymentReceived = true;
            return invoice;
        }

        /// <summary>
        /// Taksitli işlem banka komisyonu satırı
        /// </summary>
        private Invoice InstallmentCommissionLine(LogoReservation logoReservation, int rowId, decimal taxRate)
        {
            var createDate = logoReservation.ReservationDate;
            var startDate = new DateTime(2026, 6, 3, 14, 0, 0);
            var isReservationDatePast = createDate >= startDate;

            var commissionAmount = logoReservation.InstallmentCommissionAmount.ToDecimalNullSafe();
            var amount = isReservationDatePast ? Math.Round(commissionAmount / (decimal)1.2, 4) : commissionAmount;
            var tax = isReservationDatePast ? Math.Round(commissionAmount - amount, 4) : decimal.Zero;

            var invoice = MapCommonInvoiceFields(new Invoice(), logoReservation);
            invoice.RowID = rowId;
            invoice.StockCode = ((int)StockCodes.BankInstallmentCommission).ToString();
            invoice.RowDescription = "Araç kiralama banka komisyonu";
            invoice.Discount = 0M;
            invoice.InvoiceAmount = 0M;
            invoice.InvoiceTaxAmount = 0M;
            invoice.InvoiceTotalAmount = 0M;
            invoice.TaxRate = isReservationDatePast ? taxRate : decimal.Zero;
            invoice.UnitPrice = amount;
            invoice.RowAmount = amount;
            invoice.RowTaxAmount = tax;
            invoice.RowTotalAmount = Math.Round(commissionAmount, 4);
            invoice.TaxExceptionCode = isReservationDatePast ? null : "350";
            invoice.VendorPrice = 0;
            invoice.VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe();
            invoice.VendorProfitMarkup = 0M; // Gönderilmesi durumunda komisyonun bir kısmını tedarikçi karşılar anlamına geliyormuş (26.10.2023 - Fatih Bey)
            invoice.IsPaymentReceived = true;
            return invoice;
        }

        private Invoice BrokerAllowance(
            int rowId, decimal couponDiscountPercent,
            decimal invoiceAmount, decimal invoiceTaxAmount, decimal invoiceTotalAmount,
            decimal taxRate, decimal unitPrice, decimal lineAmount, decimal lineTaxAmount, decimal lineTotalAmount,
            LogoReservation logoReservation)
        {
            var invoice = MapCommonInvoiceFields(new Invoice(), logoReservation);
            invoice.RowID = rowId;
            invoice.StockCode = ((int)StockCodes.BrokerAllowance).ToString();
            invoice.RowDescription = "AKŞ Hakediş";
            invoice.Discount = Math.Round(couponDiscountPercent.ToDecimalNullSafe(), 4);
            invoice.InvoiceAmount = Math.Round(invoiceAmount, 4);
            invoice.InvoiceTaxAmount = Math.Round(invoiceTaxAmount, 4);
            invoice.InvoiceTotalAmount = Math.Round(invoiceTotalAmount, 4);
            invoice.TaxRate = Math.Round(taxRate, 4);
            invoice.UnitPrice = Math.Round(unitPrice, 4);
            invoice.RowAmount = Math.Round(lineAmount, 4);
            invoice.RowTaxAmount = Math.Round(lineTaxAmount, 4);
            invoice.RowTotalAmount = Math.Round(lineTotalAmount, 4);
            invoice.VendorPrice = CalculateVendorAllowance(logoReservation.TotalPrice.ToDecimalNullSafe(), logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), logoReservation.RentalWorkingType.ToIntNullSafe(), logoReservation.CouponDiscountType, logoReservation.CouponVendorDiscountValue.ToDecimalNullSafe());
            invoice.VendorWorkingType = logoReservation.RentalWorkingType.ToIntNullSafe();
            invoice.VendorProfitMarkup = Math.Round(logoReservation.ProfitMarkupRental.ToDecimalNullSafe(), 2);
            invoice.IsPaymentReceived = true;
            return invoice;
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

        private async Task<decimal> GetTaxRate(DateTime? invoiceDateTime, int countryId)
        {
            var taxRate = await _locationService.GetTaxRateByCountryId(countryId);
            if (taxRate != null)
            {
                if (invoiceDateTime < new DateTime(2023, 7, 10))
                    return 18;
                return taxRate.TaxRate.ToDecimalNullSafe();
            }
            return invoiceDateTime < new DateTime(2023, 7, 10) ? 18 : 20;
        }

        private decimal CalculateBrokerAllowance(decimal rentalAmount, int workingType, decimal profitMarkup)
        {
            return workingType == 0
                ? Math.Round(rentalAmount * (profitMarkup / 100), 4)
                : rentalAmount - Math.Round(rentalAmount * 100 / (100 + profitMarkup), 4);
        }

        private decimal CalculateVendorAllowance(decimal price, decimal profitMarkup, int workingType, int? couponType, decimal couponVendorDiscount)
        {
            var vendorAllowance = workingType == 1
                ? Math.Round(price * ((100 - profitMarkup) / 100), 4)
                : Math.Round(price * 100 / (100 + profitMarkup), 4);
            return couponType == 0
                ? vendorAllowance - vendorAllowance * (couponVendorDiscount / 100)
                : vendorAllowance - couponVendorDiscount.ToDecimalNullSafe();
        }

        #endregion
    }
}
