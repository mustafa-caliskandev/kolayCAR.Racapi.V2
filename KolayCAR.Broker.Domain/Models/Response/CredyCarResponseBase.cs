using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class CredyCarResponseBase
    {
        public AytuRentResponse AytuRent { get; set; }
        public CityRentResponse CityRent { get; set; }
        public TurevRentResponse TurevRent { get; set; }
        public Rez_SonucResponse Rez_Sonuc { get; set; }
        public Rez_ResultResponse Rez_Result { get; set; }
        public class WorkDay
        {
            public string Day_Name { get; set; }
            public DateTime Work_Time_Start { get; set; }
            public DateTime Work_Time_End { get; set; }
            public int ID { get; set; }
            public string text { get; set; }
        }

        public class WorkDays
        {
            public List<WorkDay> WorkDay { get; set; }
        }

        public class Location
        {
            public int Location_ID { get; set; }
            public string Location_Name { get; set; }
            public object Telephone { get; set; }
            public string Adress { get; set; }
            public string Mail_Adress { get; set; }
            public string IATA { get; set; }
            public WorkDays WorkDays { get; set; }
            public string Maps_Points { get; set; }
            public int id { get; set; }
            public string text { get; set; }
        }

        public class AytuRentResponse
        {
            public List<Location> Location { get; set; }
        }
        public class CarsList : Car
        {
            public int ID { get; set; }
            public int Group { get; set; }
            public string Image1_Path { get; set; }
            public string Image2_Path { get; set; }
            public string Image3_Path { get; set; }
            public string Image4_Path { get; set; }
            public bool Return { get; set; }
            public object SIPP { get; set; }
            public int Driver_Lisance_Age { get; set; }
            public int Provizyon { get; set; }
            public string Group_Str { get; set; }
        }

        public class CityRentResponse
        {
            public List<CarsList> Cars { get; set; }
        }

        public class Car
        {
            public string Brand { get; set; }
            public string Type { get; set; }
            public string Fuel { get; set; }
            public string Transmission { get; set; }
            public int Chairs { get; set; }
            public int Doors { get; set; }
            public object Big_Bags { get; set; }
            public string Small_Bags { get; set; }
            public string Group_Name { get; set; }
            public int Driver_Age { get; set; }
            public int id { get; set; }
            public string text { get; set; }
        }
        public class CarsAvailability : Car
        {
            public double Rez_ID { get; set; }
            public int Car_ID { get; set; }
            public string Image_Path { get; set; }
            public int Exchange { get; set; }
            public string Currency { get; set; }
            public int Daily_Rental { get; set; }
            public int Total_Rental { get; set; }
            public int S_Total_Rental { get; set; }
            public int Baby_Seat { get; set; }
            public int Navigation { get; set; }
            public int Private_Driver { get; set; }
            public int Additional_Driver { get; set; }
            public int Drop { get; set; }
            public int Discount { get; set; }
            public string Rent_Path { get; set; }
            public string Teslim_Path { get; set; }
            public int Child_Seat { get; set; }
            public int TGI { get; set; }
            public int Days { get; set; }
            public int Pickup_ID { get; set; }
            public int Drop_Off_ID { get; set; }
            public int Cars_Park_ID { get; set; }
            public int SCDW { get; set; }
            public int Cancel_Ins { get; set; }
            public int CDW { get; set; }
            public int PAI { get; set; }
            public int KM_150 { get; set; }
            public int KM_400 { get; set; }
            public int Customer_ID { get; set; }
            public object Customer_Name { get; set; }
            public object Customer_Surname { get; set; }
            public object Customer_Telephone { get; set; }
            public object Customer_Mobile { get; set; }
            public object Customer_Mail { get; set; }
            public object Customer_Adress { get; set; }
            public object Customer_City { get; set; }
            public object Customer_Country { get; set; }
            public object Promotion_ID { get; set; }
            public int Remainder_Bonus { get; set; }
            public int Group { get; set; }
            public int Pay_now { get; set; }
            public object Car_Statement { get; set; }
            public string Car_Images2 { get; set; }
            public string Car_Images3 { get; set; }
            public string Car_Images4 { get; set; }
            public string Pick_Up { get; set; }
            public string Drop_Off { get; set; }
            public int Cars_General_ID { get; set; }
            public int Driving_License_Age { get; set; }
            public string Car_park_Adress { get; set; }
            public string Maps_Points { get; set; }
            public int List_Price { get; set; }
            public int Provizyon { get; set; }
            public int Kazanc { get; set; }
            public int Yalin_Fiyat { get; set; }
            public object SIPP { get; set; }
            public int Km_Limit { get; set; }
            public int Puan_Tutar { get; set; }
            public int Cikis_Program_ID { get; set; }
            public int Donus_Program_ID { get; set; }
            public int Arac_Program_ID { get; set; }
            public int Return_Cars_Park_ID { get; set; }
            public int Car_R_ID { get; set; }
            public string Symbol { get; set; }
        }

        public class TurevRentResponse
        {
            public List<CarsAvailability> Cars { get; set; }
        }

        public class ReservationPost : Reservation
        {
            public int ID { get; set; }
            public object Payment { get; set; }
            public int Payment_ID { get; set; }
            public object False_Desc { get; set; }
            public string Guid_Str { get; set; }
        }

        public class Rez_ResultResponse
        {
            public ReservationPost Reservation { get; set; }
        }
        public class Reservation
        {
            public bool Result_ { get; set; }
            public int id { get; set; }
            public bool text { get; set; }
        }

        public class Rez_SonucResponse
        {
            public Reservation Reservation { get; set; }
        }

    }
}
