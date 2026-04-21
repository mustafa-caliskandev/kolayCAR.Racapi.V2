using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class HaraResponseBase
    {
        [JsonProperty("AytuRent")]
        public AytuRent HaraResponse { get; set; }
        [JsonProperty("TurevRent")]
        public TurevRent TurevRentResponse { get; set; }
        [JsonProperty("CityRent")]
        public CityRent CityRentResponse { get; set; }
        public RezResult Rez_Result { get; set; }
        public RezSonuc Rez_Sonuc { get; set; }

        public class WorkDay
        {
            public string Day_Name { get; set; }
            public string Work_Time_Start { get; set; }
            public string Work_Time_End { get; set; }
            public string _ID { get; set; }
        }

        public class WorkDays
        {
            public List<WorkDay> WorkDay { get; set; }
        }

        public class Location
        {
            public int Location_ID { get; set; }
            public string Location_Name { get; set; }
            public string Telephone { get; set; }
            public string Adress { get; set; }
            public string Mail_Adress { get; set; }
            public string IATA { get; set; }
            public WorkDays WorkDays { get; set; }
            public string Maps_Points { get; set; }
            public string _id { get; set; }
        }

        public class Cars
        {
            public string ID { get; set; }
            public long Rez_ID { get; set; }
            public int Car_ID { get; set; }
            public string Brand { get; set; }
            public string Type { get; set; }
            public string Fuel { get; set; }
            public string Transmission { get; set; }
            public string Chairs { get; set; }
            public string Doors { get; set; }
            public int? Small_Bags { get; set; }
            public int? Big_Bags { get; set; }
            public string Image_Path { get; set; }
            public string Exchange { get; set; }
            public string Currency { get; set; }
            public string Daily_Rental { get; set; }
            public string Total_Rental { get; set; }
            public string S_Total_Rental { get; set; }
            public string Baby_Seat { get; set; }
            public string Navigation { get; set; }
            public string Private_Driver { get; set; }
            public string Additional_Driver { get; set; }
            public string Drop { get; set; }
            public string Discount { get; set; }
            public string Rent_Path { get; set; }
            public string Teslim_Path { get; set; }
            public string Child_Seat { get; set; }
            public string TGI { get; set; }
            public int Days { get; set; }
            public int Pickup_ID { get; set; }
            public int Drop_Off_ID { get; set; }
            public int Cars_Park_ID { get; set; }
            public string SCDW { get; set; }
            public string Cancel_Ins { get; set; }
            public string CDW { get; set; }
            public string PAI { get; set; }
            public string KM_150 { get; set; }
            public string KM_400 { get; set; }
            public string Customer_ID { get; set; }
            public string Customer_Name { get; set; }
            public string Customer_Surname { get; set; }
            public string Customer_Telephone { get; set; }
            public string Customer_Mobile { get; set; }
            public string Customer_Mail { get; set; }
            public string Customer_Adress { get; set; }
            public string Customer_City { get; set; }
            public string Customer_Country { get; set; }
            public string Promotion_ID { get; set; }
            public string Remainder_Bonus { get; set; }
            public string Group { get; set; }
            public string Pay_now { get; set; }
            public string Car_Statement { get; set; }
            public string Car_Images2 { get; set; }
            public string Car_Images3 { get; set; }
            public string Car_Images4 { get; set; }
            public string Image1_Path { get; set; }
            public string Image2_Path { get; set; }
            public string Image3_Path { get; set; }
            public string Image4_Path { get; set; }
            public string Pick_Up { get; set; }
            public string Drop_Off { get; set; }
            public string Cars_General_ID { get; set; }
            public int Driver_Age { get; set; }
            public int Driving_License_Age { get; set; }
            public string Car_park_Adress { get; set; }
            public string Maps_Points { get; set; }
            public string List_Price { get; set; }
            public string Group_Name { get; set; }
            public string Provizyon { get; set; }
            public string Kazanc { get; set; }
            public string Yalin_Fiyat { get; set; }
            public string SIPP { get; set; }
            public string Km_Limit { get; set; }
            public string _id { get; set; }
            public string Return { get; set; }
            public string Group_Str { get; set; }
        }

        public class Reservation
        {
            public bool Result_ { get; set; }
            public string ID { get; set; }
            public string Payment { get; set; }
            public string Payment_ID { get; set; }
            public string False_Desc { get; set; }
            public string Guid_Str { get; set; }
            public string _id { get; set; }
        }

        public class RezSonuc
        {
            public Reservation Reservation { get; set; }
        }

        public class RezResult
        {
            public Reservation Reservation { get; set; }
        }

        public class CityRent
        {
            [JsonConverter(typeof(CustomArrayConverter<Cars>))]
            public List<Cars> Cars { get; set; }
        }

        public class TurevRent
        {
            [JsonConverter(typeof(CustomArrayConverter<Cars>))]
            public List<Cars> Cars { get; set; }
        }

        public class AytuRent
        {
            public List<Location> Location { get; set; }
        }
    }
}
