using System.Collections.Generic;
using DomainLocation = KolayCAR.Broker.Domain.Models.Location;
using PandoraLocation = KolayCAR.Broker.Domain.Models.Response.Pandora2ResponseBase.Location;

namespace KolayCAR.Broker.API.Mappers.Pandora2
{
    public static class LocationMapper
    {
        public static DomainLocation Map(this PandoraLocation location) =>
            location != null ? new DomainLocation
            {
                LocationId = Pandora2MapperHelper.ToStableId(location.Id),
                LocationCode = location.Id,
                LocationName = location.Name,
                CountryId = location.CountryId,
                CountryCode = location.CountryCode,
                IataCode = location.Code,
                IsPickup = true,
                IsOffice = true
            }
            : null;

        public static List<DomainLocation> Map(this List<PandoraLocation> locations)
        {
            var mappedLocations = new List<DomainLocation>();

            if (locations != null)
                foreach (var location in locations)
                    mappedLocations.Add(location.Map());

            return mappedLocations;
        }
    }
}
