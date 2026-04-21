using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Vehicleclassvendor
    {
        public int Vehicleclassid { get; set; }
        public int Vendorid { get; set; }
        public bool? Active { get; set; }
        public string Apivehicleclasscode { get; set; }
        public string Apivehicleclassname { get; set; }
        public decimal? Deposit { get; set; }
        public int? Mindriverage { get; set; }
        public int? Mindrivinglicenseage { get; set; }
        public DateTime Recorddate { get; set; }
        public int? Dailykmlimit { get; set; }
        public int? Maxkmlimit { get; set; }
    }
}
