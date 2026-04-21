namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class GetSettingsRequest : ReservationTrackingBase
    {
        public string CurrencyCode { get; set; }
    }
}
