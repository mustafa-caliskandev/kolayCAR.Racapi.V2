using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers
{
    public static class ReservationTrackingMapper
    {
        public static ReservationTracking Map(this Restoken resToken) =>
            resToken != null ? new ReservationTracking
            {
                Id = resToken.Id,
                ReservationTokenStr = ObjectHelper.DecompressToString(resToken.Token),
                UniqueId = resToken.Uniqueid
            }
            : null;
    }
}
