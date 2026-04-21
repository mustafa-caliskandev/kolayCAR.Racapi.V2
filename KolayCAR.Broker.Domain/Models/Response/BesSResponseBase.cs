using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class BesSResponseBase
    {
        public class Data
        {
            public int id { get; set; }
            public Name name { get; set; }
            public int city_id { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public int? on_web { get; set; }
            public int on_panel { get; set; }
            public object parent_id { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public int? sort { get; set; }
            public int multiplier { get; set; }
            public string image { get; set; }
            public string address { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string slug { get; set; }
            public object deleted_at { get; set; }
            public string quipas_code { get; set; }
            public int on_branch { get; set; }
            public int on_report { get; set; }
            public string name_tr { get; set; }
            public string branch_name_tr { get; set; }
            public City city { get; set; }
            public object parent { get; set; }
            public string code { get; set; }
            public string commission { get; set; }
            public Description description { get; set; }
            public float? fixed_price { get; set; }
            public int? multiplying_days { get; set; }
            public int extra_type { get; set; }
            public string icon { get; set; }
            public int? model_id { get; set; }
            public string sipp { get; set; }
            public string license_plate { get; set; }
            public object status_id { get; set; }
            public int api { get; set; }
            public int company_id { get; set; }
            public int? model_year { get; set; }
            public int? kilometer { get; set; }
            public string imei { get; set; }
            public string ownership { get; set; }
            public string cost { get; set; }
            public string erac_create_date { get; set; }
            public int EracId { get; set; }
            public string sasi_no { get; set; }
            public int is_ud { get; set; }
            public int is_active { get; set; }
            public int? diff_km { get; set; }
            public int all_status_company { get; set; }
            public string chassis_type { get; set; }
            public string model_image { get; set; }
            public Model model { get; set; }
            public List<Location> locations { get; set; }
            public Company company { get; set; }
            public List<VehicleDetail> vehicle_detail { get; set; }
            public string search_vehicle_key { get; set; }
            public int day_range { get; set; }
            public float drop_distance { get; set; }
            public List<SippSetting> sipp_setting { get; set; }
            public Brand brand { get; set; }
            public float drop_price { get; set; }
            public float sale_price { get; set; }
            public float price { get; set; }
            public Size size { get; set; }
            public Door door { get; set; }
            public Transmission transmission { get; set; }
            public Fuel fuel { get; set; }
        }

        public class LocationResponse
        {
            public int current_page { get; set; }
            public List<Data> data { get; set; }
            public string first_page_url { get; set; }
            public int? from { get; set; }
            public int? last_page { get; set; }
            public string last_page_url { get; set; }
            public object next_page_url { get; set; }
            public string path { get; set; }
            public int? per_page { get; set; }
            public object prev_page_url { get; set; }
            public int? to { get; set; }
            public int? total { get; set; }
            public bool status { get; set; }
            public int? code { get; set; }
            public string message { get; set; }
        }
        public class ExtraResponse
        {
            public int? current_page { get; set; }
            public List<Data> data { get; set; }
            public string first_page_url { get; set; }
            public int? from { get; set; }
            public int? last_page { get; set; }
            public string last_page_url { get; set; }
            public object next_page_url { get; set; }
            public string path { get; set; }
            public int? per_page { get; set; }
            public object prev_page_url { get; set; }
            public int? to { get; set; }
            public int? total { get; set; }
            public bool status { get; set; }
            public int? code { get; set; }
            public string message { get; set; }
        }
        public class VehicleClassResponse
        {
            public int current_page { get; set; }
            public List<Data> data { get; set; }
            public string first_page_url { get; set; }
            public int? from { get; set; }
            public int last_page { get; set; }
            public string last_page_url { get; set; }
            public object next_page_url { get; set; }
            public string path { get; set; }
            public int per_page { get; set; }
            public object prev_page_url { get; set; }
            public int? to { get; set; }
            public int? total { get; set; }
            public bool status { get; set; }
            public int? code { get; set; }
            public string message { get; set; }
        }
        public class VehicleSearchResponse
        {
            public int? current_page { get; set; }
            public List<Data> data { get; set; }
            public string first_page_url { get; set; }
            public int? from { get; set; }
            public int last_page { get; set; }
            public string last_page_url { get; set; }
            public object next_page_url { get; set; }
            public string path { get; set; }
            public int? per_page { get; set; }
            public object prev_page_url { get; set; }
            public int? to { get; set; }
            public int total { get; set; }
            public bool status { get; set; }
            public int? code { get; set; }
            public string message { get; set; }
        }
        public class ExtraSearchResponse
        {
            public int id { get; set; }
            public Name name { get; set; }
            public string code { get; set; }
            public int commission { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public object deleted_at { get; set; }
            public Description description { get; set; }
            public float? fixed_price { get; set; }
            public int multiplying_days { get; set; }
            public object sort { get; set; }
            public int? on_web { get; set; }
            public int extra_type { get; set; }
            public string icon { get; set; }
            public object prices { get; set; }
            public float price { get; set; }
            public float daily_price { get; set; }
            public float commission_total { get; set; }
        }
        public class ReservationResponse
        {
            public bool status { get; set; }
            public string voucher { get; set; }
            public int code { get; set; }
            public string message { get; set; }
        }


        //--------------
        public class Pivot
        {
            public int? vehicle_id { get; set; }
            public int? location_id { get; set; }
        }
        public class Location
        {
            public int? id { get; set; }
            public Name name { get; set; }
            public int? city_id { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public int on_web { get; set; }
            public int on_panel { get; set; }
            public object parent_id { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public int? sort { get; set; }
            public int? multiplier { get; set; }
            public string image { get; set; }
            public string address { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string slug { get; set; }
            public object deleted_at { get; set; }
            public string quipas_code { get; set; }
            public int? on_branch { get; set; }
            public int? on_report { get; set; }
            public string name_tr { get; set; }
            public string branch_name_tr { get; set; }
            public Pivot pivot { get; set; }
        }
        public class Model
        {
            public int? id { get; set; }
            public string name { get; set; }
            public string image { get; set; }
            public string slug { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public object order { get; set; }
            public int vehicle_brand_id { get; set; }
            public Brand brand { get; set; }
        }
        public class VehicleDetail
        {
            public int id { get; set; }
            public int vehicle_id { get; set; }
            public string engine_no { get; set; }
            public string license_no { get; set; }
            public string hgs_ticket { get; set; }
            public int size_id { get; set; }
            public string care_km { get; set; }
            public int care_km_interval_id { get; set; }
            public int colour_id { get; set; }
            public int brand_id { get; set; }
            public string submodel { get; set; }
            public int chassis_id { get; set; }
            public int? engine_capacity_id { get; set; }
            public int fuel_id { get; set; }
            public int transmission_id { get; set; }
            public string door_number { get; set; }
            public string seat_number { get; set; }
            public string reservation_begin_date { get; set; }
            public string spare_key { get; set; }
            public int tyre_id { get; set; }
            public object scrap_discount { get; set; }
            public string pledge_bank { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public string arrival_cost { get; set; }
            public string kdv_rate { get; set; }
            public string stump_duty { get; set; }
            public string license_plate_registration_fee { get; set; }
            public string mtv_tax { get; set; }
            public string vehicle_loan_commission_amount { get; set; }
            public string vehicle_loan_interest_amount { get; set; }
            public string other_expenses { get; set; }
            public string sub_lease_cost { get; set; }
            public float sale_price { get; set; }
            public string registration_date { get; set; }
            public object invoice_receipt_date { get; set; }
            public string insurance_policy_amount { get; set; }
            public object insurance_policy_expiry_date { get; set; }
            public int time_of_sale { get; set; }
            public string traffic_policy_amount { get; set; }
            public string traffic_policy_expiry_date { get; set; }
            public string policy_no { get; set; }
            public string traffic_policy_cancellation_amount { get; set; }
            public string sales_record_date { get; set; }
            public string date_of_sale { get; set; }
        }
        public class Company
        {
            public int id { get; set; }
            public string name { get; set; }
            public string operation_type { get; set; }
            public float? show_api_price { get; set; }
            public string email { get; set; }
            public string api_service_name { get; set; }
            public object carringo_key { get; set; }
            public string api_username { get; set; }
            public string api_password { get; set; }
            public string api_locations { get; set; }
            public object carringo_locations { get; set; }
            public object api_extras { get; set; }
            public object staff_phone_1 { get; set; }
            public object staff_phone_2 { get; set; }
            public object emergency_phone { get; set; }
            public int sales_rate { get; set; }
            public object commission { get; set; }
            public object price_group_id { get; set; }
            public int? extra_group_id { get; set; }
            public string exchange_policy_code { get; set; }
            public object currency_id { get; set; }
            public object priorty { get; set; }
            public object company_group_id { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public object deleted_at { get; set; }
            public int all_suppliers { get; set; }
            public int sales_rate_self { get; set; }
            public object locked { get; set; }
            public object website { get; set; }
            public object invoice_limit { get; set; }
            public string used_invoice_limit { get; set; }
            public int? all_locations { get; set; }
            public string address { get; set; }
            public string manager_name { get; set; }
            public string manager_phone { get; set; }
            public object platform { get; set; }
            public string tax_number { get; set; }
            public string tax_office { get; set; }
            public string City { get; set; }
            public string zipCode { get; set; }
            public string company_invoice_name { get; set; }
            public object EracId { get; set; }
            public object sort { get; set; }
            public int auto_service_fee { get; set; }
            public int reservation_status_completed { get; set; }
        }
        public class Description
        {
            public string en { get; set; }
            public string tr { get; set; }
            public string de { get; set; }
            public string ar { get; set; }
        }
        public class Name
        {
            public string en { get; set; }
            public string tr { get; set; }
            public string de { get; set; }
            public string ar { get; set; }
        }
        public class City
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        public class Fuel
        {
            public int id { get; set; }
            public Name name { get; set; }
            public Description description { get; set; }
            public string code { get; set; }
            public object icon { get; set; }
            public int active { get; set; }
            public int sort { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
        }
        public class Transmission
        {
            public int id { get; set; }
            public Name name { get; set; }
            public Description description { get; set; }
            public string code { get; set; }
            public object icon { get; set; }
            public int active { get; set; }
            public int sort { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
        }
        public class Door
        {
            public int id { get; set; }
            public Name name { get; set; }
            public Description description { get; set; }
            public string code { get; set; }
            public object icon { get; set; }
            public int active { get; set; }
            public int sort { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public string baggage { get; set; }
        }
        public class Size
        {
            public int id { get; set; }
            public Name name { get; set; }
            public Description description { get; set; }
            public string code { get; set; }
            public object icon { get; set; }
            public int? active { get; set; }
            public int sort { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public string baggage { get; set; }
        }
        public class Brand
        {
            public int id { get; set; }
            public string name { get; set; }
            public string image { get; set; }
            public string slug { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public object order { get; set; }
        }
        public class SippSetting
        {
            public int id { get; set; }
            public string sipp { get; set; }
            public int deposit { get; set; }
            public int? age_limit { get; set; }
            public int? license_age_limit { get; set; }
            public string descreption { get; set; }
            public string long_term_viewer { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
        }
        public class NewReponseBaseVehicleClass
        {
            public class Brand
            {
                public int id { get; set; }
                public string name { get; set; }
                public string image { get; set; }
                public string slug { get; set; }
                public string created_at { get; set; }
                public string updated_at { get; set; }
                public object order { get; set; }
            }

            public class Description
            {
                public string tr { get; set; }
                public string ar { get; set; }
                public string en { get; set; }
                public string de { get; set; }
            }

            public class Fuel
            {
                public int id { get; set; }
                public Name name { get; set; }
                public Description description { get; set; }
                public string code { get; set; }
                public object icon { get; set; }
                public int active { get; set; }
                public int sort { get; set; }
                public string created_at { get; set; }
                public string updated_at { get; set; }
            }

            public class Model
            {
                public int id { get; set; }
                public string name { get; set; }
                public string image { get; set; }
                public string slug { get; set; }
                public string created_at { get; set; }
                public string updated_at { get; set; }
                public object order { get; set; }
                public int vehicle_brand_id { get; set; }
                public List<Fuel> fuel { get; set; }
                public List<Transmission> transmission { get; set; }
                public Brand brand { get; set; }
            }

            public class Name
            {
                public string tr { get; set; }
                public string en { get; set; }
                public string de { get; set; }
                public object ar { get; set; }
            }

            public class Root
            {
                public int id { get; set; }
                public int model_id { get; set; }
                public string sipp { get; set; }
                public int model_year { get; set; }
                public int kilometer { get; set; }
                public int? diff_km { get; set; }
                public string chassis_type { get; set; }
                public string model_image { get; set; }
                public Model model { get; set; }
                public List<List<object>> vehicle_detail { get; set; }
            }

            public class Transmission
            {
                public int id { get; set; }
                public Name name { get; set; }
                public Description description { get; set; }
                public string code { get; set; }
                public object icon { get; set; }
                public int active { get; set; }
                public int sort { get; set; }
                public string created_at { get; set; }
                public string updated_at { get; set; }
            }

        }
    }
}
