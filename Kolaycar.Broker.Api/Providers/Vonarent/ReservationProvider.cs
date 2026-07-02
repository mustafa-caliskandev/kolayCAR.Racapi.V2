using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Vonarent;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Vonarent;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Vonarent
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly string _apiBaseUrl;
        private readonly IConfigurationService _configurationService;
        private const string SelectExtrasPath = "/api/remote/v1/extra/select-extras";
        private const string SetCustomerPath = "/api/remote/v1/reservation/set-customer";
        private const string CompleteReservationPath = "/api/remote/v1/reservation/complete-reservation";
        private const string CancelReservationPathPrefix = "/api/remote/v1/reservation-view/cancel-reservation";

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _apiBaseUrl = apiBaseUrl;
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            if (reservationToken == null)
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon istegi icin reservation token bulunamadi.");

            var bearerToken = reservationToken.APIReferenceCode.ToStringNullSafe();
            if (string.IsNullOrWhiteSpace(bearerToken))
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon istegi icin bearer token bulunamadi.");

            var httpManager = new HttpManager(_apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            var authProvider = new AuthProvider(_apiBaseUrl, vendor.APITimeout);

            try
            {
                var selectExtrasRequest = postReservationRequest.MapToSelectExtrasRequest(localReservation, apiExtras);

                if (selectExtrasRequest.Extras?.Count > 0)
                {
                    var selectExtrasResult = await PostStatusAsync(httpManager, authProvider, vendor, bearerToken, SelectExtrasPath, selectExtrasRequest, localReservation, "select-extras", "ekstra secim servisi basarisiz.");

                    Serilog.Log.Error("{@VonarentExtrasResult}", selectExtrasResult.ToJson());

                    if (!selectExtrasResult.Success)
                        return selectExtrasResult;
                }

                var setCustomerRequest = postReservationRequest.MapToSetCustomerRequest();

                var setCustomerResult = await PostStatusAsync(httpManager, authProvider, vendor, bearerToken, SetCustomerPath, setCustomerRequest, localReservation, "set-customer", "musteri bilgisi gonderme servisi basarisiz.");

                Serilog.Log.Error("{@VonarentSetCustomer}", setCustomerResult.ToJson());

                if (!setCustomerResult.Success)
                    return setCustomerResult;

                await WriteStepRequestLog(localReservation, "complete-reservation", new { });

                var completeReservationHeaders = authProvider.CreateAuthorizedHeaders(vendor, bearerToken, HttpMethod.Get.Method, CompleteReservationPath);
                var completeReservationResponse = await httpManager.GetAsync2<VonarentStatusResponse>(
                    requestPath: CompleteReservationPath,
                    headers: completeReservationHeaders,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                    isReservationRequest: true);

                var completeReservationResult = completeReservationResponse?.Data?.status == 1
                    ? new ServiceResponseBase(completeReservationResponse.Data, true)
                    : CreateVendorErrorResponse(localReservation, vendor, completeReservationResponse, "rezervasyon tamamlama servisi basarisiz.");

                Serilog.Log.Error("{@VonarentReservationResult}", completeReservationResult.ToJson());

                localReservation.ReservationPostedToAPI = true;
                if (!completeReservationResult.Success)
                    return completeReservationResult;

                localReservation.APIReservationSuccessfully = true;
                localReservation.APIVendorName = vendor.VendorName;
                localReservation.APIReservationNumber = completeReservationResponse.Data.item;

                return new ServiceResponseBase(localReservation, true);
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@VonarentPostReservationError}", $"{vendor.VendorName} - {localReservation?.ReservationNumber} - {ex.ToJson()}");
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} servisinden herhangi bir veri alinamadi.");
            }
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var reservationNo = localReservation?.APIReservationNumber.ToStringNullSafe().TrimNullSafe();

            if (string.IsNullOrWhiteSpace(reservationNo))
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} iptal istegi icin rezervasyon numarasi bulunamadi.");

            var requestPath = $"{CancelReservationPathPrefix}/{System.Uri.EscapeDataString(reservationNo)}";
            var httpManager = new HttpManager(_apiBaseUrl, DbConnectionHelper.Instance().ConnectionString, timeout: vendor.APITimeout);
            var authProvider = new AuthProvider(_apiBaseUrl, vendor.APITimeout);

            try
            {
                await WriteStepRequestLog(
                    localReservation,
                    "cancel-reservation",
                    new { ReservationNo = reservationNo },
                    BrokerLogTypes.ReservationCancelVendorAPIRequest);

                var headers = authProvider.CreateAuthorizedHeaders(vendor, null, HttpMethod.Get.Method, requestPath);
                var result = await httpManager.GetAsync2<VonarentStatusResponse>(
                    requestPath: requestPath,
                    headers: headers,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation?.ReservationNumber ?? reservationNo,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    },
                    isReservationRequest: true);

                if (result?.Data?.status == 1)
                {
                    if (localReservation != null)
                        localReservation.APIReservationCancel = true;

                    return new ServiceResponseBase(localReservation, true, "Iptal islemi basarili.");
                }

                return CreateVendorErrorResponse(localReservation, vendor, result, "rezervasyon iptal servisi basarisiz.");
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@VonarentPostCancelReservationError}", $"{vendor.VendorName} - {localReservation?.ReservationNumber ?? reservationNo} - {ex.ToJson()}");
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} rezervasyon iptal servisinden herhangi bir veri alinamadi.");
            }
        }

        private async Task<ServiceResponseBase> PostStatusAsync<TRequest>(HttpManager httpManager, AuthProvider authProvider, Vendor vendor, string bearerToken, string requestPath, TRequest request, Reservation localReservation, string stepName, string fallbackMessage)
            where TRequest : class
        {
            await WriteStepRequestLog(localReservation, stepName, request);

            var headers = authProvider.CreateAuthorizedHeaders(vendor, bearerToken, HttpMethod.Post.Method, requestPath, body: request);
            var content = new StringContent(SignatureHelper.SerializeBody(request), Encoding.UTF8, "application/json");

            var result = await httpManager.PostAsync<VonarentStatusResponse>(
                requestPath: requestPath,
                content: content,
                headers: headers,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            if (result?.Data?.status == 1)
                return new ServiceResponseBase(result.Data, true);

            return CreateVendorErrorResponse(localReservation, vendor, result, fallbackMessage);
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

        private static ServiceResponseBase CreateVendorErrorResponse(Reservation localReservation, Vendor vendor, HttpResult<VonarentStatusResponse> result, string fallbackMessage)
        {
            var supplierMessage = VonarentResponseMessageHelper.ExtractMessage(result?.Data, result?.ServiceMessage, result?.Message);

            if (!string.IsNullOrWhiteSpace(supplierMessage) && localReservation != null)
                localReservation.APIMessage = supplierMessage;

            return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} {fallbackMessage}", serviceMessage: supplierMessage, serviceCode: result != null ? ((int)result.HttpStatusCode).ToString() : string.Empty);
        }
    }
}
