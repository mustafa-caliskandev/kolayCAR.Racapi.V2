using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Smartvehiclesequence
    {
        public int Id { get; set; }
        public int Vendorid { get; set; }
        public int? Locationid { get; set; }
        public DateTime? Startdate { get; set; }
        public DateTime? Enddate { get; set; }
        public bool Active { get; set; }
        public DateTime Createdate { get; set; }
    }
}
