namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class GetReservationsRequest : IntegrationBase
    {
        public string ReservationNumber { get; set; }
        public string APIReservationNumber { get; set; }
        public string CustomerEmail { get; set; }
        public int? AgencyId { get; set; }
        public int? MemberId { get; set; }
        public string AgencyName { get; set; }
        public string PickupDateStart { get; set; }
        public string PickupDateEnd { get; set; }
        public string ReturnDateStart { get; set; }
        public string ReturnDateEnd { get; set; }
        public string ReservationDateStart { get; set; }
        public string ReservationDateEnd { get; set; }
        public string ReservationInfo { get; set; }
        public LanguageTypes? LanguageType { get; set; }
    }
}
