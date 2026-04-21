using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Vehicleresultstatistic
    {
        public int Id { get; set; }
        public DateTime Recorddate { get; set; }
        public int Pickuplocationid { get; set; }
        public int Returnlocationid { get; set; }
        public int Vendorid { get; set; }
        public string Vendorname { get; set; }
        public int? Apivendorid { get; set; }
        public string Apivendorname { get; set; }
        public int? Vehicleid { get; set; }
        public string Vehiclecode { get; set; }
        public string Vehiclename { get; set; }
        public decimal Dailyprice { get; set; }
        public decimal Apidailyprice { get; set; }
        public int Rentalduration { get; set; }
        public string Vehicleimageurl { get; set; }
        public long? Agencyid { get; set; }
        public string Vendorlogourl { get; set; }
        public int? Fuelid { get; set; }
        public int? Transmissionid { get; set; }
        public int? Baggageid { get; set; }
        public int? Categoryid { get; set; }
        public int? Personid { get; set; }
        public int? Typeid { get; set; }
        public DateTime Pickupdate { get; set; }
        public DateTime Returndate { get; set; }
        public int Currencyid { get; set; }
        public int Langid { get; set; }
        public decimal? Exchangerate { get; set; }
    }
}
