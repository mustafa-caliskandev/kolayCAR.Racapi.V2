using KolayCAR.Broker.Domain.Models.Response.Vonarent;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Vonarent
{
    public static class LocationMapper
    {
        public static Domain.Models.Location Map(this VonarentLocationItem item)
            => item != null && !string.IsNullOrWhiteSpace(item.id) && !string.IsNullOrWhiteSpace(item.name)
                ? new Domain.Models.Location
                {
                    LocationCode = item.id,
                    LocationName = item.name,
                    IsPickup = true,
                    IsOffice = true
                }
                : null;

        public static List<Domain.Models.Location> Map(this List<VonarentLocationItem> items)
            => items?
                .Select(x => x.Map())
                .Where(x => x != null)
                .ToList()
                ?? new List<Domain.Models.Location>();
    }
}
