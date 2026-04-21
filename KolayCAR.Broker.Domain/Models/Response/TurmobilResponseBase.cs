using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class TurmobilResponseBase
    {

        public class AuthLoginResponse
        {
            public string Token { get; set; }
        }
        public class Location
        {
            public string address { get; set; }
            public string phone { get; set; }
            public string geoLocation { get; set; }
            public string name { get; set; }
            public string deliveryType { get; set; }
            public string closeTime { get; set; }
            public string id { get; set; }
            public string openTime { get; set; }
            public string email { get; set; }
        }

        public class LocationTurmobil
        {
            public List<Location> data { get; set; }
            public string responseCode { get; set; }
            public string responseMsg { get; set; }
        }

        //public class LocationTurmobil 
        //{
        //    public List<Location> data { get; set; }
        //}
        //public class Location
        //{
        //    public string address { get; set; }
        //    public string phone { get; set; }
        //    public string geoLocation { get; set; }
        //    public string name { get; set; }
        //    public string deliveryType { get; set; }
        //    public string closeTime { get; set; }
        //    public int id { get; set; }
        //    public string openTime { get; set; }
        //    public string email { get; set; }
        //}
        public class vehicleTurmobil
        {
            public List<vehicle> data { get; set; }
        }
        public class vehicle
        {
            public int ageLimit { get; set; }
            public int vehicleTypeId { get; set; }
            public string groupType { get; set; }
            public string fuelType { get; set; }
            public string vehicleImage { get; set; }
            public int licenseAgeLimit { get; set; }
            public List<string> additionalProperties { get; set; }
            public int provisionAmount { get; set; }
            public string vehicleTypeName { get; set; }
            public int monthlyDistanceLimit { get; set; }
            public string gear { get; set; }
            public int dailyDistanceLimit { get; set; }
        }
        public class ExtrasTurmobil
        {
            public List<Extra> data { get; set; }
            public string responseCode { get; set; }
            public string responseMsg { get; set; }
        }
        public class Extra
        {
            public int id { get; set; }
            public string name { get; set; }
            public string detail { get; set; }
            public int amount { get; set; }
            public int typeId { get; set; }
            public ExtraRentalTypes calculationType { get; set; }
            public int maxQuantity { get; set; }

        }
        public class LocationVehiclesTurmobil
        {
            public List<locationVehicles> data { get; set; }
        }

        public class locationVehicles
        {
            public int vehicleTypeId { get; set; }
            public int amount { get; set; }
            public int dropAmount { get; set; }
            public string groupType { get; set; }
            public int hireDay { get; set; }
            public float dailyAmountLater { get; set; }
            public int licenseAgeLimit { get; set; }
            public float amountLater { get; set; }
            public float totalAmountLater { get; set; }
            public int totalDistanceLimit { get; set; }
            public string tradeMark { get; set; }
            public float provisionAmount { get; set; }
            public int ageLimit { get; set; }
            public float totalAmount { get; set; }
            public string fuelType { get; set; }
            public float dailyAmount { get; set; }
            public string[] additionalProperties { get; set; }
            public string vehicleTypeName { get; set; }
            public string gear { get; set; }
            public int dailyDistanceLimit { get; set; }
        }

        public class PostReservationResponse
        {
            public string uuid { get; set; }
            public string responseCode { get; set; }
            public string responseMsg { get; set; }
        }
        public class CancelReservationResponse
        {
            public string responseCode { get; set; }
            public string responseMsg { get; set; }
        }
    }
}
