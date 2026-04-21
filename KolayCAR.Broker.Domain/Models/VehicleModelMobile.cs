using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models
{
    public class VehicleModelMobile
    {
        public List<Filter> Filters { get; set; }
        public List<FastFilter> FastFilters { get; set; }
        public List<OrderOption> OrderOptions { get; set; }
        public List<VendorLocation> VendorLocations { get; set; }
        public List<VehicleModel> Vehicles { get; set; }
    }

    public class VehicleModel
    {
        public string ReservationToken { get; set; }
        public string VehicleImage { get; set; }
        public string VehicleName { get; set; }
        public VehicleCategoryTypes VehicleCategoryType { get; set; }
        public string VehicleCategoryTypeName { get; set; }
        public FuelTypes VehicleFuelType { get; set; }
        public string VehicleFuelTypeName { get; set; }
        public TransmissionTypes VehicleTransmissionType { get; set; }
        public string VehicleTransmissionTypeName { get; set; }
        public PassangerQuantityTypes PassangerQuantityType { get; set; }
        public string PassangerQuantityName { get; set; }
        public BaggageQuantityTypes BaggageQuantityType { get; set; }
        public string BaggageQuantityName { get; set; }
        public VehicleTypes VehicleType { get; set; }
        public string VehicleTypeName { get; set; }
        public float DailyPrice { get; set; }
        public int RentalDuration { get; set; }
        public float TotalPrice { get; set; }
        public float OneWayFee { get; set; }
        public float ServiceCharge { get; set; }
        public string VendorLogo { get; set; }
        public string VendorName { get; set; }
        public float? VendorScore { get; set; }
        public int VendorCommentCount { get; set; }
        public int? TotalKmLimit { get; set; }
        public int? VendorMinimumDriverAge { get; set; }
        public int? VendorMinimumDrivingLicenseAge { get; set; }
        public float? DepositPrice { get; set; }
        public string CurrencyCode { get; set; }
        public int CurrencyId { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public OfficeLocation VendorOfficeLocation { get; set; }
        public List<FeaturesSorting> VehicleDetails { get; set; }
        public List<FeaturesSorting> VehicleFeatures { get; set; }
        public List<Badge> Badges { get; set; }
        public List<SortableParam> SortableParams { get; set; }
        public Promotions VendorPromotion { get; set; }
        public MobileReviewsModel Reviews { get; set; }
    }

    public class SortableParam
    {
        public int DataType { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
    }

    public class Badge
    {
        public int ConditionId { get; set; }
        public int Order { get; set; }
        public string Text { get; set; }
        public string BorderColor { get; set; }
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }
        public string IconPath { get; set; }
    }

    public class Filter
    {
        public int Order { get; set; }
        public string Icon { get; set; }
        public string Header { get; set; }
        public List<ChildFeatures> Children { get; set; }
    }

    public class FastFilter
    {
        public string Icon { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public List<ChildFeatures> Children { get; set; }
    }

    public class OrderOption
    {
        public int? Order { get; set; }
        public string Icon { get; set; }
        public string TargetName { get; set; }
        public string Text { get; set; }
        public int SortType { get; set; }
        public bool? IsDefault { get; set; }
    }

    public class Promotion
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class VendorLocation
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    public class ChildFeatures
    {
        public string Icon { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public class OfficeLocation
    {
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string GoogleLink { get; set; }
        public string ShortAddress { get; set; }
    }

    public class FeaturesSorting
    {
        public int? Order { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
        public string Key { get; set; }
        public string ParentKey { get; set; }
        public string Header { get; set; }
    }

    public class Promotions
    {
        public string Code { get; set; }
        public int? Order { get; set; }
        public string Text { get; set; }
    }

    public enum SortTypes
    {
        IncreaseOrder = 0, //Ascending
        DecreaseOrder = 1, //Descending
    }

    public enum DataTypes
    {
        Int = 1,
        String = 2,
        Decimal = 3,
        Date = 4
    }

    public enum FilterModelType
    {
        Filter = 1,
        FastFilter = 2,
        OrderOptions = 3
    }

    public enum VehicleFeatures
    {
        VehicleCategoryType = 1,
        FuelType = 2,
        TransmissionType = 3,
        PassengerQuantityType = 4,
        BaggageQuantityType = 5,
        VehicleType = 6
    }

    public enum VehicleDetails
    {
        VehicleDepositPrice = 1,
        VehicleMinimumDriverAge = 2,
        VehicleMinimumLicenseAge = 3,
        VehicleDeliveryType = 4,
        VehicleTotalKm = 5
    }
}
