using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Scoreusagehistory
    {
        public int Id { get; set; }
        public int Agencyid { get; set; }
        public int Memberid { get; set; }
        public int Couponid { get; set; }
        public int Score { get; set; }
        public int Moneyforreservationscore { get; set; }
        public decimal Reservationscoreformoney { get; set; }
        public DateTime? Createdate { get; set; }
        public bool? Active { get; set; }
    }
}
