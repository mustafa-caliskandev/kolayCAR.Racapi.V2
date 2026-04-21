using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class Ekar2ResponseBase
    {
        public class RootVehicle
        {
            public string sipp { get; set; }
            public int minDriverAge { get; set; }
            public int minLicenseYear { get; set; }
            public int passenger { get; set; }
            public double blockedAmount { get; set; }
            public string vehicleFuelType { get; set; }
            public string vehicleGearbox { get; set; }
            public string vehicleClass { get; set; }
            public string vehicleImageUrl { get; set; }
        }
        public class Vehicle : RootVehicle
        {
            public int id { get; set; }
            public string name { get; set; }
            public int creditCardCount { get; set; }
            public string vehicleBrand { get; set; }
            public string vehicleModel { get; set; }
        }

        public class Ekar2VehicleListResult
        {
            public List<Vehicle> content { get; set; }
            public int pageNo { get; set; }
            public int pageSize { get; set; }
            public int totalElements { get; set; }
            public string sortBy { get; set; }
        }

        public class Ekar2VehicleListResponse
        {
            public bool success { get; set; }
            public Ekar2VehicleListResult result { get; set; }
            public string message { get; set; }
        }

        public class Location
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isAirport { get; set; }
            public string address { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public List<WorkingHour> workingHours { get; set; }
        }

        public class Ekar2LocationListResult
        {
            public List<Location> content { get; set; }
            public int pageNo { get; set; }
            public int pageSize { get; set; }
            public int totalElements { get; set; }
            public string sortBy { get; set; }
        }

        public class Ekar2LocationListResponse
        {
            public bool success { get; set; }
            public Ekar2LocationListResult result { get; set; }
            public string message { get; set; }
        }

        public class WorkingHour
        {
            public string day { get; set; }
            public string openingTime { get; set; }
            public string closingTime { get; set; }
        }

        public class Extra
        {
            public int id { get; set; }
            public string name { get; set; }
            public string sellType { get; set; }
            public bool isAssurance { get; set; }
            public double totalPrice { get; set; }
        }

        public class Ekar2ExtraListResult
        {
            public List<Extra> content { get; set; }
            public int pageNo { get; set; }
            public int pageSize { get; set; }
            public int totalElements { get; set; }
            public string sortBy { get; set; }
        }

        public class Ekar2ExtraListResponse
        {
            public bool success { get; set; }
            public Ekar2ExtraListResult result { get; set; }
            public string message { get; set; }
        }
        public class AvailableVehicle : RootVehicle
        {
            public int vehicleGroupId { get; set; }
            public string vehicleGroupName { get; set; }
            public int kilometerLimit { get; set; }
            public int rentalDay { get; set; }
            public double oneWayPrice { get; set; }
            public double dailyRentalPrice { get; set; }
            public double totalRentalPrice { get; set; }
            public double totalPrice { get; set; }
            public string currencyCode { get; set; }
            public double currencyRate { get; set; }
            public List<Extra> extras { get; set; }
        }

        public class Ekar2AvailableVehicleResponse
        {
            public bool success { get; set; }
            public List<AvailableVehicle> result { get; set; }
            public string message { get; set; }
        }
        public class Ekar2PostCancelReservationResponseResult
        {
            public int id { get; set; }
        }

        public class Ekar2PostCancelReservationResponse
        {
            public bool success { get; set; }
            public Ekar2PostCancelReservationResponseResult result { get; set; }
            public string message { get; set; }
        }

        public class Ekar2PostReservationResponseResult
        {
            public int id { get; set; }
            public int vehicleGroupId { get; set; }
            public string sipp { get; set; }
            public string pickupDate { get; set; }
            public string pickupTime { get; set; }
            public int pickupLocationId { get; set; }
            public string pickupLocation { get; set; }
            public string returnDate { get; set; }
            public string returnTime { get; set; }
            public int returnLocationId { get; set; }
            public string returnLocation { get; set; }
            public double dailyPrice { get; set; }
            public double totalPrice { get; set; }
            public string currencyCode { get; set; }
            public string reservationStatus { get; set; }
            public List<object> extras { get; set; }
        }

        public class Ekar2PostReservationResponse
        {
            public bool success { get; set; }
            public Ekar2PostReservationResponseResult result { get; set; }
            public string message { get; set; }
        }

    }
}
