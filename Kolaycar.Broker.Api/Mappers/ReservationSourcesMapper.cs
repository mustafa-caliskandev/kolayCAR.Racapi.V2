using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers
{
    public static class ReservationSourcesMapper
    {
        public static ReservationSource Map(this Ressource reservationSource) =>
            reservationSource != null ? new ReservationSource
            {
                ReservationSourceId = reservationSource.Id,
                ReservationSourceName = reservationSource.Sourcename
            }
            : null;

        public static List<ReservationSource> Map(this List<Ressource> reservationSources)
        {
            var _reservationSources = new List<ReservationSource>();

            if (reservationSources != null && reservationSources.Count != 0)
                foreach (var reservationSource in reservationSources)
                    _reservationSources.Add(reservationSource.Map());

            return _reservationSources;
        }
    }
}
