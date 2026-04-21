namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class ReservationStepsBase : ReservationTrackingBase
    {
        public string CurrencyCode { get; set; }
        public int PickupLocationId { get; set; }
        public int ReturnLocationId { get; set; }
        public string PickupDate { get; set; }
        public string ReturnDate { get; set; }
        public string PickupTime { get; set; }
        public string ReturnTime { get; set; }
        public string UserToken { get; set; }
        public string CouponCode { get; set; }
        public int? MemberId { get; set; }
        public bool IsReservationRequest { get; set; }
        public string SessionCode { get; set; }
        public string ApiLocationCode { get; set; }
    }
}
