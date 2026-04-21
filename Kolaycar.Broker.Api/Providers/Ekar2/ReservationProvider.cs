using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.Ekar2RequestBase;

namespace KolayCAR.Broker.API.Providers.Ekar2
{
    public class ReservationProvider : IReservationProvider
    {
        private HttpManager _httpManager { get; set; }
        private AuthProvider _authProvider { get; set; }
        private IConfigurationService _configurationService { get; set; }
        public ReservationProvider(string ApiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(ApiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _configurationService = configurationService;
            _authProvider = new AuthProvider(ApiBaseUrl);
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var result = await _httpManager.PostAsync2<string, Ekar2ResponseBase.Ekar2PostCancelReservationResponse>(
                    requestPath: $"/api/v1/reservations/{localReservation.APIReservationNumber}",
                    headers: _authProvider.GetHeaders(vendor),
                    parameters: PostCancelReservationRequestParameters(postCancelReservationRequest, localReservation),
                     brokerLogModel: new BrokerLogModel
                     {
                         LogKey = localReservation.ReservationNumber,
                         LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                     },
                     isReservationRequest: true
                );
            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(localReservation.APIReferenceCode),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });
            if (result.Success == true && result.Data.success)
            {
                localReservation.APIReservationCancel = true;

                return new ServiceResponseBase
                {
                    Success = true,
                    Data = localReservation
                };
            }
            return new ServiceResponseBase
            {
                Message = "Ekar servisine ulaşılamadı !",
                Success = false,
                Data = localReservation
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var postReservationRequestParameters = PostReservationRequestParameters(postReservationRequest, reservationToken, additionalInformation, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@EkarPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await _httpManager.PostAsync2<PostReservationRequestBody, Ekar2ResponseBase.Ekar2PostReservationResponse>(
            requestPath: @"/api/v1/reservations",
            parameters: PostReservationRequestParameters(postReservationRequest, reservationToken, additionalInformation, localReservation),
            headers: _authProvider.GetHeaders(vendor),
            brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationVendorAPIResponse
            },
            isReservationRequest: true);

            Serilog.Log.Error("{@EkarPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result != null && result.Success == true && result.Data.success && result.Data.result.id != 0)
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Data.result.id.ToStringNullSafe();

                return new ServiceResponseBase
                {
                    Success = localReservation != null && result != null && result.Data != null,
                    Data = localReservation
                };
            }
            else if (result != null && result.Data != null)
            {
                Serilog.Log.Error("{EkarReservationError}", "rezervasyon oluşturulamadı"); //response içerisinde error message alanı yok
                localReservation.APIMessage = "rezervasyon oluşturulamadı";
            }

            Serilog.Log.Error("Ekar servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Ekar servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Ekar servisinden herhangi bir veri alınamadı!"
            };

        }

        private IDictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ReservationToken reservationToken, ResponseReservationStepsAdditionalInformation additionalInformation, Reservation localReservation)
        {
            return new Dictionary<string, object>()
            {
                { "currency", "TRY"},
                { "customerEmail", postReservationRequest.CustomerEmail },
                { "customerGSM", postReservationRequest.CustomerTelephone.Replace(" ", string.Empty)},
                { "customerName", postReservationRequest.CustomerName },
                { "customerPersonalNumber", postReservationRequest.CustomerPersonalNumber },
                { "customerSurname", postReservationRequest.CustomerSurname },
                { "flightNo", postReservationRequest.FlightNumberArrival },
                { "paymentType", PaymentType.RENT_FEE_RECEIVED },
                { "pickupDate", Convert.ToDateTime(postReservationRequest.PickupDate).ToString("dd.MM.yyyy") },
                { "pickupTime", Convert.ToDateTime(postReservationRequest.PickupTime).ToString("HH:mm") },
                { "returnDate", Convert.ToDateTime(postReservationRequest.ReturnDate).ToString("dd.MM.yyyy") },
                { "returnTime", Convert.ToDateTime(postReservationRequest.ReturnTime).ToString("HH:mm") },
                { "pickupLocationId", additionalInformation.APIPickupLocationCode.ToIntNullSafe() },
                { "returnLocationId", additionalInformation.APIReturnLocationCode.ToIntNullSafe() },
                { "reservationUniqueId", localReservation.ReservationNumber },
                { "vehicleGroupId", reservationToken.VehicleCode.ToIntNullSafe() }
            };
        }
        private IDictionary<string, object> PostCancelReservationRequestParameters(PostCancelReservationRequest postCancelReservationRequest, Reservation localReservation)
        {
            return new Dictionary<string, object>()
            {
                { "language", "tr"},
                { "reservationId", localReservation.APIReservationNumber},
                { "cancelDescription",postCancelReservationRequest.CancelNote }
            };
        }
    }
}
