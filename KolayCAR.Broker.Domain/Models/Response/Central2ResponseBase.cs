using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class Central2ResponseBase
    {
        public class CentralResponse
        {
            [JsonProperty("?xml")]
            public Xml Xml { get; set; }
            public Sistemrent Sistemrent { get; set; }
            public Turevsistem Turevsistem { get; set; }
        }
        public class CentralReservationResponse
        {
            [JsonProperty("?xml")]
            public Xml Xml { get; set; }
            [JsonProperty("Turevsistem")]
            public ResResponse Turevsistem { get; set; }
        }
        public class Sistemrent
        {
            public List<Location> Location { get; set; }
            public List<Group> Group { get; set; }
        }
        public class Turevsistem
        {
            [JsonProperty("MesajBilgi")]
            public List<MesajBilgi> ExtraList { get; set; }
            public List<Car> Car { get; set; }
        }
        public class ResResponse
        {
            public MesajBilgi MesajBilgi { get; set; }
        }
        public class Xml
        {
            [JsonProperty("@version")]
            public string Version { get; set; }

            [JsonProperty("@encoding")]
            public string Encoding { get; set; }
        }
        public class Group
        {
            [JsonProperty("@id")]
            public string Id { get; set; }
            public string Group_ID { get; set; }
            public string Group_Name { get; set; }
            public string Driving_License_Age { get; set; }
            public string Driver_Age { get; set; }
            public string SIPP { get; set; }
            public string Provision { get; set; }
            public string Currency { get; set; }
            public string Big_Bags { get; set; }
            public string Small_Bags { get; set; }
            public string Chairs { get; set; }
            public string Brand { get; set; }
            public string Type { get; set; }
            public string Fuel { get; set; }
            public string Transmission { get; set; }
            public string Group_Str { get; set; }
            public string Image1_Path { get; set; }
        }
        public class WorkDay
        {
            [JsonProperty("@ID")]
            public string ID { get; set; }
            public string Day_Name { get; set; }
            public string Work_Time_Start { get; set; }
            public string Work_Time_End { get; set; }
        }
        public class WorkDays
        {
            public List<WorkDay> WorkDay { get; set; }
        }
        public class Location
        {
            [JsonProperty("@id")]
            public string Id { get; set; }
            public string Location_ID { get; set; }
            public string Location_Name { get; set; }
            public string Address { get; set; }
            public string Mail_Adress { get; set; }
            public string Telephone { get; set; }
            public string Delivery_Type { get; set; }
            public string Maps_Point { get; set; }
            public string IATA { get; set; }
            public WorkDays WorkDays { get; set; }
        }
        public class MesajBilgi
        {
            [JsonProperty("@id")]
            public string Id { get; set; }
            public string Kod { get; set; }
            public string Subject { get; set; }
            public string Desc { get; set; }
            public string Status { get; set; }
            public string Key { get; set; }
        }
        public class Car
        {
            [JsonProperty("@id")]
            public string Id { get; set; }
            public string Rez_ID { get; set; }
            public string Cars_Park_ID { get; set; }
            public string Cars_Park_Name { get; set; }
            public string Car_Name { get; set; }
            public string SIPP { get; set; }
            public string Driving_License_Age { get; set; }
            public string Driver_Age { get; set; }
            public string Daily_Rental { get; set; }
            public string Days { get; set; }
            public string Total_Rental { get; set; }
            public string Office_Daily_Rental { get; set; }
            public string Big_Bags { get; set; }
            public string Small_Bags { get; set; }
            public string Chairs { get; set; }
            public string Reservation_Source { get; set; }
            public string Reservation_Source_ID { get; set; }
            public string Group_ID { get; set; }
            public string Provision { get; set; }
            public string Km_Limit { get; set; }
            public string Drop { get; set; }
            public string Image_Path { get; set; }
            public string CDW { get; set; }
            public string SCDW { get; set; }
            public string LCF { get; set; }
            public string PAI { get; set; }
            public string Cancel { get; set; }
            public string XKP { get; set; }
            public string Mini_Damage_Insurance { get; set; }
            public string Super_Mini_Damage_Insurance { get; set; }
            public string Max_Assurance { get; set; }
            public string Charger { get; set; }
            public string IMM { get; set; }
            public string Exemption_Insuranc { get; set; }
            public string Young_Drive { get; set; }
            public string Baby_Seat { get; set; }
            public string Navigation { get; set; }
            public string Additional_Driver { get; set; }
            public string LCF_Free { get; set; }
            public string SCDW_Free { get; set; }
            public string CDW_Free { get; set; }
            public string PAI_Free { get; set; }
            public string Currency { get; set; }
            public string Exchange { get; set; }
            public string Currency_Symbol { get; set; }
            public string Fuel { get; set; }
            public string Transmission { get; set; }
            public string Prepayment { get; set; }
            public string Baby_Seat_Free { get; set; }
            public string Navigation_Free { get; set; }
            public string Additional_Driver_Free { get; set; }
            public string Rent_Path { get; set; }
            public string Location_Kod { get; set; }
            public string Service { get; set; }
            public string Brand { get; set; }
            public string Type { get; set; }
            public string UpCar { get; set; }
            public string UpPark { get; set; }
            public string UpPrice { get; set; }
        }
    }
}
