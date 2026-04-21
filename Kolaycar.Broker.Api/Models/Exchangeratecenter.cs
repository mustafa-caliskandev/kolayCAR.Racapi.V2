using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Exchangeratecenter
    {
        public int Recid { get; set; }
        public DateTime? Date { get; set; }
        public string Currencycode { get; set; }
        public decimal? Exchangerate { get; set; }
        public int? Piece { get; set; }
    }
}
