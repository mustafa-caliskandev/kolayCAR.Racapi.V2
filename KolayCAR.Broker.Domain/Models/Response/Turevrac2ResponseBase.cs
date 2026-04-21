using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class Turevrac2ResponseBase
    {
        public class Location
        {
            public string location_id { get; set; }
            public string location_name { get; set; }
            public string address { get; set; }
            public string mail_adress { get; set; }
            public string telephone { get; set; }
            public string delivery_type { get; set; }
            public string maps_point { get; set; }
            public List<Workday> Workdays { get; set; }
        }

        public class Workday
        {
            public string workday_id { get; set; }
            public string day_name { get; set; }
            public string work_time_start { get; set; }
            public string work_time_end { get; set; }
        }
        public class Vehicle
        {
            public string group_id { get; set; }
            public string group_name { get; set; }
            public string driving_license_age { get; set; }
            public string driver_age { get; set; }
            public string sipp { get; set; }
            public string provision { get; set; }
            public string currency { get; set; }
            public string big_bags { get; set; }
            public string small_bags { get; set; }
            public string chairs { get; set; }
            public string brand { get; set; }
            public string type { get; set; }
            public string fuel { get; set; }
            public string transmission { get; set; }
            public string group_str { get; set; }
            public string image_path { get; set; }
        }
        public class AvailableVehicle
        {
            public string rez_id { get; set; }
            public string cars_park_id { get; set; }
            public string cars_park_name { get; set; }
            public string group_id { get; set; }
            public string car_name { get; set; }
            public string driving_license_age { get; set; }
            public string driver_age { get; set; }
            public string sipp { get; set; }
            public string days { get; set; }
            public string daily_rental { get; set; }
            public string total_rental { get; set; }
            public string office_price { get; set; }
            public string currency { get; set; }
            public string currency_symbol { get; set; }
            public string reservation_source { get; set; }
            public string reservation_source_id { get; set; }
            public string km_limit { get; set; }
            public string drop { get; set; }
            public string provision { get; set; }
            public string big_bags { get; set; }
            public string small_bags { get; set; }
            public string chairs { get; set; }
            public string brand { get; set; }
            public string type { get; set; }
            public string fuel { get; set; }
            public string transmission { get; set; }
            public string group_str { get; set; }
            public string image_path { get; set; }
            public string car_web_id { get; set; }
            public List<Service> Services { get; set; }
        }

        public class Service
        {
            public string service_name { get; set; }
            public string service_title { get; set; }
            public string service_desc { get; set; }
            public string service_total_price { get; set; }
        }
        public class Reservation
        {
            public string id { get; set; }
            public string rez_id { get; set; }
            public bool success { get; set; }
        }
        public class CancelReservation
        {
            public string rez_id { get; set; }
            public bool success { get; set; }
        }
    }
}
