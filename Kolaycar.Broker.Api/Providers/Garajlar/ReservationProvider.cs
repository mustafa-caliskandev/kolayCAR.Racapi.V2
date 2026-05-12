using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.GarajlarRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.GarajlarResponseBase;

namespace KolayCAR.Broker.API.Providers.Garajlar
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly IConfigurationService _configurationService;
        private readonly AuthProvider _authProvider;
        private readonly LocationProvider _locationProvider;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _httpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _authProvider = new AuthProvider(apiBaseUrl);
            _locationProvider = new LocationProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var token = await _authProvider.GetTokenHeader(vendor);
            if (token is null)
                return new(localReservation, false, $"{vendor.VendorName} token bilgisi alınamadı!");

            var entity = GetEntity(localReservation);

            await _configurationService.WriteLog(new BrokerLogModel(localReservation.ReservationNumber, entity.ToJson(), BrokerLogTypes.ReservationCancelVendorAPIRequest));

            Serilog.Log.Error("{@GarajlarPostCancelReservationsRequestParameters}", entity.ToJson());

            var result = await _httpManager.PostAsyncWithModelResult<object, CancelResponse>(
                requestPath: "/api/obilet/cancel-reservation",
                headers: token,
                entity: entity,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@GarajlarPostCancelReservationsResponse}", result);

            if (result?.Data?.status ?? false)
            {
                localReservation.APIReservationCancel = true;
                return new(localReservation, true);
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisi rezervasyon iptali başarısız!");
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var token = await _authProvider.GetTokenHeader(vendor);
            if (token is null)
                return new(localReservation, false, $"{vendor.VendorName} token bilgisi alınamadı!");

            var entity = GetEntity(postReservationRequest, localReservation, reservationToken, additionalInformation);

            await _configurationService.WriteLog(new BrokerLogModel(localReservation.ReservationNumber, entity.ToJson(), BrokerLogTypes.ReservationVendorAPIRequest));

            Serilog.Log.Error("{@GarajlarPostReservationRequestParameters}", entity.ToJson());

            var result = await _httpManager.PostAsyncWithModelNullValueHandlingResult<ReservationRequest, ResponseBase<object>>(
                requestPath: "/api/obilet/set-reservation",
                headers: token,
                entity: entity,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@GarajlarPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result?.Data?.data != null && result.Data.success)
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Data.data.ToStringNullSafe();

                var locationResult = await _locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@GarajlarGetLocationsResponse}", locationResult.Data);

                if (locationResult.Success && locationResult.Data is List<Domain.Models.Location> locations)
                {
                    localReservation.APIVendorPickupAddress = locations.FirstOrDefault(x => x.LocationCode == additionalInformation.APIPickupLocationCode)?.Address;
                    localReservation.APIVendorPickupPhone = locations.FirstOrDefault(x => x.LocationCode == additionalInformation.APIPickupLocationCode)?.PhoneNumber;

                    localReservation.APIVendorReturnAddress = locations.FirstOrDefault(x => x.LocationCode == additionalInformation.APIReturnLocationCode)?.Address;
                    localReservation.APIVendorReturnPhone = locations.FirstOrDefault(x => x.LocationCode == additionalInformation.APIReturnLocationCode)?.PhoneNumber;
                }

                return new(localReservation, true);
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisinden herhangi bir veri alınamadı!");
        }

        private ReservationRequest GetEntity(PostReservationRequest postReservationRequest, Reservation reservation, ReservationToken reservationToken, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            var extras = new List<Extras>();
            if (reservation.ReservationExtras.Count > 0)
                foreach (var extra in reservation.ReservationExtras)
                    extras.Add(new Extras { code = extra.ExtraCode });

            return new ReservationRequest
            {
                personal_info = new PersonalInfo
                {
                    address = "Kağıthane/İstanbul",
                    birthday = postReservationRequest.CustomerBirthDay.ToDateTimeNullSafe().ToString("yyyy-MM-dd"),
                    city = "İstanbul",
                    country = "Türkiye",
                    district = "Kağıthane",
                    email = postReservationRequest.CustomerEmail,
                    first_name = postReservationRequest.CustomerName,
                    last_name = postReservationRequest.CustomerSurname,
                    identity = postReservationRequest.CustomerPersonalNumber,
                    identity_type = (System.Text.RegularExpressions.Regex.IsMatch(postReservationRequest.CustomerPersonalNumber, "[a-zA-Z]") || postReservationRequest.CustomerPersonalNumber.Length < 11) ? "3" : "1",
                    telephone = postReservationRequest.CustomerTelephone
                },
                reservation_info = new ReservationInfo
                {
                    contract_number = reservation.ReservationNumber,
                    sub_group_short_name = reservationToken.VehicleCode,
                    startLocationCode = additionalInformation.APIPickupLocationCode,
                    endLocationCode = additionalInformation.APIReturnLocationCode,
                    main_group_id = reservationToken.APIReferenceCode.Split('-')[0].ToIntNullSafe(),
                    main_rule_id = reservationToken.APIReferenceCode.Split('-')[3].ToIntNullSafe(),
                    sub_group_id = reservationToken.APIReferenceCode.Split('-')[1].ToIntNullSafe(),
                    campaign_id = reservationToken.APIReferenceCode.Split('-')[2],
                    startDateTime = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm"),
                    endDateTime = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm"),

                },
                extras = extras.Count > 0 ? extras : null
            };
        }
        private CancelReservationRequest GetEntity(Reservation localReservation) => new() { reservationId = localReservation.APIReservationNumber.ToIntNullSafe() };
    }
}
