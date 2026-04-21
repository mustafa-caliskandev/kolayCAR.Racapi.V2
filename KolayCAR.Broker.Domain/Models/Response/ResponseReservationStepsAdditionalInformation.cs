using System;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class ResponseReservationStepsAdditionalInformation
    {
        public Agency Agency { get; set; }
        public Vendor Vendor { get; set; }
        public int APIVendorId { get; set; }
        public int VehicleId { get; set; }
        public string LanguageCode { get; set; }
        public string CurrencyCode { get; set; }
        public CurrencyTypes APICurrencyType { get; set; }
        public int PickupLocationId { get; set; }
        public int? APIPickupLocationId { get; set; }
        public string APIPickupLocationCode { get; set; }
        public string PickupLocationName { get; set; }
        public string APIPickupLocationName { get; set; }
        public int ReturnLocationId { get; set; }
        public int? APIReturnLocationId { get; set; }
        public string APIReturnLocationCode { get; set; }
        public string ReturnLocationName { get; set; }
        public string APIReturnLocationName { get; set; }
        public DateTime PickupDateTime { get; set; }
        public DateTime ReturnDateTime { get; set; }
        public string ReferenceCode { get; set; }
        public string APIReferenceCode { get; set; }
        public int RentalDuration { get; set; }
        public float APIExtraAmount { get; set; }
        public ReservationToken ReservationToken { get; set; }
        public string CityCode { get; set; }
        public string DistrictCode { get; set; }
        public bool UseOnlyDefaultCurrency { get; set; }
        public int? CouponType { get; set; }
        public decimal? CouponDiscountAmount { get; set; }
        public bool? FlightCardMandatory { get; set; }
        public string LocationRateCode { get; set; }
    }
}
