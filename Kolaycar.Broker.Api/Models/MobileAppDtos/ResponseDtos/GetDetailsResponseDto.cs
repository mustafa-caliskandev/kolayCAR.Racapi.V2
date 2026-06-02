using KolayCAR.Broker.API.Models.MobileAppDtos.GetDetailsDtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos;
using KolayCAR.Broker.API.Models.MobileAppModels;
using KolayCAR.Broker.Domain.Models;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.ResponseDtos
{
    public class GetDetailsResponseDto
    {
        public string SearchId { get; set; }
        public List<VehicleFeature> VehicleFeatures { get; set; }
        public List<VehicleDetail> VehicleDetails { get; set; }
        public List<VehicleBadge> VehicleBadges { get; set; }
        public MobileVendorDetails VendorDetails { get; set; }
        public MobilePriceDetails PriceDetails { get; set; }
        public PriceInformationSuccess AllPriceDetails { get; set; }
        public MobileRentalConditions RentalConditions { get; set; }
        public MobileRentalConditionsNew RentalConditionsV2 { get; set; }
        public MobileReservationDetails ReservationDetails { get; set; }
        public List<Extra> Extras { get; set; }
        public List<Extra> Insurances { get; set; }
        public List<RequiredDocument> RequiredDocumentsOnDelivery { get; set; }
        public List<SpecialAdvantage> SpecialAdvantages { get; set; }
        public VendorLocationConditionDto ReservationNote { get; set; }
        public VehiclePromotion VehiclePromotion { get; set; }
        public bool IsReservationTokenChange { get; set; } = false;
        public bool IsPriceChanged { get; set; } = false;
        public float OldTotalPrice { get; set; } = default(float);
    }

    public class SpecialAdvantage
    {
        public string Parameter { get; set; }
        public string Value { get; set; }
        public string IconPath { get; set; }
        public int? Order { get; set; }
    }

    public class RequiredDocument
    {
        public int Id { get; set; }
        public string Parameter { get; set; }
        public string Value { get; set; }
        public string IconPath { get; set; }
        public int? Order { get; set; }
        public int Type { get; set; }
    }

    public class MobileReservationDetails
    {
        public string RezToken { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string VehicleName { get; set; }
        public List<VehicleImage> VehicleImages { get; set; }
        public MobileLocation PickupLocation { get; set; }
        public MobileLocation ReturnLocation { get; set; }
        public string GearType { get; set; }
        public String FuelType { get; set; }
        public string VehicleBrandName { get; set; }
        public string VehicleModelName { get; set; }
        public string VehicleCategoryTypeName { get; set; }
        public string PickupLocationName { get; set; }
        public string PickupLocationCode { get; set; }
        public int PickupLocationId { get; set; }
        public string ReturnLocationName { get; set; }
        public string ReturnLocationCode { get; set; }
        public int ReturnLocationId { get; set; }
        public int RentalDuration { get; set; }
        public int? TotalKMLimit { get; set; }
        public bool IsFlightNumberRequired { get; set; } = false;
    }

    public class MobileRentalConditions
    {
        public string Header { get; set; }
        public List<MobileRentalCondition> RentalConditions { get; set; }
    }

    public class MobileRentalConditionsNew
    {
        public string Header { get; set; }
        public string Text { get; set; }
    }

    public class MobileRentalCondition
    {
        public int ConditionId { get; set; }
        public string Value { get; set; }
        public string ConditionName { get; set; }
        public string IconPath { get; set; }
    }

    public class MobilePriceDetails
    {
        public float VendorLocationFee { get; set; }
        public string Header { get; set; }
        public int RentalDuration { get; set; }
        public float DailyPrice { get; set; }
        public float OneWayFee { get; set; }
        public float ExtraPrice { get; set; }
        public float TotalPrice { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class MobileVendorDetails
    {
        public VendorOfficeLocation OfficeLocation { get; set; }
        public string Header { get; set; }
        public int VendorId { get; set; }
        public string VendorPhone { get; set; }
        public string VendorEmail { get; set; }
        public string VendorName { get; set; }
        public string VendorLogo { get; set; }
        public decimal? VendorScore { get; set; }
        public int? VendorCommentCount { get; set; }
        public int DeliveryType { get; set; }
        public int ReturnDeliveryType { get; set; }
        public DeliveryType DeliveryTypeName { get; set; }
        public List<Detail> ReservationVendorDetail { get; set; }
        public bool IsFindeksRequired { get; set; }
        public bool IsCouponActive { get; set; }
    }

    public class Detail
    {
        public string Parameter { get; set; }
        public string Text { get; set; }
        public string IconPath { get; set; }
    }

    public class MobileLocation
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string Name { get; set; }
    }
}
