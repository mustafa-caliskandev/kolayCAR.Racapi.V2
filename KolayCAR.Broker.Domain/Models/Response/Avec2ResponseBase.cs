using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class Avec2ResponseBase
    {

        public CityRentResponse CityRent { get; set; }
        public GrsrentResponse Grsrent { get; set; }
        public LocationsResponse Locations { get; set; }
        public Rez_Result rez_Result { get; set; }
        public Rez_Sonuc rez_Sonuc { get; set; }

        public class CarsProperty
        {
            public string Brand { get; set; }
            public string Type { get; set; }
            public string Fuel { get; set; }
            public string Transmission { get; set; }
            public int Group { get; set; }
            public string Chairs { get; set; }
            public string Doors { get; set; }
            public string Big_Bags { get; set; }
            public string Small_Bags { get; set; }
            public object Car_Statement { get; set; }
            public string Group_Name { get; set; }
            public int Driver_Age { get; set; }
            public int Driving_License_Age { get; set; }
            public int Provizyon { get; set; }
            public int id { get; set; }
            public string text { get; set; }
        }
        public class Cars : CarsProperty
        {
            public int ID { get; set; }
            public string Image1_Path { get; set; }
            public string Image2_Path { get; set; }
            public string Image3_Path { get; set; }
            public string Image4_Path { get; set; }
            public int Car_Price { get; set; }
            public bool Return { get; set; }
            public string Group_Str { get; set; }

        }


        public class CityRentResponse
        {
            public List<Cars> Cars { get; set; }
        }



        public class CarList : CarsProperty
        {

            public long Rez_ID { get; set; }
            public int Car_ID { get; set; }
            public string Image_Path { get; set; }
            public string Exchange { get; set; }
            public string Currency { get; set; }
            public float Daily_Rental { get; set; }
            public float Total_Rental { get; set; }
            public float S_Total_Rental { get; set; }
            public float Baby_Seat { get; set; }
            public float Navigation { get; set; }
            public float Private_Driver { get; set; }
            public float Additional_Driver { get; set; }
            public float Drop { get; set; }
            public float Discount { get; set; }
            public string Rent_Path { get; set; }
            public string Teslim_Path { get; set; }
            public int Child_Seat { get; set; }
            public float TGI { get; set; }
            public int Days { get; set; }
            public int Pickup_ID { get; set; }
            public int Drop_Off_ID { get; set; }
            public int Cars_Park_ID { get; set; }
            public float Additional_KM { get; set; }
            public float SCDW { get; set; }
            public int Cancel_Ins { get; set; }
            public float CDW { get; set; }
            public int PAI { get; set; }
            public float KM_150 { get; set; }
            public float KM_400 { get; set; }
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
            public bool Car_Park { get; set; }
            public int Pay_now { get; set; }
            public string Car_Images2 { get; set; }
            public string Car_Images3 { get; set; }
            public string Car_Images4 { get; set; }
            public string Pick_Up { get; set; }
            public string Drop_Off { get; set; }
            public int Cars_General_ID { get; set; }
            public object Car_park_Adress { get; set; }
            public object Maps_Points { get; set; }
            public float List_Price { get; set; }
            public int Tablet_Navigation { get; set; }
            public int Address_Delivery { get; set; }
            public float Young_Driver { get; set; }
            public int Aylik { get; set; }
            public int Kazanc { get; set; }
            public string Arac_Env { get; set; }
            public int Puan_Tutar { get; set; }
            public int Cikis_Program_ID { get; set; }
            public int Donus_Program_ID { get; set; }
            public int Arac_Program_ID { get; set; }
            public int Indirim_Gun { get; set; }
            public int Yalin_Fiyat { get; set; }
            public int Pro_Ind { get; set; }
            public int Kzn { get; set; }
            public object Dll_Str { get; set; }
            public object Kamp { get; set; }
            public string URL_Path { get; set; }

        }

        public class GrsrentResponse
        {
            public List<CarList> Cars { get; set; }
        }
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
            public string Telephone { get; set; }
            public string Adress { get; set; }
            public string Mail_Adress { get; set; }
            public object IATA { get; set; }
            public WorkDays WorkDays { get; set; }
            public object Maps_Points { get; set; }
            public int id { get; set; }
            public string text { get; set; }
        }

        public class LocationsResponse
        {
            public List<Location> Location { get; set; }
        }
        public class Reservation : ReservationProperty
        {
            public int ID { get; set; }
            public bool Payment { get; set; }
            public int Payment_ID { get; set; }
            public string False_Desc { get; set; }
            public string Guid_Str { get; set; }
        }

        public class Rez_Result
        {
            public Reservation Reservation { get; set; }
        }

        public class ReservationProperty
        {
            public bool Result_ { get; set; }
            public object id { get; set; }
            public bool text { get; set; }
        }

        public class Rez_Sonuc
        {
            public ReservationProperty Reservation { get; set; }
        }
    }
}
