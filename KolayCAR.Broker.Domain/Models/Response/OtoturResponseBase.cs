using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{

    public class OtoturResponseBase
    {
        public class OtoturAuthResponse
        {
            public string token { get; set; }
        }
        public class Content
        {
            public string id { get; set; }
            public string name { get; set; }
            public string isAirport { get; set; }
            public string adress { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
        }

        public class Result
        {
            public List<Content> content { get; set; }
            public int pageNo { get; set; }
            public int pageSize { get; set; }
            public int totalElements { get; set; }
        }

        public class OtoturBaseResponse
        {
            public string message { get; set; }
            public bool success { get; set; }
            public string timestamp { get; set; }
            public Result result { get; set; }
        }

        //Araç Sınıfları
        public class OtoturVehicleResponseBase
        {
            public string message { get; set; }
            public bool success { get; set; }
            public string timestamp { get; set; }
            public VehicleResult result { get; set; }
        }
        public class VehicleResult
        {
            public List<OtoturVehicleResponse> content { get; set; }
            public int pageNo { get; set; }
            public int pageSize { get; set; }
            public int totalElements { get; set; }
        }
        public class OtoturAvailableVehicleResponseBase
        {
            public string message { get; set; }
            public bool success { get; set; }
            public string timestamp { get; set; }
            public AvailableVehicleResult result { get; set; }
        }
        public class AvailableVehicleResult
        {
            public List<OtoturAvailableVehicleResponse> content { get; set; }
            public int pageNo { get; set; }
            public int pageSize { get; set; }
            public int totalElements { get; set; }
        }
        public class OtoturVehicleResponse
        {
            public string aracid { get; set; }
            public string name { get; set; }
            public string marka { get; set; }
            public string model { get; set; }
            public string fuel { get; set; }
            public string transmission { get; set; }
            public string sipp { get; set; }
            public string minDriverAge { get; set; }
            public string minLicenseYear { get; set; }
            public string passenger { get; set; }
            public string blockedAmount { get; set; }
            public string creditCardCount { get; set; }
            public string kilometerlimit { get; set; }
        }

        public class OtoturAvailableVehicleResponse
        {
            public string aracid { get; set; }
            public string vehicleGroupName { get; set; }
            public string sipp { get; set; }
            public string minDriverAge { get; set; }
            public string minLicenseYear { get; set; }
            public string passenger { get; set; }
            public string blockedAmount { get; set; }
            public string rentalday { get; set; }
            public string kilometerlimit { get; set; }
            public string dropPrice { get; set; }
            public string dailyRentalPrice { get; set; }
            public string totalRentalPrice { get; set; }
            public string currencyCode { get; set; }
            public string currencyRate { get; set; }
            public List<Extra> extras { get; set; }
        }
        public class OtoturExtraResponseBase
        {
            public string message { get; set; }
            public bool success { get; set; }
            public string timestamp { get; set; }
            public ExtraResult result { get; set; }
        }
        public class ExtraResult
        {
            public List<Extra> content { get; set; }
            public int pageNo { get; set; }
            public int pageSize { get; set; }
            public int totalElements { get; set; }
        }
        public class Extra
        {
            public string id { get; set; }
            public string name { get; set; }
            public string sellType { get; set; }
            public string isAssurance { get; set; }
            public string dailyAmount { get; set; }
            public string totalAmount { get; set; }
        }
        public class ReservationResultBase
        {
            public ReservationResult result { get; set; }
            public string message { get; set; }
            public bool success { get; set; }
            public string timestamp { get; set; }

        }
        public class ReservationResult
        {
            public List<Reservation> content { get; set; }
            public int totalElements { get; set; }
        }
        public class Reservation
        {
            public string id { get; set; }
        }

        public class ReservationCancel
        {
            public string group_id { get; set; }
            public string durum { get; set; }
        }

        public class ReservationCancelResult
        {
            public List<ReservationCancel> content { get; set; }
            public int pageNo { get; set; }
            public int pageSize { get; set; }
            public int totalElements { get; set; }
        }

        public class ReservationCancelResultBase
        {
            public string message { get; set; }
            public bool success { get; set; }
            public string timestamp { get; set; }
            public ReservationCancelResult result { get; set; }
        }
    }
}
