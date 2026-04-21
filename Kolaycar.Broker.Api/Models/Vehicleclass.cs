using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Vehicleclass
    {
        public int Vehicleclassid { get; set; }
        public bool? Active { get; set; }
        public string Vehicleclasscode { get; set; }
        public string Sippcode { get; set; }
        public int? Order { get; set; }
        public int Brandid { get; set; }
        public int Modelid { get; set; }
        public int? Serialid { get; set; }
        public int Categoryid { get; set; }
        public int Typeid { get; set; }
        public int Personid { get; set; }
        public int Baggageid { get; set; }
        public int Transmissionid { get; set; }
        public int Fuelid { get; set; }
        public bool? Aircondition { get; set; }
        public int? Mindriverage { get; set; }
        public int? Mindrivinglicenseage { get; set; }
        public decimal? Depositamount { get; set; }
        public int? Depositcurrencyid { get; set; }
        public bool? Depositdoublecreditcardrequiredactive { get; set; }
        public DateTime Recorddate { get; set; }
        public string Imageurl { get; set; }

        public virtual Vehiclebrand VehicleBrand { get; set; }
        public virtual Vehiclemodel VehicleModel { get; set; }
    }
}
