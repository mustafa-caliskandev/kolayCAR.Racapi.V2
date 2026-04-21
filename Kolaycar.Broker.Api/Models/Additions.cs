using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Additions
    {
        public int Id { get; set; }
        public int Userid { get; set; }
        public int Additiontypeid { get; set; }
        public DateTime? Createdate { get; set; }
        public DateTime? Laststatuschangedate { get; set; }
        public string Reservationnumber { get; set; }
        public string Additionname { get; set; }
        public string Description { get; set; }
        public decimal? Vendoramount { get; set; }
        public int? Vendorcurrencyid { get; set; }
        public decimal? Agencyamount { get; set; }
        public int? Agencycurrencyid { get; set; }
        public int Additionstatustype { get; set; }
        public int? Laststatuschangeuserid { get; set; }
    }
}
