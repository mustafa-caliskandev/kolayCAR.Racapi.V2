namespace KolayCAR.Broker.API.Models
{
    public class ReservationSelectedExtra
    {
        public int Id { get; set; }
        public int ReservationDetailId { get; set; }

        public int ExtraRentalType { get; set; }
        public int RentalDuration { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public virtual ReservationDetail ReservationDetail { get; set; }
    }
}
