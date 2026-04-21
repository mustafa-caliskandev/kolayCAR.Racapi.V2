using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Responses.Eren
{
    public class ErenSearchResponse
    {
        [JsonProperty("search_request_id")]
        public string SearchRequestId { get; set; }

        [JsonProperty("available_cars")]
        public List<ErenAvailableCar> AvailableCars { get; set; }
    }

    public class ErenAvailableCar
    {
        [JsonProperty("quote_id")]
        public string QuoteId { get; set; }

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

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
        public int BigBags { get; set; }

        [JsonProperty("small_bags")]
        public int SmallBags { get; set; }

        [JsonProperty("seats")]
        public int Seats { get; set; }

        [JsonProperty("doors")]
        public int Doors { get; set; }

        [JsonProperty("daily_price")]
        public string DailyPrice { get; set; }

        [JsonProperty("total_price")]
        public string TotalPrice { get; set; }

        [JsonProperty("drop_price")]
        public string DropPrice { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("total_days")]
        public int TotalDays { get; set; }

        [JsonProperty("mileage_daily")]
        public int MileageDaily { get; set; }

        [JsonProperty("mileage_max")]
        public int MileageMax { get; set; }

        [JsonProperty("mileage_unit")]
        public string MileageUnit { get; set; }

        [JsonProperty("mileage_excess")]
        public string MileageExcess { get; set; }

        [JsonProperty("insurance_excess")]
        public string InsuranceExcess { get; set; }

        [JsonProperty("deposit")]
        public string Deposit { get; set; }

        [JsonProperty("out_of_hours")]
        public string OutOfHours { get; set; }

        [JsonProperty("fuel_policy")]
        public string FuelPolicy { get; set; }

        [JsonProperty("refuel_price_per_liter")]
        public string RefuelPricePerLiter { get; set; }

        [JsonProperty("min_driver_age")]
        public int MinDriverAge { get; set; }

        [JsonProperty("min_driving_license_age")]
        public int MinDrivingLicenseAge { get; set; }

        [JsonProperty("young_driver_age")]
        public int YoungDriverAge { get; set; }

        [JsonProperty("young_driving_license_age")]
        public int YoungDrivingLicenseAge { get; set; }

        [JsonProperty("young_driver_price_perday")]
        public string YoungDriverPricePerDay { get; set; }

        [JsonProperty("senior_driver_age")]
        public int SeniorDriverAge { get; set; }

        [JsonProperty("senior_driver_price_perday")]
        public string SeniorDriverPricePerDay { get; set; }

        [JsonProperty("extras")]
        public List<ErenExtra> Extras { get; set; }
    }

    public class ErenExtra
    {
        [JsonProperty("service_id")]
        public int ServiceId { get; set; }

        [JsonProperty("service_name")]
        public string ServiceName { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("perday")]
        public int PerDay { get; set; }
    }
}
