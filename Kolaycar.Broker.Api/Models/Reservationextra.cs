using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Reservationextra
    {
        public int Id { get; set; }
        public string Resno { get; set; }
        public long Resid { get; set; }
        public int Extraid { get; set; }
        public int Piece { get; set; }
        public string Extraname { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Productcode { get; set; }
        public decimal? Apiprice { get; set; }
        public int? Extrarentaltype { get; set; }
        public int? ExtraType { get; set; }
        public string ExtraDescription { get; set; }
        public decimal? AgencyAmount { get; set; }
    }
}
