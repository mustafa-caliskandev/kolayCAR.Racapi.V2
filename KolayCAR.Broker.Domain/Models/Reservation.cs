using KolayCAR.Broker.Domain.Models.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace KolayCAR.Broker.Domain.Models
{
    public class Reservation : RestrictedReservation
    {
        public int? AgencyId { get; set; }
        public long ReservationId { get; set; }
        public string APIReservationNumber { get; set; }
        public int? MemberId { get; set; }
        public VendorTypes VendorType { get; set; }
        public string VendorPhone { get; set; }
        public string VendorEmail { get; set; }
        public string VendorName { get; set; }
        public string VendorLogo { get; set; }
        public string AgencyCode { get; set; }
        public int VendorId { get; set; }
        public int APIVendorId { get; set; }
        public string APIVendorName { get; set; }
        public int VehicleId { get; set; }
        public CurrencyTypes CurrencyType { get; set; }
        public LanguageTypes LanguageType { get; set; }
        public bool BulletinActive { get; set; }
        public string ExtraIds { get; set; }
        public string IpAddress { get; set; }
        public string SkyScannerRedirectId { get; set; }
        public string ReservationStatusHistory { get; set; }
        public string PdfUrl { get; set; }
        public bool ReservationPostedToAPI { get; set; }
        public bool APIReservationSuccessfully { get; set; }
        public float APIDailyPrice { get; set; }
        public float APIExtraAmount { get; set; }
        public float APIOneWayFee { get; set; }
        public float APITotalAmount { get; set; }
        public float AgencyCommission { get; set; }
        public float ProfitMarkup { get; set; }
        public string AgencyName { get; set; }
        public bool APIReservationCancel { get; set; }
        public float ServiceCharge { get; set; }
        public CurrencyTypes ServiceChargeCurrencyType { get; set; }
        public string APIVendorPickupAddress { get; set; }
        public string APIVendorReturnAddress { get; set; }
        public string APIVendorPickupPhone { get; set; }
        public string APIVendorReturnPhone { get; set; }
        public string APIMessage { get; set; }
        public string APIReferenceCode { get; set; }
        public string APIReferenceCode2 { get; set; }
        public string APIReferenceCode3 { get; set; }
        public CurrencyTypes APICurrencyType { get; set; }
        public string APICurrencyCode { get; set; }
        public float ExchangeRate { get; set; }
        public float APIExchangeRate { get; set; }
        public bool APIPhoneActive { get; set; }
        public string PaymentResultMessage { get; set; }
        public string PaymentResultCode { get; set; }
        public string Bank { get; set; }
        public int? BankId { get; set; }
        public string ProvisionNumber { get; set; }
        public float? PaymentAmount { get; set; }
        public string AlertErrorCode { get; set; }
        public string LogResultNumber { get; set; }
        public string LogErrorCode { get; set; }
        public int CreditCardDiscountPercent { get; set; }
        public PaymentTypes PaymentType { get; set; }
        public string CustomerTitle { get; set; }
        public string CustomerTaxOffice { get; set; }
        public string CustomerTaxNumber { get; set; }
        public int Score { get; set; }
        public int? CouponId { get; set; }
        public float CouponDiscountValue { get; set; }
        public CouponDiscountTypes? CouponDiscountType { get; set; }
        public CurrencyTypes? CouponCurrencyType { get; set; }
        public List<Addition> Additions { get; set; }
        public float APIPaidAmount { get; set; }
        public float SpecialDailyPrice { get; set; }
        public float SpecialOneWayFee { get; set; }
        public CurrencyTypes BaseRequestCurrencyType { get; set; }
        public string BaseRequestCurrencyCode { get; set; }
        public string SIPPCode { get; set; }
        public string VehicleCode { get; set; }
        public bool UseOnlyDefaultCurrency { get; set; }
        public bool IsSpecialWebSiteAgency { get; set; }
        public string MemberName { get; set; }
        public int UpdateCount { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public ReservationToken ReservationToken { get; set; }
        public bool CommercialAllowance { get; set; }
        public int InstallmentCount { get; set; }
        public string DefaultCustomerMailAddress { get; set; }
        public VendorWorkingTypes RentalWorkingType { get; set; }
        public VendorWorkingTypes AdditionalProductWorkingType { get; set; }
        public VendorWorkingTypes OneWayFeeWorkingType { get; set; }
        public float AgencyRentalProfitMarkup { get; set; }
        public string EncryptedReservationId { get; set; }
        public bool AdvancedPaymentWithoutPayment { get; set; }
        public _penaltyStatus? PenaltyStatus { get; set; }
        //public string SurveyComment { get; set; }
        public float VendorScore { get; set; }
        public int CommentCount { get; set; }
        public string DetailPageContentUrl { get; set; }
        public string CityOfPickupLocation { get; set; }
        public string CityOfReturnLocation { get; set; }
        public string VehicleBrandName { get; set; }
        public string VehicleModelName { get; set; }
        public string INVOICENUMBER { get; set; }
        public string AGENCYINVOICENUMBER { get; set; }
        public string ReturnInvoiceNumber { get; set; }
        public string AgencyReturnInvoiceNumber { get; set; }
        public string FriendlyReservationNumber { get; set; }
        public string ProvizyonNo { get; set; }
        public string CreditCardBank { get; set; }
        public float? VendorDiscountValue { get; set; }
        public string Canceller { get; set; }
        public string CreditCardNumber { get; set; }
        public string CardInfo { get; set; }
        public decimal? InstallmentCommissionAmount { get; set; }
        public CreditType CreditType { get; set; }
        public bool IsFullCredit { get; set; }
        [NotMapped]
        public string VehicleDescription { get; set; }
        public string CouponName { get; set; }
        public decimal? PremiumExtraAmount { get; set; }
        public string PaymentCode { get; set; }
        public string PickupOfficeWorkingHours { get; set; }
        public string ReturnOfficeWorkingHours { get; set; }
        public int? ApiDeliveryTypeId { get; set; }
    }

    public class RestrictedReservation
    {
        public string ReservationNumber { get; set; }
        public string AgencyReservationReference { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int PickupLocationId { get; set; }
        public int ReturnLocationId { get; set; }
        public string PickupAddress { get; set; }
        public string ReturnAddress { get; set; }
        public string Vendor { get; set; }
        public string VendorPickupPhone { get; set; }
        public string VendorReturnPhone { get; set; }
        public string CurrencyCode { get; set; }
        public int RentalDuration { get; set; }
        public float DailyPrice { get; set; }
        public float ExtraPrice { get; set; }
        public float OneWayFee { get; set; }
        public float TotalPrice { get; set; }
        public float PaidAmount { get; set; }
        public List<ReservationExtra> ReservationExtras { get; set; }
        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerMail { get; set; }
        public string CustomerIdentityNumber { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerArrivalFlightNumber { get; set; }
        public string CustomerReturnFlightNumber { get; set; }
        public string DepartureInfo { get; set; }
        public string CustomerNote { get; set; }
        public string VehicleName { get; set; }
        public string PickupLocationName { get; set; }
        public string ReturnLocationName { get; set; }
        public List<ReservationStatusHistory> ReservationStatusHistories { get; set; }
        public ReservationStatusTypes ReservationStatusType { get; set; }
        public string ReservationStatusNote { get; set; }
        public string StatusName { get; set; }
        public bool IsCancelable { get; set; }
        public float? DepositPrice { get; set; }
        public int VendorMinimumDriverAge { get; set; }
        public int VendorMinimumDrivingLicenseAge { get; set; }
        public bool SendReservationMail { get; set; }
        public DateTime? CustomerBirthday { get; set; }
        public string VehicleImageUrl { get; set; }
        public bool IsOffice { get; set; }
        public bool IsAirport { get; set; }
        public FuelTypes FuelType { get; set; }
        public string FuelTypeName { get; set; }
        public TransmissionTypes TransmissionType { get; set; }
        public string TransmissionTypeName { get; set; }
        public VehicleTypes VehicleType { get; set; }
        public string VehicleTypeName { get; set; }
        public VehicleCategoryTypes VehicleCategoryType { get; set; }
        public string VehicleCategoryTypeName { get; set; }
        public PassangerQuantityTypes PassangerQuantityType { get; set; }
        public string PassangerQuantityTypeName { get; set; }
        public BaggageQuantityTypes BaggageQuantityType { get; set; }
        public string BaggageQuantityTypeName { get; set; }
        public bool DepositCreditCardRequired { get; set; }
        public bool ExtraPricePayToDelivery { get; set; }
        public bool OneWayFeePayToDelivery { get; set; }
        public int? ReservationSourceId { get; set; }
        public string ReservationSourceName { get; set; }
        public string CouponCode { get; set; }
        public float CouponDiscountAmount { get; set; }
        public int? TotalKMLimit { get; set; }
        public int CancellationPenaltyRate { get; set; }
        public float CancellationRefundedAmount { get; set; }
        public int CancellationPenaltyHour { get; set; }
        public bool? PaymentRefundSuccess { get; set; }
        public string ReservationTokenText { get; set; }
        public float CancellationPenaltyAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public bool? VendorFlightPassRequired { get; set; }
        public string ExternalCreditCardInfo { get; set; }
    }

    public enum CurrencyTypes
    {
        TRY,
        USD,
        EUR,
        GBP,
        RUB,
        JPY,
        SAR,
        CHF,
        AED,
        AUD,
        BGN,
        CAD,
        CZK,
        DKK,
        HKD,
        HUF,
        ILS,
        ISK,
        MXN,
        NOK,
        NZD,
        PLN,
        QAR,
        RON,
        RSD,
        SEK,
        SGD,
        THB,
        ZAR,
        KRW,
        COP,
        PHP,
        INR,
        CLP,
        AZN,
        BAM,
        BRL,
        GEL,
        EGP,
        MDL,
        UAH,
        MAD
    }

    public enum LanguageTypes
    {
        TR,
        EN,
        DE,
        NL,
        FR,
        RU,
        AR
    }

    public enum PaymentTypes
    {
        PayAll,
        AdvancePayment,
        CommissionFree,
        PayOnDelivery,
        PayToAgency
    }

    public enum ReservationStatusTypes
    {
        ReservationReceived,
        Approved,
        PaymentCompleted,
        Cancelled,
        WaitingForPayment,
        DeliveredFromReservation,
        Unapproved
    }

    public enum CouponStatusType
    {
        all,
        used,
        expired
    }
}
