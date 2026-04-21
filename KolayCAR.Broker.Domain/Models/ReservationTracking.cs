namespace KolayCAR.Broker.Domain.Models
{
    public class ReservationTracking
    {
        public long Id { get; set; }
        public string ReservationTokenStr { get; set; }
        public string UniqueId { get; set; }
    }
}
