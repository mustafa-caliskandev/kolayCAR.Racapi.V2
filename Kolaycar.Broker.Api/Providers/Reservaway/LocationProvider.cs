using KolayCAR.Broker.API.Mappers.Reservaway;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Reservaway;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerLocation = KolayCAR.Broker.Domain.Models.Location;

namespace KolayCAR.Broker.API.Providers.Reservaway
{
    public class LocationProvider : ILocationProvider
    {
        private readonly HttpManager _httpManager;

        public string ProviderName => "Reservaway";

        public LocationProvider(string apiBaseUrl, int timeout = 0)
        {
            _httpManager = new HttpManager(ReservawayMapperHelper.NormalizeBaseUrl(apiBaseUrl), timeout: timeout);
        }

        public async Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            var locationsResponse = await GetLocations(vendor, languageId);
            var locations = locationsResponse.Data as List<BrokerLocation>;
            var location = locations?.FirstOrDefault(x => x.LocationCode == locationCode);

            return new ServiceResponseBase(location, location != null);
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var visitorSessionId = System.Guid.NewGuid().ToString("N");
            var result = await _httpManager.GetAsync2<ReservawayLocationResponse>(
                requestPath: ReservawayRequestHelper.LocationListPath,
                headers: ReservawayRequestHelper.CreateHeaders(visitorSessionId),
                isReservationRequest: true);

            if (result?.Data?.locations == null)
                return new ServiceResponseBase(
                    data: result?.Data,
                    success: false,
                    message: $"{vendor.VendorName} lokasyon servisi basarisiz.",
                    serviceMessage: result?.Data?.message ?? result?.ServiceMessage ?? result?.Message);

            var locations = result.Data.locations.Map();

            if (!string.IsNullOrWhiteSpace(locationName))
            {
                locations = locations
                    .Where(x => x.LocationName != null && x.LocationName.ToLowerInvariant().Contains(locationName.Trim().ToLowerInvariant()))
                    .ToList();
            }

            return new ServiceResponseBase(locations, true);
        }
    }
}
