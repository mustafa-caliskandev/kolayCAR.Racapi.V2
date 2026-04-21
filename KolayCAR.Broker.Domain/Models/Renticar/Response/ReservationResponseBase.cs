namespace KolayCAR.Broker.Domain.Models.Renticar.Response
{
    public class ReservationResponseBase
    {
        public string reservationId { get; set; }
        public string reservationCode { get; set; }
        public string vendorReservationNo { get; set; }
        public ReservationDetail reservationDetail { get; set; }
    }
}
