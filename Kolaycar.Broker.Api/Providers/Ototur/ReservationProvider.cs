using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.OtoturResponseBase;

namespace KolayCAR.Broker.API.Providers.Ototur
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager _RestManager;
        AuthProvider _authProvider { get; set; }
        ILocationProvider _locationProvider { get; set; }

        private readonly IConfigurationService _configurationService;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _authProvider = new AuthProvider(apiBaseUrl);
            _locationProvider = new LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
            _RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Domain.Models.Reservation localReservation)
        {
            var token = await _authProvider.GetToken();

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(getCancelReservationData(postCancelReservationRequest, localReservation)),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            var result = await _RestManager.PostFormUrlEncoded<ReservationCancelResultBase>
                (
                  requestPath: $"reservations/{localReservation.APIReservationNumber}",
                  headers: CreateAuthHeaderWithContentType(token),
                  brokerLogModel: new BrokerLogModel
                  {
                      LogKey = localReservation.ReservationNumber,
                      LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                  },
                  postData: getCancelReservationData(postCancelReservationRequest, localReservation),
                  isReservationRequest: true
                );

            if (result != null)
            {
                if (result.result.content[0].durum == "Cancelled" && result.success)
                {

                    localReservation.APIReservationCancel = true;

                    return new ServiceResponseBase
                    {
                        Success = true,
                        Data = localReservation
                    };
                }
            }
            return new ServiceResponseBase
            {
                Data = null,
                Success = false,
                Message = "Ototur servisine ulaşılamadı !"
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Domain.Models.Reservation localReservation, List<Domain.Models.Extra> apiExtras)
        {
            var token = await _authProvider.GetToken();
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postReservationRequestParameters = getData(postReservationRequest, additionalInformation, reservationNumber, reservationToken, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@OtoturPostReservationRequestParameters}", postReservationRequestParameters);
            var result = await _RestManager.PostFormUrlEncoded<ReservationResultBase>(requestPath: "reservations", headers: CreateAuthHeaderWithContentType(token), brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationVendorAPIResponse
            },
            postData: getData(postReservationRequest, additionalInformation, reservationNumber, reservationToken, localReservation), isReservationRequest: true);

            Serilog.Log.Error("{@OtoturPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result != null)
            {

                if (result.success == false)
                {
                    Serilog.Log.Error("Ototur servisi rezervasyonu reddetti!");

                    return new ServiceResponseBase
                    {
                        Success = false,
                        Data = localReservation,
                        Message = "Ototur servisi rezervasyonu reddetti!"
                    };
                }
                else
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.result.content[0].id.ToString();

                    var location = await _locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                    Serilog.Log.Error("{@OtoturGetLocationsResponse}", location.Data);
                    var reservationLocation = new List<Domain.Models.Location>();
                    if (location.Success)
                    {
                        reservationLocation = location.Data as List<Domain.Models.Location>;

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

            }

            Serilog.Log.Error(vendor.VendorName + " servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = vendor.VendorName + " servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = vendor.VendorName + " servisinden herhangi bir veri alınamadı!"
            };
        }
        public Dictionary<string, object> CreateAuthHeaderWithContentType(OtoturAuthResponse ototurAuthResponse) =>
             new Dictionary<string, object>()
             {
                { "Authorization", $"Bearer {ototurAuthResponse.token}"},
                { "Content-Type", "application/x-www-form-urlencoded"}
             };
        private IEnumerable<KeyValuePair<string, string>> getCancelReservationData(PostCancelReservationRequest postCancelReservationRequest, Domain.Models.Reservation localReservation)
        {
            return new List<KeyValuePair<string, string>>() {
            new KeyValuePair<string, string>("language", (localReservation.LanguageType == LanguageTypes.TR) ? "tr" : "en"),
            new KeyValuePair<string, string>("reservationId", localReservation.APIReservationNumber),
            new KeyValuePair<string, string>("cancelDescription", postCancelReservationRequest.CancelNote)};
        }
        private IEnumerable<KeyValuePair<string, string>> getData(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, Domain.Models.Reservation localReservation)
        {
            string extras = "";
            if (postReservationRequest.ExtraList != null && postReservationRequest.ExtraList != "")
            {
                string[] extraArray = postReservationRequest.ExtraList.Split('|');
                for (int i = 0; i < extraArray.Length; i++)
                {
                    if (i == extraArray.Length - 1)
                    {
                        extras = extras + extraArray[i].ElementAt(0);
                    }
                    else
                        extras = extras + extraArray[i].ElementAt(0) + ',';
                }
            }
            if (postReservationRequest.PostReservationRequestV2?.Extras?.Count > 0)
            {
                extras = string.Join(',', postReservationRequest.PostReservationRequestV2.Extras.Select(e => e.ApiExtraCode));
            }
            return new List<KeyValuePair<string, string>>() {
             new KeyValuePair<string, string>("language", additionalInformation.LanguageCode),
            new KeyValuePair<string, string>("currency", additionalInformation.CurrencyCode),
            new KeyValuePair<string, string>("customerName", postReservationRequest.CustomerName),
            new KeyValuePair<string, string>("customerSurname", postReservationRequest.CustomerSurname),
            new KeyValuePair<string, string>("customerPersonelNumber", postReservationRequest.CustomerPersonalNumber),
            new KeyValuePair<string, string>("customerEmail", postReservationRequest.CustomerEmail),
            new KeyValuePair<string, string>("customerGSM", postReservationRequest.CustomerTelephone),
            new KeyValuePair<string, string>("customerNote", postReservationRequest.CustomerNote),
            new KeyValuePair<string, string>("flightNo", postReservationRequest.FlightNumberArrival),
            new KeyValuePair<string, string>("pickupDate", postReservationRequest.PickupDate),
            new KeyValuePair<string, string>("pickupLocationId", postReservationRequest.PickupLocationId.ToString()),
            new KeyValuePair<string, string>("pickupTime", postReservationRequest.PickupTime),
            new KeyValuePair<string, string>("returnDate", postReservationRequest.ReturnDate),
            new KeyValuePair<string, string>("returnLocationId", postReservationRequest.ReturnLocationId.ToString()),
            new KeyValuePair<string, string>("returnTime", postReservationRequest.ReturnTime),
            new KeyValuePair<string, string>("reservationNotes", localReservation.ReservationStatusNote),
            new KeyValuePair<string, string>("reservationUniqueId", reservationNumber),
            new KeyValuePair<string, string>("vehicleGroupId", reservationToken.VehicleCode),
            new KeyValuePair<string, string>("extras", extras)
        };
        }
        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Domain.Models.Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "login", vendor.ApiKey },
                { "passwd",  vendor.ApiPassword},
                { "ReservationId",  reservation.APIReservationNumber},
            };


    }
}
