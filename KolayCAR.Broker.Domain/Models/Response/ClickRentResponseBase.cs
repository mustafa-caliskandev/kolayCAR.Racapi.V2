using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class ClickRentResponseBase
    {

        public class AuthLoginResponse
        {
            public string Token { get; set; }
        }

        public class LocationResponse : List<Location>
        {

            //public List<Location> Data { get; set; }
        }

        public class Location
        {
            public string Geocoord { get; set; }
            public string IataCode { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string PhoneNumber { get; set; }
            public string Adress { get; set; }
            public string ZipCode { get; set; }
            public string Country { get; set; }
            public string RegionCode { get; set; }
            public string StationCode { get; set; }
            public string StationType { get; set; }
            public string LocationEmail { get; set; }
            public string PickUpInstructions { get; set; }
            public List<string> OpenningDays { get; set; }
        }

        public class Static_Vehicle
        {
            public int Id { get; set; }
            public string Group { get; set; }
            public string Acriss { get; set; }
            public string Transmission { get; set; }
            public float ExcessAmount { get; set; }
            public float StandartDeposit { get; set; }
            public float ExcessDeposit { get; set; }
            public int Seats { get; set; }
            public int Doors { get; set; }
            public int MinAge { get; set; }
            public int FuelTank { get; set; }
            public string ClickRelax { get; set; }
            public string ModelSuggestion { get; set; }
            public string GroupName { get; set; }
        }

        public class Vehicle
        {
            public string acriss { get; set; }
            public int id { get; set; }
            public string marcaModelo { get; set; }
            public string transmission { get; set; }
            public double excessAmount { get; set; }
            public double securityDeposit { get; set; }
            public string seatDoorsLuggage { get; set; }
            public int minAge { get; set; }
            public double fullTankPrice { get; set; }
            public string convertible { get; set; }
            public string imageURL { get; set; }
            public double price { get; set; }
            public double oneWay { get; set; }
            public object outHoursPickup { get; set; }
            public string currency { get; set; }
            public object outHoursreturn { get; set; }

        }
    }
}