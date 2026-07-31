using KolayCAR.Broker.Domain.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.RequestDtos
{
    public class PostReservationRequestDto
    {
        public List<ExtraMobileSuccess> Extras { get; set; }
        public InvoiceData InvoiceData { get; set; }
        public string LanguageCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public string CustomerTelephone { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPersonalNumber { get; set; }
        public string CustomerNote { get; set; }
        public string FlightNumberArrival { get; set; }
        public float PaidAmount { get; set; }
        public bool? CreditCardPaymentTypeActive { get; set; } = false;
        public bool? AdvancePaymentTypeActive { get; set; } = false;
        public string CreditCardHolder { get; set; }
        public string CreditCardNumber { get; set; }
        public int? ExpiredYear { get; set; }
        public int? ExpiredMonth { get; set; }
        public string SecurityCode { get; set; }
        public int? InstallmentCount { get; set; }
        public bool? ThreeDPaymentActive { get; set; } = false;
        public float SpecialDailyPrice { get; set; } = -1;
        public float SpecialOneWayFee { get; set; } = -1;
        public float DailyPrice { get; set; } = 0f;
        public float MarkupAmount { get; set; } = 0f;
        public bool? IsCommissionFreePrice { get; set; } = false;
        public float ExtraAmount { get; set; }
        public bool SendReservationMail { get; set; }
        public string CustomerBirthDay { get; set; }
        public PaymentTypes? PaymentType { get; set; }
        public bool ExtraPricePayToDelivery { get; set; }
        public bool OneWayFeePayToDelivery { get; set; } = true;
        public bool IsSpecialWebSiteAgency { get; set; } = false;
        public bool CommercialAllowance { get; set; }
        public bool AdvancedPaymentWithoutPayment { get; set; } = false;
        public float PaidAmountAfterUsingCouponCode { get; set; }
        public bool HighAmountDiscountActive { get; set; } = true;
        public bool? FullCredit { get; set; } = false;
        public int PickupLocationId { get; set; }
        public int ReturnLocationId { get; set; }
        public string ReservationToken { get; set; }
        public string Bank { get; set; }
        public string CouponCode { get; set; }
        public string BankAccountCode { get; set; } = "";
        public string BankAccountingCode { get; set; } = "";
        public string CreditCardBank { get; set; }
        public string ProvisionNumber { get; set; }
        public string OrderNumber { get; set; } = "";
        public bool? ContactPermission { get; set; }
        public decimal? InstallmentCommissionAmount { get; set; }
        public string RequestId { get; set; }
        public float? InstallmentFee { get; set; }
        public int CouponDiscountType { get; set; }
        public decimal CouponDiscountValue { get; set; }
        public decimal CouponDiscountAmount { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public bool HasRefundableCoupon { get; set; } = false;
        public decimal? CouponBonusAmount { get; set; }
    }

    public class InvoiceData
    {
        #region Fatura Bilgileri
        public string Address { get; set; }
        public string Title { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        //public string ZipCode { get; set; }
        #endregion
    }

    public class ExtraMobileSuccess
    {
        public int ExtraId { get; set; }
        public ExtraRentalTypes? ExtraRentalType { get; set; }
        public float Price { get; set; }
        public int RentalDuration { get; set; }
    }
}
