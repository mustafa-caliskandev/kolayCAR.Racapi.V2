using Kolaycar.Broker.Api.Mappers.YesOto;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.YesOto;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.YesOto;
using KolayCAR.Broker.Infrastructure.Managers;
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
            _httpManager = new HttpManager(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest request, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);
            if (string.IsNullOrEmpty(accessToken))
            {
                return new ServiceResponseBase(null, false, "Token bilgisi alınamadı!");
            }

            var createRequest = request.Map(reservationToken);
            createRequest.SalesChannelId = vendor.ApiKey;

            var parameters = new Dictionary<string, object>();
            var headers = new Dictionary<string, object>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {accessToken}" }
            };

            var response = await _httpManager.PostAsyncWithModel<YesOtoCreateReservationRequest, YesOtoCreateReservationResponse>(
                "/api/app/bookingUI/completeReservation",
                createRequest,
                parameters,
                headers
            );

            if (response != null && response.success && response.data != null)
            {
                localReservation.ReservationPostedToAPI = true;
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = response.data.reservationCode;
                localReservation.APIVendorName = vendor.VendorName;

                return new ServiceResponseBase(localReservation, true);
            }

            return new ServiceResponseBase(null, false, response?.message ?? "Rezervasyon oluşturulamadı.");
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);
            if (string.IsNullOrEmpty(accessToken))
            {
                return new ServiceResponseBase(null, false, "Token bilgisi alınamadı!");
            }

            var cancelRequest = new YesOtoCancelReservationRequest
            {
                reservationCode = localReservation.APIReservationNumber
            };

            var parameters = new Dictionary<string, object>();
            var headers = new Dictionary<string, object>
            {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {accessToken}" }
            };

            var response = await _httpManager.PostAsyncWithModel<YesOtoCancelReservationRequest, YesOtoCreateReservationResponse>(
                "/api/app/reservationUI/reservationCancellationsConfirm",
                cancelRequest,
                parameters,
                headers
            );

            if (response != null && response.success)
            {
                localReservation.APIReservationCancel = true;
                return new ServiceResponseBase(localReservation, true);
            }

            return new ServiceResponseBase(null, false, response?.message ?? "Rezervasyon iptal edilemedi.");
        }
    }
}
