namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class GetAgenciesRequest : ReservationTrackingBase
    {
        public int AgencyId { get; set; }
    }
}
