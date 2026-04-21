using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class WorkDay
    {
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
        public int Location_ID { get; set; }
        public string Location_Name { get; set; }
        public string Telephone { get; set; }
        public string Adress { get; set; }
        public string Post_Code { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Mail_Adress { get; set; }
        public string IATA { get; set; }
        public WorkDays WorkDays { get; set; }
        public string Maps_Points { get; set; }
    }

    public class Car
    {
        public long Rez_ID { get; set; }
        public int Car_ID { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Fuel { get; set; }
        public string Transmission { get; set; }
        public string Chairs { get; set; }
        public int Doors { get; set; }
        public int Small_Bags { get; set; }
        public int Big_Bags { get; set; }
        public string Image_Path { get; set; }
        public float Exchange { get; set; }
        public string Currency { get; set; }
        public float Daily_Rental { get; set; }
        public float Total_Rental { get; set; }
        public float Baby_Seat { get; set; }
        public float Navigation { get; set; }
        public float Private_Driver { get; set; }
        public float Additional_Driver { get; set; }
        public float Drop { get; set; }
        public float Discount { get; set; }
        public string Rent_Path { get; set; }
        public string Mobil_Path { get; set; }
        public float Child_Seat { get; set; }
        public float TGI { get; set; }
        public float KM_150 { get; set; }
        public float KM_400 { get; set; }
        public int Days { get; set; }
        public int Pickup_ID { get; set; }
        public int Drop_Off_ID { get; set; }
        public int Cars_Park_ID { get; set; }
        public float Additional_KM { get; set; }
        public float SCDW { get; set; }
        public float SDW { get; set; }
        public string Cancel_Ins { get; set; }
        public float CDW { get; set; }
        public float PAI { get; set; }
        public string Promotion_ID { get; set; }
        public string Remainder_Bonus { get; set; }
        public bool Car_Park { get; set; }
        public int Group { get; set; }
        public float Pay_now { get; set; }
        public string Car_Statement { get; set; }
        public string Car_Images2 { get; set; }
        public string Car_Images3 { get; set; }
        public string Car_Images4 { get; set; }
        public string Pick_Up { get; set; }
        public string Drop_Off { get; set; }
        public int Cars_General_ID { get; set; }
        public int Driving_License_Age { get; set; }
        public int Driver_Age { get; set; }
        public string Car_park_Adress { get; set; }
        public string Maps_Points { get; set; }
        public float List_Price { get; set; }
        public float Wifi { get; set; }
        public float Young_Drive { get; set; }
        public float European_Drop { get; set; }
        public string Group_Name { get; set; }
        public float Price_List2 { get; set; }
        public string Dll { get; set; }
        public long Arac_Program_ID { get; set; }
        public string ID { get; set; }
        public string Image1_Path { get; set; }
        public string Image2_Path { get; set; }
        public string Image3_Path { get; set; }
        public string Image4_Path { get; set; }
        public string Car_Price { get; set; }
        public string SIPP { get; set; }
        public string Return { get; set; }
        public string Provizyon { get; set; }
        public string Group_Str { get; set; }
        public string _id { get; set; }
        public string Km_Limit { get; set; }
    }

    public class TurevReservation
    {
        public int _id { get; set; }
        public bool Result_ { get; set; }
        public int ID { get; set; }
        public string Payment { get; set; }
        public int Payment_ID { get; set; }
        public string False_Desc { get; set; }
        public string Guid_Str { get; set; }
        public string Dll { get; set; }

    }

    public class RezResult
    {
        [JsonProperty("Reservation")]
        public TurevReservation TurevReservation { get; set; }
    }

    public class Avekrent
    {
        public List<Location> Location { get; set; }
        public List<Car> Cars { get; set; }
    }

    public class AvekRent
    {
        public List<Location> Location { get; set; }
        public List<Car> Cars { get; set; }
    }

    public class RezSonuc
    {
        [JsonProperty("Reservation")]
        public TurevReservation TurevReservation { get; set; }
    }

    public class AvecResponseBase
    {
        public Avekrent Avekrent { get; set; }
        public AvekRent AvekRent { get; set; }
        public RezResult Rez_Result { get; set; }
        public RezSonuc Rez_Sonuc { get; set; }
    }
}
