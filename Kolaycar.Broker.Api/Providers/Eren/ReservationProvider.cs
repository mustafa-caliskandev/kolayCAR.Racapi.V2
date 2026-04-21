using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.Eren;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.Eren;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.Eren
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _authProvider = new AuthProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);

            var extras = new List<ErenBookExtra>();
            if (postReservationRequest.PostReservationRequestV2?.Extras != null)
            {
                foreach (var extra in postReservationRequest.PostReservationRequestV2.Extras)
                {
                    if (int.TryParse(extra.ExtraCode, out int serviceId))
                    {
                        extras.Add(new ErenBookExtra { ServiceId = serviceId });
                    }
                }
            }
            else if (!string.IsNullOrEmpty(postReservationRequest.ExtraList))
            {
                var extraCodes = postReservationRequest.ExtraList.Split(',');
                foreach (var extra in extraCodes)
                {
                    if (int.TryParse(extra.Trim(), out int serviceId))
                    {
                        extras.Add(new ErenBookExtra { ServiceId = serviceId });
                    }
                }
            }

            var bookRequest = new ErenBookRequest
            {
                BookingReference = localReservation.ReservationNumber,
                SearchRequestId = reservationToken.APIReferenceCode,
                QuoteId = reservationToken.APIReferenceCode2,
                FirstName = postReservationRequest.CustomerName,
                LastName = postReservationRequest.CustomerSurname,
                Email = postReservationRequest.CustomerEmail,
                Phone = postReservationRequest.CustomerTelephone?.Replace(" ", string.Empty),
                FlightNumber = postReservationRequest.FlightNumberArrival,
                DriverAge = Math.Max(21, (DateTime.Now.Year - (DateTime.TryParse(postReservationRequest.CustomerBirthDay, out var bday) ? bday.Year : DateTime.Now.Year - 30))), // Calculate or mock driver age
                IdNumber = postReservationRequest.CustomerPersonalNumber,
                PassportNumber = postReservationRequest.CustomerPersonalNumber, // Usually one or the other
                Comment = postReservationRequest.CustomerNote,
                Extras = extras.Count > 0 ? extras : null
            };

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(bookRequest),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@ErenPostReservationRequestParameters}", bookRequest);

            var response = await _httpManager.PostAsyncWithModel<ErenBookRequest, ErenBookResponse>(
                "/v1/book",
                bookRequest,
                headers: accessToken,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                }
            );

            Serilog.Log.Error("{@ErenPostReservationResponse}", response);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (response != null && response.Status?.ToLower() == "success")
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = response.BookingNumber;

                return new ServiceResponseBase(localReservation, true, response.Message ?? "Rezervasyon başarılı.");
            }

            localReservation.APIMessage = response?.Message ?? $"{vendor.VendorName} servisinden rezervasyon onaylanmadı!";
            return new ServiceResponseBase(localReservation, false, response?.Message ?? $"{vendor.VendorName} rezervasyon hatası!");
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var accessToken = await _authProvider.GetTokenAsync(vendor);

            var cancelRequest = new ErenCancelRequest
            {
                BookingNumber = localReservation.APIReservationNumber ?? localReservation.ReservationNumber,
                Comment = postCancelReservationRequest.CancelNote ?? "User requested cancellation"
            };

            Serilog.Log.Error("{@ErenPostCancelReservationRequestParameters}", cancelRequest);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(cancelRequest),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            var response = await _httpManager.PostAsyncWithModel<ErenCancelRequest, ErenCancelResponse>(
                "/v1/cancel",
                cancelRequest,
                headers: accessToken,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }
            );

            Serilog.Log.Error("{@ErenPostCancelReservationResponse}", response);

            if (response != null && response.Status?.ToLower() == "success")
            {
                localReservation.APIReservationCancel = true;
                return new ServiceResponseBase(localReservation, true, response.Message ?? "İptal işlemi başarılı.");
            }

            return new ServiceResponseBase(localReservation, false, response?.Message ?? "İptal işlemi başarısız.");
        }
    }
}
