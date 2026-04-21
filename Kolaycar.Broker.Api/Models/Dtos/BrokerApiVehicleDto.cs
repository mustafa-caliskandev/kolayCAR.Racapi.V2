using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models.Dtos
{
    public class BrokerApiVehicleDto : RestrictedVehicle
    {
        public int? OrderNo { get; set; }

        public string VendorPhone { get; set; }
        public string VendorEmail { get; set; }
        public string VendorLogo { get; set; }
        public List<BrokerApiExtraDto> Extras { get; set; }
        public float ServiceCharge { get; set; }
        public string VendorName { get; set; }
        public bool IsFindeksRequired { get; set; }
        public int VendorId { get; set; }
        public string VehicleCode { get; set; }
        public float DailyPricePayNow { get; set; }
        public float TotalPricePayNow { get; set; }
        public UsingCouponCode UsingCouponCode { get; set; }
        public List<int> ActivePaymentTypes { get; set; }
        public float? OptionalRentalAdvancePaymentPercent { get; set; } //Eğer değer var ise peşinat ödemesi için kullanılan acente komisyon yüzdesi veya kar marjı alanarını ezer
        public float? OptionalAdditionalProductAdvancePaymentPercent { get; set; } //Eğer değer var ise peşinat ödemesi için kullanılan acente komisyon yüzdesi veya kar marjı alanarını ezer
        public float? OptionalOneWayFeeAdvancePaymentPercent { get; set; } //Eğer değer var ise peşinat ödemesi için kullanılan acente komisyon yüzdesi veya kar marjı alanarını ezer
        public float? VendorScore { get; set; }
        public int VendorCommentCount { get; set; }
        public bool VendorCouponUsing { get; set; }
        public string CityOfPickupLocation { get; set; }
        public string CityOfReturnLocation { get; set; }
        public string VehicleBrandName { get; set; }
        public string VehicleModelName { get; set; }
        public int? DailyKmLimit { get; set; }
        public bool IsPopular { get; set; } = false;
        public bool IsCampaignVehicle { get; set; } = false;

        public float OriginalPrice { get; set; }
    }

    public class RestrictedVehicle : VehicleListItem
    {
        public string SpecialVendorName { get; set; }
        public string SpecialVendorId { get; set; }
        public string SpecialVendorLogo { get; set; }
        public string BaseVendorId { get; set; }
        public bool FullCredit { get; set; } = false;
        public int PickupLocationId { get; set; }
        public string PickupLocationName { get; set; }
        public int ReturnLocationId { get; set; }
        public string ReturnLocationName { get; set; }
        public DateTime PickupDateTime { get; set; }
        public DateTime ReturnDateTime { get; set; }
        public int RentalDuration { get; set; }
        public float DailyPrice { get; set; }
        public float OneWayFee { get; set; }
        public float ExtraPrice { get; set; }
        public float TotalPrice { get; set; }
        public bool IsAvailable { get; set; }
        public List<RentalCondition> RentalConditions { get; set; }
        public int? VendorMinimumDriverAge { get; set; }
        public int? VendorMinimumDrivingLicenseAge { get; set; }
        public float? DepositPrice { get; set; }
        public string ReservationToken { get; set; }
        public bool IsOffice { get; set; }
        public bool IsAirport { get; set; }
        public bool DepositCreditCardRequired { get; set; }
        public bool PersonalNumberRequired { get; set; }
        public int? TotalKmLimit { get; set; }
        public string CurrencyCode { get; set; }
        public int Priority { get; set; } = 0;
        public bool? IsAdditionalProductPricePOA { get; set; }
        public bool? IsOneWayFeePOA { get; set; }
    }

    public class VehicleListItem
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string VehicleDescription { get; set; }
        public string SippCode { get; set; }
        public List<VehicleImage> VehicleImages { get; set; }
        public int VehicleType { get; set; }
        public string VehicleTypeName { get; set; }
        public int TransmissionType { get; set; }
        public string TransmissionTypeName { get; set; }
        public int VehicleCategoryType { get; set; }
        public string VehicleCategoryTypeName { get; set; }
        public int PassangerQuantityType { get; set; }
        public string PassangerQuantityName { get; set; }
        public int FuelType { get; set; }
        public string FuelTypeName { get; set; }
        public int BaggageQuantityType { get; set; }
        public string BaggageQuantityName { get; set; }
        public bool IsThereAirCondition { get; set; }
    }

    public class RentalCondition
    {
        public int ConditionId { get; set; }
        public string ConditionCode { get; set; }
        public string ConditionName { get; set; }
        public int ConditionSequence { get; set; }
    }

    public class UsingCouponCode
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int CouponUsageResultType { get; set; }
        public int CouponId { get; set; }
        public string CouponCode { get; set; }
        public float DiscountValue { get; set; }
        public float DiscountAmount { get; set; }
        public int CouponDiscountType { get; set; }
        public float TotalPriceBeforeDiscount { get; set; }
        public float TotalPricePayNowBeforeDiscount { get; set; }
    }

    public class VehicleImage
    {
        public string Url { get; set; }
    }

}
