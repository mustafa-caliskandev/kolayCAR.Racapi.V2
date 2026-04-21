using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class GetSummaryResponse
    {
        public List<ReservationExtra> Extras { get; set; }
        public Vehicle Vehicle { get; set; }
        public CouponUsageResultTypes CouponUsageResultType { get; set; }
    }

    public class GetSummaryResponseRestricted
    {
        public object Extras { get; set; }
        public object Vehicle { get; set; }
    }
}
