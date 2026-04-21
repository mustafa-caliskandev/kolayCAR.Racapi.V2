using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.SixtRequestBase;

namespace KolayCAR.Broker.API.Providers.Sixt2
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly IConfigurationService _configurationService;
        HttpManager _httpManager;
        AuthProvider _authProvider;
        LocationProvider _locationProvider;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService, ICacheService cacheService)
        {
            _httpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _configurationService = configurationService;
            _authProvider = new AuthProvider(apiBaseUrl, cacheService);
            _locationProvider = new LocationProvider(apiBaseUrl, cacheService);
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var token = await _authProvider.GetBearerToken(vendor);
            var entity = new SixtCancelReservationRequest { cancel_reason = "2" };
            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(localReservation.APIReservationNumber),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@SixtPostCancelReservationsRequestParameters}", localReservation.APIReservationNumber);

            var result = await _httpManager.DeleteAsync<SixtCancelReservationRequest>(
                requestPath: $"api/v1/reservations/{localReservation.APIReservationNumber}",
                headers: token,
                entity: entity,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);
            Serilog.Log.Error("{@SixtPostCancelReservationResult}", result);

            if (result == HttpStatusCode.OK)
            {
                localReservation.APIReservationCancel = true;

                return new(localReservation, true);
            }
            return new(localReservation, false, "Sixt servisinden rezervasyon iptal edilemedi!");
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var entity = CreateReservationSaveRequestParameters(reservationToken, additionalInformation, vendor, postReservationRequest, localReservation, apiExtras);

            await _configurationService.WriteLog(new BrokerLogModel(localReservation.ReservationNumber, entity.ToJson(), BrokerLogTypes.ReservationVendorAPIRequest));

            Serilog.Log.Error("{@SixtPostReservationRequestParameters}", entity);

            var token = JsonConvert.DeserializeObject<Dictionary<string, object>>(reservationToken.APIReferenceCode2);

            if (token != null)
            {
                var result = await _httpManager.PostAsync2<SixtPostReservationRequest, SixtResponseBase<SixtReservationResponse>>(
                requestPath: "/api/v1/reservations",
                headers: token,
                entity: entity,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                }, isReservationRequest: true);

                Serilog.Log.Error("{@SixtPostReservationReservationResult}", result);
                localReservation.ReservationPostedToAPI = true;

                if (result?.Data?.result != null)
                {
                    localReservation.APIReservationNumber = result.Data.result.trans_no;
                    localReservation.APIReservationSuccessfully = true;

                    var locations = await _locationProvider.GetLocations(vendor, (int)postReservationRequest.LanguageCode.ToEnum<LanguageTypes>());
                    if (locations != null)
                    {
                        var reservationLocation = locations.Data as List<Domain.Models.Location>;
                        var pickupLocation = reservationLocation.FirstOrDefault(e => e.LocationCode == additionalInformation.APIPickupLocationCode);
                        var returnLocation = reservationLocation.FirstOrDefault(e => e.LocationCode == additionalInformation.APIReturnLocationCode);

                        if (pickupLocation != null)
                        {
                            localReservation.APIVendorPickupAddress = pickupLocation.Address;
                            localReservation.APIVendorPickupPhone = pickupLocation.PhoneNumber;
                        }

                        if (returnLocation != null)
                        {
                            localReservation.APIVendorPickupAddress = returnLocation.Address;
                            localReservation.APIVendorPickupPhone = returnLocation.PhoneNumber;
                        }
                    }
                    return new(localReservation, true);
                }
                return new(null, false, "Sixt servisi ile bağlantı kurulamadı!");
            }
            return new(null, false, "Sixt servisi ile bağlantı kurulamadı!");
        }

        private SixtPostReservationRequest CreateReservationSaveRequestParameters(ReservationToken reservationToken, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, PostReservationRequest postReservationRequest, Reservation localReservation, List<Extra> apiExtras)
        {
            var extras = localReservation?.ReservationExtras?
              .Where(extra => extra?.ExtraCode != null)
              .Select(extra => new SixtRequestBase.SixtExtra
              {
                  extra_code = extra.ExtraCode,
                  name = apiExtras?.FirstOrDefault(e => e?.ExtraCode == extra.ExtraCode)?.ExtraName ?? "",
                  //price = extra.APIPrice.ToStringNullSafe(),
                  price = extra.ApiPrice.ToStringNullSafe(),
                  piece = 1
              })
              .ToList() ?? new List<SixtRequestBase.SixtExtra>();

            return new SixtPostReservationRequest
            {
                unid = reservationToken.APIReferenceCode,
                vehicle_group = reservationToken.VehicleCode.Contains("|") ? reservationToken.VehicleCode.Split("|")[0] : reservationToken.VehicleCode,
                customer = new SixtCustomer
                {
                    email = postReservationRequest.CustomerEmail,
                    name = postReservationRequest.CustomerName,
                    surname = postReservationRequest.CustomerSurname,
                    gsm = postReservationRequest.CustomerTelephone,
                    gsm_prefix = "+90",
                    gender = "1",
                    id_birth_date = postReservationRequest.CustomerBirthDay.ToDateTimeNullSafe().ToString("yyyy-MM-dd"),
                    id_no = postReservationRequest.CustomerPersonalNumber
                },
                pickup = new SixtLocation
                {
                    station_id = additionalInformation.APIPickupLocationCode.Split("~")[0],
                    station_code = additionalInformation.APIPickupLocationCode.Split("~")[1],
                    date = postReservationRequest.PickupDate.ToDateTimeNullSafe().ToString("yyyy-MM-dd"),
                    time = postReservationRequest.PickupTime
                },
                returnLocation = new SixtLocation
                {
                    station_id = additionalInformation.APIReturnLocationCode.Split("~")[0],
                    station_code = additionalInformation.APIReturnLocationCode.Split("~")[1],
                    date = postReservationRequest.ReturnDate.ToDateTimeNullSafe().ToString("yyyy-MM-dd"),
                    time = postReservationRequest.ReturnTime
                },
                extras = extras,
                reservation = new SixtReservation
                {
                    comment = postReservationRequest.CustomerNote,
                    flight_no = postReservationRequest.FlightNumberArrival,
                    ip_address = postReservationRequest.CustomerIPAddress,
                    user_agent = postReservationRequest.UserAgent
                }
            };
        }
    }
}
