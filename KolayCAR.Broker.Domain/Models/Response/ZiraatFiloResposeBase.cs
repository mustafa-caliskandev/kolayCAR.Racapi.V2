using Newtonsoft.Json;
using System.Collections.Generic;
namespace KolayCAR.Broker.Domain.Models.Response
{
    public class ZiraatFiloResposeBase
    {
        public class Group
        {
            [JsonProperty("@id")]
            public string id { get; set; }
            public string Group_ID { get; set; }
            public string Group_Name { get; set; }
            public string Brand { get; set; }
            public string Type { get; set; }
            public string CaseType { get; set; }
            public string Driving_License_Age { get; set; }
            public string Driver_Age { get; set; }
            public string SIPP { get; set; }
            public string Provision { get; set; }
            public object Currency { get; set; }
            public object Big_Bags { get; set; }
            public object Small_Bags { get; set; }
            public string Chairs { get; set; }
            public string Fuel { get; set; }
            public string Transmission { get; set; }
            public object Group_Str { get; set; }
            public object Image_Path { get; set; }
            public object Image_ID { get; set; }
        }

        public class Root
        {
            [JsonProperty("?xml")]
            public Xml xml { get; set; }
            public Turevsistem Turevsistem { get; set; }
            public Sistemrent Sistemrent { get; set; }
        }

        public class Sistemrent
        {
            public List<Group> Group { get; set; }
            public List<Location> Location { get; set; }
        }

        public class Xml
        {
            [JsonProperty("@version")]
            public string version { get; set; }

            [JsonProperty("@encoding")]
            public string encoding { get; set; }
        }
        public class AuthResponse
        {
            public string refresh { get; set; }
            public string access { get; set; }
            public string application_id { get; set; }
        }

        public class Location
        {
            [JsonProperty("@id")]
            public string id { get; set; }
            public string Location_ID { get; set; }
            public string Location_Name { get; set; }
            public object Address { get; set; }
            public string Mail_Adress { get; set; }
            public string Telephone { get; set; }
            public string Delivery_Type { get; set; }
            public string Maps_Point { get; set; }
            public object WorkDays { get; set; }
        }

        public class MesajBilgi
        {
            [JsonProperty("@id")]
            public string id { get; set; }
            public string Subject { get; set; }
            public string Desc { get; set; }
            public string Kod { get; set; }
        }
        public class Turevsistem
        {
            public string id { get; set; }
            public string Key { get; set; }
            public string Status { get; set; }
            public List<MesajBilgi> MesajBilgi { get; set; }
            public List<Car> Car { get; set; }
        }
        public class Car
        {
            public string id { get; set; }
            public string Rez_ID { get; set; }
            public string Group_ID { get; set; }
            public string SIPP { get; set; }
            public string Car_Name { get; set; }
            public string Driving_License_Age { get; set; }
            public string Driver_Age { get; set; }
            public string Big_Bags { get; set; }
            public string Chairs { get; set; }
            public string Km_Limit { get; set; }
            public string Transmission { get; set; }
            public string Prepayment { get; set; }
            public string Daily_Rental { get; set; }
            public string Total_Rental { get; set; }
            public string Drop { get; set; }
            public string Office_Daily_Rental { get; set; }
            public string Office_Price { get; set; }
            public string Provision { get; set; }
            public string Brand { get; set; }
            public string Type { get; set; }
            public string Fuel { get; set; }
            public string Location_Kod { get; set; }
            public string Reservation_Source { get; set; }
            public string Reservation_Source_ID { get; set; }
            public string Days { get; set; }
            public string Pro_ID { get; set; }
            public string Cars_Park_ID { get; set; }
            public string Child_Seat { get; set; }
            public string Child_Seat_Free { get; set; }
            public string Child_Seat_Daily { get; set; }
            public string Diger { get; set; }
            public string Diger_Free { get; set; }
            public string Diger_Daily { get; set; }
            public string Additional_Driver { get; set; }
            public string Additional_Driver_Free { get; set; }
            public string Additional_Driver_Daily { get; set; }
            public string PAI { get; set; }
            public string PAI_Free { get; set; }
            public string PAI_Daily { get; set; }
            public string Young_Drive { get; set; }
            public string Young_Drive_Free { get; set; }
            public string Young_Drive_Daily { get; set; }
            public string IMM { get; set; }
            public string IMM_Free { get; set; }
            public string IMM_Daily { get; set; }
            public string Cancel { get; set; }
            public string Cancel_Free { get; set; }
            public string Cancel_Daily { get; set; }
            public string Winter_Tire { get; set; }
            public string Winter_Tire_Free { get; set; }
            public string Winter_Tire_Daily { get; set; }
            public string LCF { get; set; }
            public string LCF_Free { get; set; }
            public string LCF_Daily { get; set; }
            public string Max_Assurance { get; set; }
            public string Max_Assurance_Free { get; set; }
            public string Max_Assurance_Daily { get; set; }
            public string XKP { get; set; }
            public string XKP_Free { get; set; }
            public string XKP_Daily { get; set; }
            public string Mini_Damage_Insurance { get; set; }
            public string Mini_Damage_Insurance_Free { get; set; }
            public string Mini_Damage_Insurance_Daily { get; set; }
            public string MKP { get; set; }
            public string MKP_Free { get; set; }
            public string MKP_Daily { get; set; }
            public string SKP { get; set; }
            public string SKP_Free { get; set; }
            public string SKP_Daily { get; set; }
            public string Super_Mini_Damage_Insurance { get; set; }
            public string Super_Mini_Damage_Insurance_Free { get; set; }
            public string Super_Mini_Damage_Insurance_Daily { get; set; }
        }
    }
}

