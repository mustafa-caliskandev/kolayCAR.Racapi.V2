using Newtonsoft.Json;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Aytu2
{
    public class AytuResponseBase
    {
        public Sistemrent Sistemrent { get; set; }
    }

    public class AytuCarListResponseBase
    {
        [JsonProperty("Sistemrent")]
        public CarListScope CarListScope { get; set; }
    }

    public class CarListScope
    {
        [JsonProperty("Group")]
        public List<AytuCar> CarList { get; set; }
    }

    public class Sistemrent
    {
        [JsonProperty("Location")]
        public List<AytuLocation> Location { get; set; }
        [JsonProperty("Car")]
        public List<AytuRez> Vehicles { get; set; }
        [JsonProperty("Rezervation")]
        public AytuReservation AytuReservation { get; set; }
        [JsonProperty("Rezervasyon")]
        public AytuRezervasyon AytuRezervasyon { get; set; }
    }



    public class AytuLocation
    {
        public string Location_ID { get; set; }
        public string Location_Name { get; set; }
        public string Address { get; set; }
        public string Mail_Adress { get; set; }
        public string Telephone { get; set; }
        public string _id { get; set; }
    }

    public class AytuCar
    {
        public string Group_ID { get; set; }
        public string Group_Name { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Fuel { get; set; }
        public string Transmission { get; set; }
        public string Driving_License_Age { get; set; }
        public string Driver_Age { get; set; }
        public string SIPP { get; set; }
        public string Provision { get; set; }
        public string _id { get; set; }
    }

    public class AytuRez
    {
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
        public string _id { get; set; }
    }

    public class AytuReservation
    {
        public string ID { get; set; }
        public string Key { get; set; }
        public bool Status { get; set; }
        public string _id { get; set; }
    }

    public class AytuRezervasyon
    {
        public string Kayit_No { get; set; }
        public bool Durum { get; set; }
        public string _id { get; set; }
    }
}
