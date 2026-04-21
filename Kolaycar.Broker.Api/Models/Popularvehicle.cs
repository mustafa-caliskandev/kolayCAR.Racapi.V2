using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Popularvehicle
    {
        public int Id { get; set; }
        public int Vendorid { get; set; }
        public bool Active { get; set; }
        public string Vehiclecode { get; set; }
        public string Vehiclename { get; set; }
        public bool Islocalvehicle { get; set; }
        public DateTime Recorddate { get; set; }
    }
}
