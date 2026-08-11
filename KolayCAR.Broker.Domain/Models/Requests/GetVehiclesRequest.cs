namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class GetVehiclesRequest : ReservationStepsBase
    {
        public bool DisableTimeout { get; set; } = false;
        public string Guid { get; set; }
        public string Verbose { get; set; }
    }
}
