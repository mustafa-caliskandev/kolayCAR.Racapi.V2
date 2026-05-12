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
using System.Linq;
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

            var bookRequest = GetEntity(postReservationRequest, reservationToken, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(bookRequest),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@ErenPostReservationRequestParameters}", bookRequest);

            var response = await _httpManager.PostAsyncWithModelResult<ErenBookRequest, ErenBookResponse>(
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

            if (response?.Data != null && response.Data.Status?.ToLower() == "success")
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = response.Data.BookingNumber;

                return new ServiceResponseBase(localReservation, true, response.Data.Message ?? "Rezervasyon başarılı.");
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, response, "rezervasyon hatası!");
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

            var response = await _httpManager.PostAsyncWithModelResult<ErenCancelRequest, ErenCancelResponse>(
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

            if (response?.Data != null && response.Data.Status?.ToLower() == "success")
            {
                localReservation.APIReservationCancel = true;
                return new ServiceResponseBase(localReservation, true, response.Data.Message ?? "İptal işlemi başarılı.");
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, response, "iptal işlemi başarısız.");
        }

        private ErenBookRequest GetEntity(PostReservationRequest postReservationRequest, ReservationToken reservationToken, Reservation localReservation)
        {
            var extras = new List<ErenBookExtra>();

            var requestExtras = postReservationRequest.PostReservationRequestV2?.Extras;

            if (requestExtras?.Any() == true)
            {
                foreach (var extra in requestExtras)
                {
                    if (int.TryParse(extra.ExtraCode, out int serviceId))
                    {
                        extras.Add(new ErenBookExtra { ServiceId = serviceId });
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(postReservationRequest.ExtraList))
            {
                foreach (var extra in postReservationRequest.ExtraList.Split(','))
                {
                    if (int.TryParse(extra.Trim(), out int serviceId))
                    {
                        extras.Add(new ErenBookExtra { ServiceId = serviceId });
                    }
                }
            }

            var payment = postReservationRequest.PostReservationRequestV2?.Payment;

            float collectedPrice = 0;

            if (payment != null && payment.PaymentType != PaymentTypes.PayOnDelivery)
            {
                if (!payment.OneWayFeePayToDelivery)
                    collectedPrice += reservationToken.APIOneWayFee;

                collectedPrice += (float)Math.Round(
                    reservationToken.APIDailyPrice * reservationToken.RentalDuration,
                    3
                );
            }

            var collectedExtras = payment?.ExtraPricePayToDelivery == true
                ? 0
                : requestExtras?.Sum(e => e.ApiPrice) ?? 0;

            var birthDateParsed = DateTime.TryParse(postReservationRequest.CustomerBirthDay, out var birthDate);

            var driverAge = birthDateParsed
                ? DateTime.Now.Year - birthDate.Year
                : 30;

            driverAge = Math.Max(21, driverAge);

            return new ErenBookRequest
            {
                BookingReference = localReservation.ReservationNumber,
                SearchRequestId = reservationToken.APIReferenceCode,
                QuoteId = reservationToken.APIReferenceCode2,
                FirstName = postReservationRequest.CustomerName,
                LastName = postReservationRequest.CustomerSurname,
                Email = postReservationRequest.CustomerEmail,
                Phone = postReservationRequest.CustomerTelephone?.Replace(" ", string.Empty),
                FlightNumber = postReservationRequest.FlightNumberArrival,
                DriverAge = driverAge,
                IdNumber = postReservationRequest.CustomerPersonalNumber,
                PassportNumber = postReservationRequest.CustomerPersonalNumber,
                Comment = postReservationRequest.CustomerNote,
                CollectedPrice = collectedPrice,
                CollectedExtras = collectedExtras,
                Extras = extras.Any() ? extras : null
            };
        }
    }
}
