using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using AyesProvider = KolayCAR.Broker.API.Providers.Ayes;

namespace KolayCAR.Broker.API.Providers.Ayes
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager { get; set; }
        ILocationProvider locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            locationProvider = new AyesProvider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postReservationRequestParameters = PostReservationRequestParameters(postReservationRequest, additionalInformation, reservationNumber, reservationToken, apiPaidAmount);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@AyesPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await RestManager.GetAsyncResult<AyesReservationResponse>(
            requestPath: "kiralik-araclar/kirala/",
            parameters: postReservationRequestParameters,
            encode: false,
            brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationVendorAPIResponse
            },
            isReservationRequest: true);

            Serilog.Log.Error("{@AyesPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result?.Data != null && result.Data.islem_durumu && !string.IsNullOrEmpty(result.Data.rezervasyon_numarasi) && result.Data.durum != "hata")
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Data.rezervasyon_numarasi;

                //var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                //Serilog.Log.Error("{@AyesGetLocationsResponse}", location);
                //var reservationLocation = new List<Domain.Models.Location>();
                //if (location.Success)
                //{
                //    reservationLocation = location.Data as List<Domain.Models.Location>;

                //    var reservationPickupLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIPickupLocationCode).FirstOrDefault();
                //    var reservationReturnLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIReturnLocationCode).FirstOrDefault();

                //    localReservation.APIVendorPickupAddress = reservationPickupLocation.Address;
                //    localReservation.APIVendorReturnAddress = reservationReturnLocation.Address;
                //    localReservation.APIVendorPickupPhone = reservationPickupLocation.PhoneNumber;
                //    localReservation.APIVendorReturnPhone = reservationReturnLocation.PhoneNumber;
                //}

                localReservation.APIVendorPickupAddress = result.Data.sube_iletisim_bilgileri.adres;
                localReservation.APIVendorReturnAddress = string.Empty;
                localReservation.APIVendorPickupPhone = result.Data.sube_iletisim_bilgileri.sube_telefon;
                localReservation.APIVendorReturnPhone = string.Empty;
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = localReservation
                };
            }
            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisinden herhangi bir veri alınamadı!");
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var postCancelReservationsRequestParameters = PostCancelReservationsRequestParameters(vendor, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postCancelReservationsRequestParameters),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@AyesPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await RestManager.GetAsyncResult<AyesReservationResponse>(
            requestPath: "api/tr/iptal",
            parameters: postCancelReservationsRequestParameters,
            brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
            },
            isReservationRequest: true);

            Serilog.Log.Error("{@AyesPostCancelReservationsResponse}", result);

            if (result?.Data != null && result.Data.islem_durumu)
            {
                localReservation.APIReservationCancel = true;

                return new ServiceResponseBase
                {
                    Success = true,
                    Data = localReservation
                };
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisi rezervasyon iptali başarısız!");
        }

        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "id",  vendor.ApiKey},
                { "rezervasyon_no", reservation.APIReservationNumber}
            };

        private Dictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                 : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);

            var parameters = new Dictionary<string, object>()
            {
                { "alis_yeri", additionalInformation.APIPickupLocationCode },
                { "alis_tarihi", additionalInformation.PickupDateTime.ToString("yyyy-MM-dd") },
                { "alis_saati", additionalInformation.PickupDateTime.ToString("HH:mm") },
                { "teslim_yeri", additionalInformation.APIReturnLocationCode },
                { "teslim_tarihi", additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd") },
                { "teslim_saati", additionalInformation.ReturnDateTime.ToString("HH:mm") },
                { "id", additionalInformation.Vendor.ApiKey },
                { "para_birimi", reservationToken.BaseVendorRequestCurrencyType},

                { "arac_id", reservationToken.APIReferenceCode},
                { "mad", postReservationRequest.CustomerName },
                { "msoyad", postReservationRequest.CustomerSurname },
                { "meposta", postReservationRequest.CustomerEmail },
                { "mcep_telefon", postReservationRequest.CustomerTelephone },
                { "mtc_pasaport_no", !string.IsNullOrEmpty(postReservationRequest.CustomerPersonalNumber) ? postReservationRequest.CustomerPersonalNumber : "11111111111" },
                { "mmesaj", postReservationRequest.CustomerNote },
                { "mgelis_ucus_no", postReservationRequest.CustomerName },
                { "mgelis_ucus_notu", postReservationRequest.DepartureInfo },
                { "mdonus_ucus_no", postReservationRequest.CustomerName},
                { "mdonus_ucus_notu", string.Empty }
            };

            if (selectedExtraCodes != null && selectedExtraCodes.Count > 0)
                foreach (var extraCode in selectedExtraCodes)
                    parameters.Add($"ekstra[{extraCode}]", 1);

            return parameters;
        }
    }
}
