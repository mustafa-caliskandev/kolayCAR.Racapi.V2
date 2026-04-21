using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class ElitcarResponseBase
    {
        public int count { get; set; }
        public List<Vehicle> vehicles { get; set; }
        public Options options { get; set; }
        public Services services { get; set; }
        public Cars cars { get; set; }
        public List<Extra> extras { get; set; }

        public int id { get; set; }
        public string pnr { get; set; }


        public class Properties
        {
            public string luggage_volume { get; set; }
            public string number_of_seats { get; set; }
        }

        public class Conditions
        {
            public int min_driver_age { get; set; }
            public int min_driving_license_age { get; set; }
        }

        public class Vehicle
        {
            public int id { get; set; }
            public int brand_id { get; set; }
            public int fuel_id { get; set; }
            public int gear_id { get; set; }
            public string sipp_code { get; set; }
            public string image_url { get; set; }
            public string brand { get; set; }
            public string name { get; set; }
            public string fuel { get; set; }
            public string gear { get; set; }
            public Properties properties { get; set; }
            public Conditions conditions { get; set; }
        }

        public class Options
        {
            public bool drop_allowed { get; set; }
            public int day { get; set; }
            public string country_code { get; set; }
            public string currency_code { get; set; }
            public int currency_eur_parity { get; set; }
            public object track_query_id { get; set; }
        }

        public class List : Vehicle
        {

            public string description { get; set; }
            public int min { get; set; }
            public int max { get; set; }
            public int price { get; set; }
            public bool free { get; set; }
            public List<int> car_ids { get; set; }
            public int available_count { get; set; }

        }

        public class ListExtra : Extra
        {
            public int min { get; set; }
            public int max { get; set; }
            public int price { get; set; }
            public bool free { get; set; }

        }

        public class Services
        {
            public int count { get; set; }
            public List<ListExtra> list { get; set; }
        }



        public class AvaibilityConditions : Conditions
        {
            public int provision { get; set; }
            public int max_km { get; set; }
        }

        public class Cars
        {
            public int count { get; set; }
            public int total_available_count { get; set; }
            public List<List> list { get; set; }
        }

        public class Extra
        {
            public int id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public int min_piece { get; set; }
            public int max_piece { get; set; }
            public int min_day { get; set; }
            public int max_day { get; set; }
            public string price_type { get; set; }
        }




    }
}
