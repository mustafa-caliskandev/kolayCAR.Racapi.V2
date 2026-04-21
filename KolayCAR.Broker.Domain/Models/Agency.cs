using System;

namespace KolayCAR.Broker.Domain.Models
{
    public class Agency : RestrictedAgency
    {
        public long AgencyId { get; set; } // post reservation da var
        public int SubAgencyId { get; set; }
        public string AgencyCode { get; set; } //post reservation da var
        public bool Active { get; set; }
        public string AgencyName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string MobilePhoneNumber { get; set; }
        public string Fax { get; set; }
        public string PostalCode { get; set; }
        public string PostOfficeBox { get; set; }
        public string Place { get; set; }
        public string CountryCode { get; set; }
        public string Tktue { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string TaxNumber { get; set; }
        public string TaxNumber2 { get; set; }
        public string Branch { get; set; }
        public DateTime? Barrier { get; set; }
        public int? Limit { get; set; }
        public DateTime? CreationDate { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public string TaxOffice { get; set; }
        public string OwnerName { get; set; }
        public string OwnerSurname { get; set; }
        public bool AgencyApplication { get; set; }
        public string Note { get; set; }
        public string Note2 { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string AuthorizedPersonName { get; set; }
        public string AgencyApiKey { get; set; }
        public string AgencyApiPassword { get; set; }
        public float AgencyCommissionAmount { get; set; }
        public AgencyCommissionTypes AgencyCommissionType { get; set; }
        public string Logo { get; set; }
        public UserRoles UserRole { get; set; }
        public bool LoginEnable { get; set; }
        public float ProfitMarkupSharingRateDailyPrice { get; set; }
        public float ProfitMarkupSharingRateAdditionalProducts { get; set; }
        public float ProfitMarkupSharingRateOneWayFee { get; set; }
        public bool CreditCardPaymentActive { get; set; }
        public bool PayAllActive { get; set; }
        public bool AdvancePaymentActive { get; set; }
        public bool CommissionFreePaymentActive { get; set; }
        public bool PayDeliveryActive { get; set; }
        public bool PayAgencyActive { get; set; }
        public int CreditCardDiscountPercent { get; set; }
        public bool FreePriceShowActive { get; set; }
        public bool FreePriceActive { get; set; }
        public bool AdvancePaymentAmountByAgencyCommissionActive { get; set; }
        public bool SendReservationMailToCustomerActive { get; set; }
        public bool IsRestrictedAPI { get; set; }
        public bool RentalAmountDeliveryPayment { get; set; }
        public bool OneWayAmountDeliveryPayment { get; set; }
        public bool AdditionalProductAmountDeliveryPayment { get; set; }
        public bool SendReservationMailToAgency { get; set; }
        public CurrencyTypes CurrencyType { get; set; }
        public bool ReservationSourceSelectActive { get; set; }
        public float? OptionalRentalAdvancePaymentPercent { get; set; }
        public float? OptionalAdditionalProductAdvancePaymentPercent { get; set; }
        public float? OptionalOneWayFeeAdvancePaymentPercent { get; set; }
        public float AgencyRentalProfitMarkup { get; set; }
        public bool CancellationPenaltyActive { get; set; }
        public bool SpecialParameters { get; set; }
        public bool FullCreditPermission { get; set; }
        public bool? IsActiveSendCheapestCar { get; set; }
    }

    public class RestrictedAgency
    {
    }

    public class AgencySettings
    {
        public bool AgencyRentalFeeTypeActive { get; set; }
        public bool AgencyExtraFeeTypeActive { get; set; }
        public bool AgencyOneWayFeeTypeActive { get; set; }
    }

    public enum AgencyCommissionTypes
    {
        CommissionCalculatedOnTotalPrice,
        CommissionCalculatedOnDailyPrice
    }

    public enum SurveyPostTypes
    {
        ToNoBody,
        ToCustomer,
        ToAgency
    }
}
