using KolayCAR.Broker.Domain.Models;
using System;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.ReservationDtos
{
    public class ReservateNowDtoMobile
    {
        //client tarafından alınan parametreler
        public string LanguageCode { get; set; }
        public int LanguageId { get; set; }
        public string CurrencyCode { get; set; }
        public int CurrencyId { get; set; }
        public string ReservationToken { get; set; }

        public PaymentTypes PaymentType { get; set; } = PaymentTypes.PayAll;

        #region Müşteri Bilgileri
        public string IdentityNumber { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

        #endregion

        #region Ödeme Bilgileri
        public string CreditCardNumber { get; set; }
        public string CreditCardOwnerName { get; set; }
        private string _expireDate;
        public string ExpireDate
        {
            get
            {
                return _expireDate ?? $"{ExpireMonth}/{ExpireYear}";
            }
            set
            {
                if (value != null)
                {
                    _expireDate = value;
                    var dates = value.Split('/');
                    ExpireMonth = !string.IsNullOrEmpty(dates[0]) ? Convert.ToInt32(dates[0]) : -1;
                    ExpireYear = !string.IsNullOrEmpty(dates[1]) ? Convert.ToInt32(dates[1]) : -1;
                }
            }
        }
        public int? ExpireMonth { get; set; }
        public int? ExpireYear { get; set; }
        public string Cvc { get; set; }
        public string PaymentCode { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? PaidAmount { get; set; }
        public int? InstallmentCount { get; set; }
        public decimal? InstallmentPercent { get; set; }
        public bool? AdvencedPayment { get; set; }
        #endregion

        //resToken ile elde edilen parametreler
        public int VendorId { get; set; }
        public int? AgencyId { get; set; }
        public int PickupLocationId { get; set; }
        public string PickupLocation { get; set; }
        public int ReturnLocationId { get; set; }
        public string ReturnLocation { get; set; }
        public string PickupDate { get; set; }
        public string ReturnDate { get; set; }
        public string VendorName { get; set; }
        public decimal? ServiceCharge { get; set; } = 0m;
        public bool? FullCredit { get; set; }
        public string VehicleBrandName { get; set; }
        public string VehicleModelName { get; set; }
        public string VehicleName { get; set; }
        public string FuelName { get; set; }
        public string TransmissionName { get; set; }
        public int Person { get; set; }
        public string PersonName { get; set; }
        public int Baggage { get; set; }
        public string BaggageName { get; set; }
        public string CategoryName { get; set; }
        public string TypeName { get; set; }
        public string DailyPrice { get; set; }
        public string Deposit { get; set; }
        public string OneWayFee { get; set; }
        public string RentalDuration { get; set; }
        public string MinimumAge { get; set; }
        public string MinimumLicenseAge { get; set; }

        //dahil olmayanlar


        public string CouponCode { get; set; }
        public string CouponName { get; set; }
        public string ReservationNote { get; set; }
        public int? MemberId { get; set; }
        public string SkyScannerRedirectId { get; set; }
        public decimal? CouponDiscountAmount { get; set; } = 0m;
        public bool? InvoiceToDifferentAddress { get; set; } = false;
        public bool? ConfirmConditions { get; set; }
        public string CountryPhoneCode { get; set; }
        public string PhoneNumberWithoutCode { get; set; }
        public string PhoneNumber
        {
            get
            {
                return $"{CountryPhoneCode} {PhoneNumberWithoutCode}";
            }
        }
        public string FlightNumber { get; set; }
        public bool? ContactPermission { get; set; } = false;
        public bool? IsNonTurkishCitizen { get; set; } = false;
        public DateTime? BirthDay { get; set; }
        /// <summary>
        /// E => Erkek
        /// K => Kadın
        /// </summary>
        public string Gender { get; set; }
        #region Fatura Bilgileri
        public string Address { get; set; }
        public string Title { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string ZipCode { get; set; }
        #endregion
        public decimal? InstallmentCommissionAmount { get; set; }
        #region Extra Bilgileri
        public string ExtraJson { get; set; }
        public string ExtraNames { get; set; }
        public string ExtraAmount { get; set; }
        public string SelectedExtrasJson { get; set; }
        #endregion
        public string DeliveryType { get; set; }
        public string KmLimit { get; set; }
        public string OfficeServicePrice { get; set; }
        public string SpecialDailyPrice { get; set; }
        public string SpecialOneWayFee { get; set; }
        public string AdditionalProductPricePoa { get; set; }
        public string OneWayFeePoa { get; set; }
    }
}
