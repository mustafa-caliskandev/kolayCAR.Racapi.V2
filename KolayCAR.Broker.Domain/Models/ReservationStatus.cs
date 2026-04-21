using System;

namespace KolayCAR.Broker.Domain.Models
{
    public class ReservationStatusHistory
    {
        public long Id { get; set; }
        public ReservationStatusTypes ReservationStatusType { get; set; }
        public string ReservationStatusName { get; set; }
        public string ReservationStatusNote { get; set; }
        public DateTime Date { get; set; }
    }
}
