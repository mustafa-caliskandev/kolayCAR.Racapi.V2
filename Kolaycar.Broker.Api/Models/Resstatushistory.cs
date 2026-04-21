using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Resstatushistory
    {
        public long Id { get; set; }
        public string Resno { get; set; }
        public long? Resid { get; set; }
        public int? Vendorid { get; set; }
        public int? Agencyid { get; set; }
        public string Agencyname { get; set; }
        public int? Resstatusid { get; set; }
        public string Resstatusname { get; set; }
        public string Resstatusnote { get; set; }
        public DateTime? Inserteddate { get; set; }
        public int? Userid { get; set; }

        public virtual Rez Rez { get; set; }
    }
}
