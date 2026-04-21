using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static KolayCAR.Broker.Domain.Models.Response.CircularResponseBase;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class Circular2ResponseBase
    {
        public class ApiLocationResponse
        {
            public User user { get; set; }
            public Dictionary<string, LocationItem> list { get; set; }
        }
        public class LocationItem
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
            public string apistatus { get; set; }
            public string instructions { get; set; }
            public string isairport { get; set; }
            public string airportcode { get; set; }
            public string addressen { get; set; }
            public string city { get; set; }
        }

        public class AuthResponse
        {
            public bool success { get; set; }
            public string token { get; set; }
            public string id { get; set; }
            public string message { get; set; }
        }
        public class LocationResponse
        {

            public User user { get; set; }
            //public Dictionary<string, Destination> list { get; set; }
            public List<Destination> list { get; set; }
            public int total { get; set; }
            public int page { get; set; }
            public int listsize { get; set; }
            public int totalpage { get; set; }
            public string sort { get; set; }
            //public object[] where { get; set; }
            public bool ascending { get; set; }
            public object search { get; set; }
            public string sitetitle { get; set; }
            public string currenturi { get; set; }
        }
        public class Destination
        {
            public int id { get; set; }
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
            public string instructions { get; set; }
            public string airportcode { get; set; }
            public string isairport { get; set; }
            public string apistatus { get; set; }
            public string addressen { get; set; }
            public string city { get; set; }
        }
        public class User
        {
            public string id { get; set; }
            public string name { get; set; }
            public object photo { get; set; }
        }
        public class ApiResponse
        {
            public User user { get; set; }
            public Dictionary<string, VehicleItem> list { get; set; }

            public int total { get; set; }
            public int page { get; set; }
            public string listsize { get; set; }
            public int totalpage { get; set; }
            public string sort { get; set; }
            // public List<object> where { get; set; } = new();
            public string ascending { get; set; }
            public object search { get; set; }
            public string sitetitle { get; set; }
            public string currenturi { get; set; }
        }

        public class CircularUser
        {
            public string id { get; set; }
            public string name { get; set; }
            public string photo { get; set; }
        }

        public class VehicleItem
        {
            public string id { get; set; }

            // Özel tarih formatı olduğu için string tuttum. İsterseniz converter yazıp DateTime'a çeviririz.
            //[JsonPropertyName("_created")]
            //public string Created { get; set; }

            //[JsonPropertyName("_modified")]
            //public string Modified { get; set; }

            public string name { get; set; }
            public string description { get; set; }
            public string examplemodel { get; set; }
            public string sizeofvehicle { get; set; }
            public string numberofdoors { get; set; }
            public string transmissionanddrive { get; set; }
            public string fuelandac { get; set; }
            public string insuranceinc { get; set; }

            // JSON alanı "class" olduğu için attribute ile eşliyoruz
            [JsonPropertyName("class")]
            public string Class { get; set; }

            public string rezrate { get; set; }
            public string minlimit { get; set; }
            public string mountlykmlimit { get; set; }
            public string dailykmlimit { get; set; }
            public string securitydeposit { get; set; }
            public string kmexceedingfee { get; set; }
            public string drivermaxage { get; set; }
            public string findex { get; set; }
            public string driverlicenseminage { get; set; }
            public string driverminage { get; set; }
            public string excessfee { get; set; }
            public string youngdriverfee { get; set; }
            public string numberofseats { get; set; }
            public string insurance { get; set; }
            public string upgradegroup { get; set; }
            public string photo { get; set; }
        }
        public class VehicleResponse
        {
            public User user { get; set; }
            //public Dictionary<string, CircularVehicle> list { get; set; }
            public List<CircularVehicle> list { get; set; }
            public int total { get; set; }
            public int page { get; set; }
            public int listsize { get; set; }
            public int totalpage { get; set; }
            public string sort { get; set; }
            //  public object[] where { get; set; }
            public bool ascending { get; set; }
            public object search { get; set; }
            public string sitetitle { get; set; }
            public string currenturi { get; set; }
        }
        public class CircularVehicle
        {
            public int id { get; set; }
            public string _created { get; set; }
            public string _modified { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string examplemodel { get; set; }
            public string sizeofvehicle { get; set; }
            public string numberofdoors { get; set; }
            public string transmissionanddrive { get; set; }
            public string fuelandac { get; set; }
            public string termstr { get; set; }
            public string termsen { get; set; }
            public double insuranceinc { get; set; }

            [JsonProperty("class")]
            public string Class { get; set; }
            public int rezrate { get; set; }
            public int minlimit { get; set; }
            public int mountlykmlimit { get; set; }
            public int dailykmlimit { get; set; }
            public double securitydeposit { get; set; }
            public double kmexceedingfee { get; set; }

            public int drivermaxage { get; set; }
            public int findex { get; set; }
            public int driverlicenseminage { get; set; }
            public int driverminage { get; set; }
            public double excessfee { get; set; }
            public double youngdriverfee { get; set; }
            public string upgradegroup { get; set; }
            public string photo { get; set; }
        }
        public class ExtraResponse
        {
            public int id { get; set; }
            public string type { get; set; }
            public string name { get; set; }
            public string nametr { get; set; }
            public float price { get; set; }
            public string priceType { get; set; }
            public string currencycode { get; set; }
            public string description { get; set; }
        }



        public class Daterange
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
            public int dailykmlimit { get; set; }
            public int mountlykmlimit { get; set; }
            public int driverminage { get; set; }
            public Photo photo { get; set; }
            public List<Daterange> dateranges { get; set; }
            public double securitydeposit { get; set; }
            public double kmexceedingfee { get; set; }
            public double excessfee { get; set; }
            public double youngdriverfee { get; set; }
        }

        public class Location
        {
            public int id { get; set; }
            public string code { get; set; }
            public string title { get; set; }
            public string city { get; set; }
            public string coord { get; set; }
            public string workinghours { get; set; }
            public string phone { get; set; }
            public string currencycode { get; set; }
            public Reservation reservation { get; set; }
            public List<Group> groups { get; set; }
            public double dropprice { get; set; }
        }

        public class Period
        {
            public int minDay { get; set; }
            public int maxDay { get; set; }
            public string title { get; set; }
            public double dailyprice { get; set; }
            public double totalprice { get; set; }
        }

        public class Photo
        {
            public string url { get; set; }
            public string mimetype { get; set; }
            public string size { get; set; }
        }

        public class Reservation
        {
            public string startdate { get; set; }
            public string enddate { get; set; }
            public string totaldays { get; set; }
            public double dropprice { get; set; }
            public Returnlocation returnlocation { get; set; }
        }

        public class Returnlocation
        {
            public int id { get; set; }
            public string code { get; set; }
            public string title { get; set; }
            public string city { get; set; }
            public string coord { get; set; }
            public string workinghours { get; set; }
            public string phone { get; set; }
        }

        public class AvailableVehicleResponse
        {
            public User user { get; set; }
            public Location location { get; set; }
            public string sitetitle { get; set; }
            public string currenturi { get; set; }
        }
        public class Data
        {
            public int id { get; set; }
        }

        public class Messages
        {
            public List<string> success { get; set; }
        }

        public class ReservationResponse
        {
            public User user { get; set; }
            public bool success { get; set; }
            public Data data { get; set; }
            public Messages messages { get; set; }
            public string currenturi { get; set; }
        }
        public class ReservationCancelBase
        {
            public User user { get; set; }
            public Result result { get; set; }
            public string currenturi { get; set; }
        }
        public class Circular2PostReservationRequestBody
        {
            public string id { get; set; }
            public string currencycode { get; set; }
            public string reserveddate { get; set; }
            public string startdate { get; set; }
            public string starttime { get; set; }
            public string expectedenddate { get; set; }
            public string expectedendtime { get; set; }
            public string cargroup { get; set; }
            public string driver { get; set; }
            public string paytype { get; set; }
            public string pricing { get; set; }
            public string specialprice { get; set; }
            public string dailyprice { get; set; }
            public string outlocation { get; set; }
            public string returnlocation { get; set; }
            public string referralagent { get; set; }
            public string referralno { get; set; }
            public string prepayment { get; set; }
            public string flightarrivalnumber { get; set; }
            public string dropprice { get; set; }
            public string extra { get; set; }
            public string outinstructions { get; set; }
            public string returninstructions { get; set; }
        }
        public class DriverResponse
        {
            public User user { get; set; }
            public bool success { get; set; }
            public Data data { get; set; }
            public Messages messages { get; set; }
            public string sitetitle { get; set; }
            public string currenturi { get; set; }
        }

    }
}
