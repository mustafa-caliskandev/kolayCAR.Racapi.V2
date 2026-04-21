using System;

namespace KolayCAR.Broker.API.Models.MobileAppModels
{
    public class GetCanceledReservationsRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
