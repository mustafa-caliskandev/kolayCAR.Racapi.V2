using System;

namespace KolayCAR.Broker.Domain.Models
{
    public class VehicleResultStatistic
    {
        public int Id { get; set; }
        public DateTime RecordDate { get; set; }
        public int PickupLocationId { get; set; }
        public int ReturnLocationId { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public int? APIVendorId { get; set; }
        public string APIVendorName { get; set; }
        public int? VehicleId { get; set; }
        public string VehicleCode { get; set; }
        public string VehicleName { get; set; }
        public float DailyPrice { get; set; }
        public int RentalDuration { get; set; }
        public string VehicleImageUrl { get; set; }
        public long? AgencyId { get; set; }
        public string VendorLogoUrl { get; set; }
        public int? FuelId { get; set; }
        public int? TransmissionId { get; set; }
        public int? BaggageId { get; set; }
        public int? CategoryId { get; set; }
        public int? PersonId { get; set; }
        public int? TypeId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int CurrencyId { get; set; }
        public int LangId { get; set; }
        public float ExchangeRate { get; set; }
    }
}
