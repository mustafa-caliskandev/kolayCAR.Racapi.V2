using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Locationvendorcloseddate
    {
        public int Id { get; set; }
        public int Locationid { get; set; }
        public int Vendorid { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Enddate { get; set; }
    }
}
