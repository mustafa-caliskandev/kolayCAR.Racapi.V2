using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos
{
    public class VehicleDto
    {
        #region Properties
        [NotMapped]
        public int? OrderNo { get; set; }
        [NotMapped]
        public int VehicleId { get; set; }
        public string ReservationToken { get; set; }
        public string VehicleName { get; set; }
        public int VehicleCategoryType { get; set; }
        public string VehicleCategoryTypeName { get; set; }
        public int VehicleFuelType { get; set; }
        public string VehicleFuelTypeName { get; set; }
        public int VehicleTransmissionType { get; set; }
        public string VehicleTransmissionTypeName { get; set; }
        public int PassengerQuantityType { get; set; }
        public string PassengerQuantityName { get; set; }
        public int BaggageQuantityType { get; set; }
        public string BaggageQuantityName { get; set; }
        public int VehicleType { get; set; }
        public string VehicleTypeName { get; set; }
        public float DailyPrice { get; set; }
        public int RentalDuration { get; set; }
        public float TotalPrice { get; set; }
        public float OneWayFee { get; set; }
        public float ServiceCharge { get; set; }
        public int VendorId { get; set; }
        public int? TotalKmLimit { get; set; }
        public int? VendorMinimumDriverAge { get; set; }
        public int? VendorMinimumDrivingLicenseAge { get; set; }
        public float? DepositPrice { get; set; }
        public string CurrencyCode { get; set; }
        public int DeliveryType { get; set; }
        public bool IsFlightNumberRequired { get; set; }
        public string PickupLocationCode { get; set; }
        public string ReturnLocationCode { get; set; }

        #endregion

        #region Models

        public List<VehicleDetail> VehicleDetails { get; set; }

        public List<VehicleFeature> VehicleFeatures { get; set; }

        public List<VehicleBadge> VehicleBadges { get; set; }

        public List<MobileFilterParameters> MobileFilterParameters { get; set; }

        public List<SortableParameter> SortableParameters { get; set; }

        public VehiclePromotion VehiclePromotion { get; set; }
        #endregion
    }
}
