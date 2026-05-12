using KolayCAR.Broker.API.Mappers.Vonarent;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests.Vonarent;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Vonarent;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Vonarent
{
    public class LocationProvider : ILocationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;
        private const string StationsPath = "/api/remote/v1/location/stations";
        private const string SelectStationPath = "/api/remote/v1/location/select-station";

        public string ProviderName => "Vonarent";

        public LocationProvider(string apiBaseUrl, int timeout = 0)
        {
            _httpManager = new HttpManager(apiBaseUrl, timeout: timeout);
            _authProvider = new AuthProvider(apiBaseUrl, timeout);
        }

        public async Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            var locationsResponse = await GetLocations(vendor, languageId);
            var locations = locationsResponse.Data as List<Domain.Models.Location>;
            var location = locations?.FirstOrDefault(x => x.LocationCode == locationCode);

            return new ServiceResponseBase(location, location != null);
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var headers = await _authProvider.GetAuthorizedHeadersAsync(vendor, HttpMethod.Get.Method, StationsPath);
            var result = await _httpManager.GetAsync2<VonarentLocationResponse>(
                requestPath: StationsPath,
                headers: headers,
                isReservationRequest: true);

            if (result?.Data?.status != 1 || result.Data.items == null)
                return new ServiceResponseBase(
                    data: result?.Data,
                    success: false,
                    message: $"{vendor.VendorName} lokasyon servisi basarisiz.",
                    serviceMessage: VonarentResponseMessageHelper.ExtractMessage(result?.Data, result?.ServiceMessage, result?.Message));

            var locations = result.Data.items.Map();

            if (!string.IsNullOrWhiteSpace(locationName))
            {
                locations = locations
                    .Where(x => x.LocationName != null && x.LocationName.ToLowerInvariant().Contains(locationName.Trim().ToLowerInvariant()))
                    .ToList();
            }

            return new ServiceResponseBase(locations, true);
        }

        public async Task<ServiceResponseBase> SelectStationAsync(Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string bearerToken = null)
        {
            if (additionalInformation == null)
                return new ServiceResponseBase(null, false, $"{vendor?.VendorName ?? ProviderName} istasyon secim istegi icin additional information bulunamadi.");

            if (string.IsNullOrWhiteSpace(additionalInformation.APIPickupLocationCode) ||
                string.IsNullOrWhiteSpace(additionalInformation.APIReturnLocationCode))
            {
                return new ServiceResponseBase(null, false, $"{vendor?.VendorName ?? ProviderName} istasyon secim istegi icin lokasyon kodlari eksik.");
            }

            var request = new VonarentSelectStationRequest
            {
                PickupStation = additionalInformation.APIPickupLocationCode,
                PickupDate = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ReturnStation = additionalInformation.APIReturnLocationCode,
                ReturnDate = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm:ss")
            };

            bearerToken ??= await _authProvider.GetTokenAsync(vendor);

            var headers = !string.IsNullOrWhiteSpace(bearerToken)
                ? _authProvider.CreateAuthorizedHeaders(vendor, bearerToken, HttpMethod.Post.Method, SelectStationPath, body: request)
                : null;

            if (headers == null || headers.Count == 0)
                return new ServiceResponseBase(null, false, $"{vendor?.VendorName ?? ProviderName} yetkilendirme tokeni alinamadi.");

            var content = new StringContent(SignatureHelper.SerializeBody(request), Encoding.UTF8, "application/json");
            var result = await _httpManager.PostAsync<VonarentStatusResponse>(
                requestPath: SelectStationPath,
                content: content,
                headers: headers,
                isReservationRequest: true);

            if (result?.Data?.status != 1)
            {
                return new ServiceResponseBase(
                    data: result?.Data,
                    success: false,
                    message: $"{vendor.VendorName} istasyon secim servisi basarisiz.",
                    serviceMessage: VonarentResponseMessageHelper.ExtractMessage(result?.Data, result?.ServiceMessage, result?.Message));
            }

            return new ServiceResponseBase(result.Data, true);
        }
    }
}
