namespace KolayCAR.Broker.API.Models
{
    public class ReservationVehicleInfo
    {
        public int Id { get; set; }
        public int ReservationDetailId { get; set; }

        public string VehicleBrandName { get; set; }
        public string VehicleModelName { get; set; }
        public string VehicleName { get; set; }
        public string FuelName { get; set; }
        public string TransmissionName { get; set; }
        public string PersonName { get; set; }
        public string CategoryName { get; set; }
        public string TypeName { get; set; }

        public string DeliveryType { get; set; }
        public string DailyPrice { get; set; }
        public string Deposit { get; set; }
        public string KmLimit { get; set; }
        public string OneWayFee { get; set; }
        public string OfficeServicePrice { get; set; }
        public string VendorProfitMarkup { get; set; }
        public string RentalDuration { get; set; }

        public string ExtraJson { get; set; }
        public string ExtraNames { get; set; }
        public string ExtraAmount { get; set; }
        public string SpecialDailyPrice { get; set; }
        public string SpecialOneWayFee { get; set; }

        public string MinimumAge { get; set; }
        public string MinimumLicenseAge { get; set; }

        public virtual ReservationDetail ReservationDetail { get; set; }
    }
}
