using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class CircularResponseBase
    {
        public List<AvaibilityVehicleResponse> results { get; set; }
        public List<AvaibilityExtraResponse> packages { get; set; }
        public AvaibilityQueryResponse query { get; set; }
        public string search_id { get; set; }
        public class AuthLoginResponse
        {
            public bool success { get; set; }
            public string token { get; set; }
            public string message { get; set; }
            public string id { get; set; }
        }

        public class CommonResponse
        {
            public int status { get; set; } = 1;
            public string message { get; set; }

        }

        public class LocationResponse : CommonResponse
        {
            public User user { get; set; }
            public Dictionary<string, Destination> List { get; set; }
            public int total { get; set; }
            public int page { get; set; }
            public string listsize { get; set; }
            public int totalpage { get; set; }
            public string sort { get; set; }
            //public List<object> where { get; set; }
            public string ascending { get; set; }
            public object search { get; set; }
            public string currenturi { get; set; }
        }

        public class Destination
        {
            public string id { get; set; }
            public string _created { get; set; }
            public string _modified { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public string returnlocation { get; set; }
            public string coord { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
            public string workinghours { get; set; }
            public string address { get; set; }
            public string city { get; set; }

        }

        public class VehicleResponse : CommonResponse
        {
            public List<Fleet> fleet { get; set; }
        }

        public class Fleet
        {
            public int carId { get; set; }
            public string carName { get; set; }
            public string fuel { get; set; }
            public string transmission { get; set; }
            public string type { get; set; }
            public string minLicenceAge { get; set; }
            public string minDriverAge { get; set; }
            public string seats { get; set; }
            public string baggage { get; set; }
            public int deposit { get; set; }
            public string deposit_currency { get; set; }
            public string SIPP { get; set; }
        }

        public class AvaibilityVehicleResponse_OLD
        {
            public int car_id { get; set; }
            public string sipp { get; set; }
            public string name { get; set; }
            public float deposit { get; set; }
            public string currency { get; set; }
            public bool is_available { get; set; }
            public string per_day { get; set; }
            public string milage_limit { get; set; }
            public string for_duration { get; set; }
            public string oneway_fee { get; set; }
            public string driver_age { get; set; }
            public string licence_age { get; set; }
            public double alp { get; set; }
        }

        public class AvaibilityExtraResponse
        {
            public int id { get; set; }
            public string eqp { get; set; }
            public string equipment { get; set; }
            public int max_amount { get; set; }
            public int per_day { get; set; }
            public int for_duration { get; set; }
        }

        public class AvaibilityQueryResponse
        {
            public int duration { get; set; }
            public string pickup_date { get; set; }
            public string dropoff_date { get; set; }
            public int pickup_location { get; set; }
            public int dropoff_location { get; set; }
            public List<object> applied_codes { get; set; }
        }

        public class ExtraResponse : CommonResponse
        {
            public int id { get; set; }
            public string type { get; set; }
            public string name { get; set; }
            public float price { get; set; }
            public string priceType { get; set; }
            public string currencycode { get; set; }
            public string description { get; set; }
        }

        //public class PostReservationResponse : CommonResponse
        //{
        //    public string reference_no { get; set; }
        //}

        #region Avaibility
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class User
        {
            public string id { get; set; }
            public string name { get; set; }
            public object photo { get; set; }
        }

        public class Returnlocation
        {
            public string id { get; set; }
            public string code { get; set; }
            public string title { get; set; }
            public string city { get; set; }
            public string coord { get; set; }
            public string workinghours { get; set; }
            public string phone { get; set; }
        }

        public class Reservation
        {
            public string startdate { get; set; }
            public string enddate { get; set; }
            public string totaldays { get; set; }
            public int dropprice { get; set; }
            public Returnlocation returnlocation { get; set; }
        }

        public class Photo
        {
            public string url { get; set; }
            public string mimetype { get; set; }
            public string size { get; set; }
        }

        public class Period
        {
            public int minDay { get; set; }
            public int maxDay { get; set; }
            public string title { get; set; }
            public double dailyprice { get; set; }
            public double totalprice { get; set; }
        }

        public class Dateranx
        {
            public string start { get; set; }
            public string end { get; set; }
            public string between { get; set; }
            public List<Period> periods { get; set; }
        }

        public class Group
        {
            public string title { get; set; }
            public int id { get; set; }
            public string examplemodel { get; set; }
            public string sizeofvehicle { get; set; }
            public string numberofdoors { get; set; }
            public string transmissionanddrive { get; set; }
            public string fuelandac { get; set; }
            public string dailykmlimit { get; set; }
            public string mountlykmlimit { get; set; }
            public string driverminage { get; set; }
            public Photo photo { get; set; }
            public List<Dateranx> dateranges { get; set; }
            public string securitydeposit { get; set; }
            public string kmexceedingfee { get; set; }
            public string excessfee { get; set; }
            public string youngdriverfee { get; set; }
        }

        public class Location
        {
            public string id { get; set; }
            public string code { get; set; }
            public string title { get; set; }
            public string city { get; set; }
            public string coord { get; set; }
            public string workinghours { get; set; }
            public string phone { get; set; }
            public string currencycode { get; set; }
            public Reservation reservation { get; set; }
            public List<Group> groups { get; set; }
        }

        public class AvaibilityVehicleResponse
        {
            public User user { get; set; }
            public Location location { get; set; }
            public string currenturi { get; set; }
        }


        #endregion

        #region AllCar
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 



        public class VehicleList
        {
            public User user { get; set; }
            public List<Group> list { get; set; }
            public int total { get; set; }
            public int page { get; set; }
            public string listsize { get; set; }
            public int totalpage { get; set; }
            public string sort { get; set; }
            public List<object> where { get; set; }
            public string ascending { get; set; }
            public object search { get; set; }
            public string currenturi { get; set; }
        }
        #endregion

        #region Post Reservation Response Model
        public class PostReservationResponseModel
        {
            public User user { get; set; }
            public bool success { get; set; }
            public Data data { get; set; }
        }

        public class Data
        {
            public int id { get; set; }
        }
        #endregion

        #region Post Cancel Reservation Response Model
        public class PostCancelReservationResponseBody
        {
            public User user { get; set; }
            public bool branch { get; set; }
            public int totalpublished { get; set; }
            public int totaltrash { get; set; }
            public int totaldraft { get; set; }
            public int totalclosed { get; set; }
            public Result result { get; set; }
            public object notifications { get; set; }
            public string currenturi { get; set; }
        }

        public class Result
        {
            public string referralno { get; set; }
            public int id { get; set; }
            public string status { get; set; }
        }
        #endregion
    }
}
