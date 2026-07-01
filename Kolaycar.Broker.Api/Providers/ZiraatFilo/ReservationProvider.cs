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
using ZFHelper = KolayCAR.Broker.API.Helpers.ZiraatFilo;

namespace KolayCAR.Broker.API.Providers.ZiraatFilo
{
    public class ReservationProvider : IReservationProvider
    {
        private HttpManager _httpManager;
        private AuthProvider _authProvider;
        ILocationProvider _locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _configurationService = configurationService;
            _locationProvider = new LocationProvider(apiBaseUrl);
            _authProvider = new AuthProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var headers = await _authProvider.GetHeaders(vendor);

            if (headers == null)
                return new(null, false, "Token bilgisi alınamadı!");

            var postCancelReservationsRequestParameters = PostCancelReservationsRequestParameters(vendor, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel(localReservation.ReservationNumber, postCancelReservationsRequestParameters.ToJson(), BrokerLogTypes.ReservationCancelVendorAPIRequest));

            Serilog.Log.Error("{@ZiraatFiloPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await _httpManager.GetXmlAsync<ZiraatFiloResposeBase.Root>(
                requestPath: "/api/broker-service/booking_cancel/",
                headers: headers,
                parameters: postCancelReservationsRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }, isReservationRequest: true);

            Serilog.Log.Error("{@ZiraatFiloPostCancelReservationsResponse}", result);

            if ((result?.Turevsistem?.Status == "200" || result?.Turevsistem?.Status == "True") && !string.IsNullOrEmpty(result?.Turevsistem?.Key))
            {
                localReservation.APIReservationCancel = true;
                return new(localReservation, true);
            }
            return new(localReservation, false, "ZiraatFilo servisi rezervasyon iptali başarısız!");
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var headers = await _authProvider.GetHeaders(vendor);

            if (headers == null)
                return new(null, false, "Token bilgisi alınamadı!");

            var postReservationRequestParameters = PostReservationRequestParameters(postReservationRequest, additionalInformation, reservationToken);

            await _configurationService.WriteLog(new BrokerLogModel(localReservation.ReservationNumber, postReservationRequestParameters.ToJson(), BrokerLogTypes.ReservationVendorAPIRequest));

            Serilog.Log.Error("{@ZiraatFiloPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await _httpManager.PostXmlAsync<ZiraatFiloResposeBase.Root>(
                requestPath: "/api/broker-service/booking-save/",
                parameters: postReservationRequestParameters,
                headers: headers,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@ZiraatFiloPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (!string.IsNullOrEmpty(result?.Turevsistem?.Key) && (result?.Turevsistem?.Status == "200" || result?.Turevsistem?.Status == "True"))
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Turevsistem.Key;

                var locationResult = await _locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@ZiraatFiloGetLocationsResponse}", locationResult.Data);

                if (locationResult.Success && locationResult.Data is List<Domain.Models.Location> locations)
                {
                    localReservation.APIVendorPickupAddress = locations.FirstOrDefault(x => x.LocationCode == additionalInformation.APIPickupLocationCode)?.Address;
                    localReservation.APIVendorPickupPhone = locations.FirstOrDefault(x => x.LocationCode == additionalInformation.APIPickupLocationCode)?.PhoneNumber;

                    localReservation.APIVendorReturnAddress = locations.FirstOrDefault(x => x.LocationCode == additionalInformation.APIReturnLocationCode)?.Address;
                    localReservation.APIVendorReturnPhone = locations.FirstOrDefault(x => x.LocationCode == additionalInformation.APIReturnLocationCode)?.PhoneNumber;
                }

                return new(localReservation, true);
            }

            Serilog.Log.Error(vendor.VendorName + " servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = vendor.VendorName + " servisinden herhangi bir veri alınamadı!";

            return new(localReservation, false, vendor.VendorName + " servisinden herhangi bir veri alınamadı!");
        }
        private Dictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "Rez_ID",reservationToken.APIReferenceCode},
                { "Group_ID", reservationToken.VehicleCode},
                { "Rental_ID", postReservationRequest.CustomerPersonalNumber},
                { "Name", postReservationRequest.CustomerName},
                { "SurName", postReservationRequest.CustomerSurname},
                { "Mail_Adress", postReservationRequest.CustomerEmail},
                { "Pickup_ID", additionalInformation.APIPickupLocationCode},
                { "Pickup_Min",  additionalInformation.PickupDateTime.ToString("mm") },
                { "Pickup_Hour",  additionalInformation.PickupDateTime.ToString("HH") },
                { "Pickup_Day",  additionalInformation.PickupDateTime.ToString("dd") },
                { "Pickup_Month",  additionalInformation.PickupDateTime.ToString("MM") },
                { "Pickup_Year",  additionalInformation.PickupDateTime.ToString("yyyy") },
                { "Drop_Off_ID",  additionalInformation.APIReturnLocationCode},
                { "Drop_Off_Min",  additionalInformation.ReturnDateTime.ToString("mm") },
                { "Drop_Off_Hour",  additionalInformation.ReturnDateTime.ToString("HH") },
                { "Drop_Off_Day",  additionalInformation.ReturnDateTime.ToString("dd") },
                { "Drop_Off_Month",  additionalInformation.ReturnDateTime.ToString("MM") },
                { "Drop_Off_Year",  additionalInformation.ReturnDateTime.ToString("yyyy") },
                { "MobilePhone", postReservationRequest.CustomerTelephone.Replace(" ",string.Empty)},
                { "Currency", ZFHelper.CurrencyHelper.GetZiraatFiloCurrencyType(reservationToken.BaseVendorRequestCurrencyType)},
            };

            if (CreditHelper.ResolveTokenCreditType(reservationToken) == CreditType.FullCredit)
                parameters.Add("Full_Credit", "True");

            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);
            if (selectedExtraCodes != null)
            {
                var extraKeys = new[]
                {
                     "XKP", "Cancel", "Diger", "Child_Seat", "SKP", "IMM", "Young_Drive", "PAI", "Max_Assurance", "Additional_Driver", "Winter_Tire", "MKP",
                     "Super_Mini_Damage_Insurance", "Mini_Damage_Insurance", "LCF"
                };

                foreach (var key in extraKeys)
                {
                    if (selectedExtraCodes.Contains(key))
                        parameters.Add(key, "ON");
                }
            }

            return parameters;
        }

        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "Rez_ID", reservation.APIReservationNumber},
            };
    }
}
