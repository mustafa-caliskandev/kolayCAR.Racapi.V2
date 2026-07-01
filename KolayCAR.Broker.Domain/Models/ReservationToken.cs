using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models
{
    public class ReservationToken
    {
        public long AgencyId { get; set; }
        public int VendorId { get; set; }
        public int APIVendorId { get; set; }
        public string APIVendorName { get; set; }
        public string APIVendorPhone { get; set; }
        public string APIVendorEmail { get; set; }
        public string APIVendorLogo { get; set; }
        public int VehicleId { get; set; }
        public int ApiVehicleId { get; set; }
        public string VehicleCode { get; set; }
        public int? APIPickupLocationId { get; set; }
        public string APIPickupLocationCode { get; set; }
        public int? APIReturnLocationId { get; set; }
        public string APIReturnLocationCode { get; set; }
        public CurrencyTypes CurrencyType { get; set; }
        public int RentalDuration { get; set; }
        public float DailyPrice { get; set; }
        public float OneWayFee { get; set; }
        public float DailyPricePayNow { get; set; }
        public float APIDailyPrice { get; set; }
        public float? APITotalPrice { get; set; }
        public float? APITotalPricePayNow { get; set; }
        public float APIDailyPricePayNow { get; set; }
        public float APIOneWayFee { get; set; }
        public string APIReferenceCode { get; set; }
        public string APIReferenceCode2 { get; set; }
        public string APIReferenceCode3 { get; set; }
        public float? DepositPrice { get; set; }
        public int VendorMinimumDriverAge { get; set; }
        public int VendorMinimumDrivingLicenseAge { get; set; }
        public float ServiceCharge { get; set; }
        public FuelTypes FuelType { get; set; }
        public TransmissionTypes TransmissionType { get; set; }
        public VehicleCategoryTypes VehicleCategoryType { get; set; }
        public VehicleTypes VehicleType { get; set; }
        public PassangerQuantityTypes PassangerQuantityType { get; set; }
        public BaggageQuantityTypes BaggageQuantityType { get; set; }
        public bool IsOffice { get; set; }
        public bool IsAirport { get; set; }
        public bool DepositCreditCardRequired { get; set; }
        public int PickupLocationId { get; set; }
        public int ReturnLocationId { get; set; }
        public LanguageTypes LanguageType { get; set; }
        public DateTime PickupDateTime { get; set; }
        public DateTime ReturnDateTime { get; set; }
        public string SippCode { get; set; }
        public string VehicleName { get; set; }
        public string VehicleImageUrl { get; set; }
        public CurrencyTypes BaseVendorRequestCurrencyType { get; set; }
        public CurrencyTypes BaseBaseVendorRequestCurrencyType { get; set; }
        public CreditType CreditType { get; set; }
        public CreditType? APICreditType { get; set; }
        public bool? APIFullCredit { get; set; }
        public VendorTypes? ApiVendorType { get; set; }
        public int? VehicleClassNo { get; set; }
        public string VehicleGroupName { get; set; }
        public string TransmissionTypeName { get; set; }
        public int? VehicleClassSize { get; set; }
        public float? ValueAddedTax { get; set; }
        public string? SpecialProfitApplied { get; set; } // Kar marjı geliştirmesi için eklendi
        public int TotalKmLimit { get; set; }
        public bool VendorFlightPassRequired { get; set; }
        public bool FullCredit { get; set; }
        public List<CyrptExtra> CyrptExtras { get; set; } = new List<CyrptExtra>();
        public int? APIDeliveryTypeId { get; set; }
        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
