using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Responses.Eren
{
    public class ErenVehicleGroupResponse
    {
        [JsonProperty("vehicle_groups")]
        public List<ErenVehicleGroup> VehicleGroups { get; set; }
    }
    public class ErenExtraListResponse
    {
        [JsonProperty("extras")]
        public List<ErenExtra> extras { get; set; }
    }
    public class ErenVehicleGroup
    {
        [JsonProperty("group_id")]
        public string GroupId { get; set; }

        [JsonProperty("sipp_code")]
        public string SippCode { get; set; }

        [JsonProperty("car_make")]
        public string CarMake { get; set; }

        [JsonProperty("car_model")]
        public string CarModel { get; set; }

        [JsonProperty("car_image")]
        public string CarImage { get; set; }

        [JsonProperty("fuel_type")]
        public string FuelType { get; set; }

        [JsonProperty("transmission_type")]
        public string TransmissionType { get; set; }

        [JsonProperty("big_bags")]
        public string BigBags { get; set; }

        [JsonProperty("small_bags")]
        public string SmallBags { get; set; }

        [JsonProperty("seats")]
        public string Seats { get; set; }

        [JsonProperty("doors")]
        public string Doors { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("deposit")]
        public string Deposit { get; set; }
    }
}
