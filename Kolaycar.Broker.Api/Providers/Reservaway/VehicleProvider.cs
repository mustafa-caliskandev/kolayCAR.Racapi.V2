using KolayCAR.Broker.API.Mappers.Reservaway;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Reservaway;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Reservaway
{
    public class VehicleProvider : IVehicleProvider
    {
        private readonly HttpManager _httpManager;

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _httpManager = new HttpManager(
                ReservawayMapperHelper.NormalizeBaseUrl(vendor.APIBaseUrl),
                timeout: disableTimeout ? 0 : vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
            => new ServiceResponseBase(null, false, "Reservaway arac listesi icin lokasyon ve tarih bilgisi gereklidir.");

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            if (additionalInformation == null)
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} arac arama icin ek bilgi bulunamadi.");

            var visitorSessionId = ReservawayRequestHelper.CreateVisitorSessionId(getVehiclesRequest);
            var searchParameters = ReservawayRequestHelper.CreateVehicleSearchParameters(getVehiclesRequest, vendor, additionalInformation, visitorSessionId);
            var headers = ReservawayRequestHelper.CreateHeaders(visitorSessionId, vendor.ApiClientId);

            var searchResult = await _httpManager.GetAsync2<ReservawayVehicleSearchResponse>(
                requestPath: ReservawayRequestHelper.VehiclesPath,
                parameters: searchParameters,
                headers: headers,
                isReservationRequest: true);

            if (searchResult?.Data?.vehicles == null)
                return CreateErrorResponse(searchResult, vendor, "arac listesi alinamadi.");

            var sellableApiVehicles = searchResult.Data.vehicles
                .Where(x => ReservawayMapperHelper.HasSellablePlan(x?.prices))
                .ToList();

            if (sellableApiVehicles.Count == 0)
                return new ServiceResponseBase(new List<Vehicle>(), false, $"{vendor.VendorName} PPFC/PPNC odeme tipine uygun arac bulunamadi.");

            var productTypes = await GetTypeItems(ReservawayRequestHelper.ProductTypesPath, headers);
            var paymentTypes = await GetTypeItems(ReservawayRequestHelper.PaymentTypesPath, headers);
            var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var vehicles = sellableApiVehicles.Map(
                getVehiclesRequest,
                additionalInformation,
                vendor,
                requestCurrencyType,
                baseVendorRequestCurrencyType,
                profitMarkups,
                exchangeRates,
                visitorSessionId,
                productTypes,
                paymentTypes);

            if (vehicles.Count == 0)
                return new ServiceResponseBase(vehicles, false, $"{vendor.VendorName} arac listesi bos dondu.");

            return new ServiceResponseBase(vehicles, true);
        }

        public async Task<ServiceResponseBase> GetVehicleDetail(
            Vendor vendor,
            ReservationToken reservationToken,
            ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            if (reservationToken == null)
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} arac detayi icin reservation token bulunamadi.");

            var visitorSessionId = ReservawayRequestHelper.CreateVisitorSessionId(null, reservationToken);
            var result = await _httpManager.GetAsync2<ReservawayVehicleResponse>(
                requestPath: ReservawayRequestHelper.VehiclePath,
                parameters: new Dictionary<string, object>
                {
                    ["vehicle_id"] = reservationToken.VehicleCode
                },
                headers: ReservawayRequestHelper.CreateHeaders(visitorSessionId, vendor.ApiClientId, reservationToken.APIReferenceCode),
                isReservationRequest: true);

            if (result?.Data?.vehicle == null || result.Data.vehicle.Count == 0)
                return CreateErrorResponse(result, vendor, "arac detayi alinamadi.");

            var planReference = ReservawayMapperHelper.ParsePlanReference(reservationToken.APIReferenceCode2);
            var vehicle = result.Data.vehicle.FirstOrDefault().Map(additionalInformation, vendor, planReference);
            return new ServiceResponseBase(new ReservawayVehicleDetailResult
            {
                Vehicle = vehicle,
                ApiVehicle = result.Data.vehicle.FirstOrDefault(),
                PlanReference = planReference
            }, vehicle != null);
        }

        private static ServiceResponseBase CreateErrorResponse<T>(HttpResult<T> result, Vendor vendor, string fallbackMessage) where T : ReservawayResponseBase
        {
            var supplierMessage = result?.Data?.message ?? result?.ServiceMessage ?? result?.Message;
            return new ServiceResponseBase(result?.Data, false, $"{vendor.VendorName} {fallbackMessage}", serviceMessage: supplierMessage, serviceCode: result != null ? ((int)result.HttpStatusCode).ToString() : string.Empty);
        }

        private async Task<List<ReservawayTypeItem>> GetTypeItems(string path, Dictionary<string, object> headers)
        {
            var result = await _httpManager.GetAsync2<ReservawayTypeResponse>(
                requestPath: path,
                headers: headers,
                isReservationRequest: true);

            return result?.Data?.types ?? new List<ReservawayTypeItem>();
        }
    }

    public class ReservawayVehicleDetailResult
    {
        public Vehicle Vehicle { get; set; }
        public ReservawayVehicleDetail ApiVehicle { get; set; }
        public ReservawayPlanReference PlanReference { get; set; }
    }
}
