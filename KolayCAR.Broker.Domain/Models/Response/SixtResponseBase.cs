using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class SixtResponseBase<T>
    {
        public string message { get; set; }
        public int statusCode { get; set; }
        public string status { get; set; }
        public T result { get; set; }
    }
    public class LoginResult
    {
        public string accessToken { get; set; }
        public int expires_in { get; set; }
        public SixtUser user { get; set; }
    }

    public class SixtUser
    {
        public string name { get; set; }
        public string email { get; set; }
    }
    public class SixtVehicle
    {
        public string brand { get; set; }
        public string model { get; set; }
        public string brand_model { get; set; }
        public string gear_types { get; set; }
        public string fuel_type { get; set; }
        public string door { get; set; }
        public string person_capacity { get; set; }
        public string tank_capacity { get; set; }
        public string suitcase_capacity { get; set; }
        public string suitcase_volume { get; set; }
        public string vehicle_type { get; set; }
        public string vehicle_class { get; set; }
        public string vehicle_group { get; set; }
        public string age_limit { get; set; }
        public string station_code { get; set; }
    }

    public class SixtVehicleListResponse
    {
        public int count { get; set; }
        public List<SixtVehicle> data { get; set; }
    }
    public class SixtLocationListResponse
    {
        public string id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public string display_name { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public string address_en { get; set; }
        public string address_directions { get; set; }
        public string address_directions_en { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string weekday_opening { get; set; }
        public string weekday_closing { get; set; }
        public bool weekday_add_hour { get; set; }
        public string weekend_opening { get; set; }
        public string weekend_closing { get; set; }
        public bool weekend_add_hour { get; set; }
        public string sunday_opening { get; set; }
        public string sunday_closing { get; set; }
        public bool sunday_add_hour { get; set; }
        public int abt_hour { get; set; }
        public int office_type { get; set; }
        public string availability_time { get; set; }
    }
    public class SixtVehiclesResponse
    {
        public string unid { get; set; }
        public List<SixtAvailableVehicle> vehicles { get; set; }
    }

    public class SixtAvailableVehicle
    {
        public string vehicle_group { get; set; }
        public string vehicle_brands { get; set; }
        public string vehicle_available { get; set; }
        public int vehicle_count { get; set; }
        public int rental_day { get; set; }
        public string standard_price { get; set; }
        public string discount { get; set; }
        public string discount_price { get; set; }
        public string calc_price { get; set; }
        public string one_way_price { get; set; }
        public string location_fee { get; set; }
        public bool is_opportunity { get; set; }
        public string person_capacity { get; set; }
        public VehicleFeatures vehicle_features { get; set; }
        public int max_km { get; set; }
        public string daily_price { get; set; }
        public string daily_total_price { get; set; }
    }

    public class VehicleFeatures
    {
        public string @class { get; set; }
        public string type { get; set; }
        public int fuel { get; set; }
        public int gear { get; set; }
        public int age { get; set; }
        public int min_license_age { get; set; }
        public int deposit_amount { get; set; }
        public int exemption_amount { get; set; }
        public int min_findeks { get; set; }
        public bool double_credit_card { get; set; }
        //public int km_per_contract { get; set; }
        //public int km_per_day { get; set; }
    }
    public class SixtExtra
    {
        public string price { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public string description { get; set; }
        public string description_en { get; set; }
        public int calculation_type { get; set; }
        public string calculation_type_description { get; set; }
        public string max_day { get; set; }
        public string max_km { get; set; }
        public string category_id { get; set; }
        public string category_name { get; set; }
        public bool multiple_sale { get; set; }
        public int star { get; set; }
        public int discount { get; set; }
        public List<object> extra_scopes { get; set; }
    }

    public class SixtGetExtrasResponse
    {
        public List<SixtExtra> extras { get; set; }
        public List<IncludedExtra> included_extras { get; set; }
    }
    public class SixtReservationResponse
    {
        public string trans_no { get; set; }
        public string vehicle_group { get; set; }
        public string reservation_source { get; set; }
        public string reservation_station_code { get; set; }
        public string pickup_station_code { get; set; }
        public string return_station_code { get; set; }
        public string pickup_date { get; set; }
        public string return_date { get; set; }
        public string pickup_time { get; set; }
        public string return_time { get; set; }
        public int rental_day { get; set; }
        public string daily_price { get; set; }
        public string total_price { get; set; }
        public string extra_amount { get; set; }
        public string total_amount { get; set; }
        public string customer_code { get; set; }
        public string corporate_customer_code { get; set; }
    }
    public class SixtExtraList
    {
        public string code { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public string description { get; set; }
        public string description_en { get; set; }
        public int calculation_type { get; set; }
        public string calculation_type_description { get; set; }
        public string max_day { get; set; }
        public string max_km { get; set; }
        public string category_id { get; set; }
        public string category_name { get; set; }
        public bool multiple_sale { get; set; }
    }

    public class SixtExtras
    {
        public List<SixtExtraList> extras { get; set; }
    }
    public class IncludedExtra
    {
        public string price { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public string description { get; set; }
        public string description_en { get; set; }
        public int calculation_type { get; set; }
        public string calculation_type_description { get; set; }
        public string max_day { get; set; }
        public string max_km { get; set; }
        public string category_id { get; set; }
        public string category_name { get; set; }
        public bool multiple_sale { get; set; }
        public int star { get; set; }
        public int discount { get; set; }
        public List<object> extra_scopes { get; set; }
    }

    public class SixtLocationItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("name_en")]
        public string NameEn { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("address_en")]
        public string AddressEn { get; set; }

        [JsonPropertyName("address_directions")]
        public string AddressDirections { get; set; }

        [JsonPropertyName("address_directions_en")]
        public string AddressDirectionsEn { get; set; }

        [JsonPropertyName("latitude")]
        public string Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public string Longitude { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("weekday_opening")]
        public string WeekdayOpening { get; set; }

        [JsonPropertyName("weekday_closing")]
        public string WeekdayClosing { get; set; }

        [JsonPropertyName("weekday_add_hour")]
        public bool WeekdayAddHour { get; set; }

        [JsonPropertyName("weekend_opening")]
        public string WeekendOpening { get; set; }

        [JsonPropertyName("weekend_closing")]
        public string WeekendClosing { get; set; }

        [JsonPropertyName("weekend_add_hour")]
        public bool WeekendAddHour { get; set; }

        [JsonPropertyName("sunday_opening")]
        public string SundayOpening { get; set; }

        [JsonPropertyName("sunday_closing")]
        public string SundayClosing { get; set; }

        [JsonPropertyName("sunday_add_hour")]
        public bool SundayAddHour { get; set; }

        [JsonPropertyName("working_hours")]
        public List<WorkingHour> WorkingHours { get; set; }

        [JsonPropertyName("location")]
        public int Location { get; set; }

        [JsonPropertyName("availability_time")]
        public string AvailabilityTime { get; set; }
    }
    public class WorkingHour
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("valid_from")]
        public string ValidFrom { get; set; }

        [JsonPropertyName("valid_until")]
        public string ValidUntil { get; set; }

        [JsonPropertyName("days")]
        public Dictionary<string, List<WorkingHourSlot>> Days { get; set; }
    }

    public class WorkingHourSlot
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public string EndTime { get; set; }
    }
}
