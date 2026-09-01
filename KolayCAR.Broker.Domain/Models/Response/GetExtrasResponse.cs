using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class GetExtrasResponse
    {
        public GetExtrasResponse(List<Extra> extras, Vehicle vehicle, List<Vehicle> availableVehicles = null)
        {
            Extras = extras;
            Vehicle = vehicle;
            AlternativeVehicles = availableVehicles?
                .Where(x => x != null && !ReferenceEquals(x, vehicle))
                .ToList() ?? new List<Vehicle>();
        }

        public GetExtrasResponse()
        {
        }

        public List<Extra> Extras { get; set; }
        public Vehicle Vehicle { get; set; }
        public List<Vehicle> AlternativeVehicles { get; set; } = new List<Vehicle>();
    }

    public class GetExtrasResponseRestricted
    {
        public object Extras { get; set; }
        public object Vehicle { get; set; }
    }
}
