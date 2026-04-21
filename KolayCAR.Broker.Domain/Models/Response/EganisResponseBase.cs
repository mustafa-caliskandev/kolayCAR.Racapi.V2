using KolayCAR.Broker.Domain.Models.Sixt.Response;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class EganisResponseBase
    {

        public class AuthLoginResponse
        {
            public string data { get; set; }
            public int resultCode { get; set; }
            public object resultDesc { get; set; }
            public bool isSucceed { get; set; }
            public object resultData { get; set; }
        }

        public class LocationResponse
        {
            public class Root
            {
                public int resultCode { get; set; }
                public string resultDesc { get; set; }
                public bool isSucceed { get; set; }
                public string resultData { get; set; }
                public List<Location> data { get; set; }
            }

            public class Location
            {
                public int locationId { get; set; }
                public string locationName { get; set; }
                public string address { get; set; }
                public string cityName { get; set; }
                public string townName { get; set; }
                public string phoneNr { get; set; }
                public string eMail { get; set; }
                public float latitude { get; set; }
                public float longitude { get; set; }
                public int rezStartLimitMin { get; set; }
                public List<WorkDay> workDays { get; set; }

            }

            public class WorkDay
            {
                public int day { get; set; }
                public string startTime { get; set; }
                public string endTime { get; set; }
            }
        }

        public class ReservationResponse
        {
            public class Data
            {
                public int reservationId { get; set; }
                public string reservationCode { get; set; }
            }

            public class Root
            {
                public int resultCode { get; set; }
                public string resultDesc { get; set; }
                public bool isSucceed { get; set; }
                public string resultData { get; set; }

                public Data data { get; set; }
            }
        }

        public class ReservationCancelResponse
        {
            public int resultCode { get; set; }
            public string resultDesc { get; set; }
            public bool isSucceed { get; set; }
            public object resultData { get; set; }

        }
        public class VehicleResponse
        {
            public class Root
            {
                public int resultCode { get; set; }
                public string resultDesc { get; set; }
                public bool isSucceed { get; set; }
                public string resultData { get; set; }
                public List<VehicleRes> data { get; set; }
            }

            public class VehicleRes
            {
                public int vehGroupId { get; set; }
                public string sippCode { get; set; }
                public string groupName { get; set; }
                public string vehicleName { get; set; }
                public string imagePath { get; set; }
                public List<VehicleModel> vehicleModels { get; set; }
                public string fuelType { get; set; }
                public string transmissionType { get; set; }
                public int driverAge { get; set; }
                public int drivingLicenseAge { get; set; }
                public int doorCount { get; set; }
                public int seatCount { get; set; }
                public int largeLuggageCapacity { get; set; }
                public int smallLuggageCapacity { get; set; }
                public List<PriceIncluded> priceIncludeds { get; set; }
                public int totalDays { get; set; }
                public int kmLimit { get; set; }
                public float rentalFee { get; set; }
                public float dailyRentalFee { get; set; }
                public float provosionFee { get; set; }
                public float dropFee { get; set; }
                public string currenyCode { get; set; }
                public List<Extras> extras { get; set; }
                public List<AssurancePackAges> assurancePackAges { get; set; }
                public ExchangeRates exchangeRates { get; set; }

            }

            public class PriceIncluded
            {
                public string code { get; set; }
                public string name { get; set; }
            }

            public class VehicleModel
            {
                public int modelId { get; set; }
                public string modelDesc { get; set; }
                public string imagePath { get; set; }
            }

            public class Extras
            {
                public int id { get; set; }
                public string code { get; set; }
                public string name { get; set; }
                public float fee { get; set; }
                public int maxQuantity { get; set; }
            }

            public class AssurancePackAges
            {
                public int id { get; set; }
                public string code { get; set; }
                public string name { get; set; }
                public float fee { get; set; }
            }

            public class ExchangeRates
            {
                public int additionalProp1 { get; set; }
                public int additionalProp2 { get; set; }
                public int additionalProp3 { get; set; }
            }

        }
    public class ExtraResponse
        {
            public class ExtraList
            {
                [JsonPropertyName("extras")]
                public List<Extra> Extras { get; set; }
            }

            public class Extra
            {
                public int id { get; set; }
                public string code { get; set; }
                public string name { get; set; }
                public int rentalType { get; set; }
                public int maxQuantity { get; set; }
            }
        }
    }
}
