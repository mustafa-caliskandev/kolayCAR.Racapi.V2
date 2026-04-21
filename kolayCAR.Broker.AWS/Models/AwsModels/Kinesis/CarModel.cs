using Newtonsoft.Json;

namespace kolayCAR.Broker.AWS.Models.AwsModels.Kinesis
{
    public class CarModel
    {
        [JsonProperty("vehicle_token")]
        public string ReservationToken { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("supplier")]
        public string Supplier { get; set; }

        [JsonProperty("car_group")]
        public string Group { get; set; }

        [JsonProperty("fuel_type")]
        public string FuelType { get; set; }

        [JsonProperty("gear_type")]
        public string TransmissionType { get; set; }

        [JsonProperty("cancellation_policy")]
        public string CancellationPolicy { get; set; }

        [JsonProperty("car_type")]
        public string VehicleType { get; set; }

        [JsonProperty("total_km")]
        public int TotalKm { get; set; }

        [JsonProperty("minimum_driver_age")]
        public int MinimumDriverAge { get; set; }

        [JsonProperty("minimum_driving_licence")]
        public int MinimumLicenseYear { get; set; }

        [JsonProperty("deposit")]
        public float Deposit { get; set; }

        [JsonProperty("daily_price")]
        public float DailyPrice { get; set; }

        [JsonProperty("total_price")]
        public float TotalPrice { get; set; }

        [JsonProperty("drop_price")]
        public float DropPrice { get; set; }

        //[JsonProperty("office_service_charge")]
        //public float OfficeServicePrice { get; set; }

        [JsonProperty("people_luggage")]
        public PeopleLuggage PeopleLuggage { get; set; }
    }

    public struct PeopleLuggage
    {

        [JsonProperty("people")]
        public int People { get; set; }


        [JsonProperty("luggage")]
        public int Luggage { get; set; }
    }
}
