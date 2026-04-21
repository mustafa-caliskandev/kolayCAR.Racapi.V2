namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class ReservationTrackingBase : IntegrationBase
    {
        public string LanguageCode { get; set; }
        private string reservationToken;
        public string ReservationToken { get => reservationToken; set => reservationToken = value?.Replace(" ", "+"); }
    }
}
