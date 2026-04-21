using System;

namespace KolayCAR.Broker.API.Models.MobileAppModels
{
    public class GetReservationRequest
    {
        public string ReservationNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
