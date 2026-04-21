using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class GarajlarResponseBase
    {
        public class RootResponse 
        {
            public bool success { get; set; }
            public string message { get; set; }
        }
        public class CancelResponse 
        {
            public bool status { get; set; }
            public string messages { get; set; }
        }
        public class ResponseBase<T> : RootResponse
        {
            public T data { get; set; }
        }
        public class LocationListResponse
        {
            public string code { get; set; }
            public string telephone { get; set; }
            public string email { get; set; }
            public string province { get; set; }
            public string district { get; set; }
            public string address { get; set; }
            public string monday { get; set; }
            public string tuesday { get; set; }
            public string wednesday { get; set; }
            public string thursday { get; set; }
            public string friday { get; set; }
            public string saturday { get; set; }
            public string sunday { get; set; }
            public string web_name { get; set; }
            public bool? airport_location { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public string old_code { get; set; }
        }
        public class GarajlarExtra
        {
            public string code { get; set; }
            public string name { get; set; }
            public int is_driver_required { get; set; }
            public string description { get; set; }
            public int price { get; set; }
            public int total_price { get; set; }
        }

        public class AvailabilityVehiclesResponse
        {
            public int main_group_id { get; set; }
            public string main_group_code { get; set; }
            public int sub_group_id { get; set; }
            public string sub_group_name { get; set; }
            public string sub_group_short_name { get; set; }
            public int min_driver_age { get; set; }
            public int min_driving_license_year { get; set; }
            public int min_young_driver_age { get; set; }
            public int min_young_drivers_license_year { get; set; }
            public string provision_amount { get; set; }
            public string class_name { get; set; }
            public int baggage_capacity { get; set; }
            public int passenger_capacity { get; set; }
            public string segment { get; set; }
            public string brand { get; set; }
            public string model { get; set; }
            public string model_year { get; set; }
            public string fuel_type { get; set; }
            public string transmission_type { get; set; }
            public string daily_price { get; set; }
            public int days { get; set; }
            public int total_km_limit { get; set; }
            public int? main_rule_id { get; set; }
            public List<Campaign> campaigns { get; set; }
            public List<GarajlarExtra> extras { get; set; }
            public float drop_price { get; set; }
            public float grand_total { get; set; }
            public float total_discount { get; set; }
            public float grand_total_without_discount { get; set; }
        }
        public class Campaign
        {
            public int campaign_id { get; set; }
        }

        public class GarajlarVehicle
        {
            public string main_group_code { get; set; }
            public string sub_group_name { get; set; }
            public string sub_group_short_name { get; set; }
            public int min_driver_age { get; set; }
            public int min_driving_license_year { get; set; }
            public int min_young_driver_age { get; set; }
            public int min_young_drivers_license_year { get; set; }
            public string provision_amount { get; set; }
            public string class_name { get; set; }
            public int baggage_capacity { get; set; }
            public int passenger_capacity { get; set; }
            public string segment { get; set; }
            public string brand { get; set; }
            public string model { get; set; }
            public string fuel_type { get; set; }
            public string transmission_type { get; set; }
        }
        public class GarajlarExtraList
        {
            public string code { get; set; }
            public string name { get; set; }
            public string description { get; set; }
        }
        public class ReservationResponse
        {
            public bool success { get; set; }
            public string message { get; set; }
        }
    }
}
