using System;

namespace KolayCAR.Broker.API.Models.MobileAppModels
{
    public class MobileFindeksUrl
    {
        public int Id { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime DriverLicenseDate { get; set; }
        public string Tckn { get; set; }
        public string ReservationToken { get; set; }
        public string UniqueId { get; set; }
        public int VendorId { get; set; }
    }
}
