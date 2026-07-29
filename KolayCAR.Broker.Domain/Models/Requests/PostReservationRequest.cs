namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class PostReservationRequest : ReservationStepsBase
    {
        public int? AgencyId { get; set; }
        public int? SubAgencyId { get; set; }
        public string AgencyCode { get; set; }
        public string VehicleName { get; set; }
        public string ExtraList { get; set; }
        public int? CustomerInstutionTypeNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public string CustomerTelephone { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPersonalNumber { get; set; }
        public string CustomerNote { get; set; }
        public string CustomerExplanation { get; set; }
        public string CompanyTitle { get; set; }
        public string CustomerAddress { get; set; }
        public string CompanyTaxOffice { get; set; }
        public string CompanyTaxNumber { get; set; }
        public string FlightNumberArrival { get; set; }
        public string FlightNumberDeparture { get; set; }
        public string CustomerIPAddress { get; set; }
        public float PaidAmount { get; set; }
        public string UpdateReservationNumber { get; set; }
        public bool? CreditCardPaymentTypeActive { get; set; } = false;
        public bool? AdvancePaymentTypeActive { get; set; } = false;
        public int? BankId { get; set; }
        public int? BankVendorId { get; set; }
        public string CreditCardHolder { get; set; }
        public string CreditCardNumber { get; set; }
        public int? ExpiredYear { get; set; }
        public int? ExpiredMonth { get; set; }
        public string SecurityCode { get; set; }
        public int? InstallmentCount { get; set; }
        public bool? ThreeDPaymentActive { get; set; } = false;
        public string ThreeDStatus { get; set; }
        public string ThreeDAuth { get; set; }
        public string ThreeDLevel { get; set; }
        public string ThreeDTxnId { get; set; }
        public string ThreeDMd { get; set; }
        public string ThreeDPnOrInfo { get; set; }
        public float SpecialDailyPrice { get; set; } = 0;
        public float SpecialOneWayFee { get; set; } = 0;
        public bool? IsCommissionFreePrice { get; set; } = false;
        public float ExtraAmount { get; set; }
        public bool SendReservationMail { get; set; }
        public string CustomerBirthDay { get; set; }
        public string DepartureInfo { get; set; }
        public string VehicleImageURL { get; set; }
        public string AgencyReservationReference { get; set; }
        public PaymentTypes PaymentType { get; set; }
        public bool ExtraPricePayToDelivery { get; set; }
        public bool OneWayFeePayToDelivery { get; set; }
        public bool IsSpecialWebSiteAgency { get; set; }
        public string SkyscannerRedirectID { get; set; }
        public bool CommercialAllowance { get; set; }
        public int? ReservationSourceId { get; set; }
        public bool AdvancedPaymentWithoutPayment { get; set; }
        public float PaidAmountAfterUsingCouponCode { get; set; }
        public bool HighAmountDiscountActive { get; set; }
        public bool? FullCredit { get; set; }
        [System.Text.Json.Serialization.JsonConverter(typeof(CreditTypeCodeJsonConverter))]
        [Newtonsoft.Json.JsonConverter(typeof(CreditTypeNewtonsoftJsonConverter))]
        public CreditType CreditType { get; set; }

        public string Bank { get; set; }
        public string BankAccountCode { get; set; }
        public string BankAccountingCode { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string CreditCardBank { get; set; }
        public string ProvisionNumber { get; set; }
        public string OrderNumber { get; set; }
        public bool IsRetry { get; set; }
        public bool? ContactPermission { get; set; }
        public decimal? InstallmentCommissionAmount { get; set; }
        public string CountryCode { get; set; }
        public string PaymentCode { get; set; }
        public string RequestId { get; set; }
        public string ExternalCreditCardInfo { get; set; }
        public bool SendAgencyReservationNumber { get; set; }
        public float? InstallmentFee { get; set; }
        public string? ApiExtras { get; set; }
        public int CouponDiscountType { get; set; }
        public decimal CouponDiscountValue { get; set; }
        public decimal CouponDiscountAmount { get; set; }
        public string UserAgent { get; set; }
        public float MarkupAmount { get; set; } = 0f;
        public PostReservationRequestV2 PostReservationRequestV2 { get; set; } = null;
    }
}
