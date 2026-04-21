using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Exchangerates
    {
        public int Recid { get; set; }
        public int Currencyid { get; set; }
        public DateTime? Date { get; set; }
        public decimal? Exchangerate { get; set; }
    }
}
