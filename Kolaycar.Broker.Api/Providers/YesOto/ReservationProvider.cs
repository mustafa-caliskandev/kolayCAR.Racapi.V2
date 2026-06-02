using Kolaycar.Broker.Api.Helpers.YesOto;
using Kolaycar.Broker.Api.Mappers.YesOto;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.YesOto;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.YesOto;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.YesOto
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(YesOtoConstants.NormalizeApiBaseUrl(apiBaseUrl), DbConnectionHelper.Instance().ConnectionString);
            _authProvider = new AuthProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest request, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);

            if (string.IsNullOrEmpty(accessToken))
                return new ServiceResponseBase(null, false, "Token bilgisi alınamadı!");

            var createRequest = request.Map(reservationToken, vendor, apiExtras);

            var parameters = new Dictionary<string, object>();
            var headers = CreateHeaders(accessToken);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(createRequest),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            var response = await _httpManager.PostAsyncWithModelResult<YesOtoCreateReservationRequest, YesOtoCreateReservationResponse>(
                "/api/app/reservationUI/completeReservation",
                createRequest,
                parameters,
                headers,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true
            );

            if (IsSuccessfulCreateReservationResponse(response))
            {
                var approvalNumber = GetApprovalNumber(response.Data);

                if (string.IsNullOrWhiteSpace(approvalNumber))
                    return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, response, "Rezervasyon onay numarasi alinamadi.");

                localReservation.APIReservationNumber = approvalNumber;

                var statusRequest = new YesOtoSetReservationStatusRequest
                {
                    ApprovalNumber = approvalNumber
                };

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(statusRequest),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                var statusResponse = await _httpManager.PostAsyncWithModelResult<YesOtoSetReservationStatusRequest, YesOtoCreateReservationResponse>(
                    "/api/app/reservationUI/setReservationStatus",
                    statusRequest,
                    parameters,
                    headers,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                    isReservationRequest: true
                );

                if (!IsSuccessfulStatusResponse(statusResponse))
                    return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, statusResponse, "Rezervasyon statusu onaylanamadi.");

                localReservation.ReservationPostedToAPI = true;
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIVendorName = vendor.VendorName;

                return new ServiceResponseBase(localReservation, true);
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, response, "Rezervasyon oluşturulamadı.");
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);

            if (string.IsNullOrEmpty(accessToken))
                return new ServiceResponseBase(null, false, "Token bilgisi alınamadı!");

            var approvalNumber = localReservation.APIReservationNumber;

            if (string.IsNullOrWhiteSpace(approvalNumber))
                return new ServiceResponseBase(localReservation, false, "YesOto rezervasyon onay numarasi bulunamadi.");

            var cancellationInformationRequest = new YesOtoReservationCancellationInformationRequest
            {
                ApprovalNumber = approvalNumber,
                Email = postCancelReservationRequest.CustomerEmail,
                LanguageId = null,
                BrandId = vendor.ApiClientId
            };

            var parameters = new Dictionary<string, object>();
            var headers = CreateHeaders(accessToken);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(cancellationInformationRequest),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            var informationResponse = await _httpManager.PostAsyncWithModelResult<YesOtoReservationCancellationInformationRequest, YesOtoCreateReservationResponse>(
                "/api/app/reservationUI/reservationCancellationsInformation",
                cancellationInformationRequest,
                parameters,
                headers,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true
            );

            if (!IsSuccessfulStatusResponse(informationResponse))
                return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, informationResponse, "Rezervasyon iptal bilgisi alinamadi.");

            var cancelRequest = new YesOtoCancelReservationRequest
            {
                ApprovalNumber = approvalNumber,
                Email = postCancelReservationRequest.CustomerEmail,
                LanguageId = null,
                BrandId = vendor.ApiClientId,
                ReservationCancellationReason = string.IsNullOrWhiteSpace(postCancelReservationRequest.CancelNote)
                    ? "User requested cancellation."
                    : postCancelReservationRequest.CancelNote,
                RefundAmount = localReservation.RefundAmount
            };

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(cancelRequest),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            var response = await _httpManager.PostAsyncWithModelResult<YesOtoCancelReservationRequest, YesOtoCreateReservationResponse>(
                "/api/app/reservationUI/reservationCancellationsConfirm",
                cancelRequest,
                parameters,
                headers,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true
            );

            if (IsSuccessfulStatusResponse(response))
            {
                localReservation.APIReservationCancel = true;
                return new ServiceResponseBase(localReservation, true);
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, response, "Rezervasyon iptal edilemedi.");
        }

        private static Dictionary<string, object> CreateHeaders(string accessToken)
        {
            return new Dictionary<string, object>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {accessToken}" }
            };
        }

        private static string GetApprovalNumber(YesOtoCreateReservationResponse response)
        {
            return response?.data?.approvalNumber ??
                   response?.approvalNumber ??
                   response?.data?.reservationCode ??
                   response?.reservationCode;
        }

        private static bool IsSuccessfulCreateReservationResponse(HttpResult<YesOtoCreateReservationResponse> response)
        {
            return response?.Data != null && response.Success && response.Data.success;
        }

        private static bool IsSuccessfulStatusResponse(HttpResult<YesOtoCreateReservationResponse> response)
        {
            if (response == null || !response.Success)
                return false;

            return response.Data == null || response.Data.success;
        }
    }
}
