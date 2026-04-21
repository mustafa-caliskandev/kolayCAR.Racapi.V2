using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class Avec3ResponseBase
    {
        public class AuthResponseBase
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
        }

        public class LocationResponseBase
        {
            public int total_count { get; set; }
            public int start { get; set; }
            public int limit { get; set; }
            public List<LocationBase> data { get; set; }
        }
        public class LocationBase
        {
            public int id { get; set; }
            public string old_id { get; set; }
            public string name { get; set; }
            public Address address { get; set; }
            public string phone { get; set; }
            public string slug { get; set; }
            public string iata_code { get; set; }
            public string drop_location { get; set; }
        }
        public class Address
        {
            public string address_line_1 { get; set; }
            public string address_line_2 { get; set; }
            public string city { get; set; }
            public string township { get; set; }
            public string postal_code { get; set; }
            public string country_code { get; set; }
            public string country { get; set; }
            [JsonProperty("lat")]
            public string latitude { get; set; }
            [JsonProperty("lon")]
            public string longitude { get; set; }
        }

        public class VehicleResponseBase
        {
            public int total_count { get; set; }
            public int start { get; set; }
            public int limit { get; set; }
            public List<Vehicle> data { get; set; }
        }
        public class AvailableVehicleResponse
        {
            public int total_count { get; set; }
            public int start { get; set; }
            public int limit { get; set; }
            public List<AvailableVehicle> data { get; set; }
        }
        public class AvailableVehicle
        {
            public string booking_id { get; set; }
            public int days { get; set; }
            public Vehicle vehicle { get; set; }
            public Price price_details { get; set; }
            public double total_km_limit { get; set; }
        }
        public class Vehicle
        {
            public int id { get; set; }
            public Brand brand { get; set; }
            public string name { get; set; }
            public string vehicle_group { get; set; }
            public string vehicle_type { get; set; }
            public string body_style { get; set; }
            public string gear_type { get; set; }
            public string fuel_type { get; set; }
            public double fuel_capacity { get; set; }
            public string fuel_type_unit { get; set; }
            public int min_driver_age { get; set; }
            public int min_driver_license_age { get; set; }
            public double deposit_price { get; set; }
            public double daily_km_limit { get; set; }
            public string color { get; set; }
            public double weight { get; set; }
            public double height { get; set; }
            public double width { get; set; }
            public double length { get; set; }
            public int passenger_capacity { get; set; }
            public int large_suitecase_capacity { get; set; }
            public int small_suitecase_capacity { get; set; }
            public string segment_code { get; set; }
            public string segment_text { get; set; }
            public string unique_model_code { get; set; }
            public string image { get; set; }
        }
        public class Brand
        {
            public string name { get; set; }
            public string logo { get; set; }
        }
        public class Price
        {
            public float total_price { get; set; }
            public float total_listing_price { get; set; }
            public float dropoff_price { get; set; }
            public float pay_later_price { get; set; }
            public string currency { get; set; }
        }
        public class ReservationResponseBase
        {
            public int id { get; set; }
            public string reservation_number { get; set; }
            public Driver driver { get; set; }
            public Vehicle vehicle { get; set; }
            public Price price_details { get; set; }
            public DateTime pickup_date { get; set; }
            public DateTime dropoff_date { get; set; }
            public LocationBase pickup_branch { get; set; }
            public LocationBase dropoff_branch { get; set; }

        }
        public class Driver
        {
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string country { get; set; }
            public bool foreign_citizen { get; set; }
            public string identity_number { get; set; }
            public string birth_date { get; set; }
        }
        public class PostReservationRequestBody
        {
            public string booking_id { get; set; }
            public Driver driver { get; set; }
            //  public List<AddOn> addons { get; set; }
            //  public List<AddOnBundle> addon_bundles { get; set; }
            public string payment_role { get; set; }
            public string external_id { get; set; }
        }
        public class PostReservationResponse
        {
            public string id { get; set; }
            public string reservation_number { get; set; }
            public Driver driver { get; set; }
            public Vehicle vehicle { get; set; }
            public Price price_details { get; set; }
            public string pickup_date { get; set; }
            public string dropoff_date { get; set; }
            public LocationBase pickup_branch { get; set; }
            public LocationBase dropoff_branch { get; set; }
        }
        public class PostCancelReservationRequestAvec
        {
            public string cancel_explanation { get; set; }
        }
        public class PostCancelReservationResponseAvec
        {
            public string reservation_id { get; set; }
            public string state { get; set; }
            public float? customer_paid_amount { get; set; }
            public float? refund_amount { get; set; }
            public string canceled_at { get; set; }
        }
        public class ExtraResponseAvec
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public double daily_price { get; set; }
            public string currency { get; set; }
            public string code { get; set; }
            public string type { get; set; }
            public int max_quantity { get; set; }
        }
        public class AvailableExtraAvec
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public float daily_price { get; set; }
            public string currency { get; set; }
            public string currency_symbol { get; set; }
            public string code { get; set; }
            public string type { get; set; }
            public int max_quantity { get; set; }
            public float max_price { get; set; }
            public object min_driver_age { get; set; }
            public object min_driver_license_age { get; set; }
            public int limit_price_after_x_days { get; set; }
        }
        public class AvailableExtraResponseAvec
        {
            public int total_count { get; set; }
            public int start { get; set; }
            public int limit { get; set; }
            public List<AvailableExtraAvec> data { get; set; }
        }
        public class AddOn
        {
            public string id { get; set; }
            public string code { get; set; }
            public int quantity { get; set; }
        }
        public class AddOnBundle
        {
            public string code { get; set; }
            public int id { get; set; }
        }
    }
}
