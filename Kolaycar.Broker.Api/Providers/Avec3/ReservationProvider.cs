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
using static KolayCAR.Broker.Domain.Models.Response.Avec3ResponseBase;

namespace KolayCAR.Broker.API.Providers.Avec3
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager _httpManager;
        private readonly IConfigurationService _configurationService;
        AuthProvider _authProvider;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            _configurationService = configurationService;
            _authProvider = new AuthProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var user = await _authProvider.GetTokenAsync(vendor.ApiKey, vendor.ApiPassword);
            var cancelRequestEntity = GetCancelRequestBody(postCancelReservationRequest);
            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(localReservation.APIReferenceCode),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@AvecPostCancelReservationsRequestParameters}", localReservation.APIReferenceCode);

            var result = await _httpManager.PostAsync2<PostCancelReservationRequestAvec, Avec3ResponseBase.PostCancelReservationResponseAvec>(
            requestPath: @"/branch_base/booking/" + localReservation.APIReferenceCode + "/cancel",
            headers: _authProvider.CreateAuthHeader(user.Data.access_token),
            entity: cancelRequestEntity,
            brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
            },
            isReservationRequest: true);

            Serilog.Log.Error("{@AvecPostCancelReservationsResponse}", result);

            if (result != null && result.Data.state == "canceled")
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
                Success = false,
                Data = localReservation,
                Message = "Avec servisi rezervasyon iptali başarısız!",
            };
        }

        private PostCancelReservationRequestAvec GetCancelRequestBody(PostCancelReservationRequest postCancelReservationRequest) => new PostCancelReservationRequestAvec { cancel_explanation = postCancelReservationRequest.CancelNote.ToStringNullSafe() };

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var postReservationRequestParameters = PostReservationRequestParameters(postReservationRequest, reservationToken, localReservation);
            var user = await _authProvider.GetTokenAsync(vendor.ApiKey, vendor.ApiPassword);
            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@AvecPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await _httpManager.PostAsync2<PostReservationRequestBody, PostReservationResponse>(
            requestPath: @"/branch_base/booking/",
            entity: postReservationRequestParameters,
            headers: _authProvider.CreateAuthHeader(user.Data.access_token),
            brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationVendorAPIResponse
            }, isReservationRequest: true);

            Serilog.Log.Error("{@AvecPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result != null && result.Success == true && result.Data != null && result.Data.reservation_number != "")
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Data.reservation_number.ToStringNullSafe();

                localReservation.APIVendorPickupAddress = result.Data.pickup_branch.address.address_line_2 + " - " + result.Data.pickup_branch.address.address_line_1;
                localReservation.APIVendorPickupPhone = "";

                localReservation.APIVendorReturnAddress = result.Data.dropoff_branch.address.address_line_2 + " - " + result.Data.dropoff_branch.address.address_line_1;
                localReservation.APIVendorReturnPhone = vendor.VendorPhone;

                return new ServiceResponseBase
                {
                    Success = localReservation != null && result != null && result.Data != null,
                    Data = localReservation
                };
            }
            else if (result != null && result.Data != null && !string.IsNullOrEmpty(result.Data.reservation_number))
            {
                Serilog.Log.Error("{AvecReservationError}", "rezervasyon oluşturulamadı"); //response içerisinde error message alanı yok
                localReservation.APIMessage = "rezervasyon oluşturulamadı";
            }

            Serilog.Log.Error("Avec servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Avec servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Avec servisinden herhangi bir veri alınamadı!"
            };
        }

        private PostReservationRequestBody PostReservationRequestParameters(PostReservationRequest postReservationRequest, ReservationToken reservationToken, Reservation localReservation)
        {
            string country;
            if ((long.TryParse(postReservationRequest.CustomerPersonalNumber, out _) && postReservationRequest.CustomerPersonalNumber.Length == 11))
            {
                country = "TR";
            }
            else
            {
                country = "GB";
            }
            var requestBody = new PostReservationRequestBody
            {
                booking_id = reservationToken.APIReferenceCode,
                driver =
                new Driver
                {
                    first_name = postReservationRequest.CustomerName,
                    last_name = postReservationRequest.CustomerSurname,
                    phone = postReservationRequest.CustomerTelephone,
                    birth_date = Convert.ToDateTime(postReservationRequest.CustomerBirthDay).ToString("yyyy-MM-dd"),
                    country = country,
                    email = postReservationRequest.CustomerEmail,
                    identity_number = postReservationRequest.CustomerPersonalNumber
                },
                payment_role = "semi_credit",
                external_id = localReservation.ReservationNumber
            };

            return requestBody;
        }
    }
}
