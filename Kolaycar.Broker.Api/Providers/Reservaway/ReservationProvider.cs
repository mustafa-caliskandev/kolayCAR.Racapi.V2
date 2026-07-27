using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Reservaway;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.Reservaway;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Reservaway;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Reservaway
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(ReservawayMapperHelper.NormalizeBaseUrl(apiBaseUrl), DbConnectionHelper.Instance().ConnectionString);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(
            PostReservationRequest postReservationRequest,
            Vendor vendor,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            string reservationNumber,
            ReservationToken reservationToken,
            List<ExchangeRates> exchangeRates,
            Reservation localReservation,
            List<Extra> apiExtras)
        {
            if (reservationToken == null)
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon istegi icin reservation token bulunamadi.");

            var planReference = ReservawayMapperHelper.ParsePlanReference(reservationToken.APIReferenceCode2);
            if (!ReservawayMapperHelper.IsSellablePlan(planReference))
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} sadece BSC pakette PPFC/PPNC odeme tipleri icin rezervasyon kabul eder.");

            var bookingToken = reservationToken.APIReferenceCode4.ToStringNullSafe();

            if (string.IsNullOrWhiteSpace(bookingToken))
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} booking token alinamadi.");

            var visitorSessionId = ReservawayRequestHelper.CreateVisitorSessionId(postReservationRequest, reservationToken);
            var request = postReservationRequest.MapToPartnerReserveRequest(reservationToken, bookingToken, visitorSessionId);
            if (request.vehicle_id <= 0 || string.IsNullOrWhiteSpace(request.booking_token) || string.IsNullOrWhiteSpace(request.visitor_session_id) || string.IsNullOrWhiteSpace(request.currency))
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon zorunlu alanlari eksik.");

            if (string.IsNullOrWhiteSpace(vendor.ApiClientId))
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} partner key bulunamadi.");

            try
            {
                var addExtrasResult = await AddSelectedExtras(postReservationRequest, vendor, reservationToken, localReservation, apiExtras, visitorSessionId);
                if (!addExtrasResult.Success)
                    return addExtrasResult;

                await WriteStepRequestLog(localReservation, "external-partner-reserve", request);

                var result = await _httpManager.PostAsyncWithModelResult<ReservawayPartnerReserveRequest, ReservawayPartnerReserveResponse>(
                    requestPath: ReservawayRequestHelper.PartnerReservePath,
                    entity: request,
                    headers: ReservawayRequestHelper.CreatePartnerHeaders(visitorSessionId, vendor.ApiClientId),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                    isReservationRequest: true);

                if (result?.Success == true && result.Data != null)
                {
                    localReservation.ReservationPostedToAPI = true;
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIVendorName = vendor.VendorName;
                    localReservation.APIReservationNumber = ResolveReservationNumber(result.Data, reservationNumber);
                    localReservation.APIReferenceCode = bookingToken;
                    localReservation.APIReferenceCode2 = reservationToken.APIReferenceCode;
                    localReservation.APIReferenceCode3 = visitorSessionId;

                    return new ServiceResponseBase(localReservation, true);
                }

                return CreateVendorErrorResponse(localReservation, vendor, result, "rezervasyon servisi basarisiz.");
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@ReservawayPostReservationError}", $"{vendor.VendorName} - {localReservation?.ReservationNumber} - {ex.ToJson()}");
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon servisinden herhangi bir veri alinamadi.");
            }
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var reservationNo = localReservation?.APIReservationNumber.ToStringNullSafe().TrimNullSafe();
            if (string.IsNullOrWhiteSpace(reservationNo))
                reservationNo = postCancelReservationRequest?.ReservationNumber.ToStringNullSafe().TrimNullSafe();

            if (string.IsNullOrWhiteSpace(reservationNo))
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} iptal istegi icin rezervasyon numarasi bulunamadi.");

            if (string.IsNullOrWhiteSpace(vendor.ApiClientId))
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} partner key bulunamadi.");

            var visitorSessionId = !string.IsNullOrWhiteSpace(localReservation?.APIReferenceCode3)
                ? localReservation.APIReferenceCode3
                : System.Guid.NewGuid().ToString("N");

            var request = new ReservawayPartnerCancelRequest
            {
                reservation_number = reservationNo,
                reason = !string.IsNullOrWhiteSpace(postCancelReservationRequest?.CancelNote)
                    ? postCancelReservationRequest.CancelNote.Trim()
                    : "Customer requested cancellation"
            };

            try
            {
                await WriteStepRequestLog(localReservation, "external-partner-cancel", request, BrokerLogTypes.ReservationCancelVendorAPIRequest);

                var result = await _httpManager.PostAsyncWithModelResult<ReservawayPartnerCancelRequest, ReservawayPartnerCancelResponse>(
                    requestPath: ReservawayRequestHelper.PartnerCancelPath,
                    entity: request,
                    headers: ReservawayRequestHelper.CreatePartnerHeaders(visitorSessionId, vendor.ApiClientId),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation?.ReservationNumber ?? reservationNo,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    },
                    isReservationRequest: true);

                if (result?.Success == true)
                {
                    if (localReservation != null)
                    {
                        localReservation.APIReservationCancel = true;
                        localReservation.APIMessage = result.Data?.message;
                    }

                    return new ServiceResponseBase(localReservation, true, result.Data?.message ?? "Iptal islemi basarili.");
                }

                return CreateVendorErrorResponse(localReservation, vendor, result, "rezervasyon iptal servisi basarisiz.");
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@ReservawayPostCancelReservationError}", $"{vendor.VendorName} - {localReservation?.ReservationNumber ?? reservationNo} - {ex.ToJson()}");
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon iptal servisinden herhangi bir veri alinamadi.");
            }
        }

        private async Task<ServiceResponseBase> AddSelectedExtras(
            PostReservationRequest postReservationRequest,
            Vendor vendor,
            ReservationToken reservationToken,
            Reservation localReservation,
            List<Extra> apiExtras,
            string visitorSessionId)
        {
            var selectedExtras = GetSelectedExtras(postReservationRequest, localReservation);
            var extras = CreateAddExtrasRequest(selectedExtras, apiExtras, out var unresolvedExtraCount);
            if (unresolvedExtraCount > 0)
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} secilen ekstra idleri cozumlenemedi.");

            if (extras.Count == 0)
                return new ServiceResponseBase(localReservation, true);

            var vehicleId = reservationToken.VehicleCode.ToLongNullSafe();
            if (vehicleId <= 0)
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} ekstra secimi icin arac id bulunamadi.");

            var parameters = new Dictionary<string, object>
            {
                ["vehicle_id"] = vehicleId,
                ["extras"] = extras.ToJson()
            };

            await WriteStepRequestLog(localReservation, "vehicle-add-extras", parameters);

            var result = await _httpManager.GetAsync2<ReservawayAddExtrasResponse>(
                requestPath: ReservawayRequestHelper.VehicleAddExtrasPath,
                parameters: parameters,
                headers: ReservawayRequestHelper.CreateHeaders(visitorSessionId, vendor.ApiClientId, reservationToken.APIReferenceCode),
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation?.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            if (result?.Success == true)
                return new ServiceResponseBase(localReservation, true);

            return CreateVendorErrorResponse(localReservation, vendor, result, "ekstra secim servisi basarisiz.");
        }

        private static Dictionary<string, ReservawayAddExtraRequestItem> CreateAddExtrasRequest(
            List<ReservationExtra> selectedExtras,
            List<Extra> apiExtras,
            out int unresolvedExtraCount)
        {
            var extras = new Dictionary<string, ReservawayAddExtraRequestItem>();
            unresolvedExtraCount = 0;

            foreach (var selectedExtra in selectedExtras)
            {
                var extraId = ResolveReservawayExtraId(selectedExtra, apiExtras);
                if (extraId <= 0)
                {
                    unresolvedExtraCount++;
                    continue;
                }

                var key = extraId.ToString();
                var quantity = selectedExtra.Piece > 0 ? selectedExtra.Piece : 1;

                if (extras.TryGetValue(key, out var existingExtra))
                {
                    existingExtra.quantity += quantity;
                    continue;
                }

                extras[key] = new ReservawayAddExtraRequestItem
                {
                    id = extraId,
                    quantity = quantity
                };
            }

            return extras;
        }

        private static List<ReservationExtra> GetSelectedExtras(PostReservationRequest postReservationRequest, Reservation localReservation)
        {
            if (localReservation?.ReservationExtras?.Count > 0)
                return localReservation.ReservationExtras;

            if (postReservationRequest?.PostReservationRequestV2?.Extras?.Count > 0)
            {
                return postReservationRequest.PostReservationRequestV2.Extras
                    .Select(x => new ReservationExtra
                    {
                        ExtraId = x.ExtraId,
                        ExtraCode = x.ExtraCode,
                        ApiExtraCode = x.ApiExtraCode,
                        Piece = x.Piece > 0 ? x.Piece : 1
                    })
                    .ToList();
            }

            var selectedExtras = ReservationHelper.GetReservationExtrasFromStringList(postReservationRequest?.ExtraList);
            return selectedExtras.Count > 0
                ? selectedExtras
                : localReservation?.ReservationExtras ?? new List<ReservationExtra>();
        }

        private static int ResolveReservawayExtraId(ReservationExtra selectedExtra, List<Extra> apiExtras)
        {
            if (selectedExtra == null)
                return 0;

            var apiExtra = apiExtras?.FirstOrDefault(x =>
                x.ExtraId == selectedExtra.ExtraId ||
                x.ExtraCode == selectedExtra.ExtraCode ||
                x.ExtraCode == selectedExtra.ApiExtraCode ||
                x.ApiExtraCode == selectedExtra.ExtraCode ||
                x.ApiExtraCode == selectedExtra.ApiExtraCode);

            var candidates = new[]
            {
                selectedExtra.ExtraCode,
                selectedExtra.ApiExtraCode,
                apiExtra?.ExtraCode,
                apiExtra?.ApiExtraCode,
                selectedExtra.ExtraId > 0 ? selectedExtra.ExtraId.ToString() : null,
                apiExtra?.ExtraId > 0 ? apiExtra.ExtraId.ToString() : null
            };

            foreach (var candidate in candidates)
            {
                var extraId = candidate.ToIntNullSafe();
                if (extraId > 0)
                    return extraId;
            }

            return 0;
        }

        private async Task WriteStepRequestLog(Reservation localReservation, string stepName, object request, BrokerLogTypes logType = BrokerLogTypes.ReservationVendorAPIRequest)
        {
            if (_configurationService == null || localReservation == null)
                return;

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = new { Step = stepName, Request = request }.ToJson(),
                LogType = logType
            });
        }

        private static string ResolveReservationNumber(ReservawayCustomerResponse response, string fallbackReservationNumber)
        {
            if (!string.IsNullOrWhiteSpace(response?.customer?.order_id))
                return response.customer.order_id;

            var referenceId = response?.order?.data?.SelectToken("reference_id")?.ToString()
                ?? response?.order?.data?.SelectToken("order_id")?.ToString()
                ?? response?.order?.data?.SelectToken("id")?.ToString();

            return !string.IsNullOrWhiteSpace(referenceId) ? referenceId : fallbackReservationNumber;
        }

        private static string ResolveReservationNumber(ReservawayPartnerReserveResponse response, string fallbackReservationNumber)
            => !string.IsNullOrWhiteSpace(response?.reservationNumber)
                ? response.reservationNumber
                : fallbackReservationNumber;

        private static ServiceResponseBase CreateVendorErrorResponse<T>(Reservation localReservation, Vendor vendor, HttpResult<T> result, string fallbackMessage)
            where T : ReservawayResponseBase
        {
            var supplierMessage = result?.Data?.message ?? result?.ServiceMessage ?? result?.Message;

            if (!string.IsNullOrWhiteSpace(supplierMessage) && localReservation != null)
                localReservation.APIMessage = supplierMessage;

            return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} {fallbackMessage}", serviceMessage: supplierMessage, serviceCode: result != null ? ((int)result.HttpStatusCode).ToString() : string.Empty);
        }
    }
}
