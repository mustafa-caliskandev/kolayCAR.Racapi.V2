namespace KolayCAR.Broker.API.Models.MobileAppDtos.RequestDtos
{
    public class CancelLocalRequest
    {
        public string ReservationNumber { get; set; }
        public string CustomerEmail { get; set; }
        public string CancelNote { get; set; }
        public int UserId { get; set; }
    }
}
