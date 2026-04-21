using Newtonsoft.Json;

namespace kolayCAR.Broker.AWS.Models.AwsModels.Kinesis
{
    public class FilterModel
    {
        [JsonProperty("sorting_type")]
        public string SortType { get; set; }

        [JsonProperty("rental_firms")]
        public string Vendors { get; set; }

        [JsonProperty("vehicle_categories")]
        public string VehicleCategories { get; set; }

        [JsonProperty("vehicle_models")]
        public string VehicleModels { get; set; }

        [JsonProperty("vehicle_fuels")]
        public string Fuels { get; set; }

        [JsonProperty("vehicle_transmissions")]
        public string Transmissions { get; set; }

        [JsonProperty("vehicle_min_driver_ages")]
        public string MinimumAges { get; set; }

        [JsonProperty("vehicle_min_license_years")]
        public string MinimumDrivingLicenseYears { get; set; }

        [JsonProperty("delivery_types")]
        public string DeliveryTypes { get; set; }

        [JsonProperty("min_price")]
        public int MinPrice { get; set; }

        [JsonProperty("max_price")]
        public int MaxPrice { get; set; }

        [JsonProperty("min_km")]
        public int MinKm { get; set; }

        [JsonProperty("max_km")]
        public int MaxKm { get; set; }

        [JsonProperty("min_deposit")]
        public int MinimumDeposit { get; set; }

        [JsonProperty("max_deposit")]
        public int MaximumDeposit { get; set; }

    }
}
