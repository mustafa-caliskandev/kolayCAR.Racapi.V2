using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class ErboycarResponseBase
    {
        public List<Location> Locations { get; set; }
        public VehicleResponse Vehicles { get; set; }
        public ExtraResponse Extras { get; set; }

        public class Location
        {
            public string _id { get; set; }
            public string phone { get; set; }
            public string address { get; set; }
            public string name { get; set; }
            public string code { get; set; }
            public string email { get; set; }
            public string pickup_location { get; set; }
            public string vendor { get; set; }
        }

        public class UnAvailableVehicle
        {
            public string _id { get; set; }
            public string group { get; set; }
            public string model { get; set; }
            public string image_url { get; set; }
            public string brand { get; set; }
            public int count { get; set; }
            public float price { get; set; }
            public string status { get; set; }
            public float discounted_daily_price { get; set; }
            public float number_of_passengers { get; set; }
            public int number_of_doors { get; set; }
            public string driver_age { get; set; }
            public string min_licence_age { get; set; }
            public string deposit { get; set; }
            public int daily_km { get; set; }
            public int max_km { get; set; }
            public string transmission_type { get; set; }
            public string fuel_type { get; set; }
            public string _token { get; set; }
        }

        public class AvailableVehicle
        {
            public string _id { get; set; }
            public string group { get; set; }
            public string model { get; set; }
            public string image_url { get; set; }
            public string brand { get; set; }
            public int count { get; set; }
            public float price { get; set; }
            public string status { get; set; }
            public float discounted_daily_price { get; set; }
            public float number_of_passengers { get; set; }
            public int number_of_doors { get; set; }
            public string driver_age { get; set; }
            public string min_licence_age { get; set; }
            public string deposit { get; set; }
            public int daily_km { get; set; }
            public string max_km { get; set; }
            public string transmission_type { get; set; }
            public string fuel_type { get; set; }
            public string _token { get; set; }
        }

        public class VehicleResponse
        {
            public string discountRate { get; set; }
            public int rentalDays { get; set; }
            public object oneWayPrice { get; set; }
            public List<UnAvailableVehicle> unAvailableVehicles { get; set; }
            public List<AvailableVehicle> availableVehicles { get; set; }
            public DateTime isoPickupDateTime { get; set; }
            public DateTime isoDropDateTime { get; set; }
            public string pickupStation { get; set; }
            public string dropStation { get; set; }
            public string contractNo { get; set; }
            public string resNo { get; set; }
            public object monthly { get; set; }
        }

        public class Price
        {
            public float daily_price { get; set; }
            public string currency { get; set; }
            public float constant_price { get; set; }
        }

        public class VehicleExtraCategory
        {
            public string name { get; set; }
        }

        public class ExtraResponse
        {
            public List<Price> prices { get; set; }
            public string _id { get; set; }
            public object note { get; set; }
            public string price_calculation_type { get; set; }
            public DateTime _updated { get; set; }
            public VehicleExtraCategory vehicle_extra_category { get; set; }
            public float price_tl { get; set; }
            public object vehicle_group { get; set; }
            public float price_usd { get; set; }
            public DateTime _created { get; set; }
            public string name_tr { get; set; }
            public float price_gbp { get; set; }
            public string name_en { get; set; }
            public float price_euro { get; set; }
            public string cost_responsible { get; set; }
            public int edit_allowed_on_contract { get; set; }
            public string extra_type { get; set; }
        }

        public class VehicleListItem
        {
            public string _id { get; set; }
            public DateTime _created { get; set; }
            public string group { get; set; }
            public DateTime _updated { get; set; }
            public string similar_vehicle { get; set; }
            public int number_of_passengers { get; set; }
            public int number_of_doors { get; set; }
            public string deposit { get; set; }
            public string driver_age { get; set; }
            public string daily_km { get; set; }
            public string min_licence_age { get; set; }
            public string transmission_type { get; set; }
            public string fuel_type { get; set; }
            public string fuel_type_en { get; set; }
            public string transmission_type_en { get; set; }
            public string vendor { get; set; }
            public string image_url { get; set; }
            public string max_km { get; set; }
            public string segment { get; set; }
            public int? segment_id { get; set; }
        }

        public class Customer
        {
            public string _id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string phone_mobile { get; set; }
            public string email { get; set; }
            public int __v { get; set; }
        }

        public class Company
        {
        }

        public class PickupStation
        {
            public string _id { get; set; }
            public string name { get; set; }
            public string code { get; set; }
        }

        public class DropStation
        {
            public string _id { get; set; }
            public string name { get; set; }
            public string code { get; set; }
        }

        public class VehicleGroup
        {
            public string _id { get; set; }
            public string group { get; set; }
        }

        public class VehicleExtrasCustomPrice
        {
            public string vehicle_extra { get; set; }
            public float constant_price { get; set; }
            public float daily_price { get; set; }
            public string currency { get; set; }
        }

        public class Summary
        {
            public DateTime _created { get; set; }
            public string status { get; set; }
            public string comment { get; set; }
            public float vehicle_extras_total_price_tl { get; set; }
            public float final_price_tl { get; set; }
            public DateTime start_date { get; set; }
            public DateTime end_date { get; set; }
            public string res_no { get; set; }
            public string contract_no { get; set; }
            public string reservation_source { get; set; }
            public int is_cancel { get; set; }
            public float paid { get; set; }
            public string customer_type { get; set; }
            public Customer customer { get; set; }
            public Company company { get; set; }
            public PickupStation pickup_station { get; set; }
            public DropStation drop_station { get; set; }
            public VehicleGroup vehicle_group { get; set; }
            public float daily_price_tl { get; set; }
            public int reservation_days { get; set; }
            public List<VehicleExtrasCustomPrice> vehicle_extras_custom_prices { get; set; }
            public float daily_price { get; set; }
            public string payment_type { get; set; }
            public string currency { get; set; }
            public string price_list { get; set; }
            public int pickup_fuel_condition { get; set; }
            public int delivery { get; set; }
            public int exchange_rate { get; set; }
            public string discount_type { get; set; }
            public string user_res_open { get; set; }
            public int vat { get; set; }
            public int discount_rate { get; set; }
            public float discounted_daily_price { get; set; }
            public float discounted_rate_price { get; set; }
            public int is_gone { get; set; }
            public string comment2 { get; set; }
            public float rental_price_total_without_vat_tl { get; set; }
            public float vat_tl { get; set; }
            public float rental_price_total_tl { get; set; }
            public object oneWayPrice { get; set; }
            public int api_res { get; set; }
            public int partial_payment { get; set; }
            public float partial_amount { get; set; }
            public string contract_currency { get; set; }
        }

        public class Reservation
        {
            public string reservation_status { get; set; }
            public Summary summary { get; set; }
            public string statusCode { get; set; }
            public string code { get; set; }
            public string error { get; set; }
            public string message { get; set; }
        }

        public class ReservationCancel
        {
            public string status { get; set; }
            public string message { get; set; }
            public string error { get; set; }
            public int statusCode { get; set; }
        }
    }
}
