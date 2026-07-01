using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class GetExtrasResponse
    {
        public GetExtrasResponse(List<Extra> extras, Vehicle vehicle)
        {
            this.Extras = extras;
            this.Vehicle = vehicle;
        }
        public GetExtrasResponse()
        {
        }
        public List<Extra> Extras { get; set; }
        public Vehicle Vehicle { get; set; }
    }

    public class GetExtrasResponseRestricted
    {
        public object Extras { get; set; }
        public object Vehicle { get; set; }
    }
}
