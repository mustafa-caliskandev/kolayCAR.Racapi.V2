using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models
{
    public class Vehicle : RestrictedVehicle
    {
        public string VendorPhone { get; set; }
        public string VendorEmail { get; set; }
        public string VendorLogo { get; set; }
        public List<Extra> Extras { get; set; }
        public float ServiceCharge { get; set; }
        public string VendorName { get; set; }
        public string ApiVendorName { get; set; }
        public bool IsFindeksRequired { get; set; }
        public int VendorId { get; set; }
        public string VehicleCode { get; set; }
        public float DailyPricePayNow { get; set; }
        public float TotalPricePayNow { get; set; }
        public UsingCouponCode UsingCouponCode { get; set; }
        public List<PaymentTypes> ActivePaymentTypes { get; set; }
        public float? OptionalRentalAdvancePaymentPercent { get; set; } //Eğer değer var ise peşinat ödemesi için kullanılan acente komisyon yüzdesi veya kar marjı alanarını ezer
        public float? OptionalAdditionalProductAdvancePaymentPercent { get; set; } //Eğer değer var ise peşinat ödemesi için kullanılan acente komisyon yüzdesi veya kar marjı alanarını ezer
        public float? OptionalOneWayFeeAdvancePaymentPercent { get; set; } //Eğer değer var ise peşinat ödemesi için kullanılan acente komisyon yüzdesi veya kar marjı alanarını ezer
        public float? VendorScore { get; set; }
        public int VendorCommentCount { get; set; }
        public int? DailyKMLimit { get; set; }
        public VendorTypes? VendorType { get; set; }
        public bool VendorCouponUsing { get; set; }
        public string CityOfPickupLocation { get; set; }
        public string CityOfReturnLocation { get; set; }
        public string VehicleModelName { get; set; }
        public string VehicleBrandName { get; set; }
        public float OriginalPrice { get; set; }
        public float ApparentPrice { get; set; }
        public int? VehicleClassNo { get; set; }
        public string VehicleGroupName { get; set; }
        public bool? FindeksRequired { get; set; }
        public bool? IsAdditionalProductPricePOA { get; set; }
        public bool? IsOneWayFeePOA { get; set; }
        public float? ApiDailyPrice { get; set; }
        public int? ApiDeliveryTypeId { get; set; }
        public int? OldDriverMaxAge { get; set; }
        public int? OldDriverMinAge { get; set; }
        public int? YoungDriverMinAge { get; set; }
        public int? YoungDriverMaxAge { get; set; }
        public int? SimilarVehicleId { get; set; }
        public string? SpecialProfitApplied { get; set; }
        public VendorWorkingTypes RentalWorkingTypes { get; set; }
        public float ProfitMarkupDailyPrice { get; set; }
        public OfficeHours PickUpOfficeHours { get; set; }
        public OfficeHours ReturnOfficeHours { get; set; }
        public string PickupLocationAddress { get; set; }
        public string ReturnLocationAddress { get; set; }
        public bool PassportRequired { get; set; }
    }

    public class RestrictedVehicle : VehicleListItem
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string GoogleLink { get; set; }
        public string ShortAddress { get; set; }
        public string SpecialVendorName { get; set; }
        public string SpecialVendorId { get; set; }
        public string SpecialVendorLogo { get; set; }
        public int BaseVendorId { get; set; }
        public bool FullCredit { get; set; } = false;
        public int PickupLocationId { get; set; }
        public string PickupLocationName { get; set; }
        public string PickupLocationCode { get; set; }
        public int ReturnLocationId { get; set; }
        public string ReturnLocationName { get; set; }
        public string ReturnLocationCode { get; set; }
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
        public int? TotalKMLimit { get; set; }
        public string CurrencyCode { get; set; }
        public PriorityTypes Priority { get; set; } = PriorityTypes.P0;
        public DeliveryType DeliveryType { get; set; }
        public List<VehicleFeaturesSorting> VehicleFeaturesSorting { get; set; }
        public CurrencyTypes BaseVendorCurrencyTypes { get; set; }

    }

    public class VehicleListItem
    {
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string VehicleDescription { get; set; }
        public string SippCode { get; set; }
        public List<VehicleImage> VehicleImages { get; set; }
        public VehicleTypes VehicleType { get; set; }
        public string VehicleTypeName { get; set; }
        public TransmissionTypes TransmissionType { get; set; }
        public string TransmissionTypeName { get; set; }
        public VehicleCategoryTypes VehicleCategoryType { get; set; }
        public string VehicleCategoryTypeName { get; set; }
        public string VehicleCategoryImagePath { get; set; }
        public PassangerQuantityTypes PassangerQuantityType { get; set; }
        public string PassangerQuantityName { get; set; }
        public FuelTypes FuelType { get; set; }
        public string FuelTypeName { get; set; }
        public BaggageQuantityTypes BaggageQuantityType { get; set; }
        public string BaggageQuantityName { get; set; }
        public bool IsThereAirCondition { get; set; }
        public bool? VendorFlightPassRequired { get; set; }
    }

    public class VehicleImage
    {
        public string Url { get; set; }
    }

    public class RentalCondition
    {
        public int ConditionId { get; set; }
        public string ConditionCode { get; set; }
        public string ConditionName { get; set; }
        public int ConditionSequence { get; set; }
        public string IconPath { get; set; }
    }
    public class VehicleFeaturesSorting
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
    }



    public enum VehicleTypes
    {
        None = -1,
        Cabrio,
        Coupe,
        ThreeDoorHatchback,
        FiveDoorHatchback,
        Sedan,
        StationWagon,
        SUV, //DB de Suv/Jeep
        Van,
        Pickup,
        SUV2,  //01.11.2024
        ECOSUV,
        Minivan,
        TwoToThreeDoor,
        TwoToFourDoor,
        FourtoFiveDoor,
        Crossover,
        Monospace,
        Convertible,
        SixOrMorePassengerVan,
        Special,
        Coupe2,
        OpenAirAllTerrain,
        PickupSingleOrExtendedCabTwoDoor,
        PickupDoubleCabFourDoor,
        Sport,
        Roadster,
        SpecialOfferCar,
        LimousineSedan,
        CommercialVanTruck,
        RecreationalVehicle,
        WagonEstate,
        TwoWheelVehicle,
        MotorHome
    }

    public enum TransmissionTypes
    {
        None = -1,
        Manuel,
        Automatic,
        SemiAutomatic
    }

    public enum VehicleCategoryTypes
    {
        None = -1,
        Economic,
        Compact,
        Standard,
        Luxury,
        Minibus,
        SUV4x4,
        Intermediate,
        FullSize,
        Minivan,
        SUV4x2,
        SUV,
        Caravan,
        Premium,
        CompactElite,
        Prestige
    }
    public enum PassangerQuantityTypes
    {
        None = -1,
        OnePerson,
        TwoPerson,
        ThreePerson,
        FourPerson,
        FivePerson,
        SixPerson,
        SevenPerson,
        EightPerson,
        NinePerson,
        TenPerson,
        ElevenPerson,
        TwelvePerson,
        ThirteenPerson,
        FourteenPerson,
        FifteenPerson,
        SixteenPerson,
        SeventeenPerson,
        EighteenPerson,
        NineteenPerson,
        TwentyPerson
    }

    public enum FuelTypes
    {
        None = -1,
        Gasoline,
        GasolineAndLPG,
        Diesel,
        Electric,
        HybritDiesel,
        HybritGasoline,
        GasolineAndDiesel,
        Undefined, //01.11.2024
        Hybrid,
        ElectricUnder250MilesOr400Km,
        ElectricOver250MilesOr400Km,
        Hydrogen,
        MultiFuel,
        Ethanol,
        BatteryElectricVehicle
    }

    public enum BaggageQuantityTypes
    {
        None = -1,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight
    }

    //TODO: Payment sınıfına taşınacak
    public enum CreditCardPaymentTypes
    {
        AdvancePaymentPassivePayAllActive,
        AdvancePaymentActivePayAllPassive,
        AdvancePaymentActivePayAllActive
    }

    public enum DiscountTypes
    {
        None = -1,
        Coupon
    }

    public enum PriorityTypes
    {
        P0,
        P1
    }
    #region YolcuDeliveryTypes
    public enum DeliveryType
    {
        None,
        NonTerminalMeetAndGreet,
        MeetAndGreet,
        InTerminalOffice,
        DeliveredToAddress,
        FromOffice,
        NonTerminalValet
    }
    #endregion
}
