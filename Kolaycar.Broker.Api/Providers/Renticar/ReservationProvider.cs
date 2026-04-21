using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Renticar.Request;
using KolayCAR.Broker.Domain.Models.Renticar.Response;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Renticar
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        ILocationProvider locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService, IMemoryCache memoryCache)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            AuthProvider = new AuthProvider(apiBaseUrl);
            locationProvider = new LocationProvider(apiBaseUrl, memoryCache);
            _configurationService = configurationService;
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var reservationCancelRequestBody = CreateCancelRequestBody(localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(reservationCancelRequestBody),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@RenticarPostCancelReservationsRequestParameters}", reservationCancelRequestBody);

            var auth = await AuthProvider.GetToken(vendor);

            if (auth != null && auth.status == "success")
            {
                var result = await RestManager.PostAsync<ReservationCancelRequestBody, ReservationCancelResponseBody>(
                    requestPath: "reservation/cancel",
                    headers: AuthProvider.CreateHeader(auth.token),
                    entity: reservationCancelRequestBody,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    },
                   isReservationRequest: true);

                Serilog.Log.Error("{@RenticarPostCancelReservationsResponse}", result);

                if (result != null && result.result == "success")
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
                    Message = result?.message
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Renticar kullanıcı girişi yapılamadı!"
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Domain.Models.Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);

            var reservationRequestBody = CreateRequestBody(postReservationRequest, reservationToken);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(reservationRequestBody),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@RenticarPostReservationRequestParameters}", reservationRequestBody);

            var auth = await AuthProvider.GetToken(vendor);

            if (auth != null && auth.status == "success")
            {
                var result = await RestManager.PostAsync<ReservationRequestBody, ReservationResponseBase>(
                    requestPath: "reservation",
                    entity: reservationRequestBody,
                    headers: AuthProvider.CreateHeader(auth.token),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                     isReservationRequest: true);

                Serilog.Log.Error("{@RenticarPostReservationResult}", result);

                localReservation.ReservationPostedToAPI = true;

                if (result != null && !string.IsNullOrEmpty(result.reservationId))
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.reservationId;
                    localReservation.APIReferenceCode2 = result.reservationCode;
                    localReservation.APIReferenceCode3 = result.vendorReservationNo;

                    var reservationLocation = new List<Domain.Models.Location>();
                    var locations = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                    if (locations.Success)
                    {
                        reservationLocation = locations.Data as List<Domain.Models.Location>;

                        if (reservationLocation != null && reservationLocation.Count > 0)
                        {
                            var reservationPickupLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIPickupLocationCode).FirstOrDefault();
                            var reservationReturnLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIReturnLocationCode).FirstOrDefault();

                            if (reservationPickupLocation != null)
                            {
                                localReservation.APIVendorPickupAddress = reservationPickupLocation.Address;
                                localReservation.APIVendorPickupPhone = reservationPickupLocation.PhoneNumber;
                            }

                            if (reservationReturnLocation != null)
                            {
                                localReservation.APIVendorReturnAddress = reservationReturnLocation.Address;
                                localReservation.APIVendorReturnPhone = reservationReturnLocation.PhoneNumber;
                            }
                        }
                    }

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
                    Message = "Renticar rezervasyon oluşturulamadı!"
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Renticar kullanıcıc girişi yapılamadı!"
            };
        }

        private static ReservationRequestBody CreateRequestBody(PostReservationRequest postReservationRequest, ReservationToken reservationToken)
        {
            var certificateType = postReservationRequest.CustomerPersonalNumber != null && postReservationRequest.CustomerPersonalNumber.Length == 11 && !System.Text.RegularExpressions.Regex.IsMatch(postReservationRequest.CustomerPersonalNumber, "[a-zA-Z]") ? "tc" : "other";

            var birthDay = DateTime.ParseExact(postReservationRequest.CustomerBirthDay ?? "01.01.1999", "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

            return new ReservationRequestBody
            {
                offerId = reservationToken.APIReferenceCode,
                currency = postReservationRequest.CurrencyCode,
                driverInfo = new DriverInfo
                {
                    name = postReservationRequest.CustomerName,
                    lastname = postReservationRequest.CustomerSurname,
                    birthday = birthDay.ToString("yyyy-MM-dd"),
                    email = postReservationRequest.CustomerEmail,
                    phone = postReservationRequest.CustomerTelephone,
                    identity = new Identity
                    {
                        certificateType = certificateType,
                        value = postReservationRequest.CustomerPersonalNumber
                    }
                },
                extras = new List<Domain.Models.Renticar.Response.Extra>(),
                reservationType = postReservationRequest.FullCredit.ToBoolNullSafe() == true ? "fullCredit"
                                    : postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? "payOnArrival"
                                    : ""

            };
        }

        private static ReservationCancelRequestBody CreateCancelRequestBody(Reservation localReservation) => new ReservationCancelRequestBody
        {
            reservationId = localReservation.APIReservationNumber
        };
    }
}
