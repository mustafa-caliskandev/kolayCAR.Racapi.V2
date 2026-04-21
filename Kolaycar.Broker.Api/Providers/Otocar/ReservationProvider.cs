using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OtocarProvider = KolayCAR.Broker.API.Providers.Otocar;

namespace KolayCAR.Broker.API.Providers.Otocar
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager HttpManager { get; set; }
        ILocationProvider LocationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            HttpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/* connectionString: configurationService.GetConnectionString()*/);
            LocationProvider = new OtocarProvider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postReservationRequestParameters = PostReservationRequestParameters(postReservationRequest, additionalInformation, reservationNumber, reservationToken, apiPaidAmount, vendor, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@OtocarPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await HttpManager.GetXmlAsync<OtocarResponseBase.ReservationResponse>(
                requestPath: $"{vendor.ApiKey}_rezervasyonyap",
                parameters: postReservationRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@OtocarPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result != null && result.ArrayOfRezervasyon.rezervasyon != null && !string.IsNullOrEmpty(result.ArrayOfRezervasyon.rezervasyon.rezervasyon_no))
            {
                localReservation.APIReservationSuccessfully = result != null && !string.IsNullOrEmpty(result.ArrayOfRezervasyon.rezervasyon.rezervasyon_no);
                localReservation.APIReservationNumber = result.ArrayOfRezervasyon.rezervasyon.rezervasyon_no.ToStringNullSafe();

                var location = await LocationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@OtocarGetLocationsResponse}", location);
                var reservationLocation = new List<Domain.Models.Location>();
                if (location.Success)
                {
                    reservationLocation = location.Data as List<Domain.Models.Location>;

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
                return new ServiceResponseBase
                {
                    Success = localReservation != null && result != null && !string.IsNullOrEmpty(result.ArrayOfRezervasyon.rezervasyon.rezervasyon_no),
                    Data = localReservation
                };
            }


            Serilog.Log.Error("Otocar servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Otocar servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Otocar servisinden herhangi bir veri alınamadı!"
            };
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

            Serilog.Log.Error("{@OtocarPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await HttpManager.GetXmlAsync<OtocarResponseBase.ReservationCancelResponse>(
                requestPath: $"{vendor.ApiKey}_rezervasyoniptal",
                parameters: postCancelReservationsRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }, isReservationRequest: true);

            Serilog.Log.Error("{@OtocarPostCancelReservationsResponse}", result);

            if (result != null && !string.IsNullOrEmpty(result.ArrayOfRezervasyon.rezervasyon.durum))
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
                Message = "Otocar servisi rezervasyon iptali başarısız!",
            };
        }

        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
            new Dictionary<string, object>()
           {
                { "rezervasyon_kodu", reservation.ReservationNumber},
                { "musteri_soyad",reservation.CustomerSurname},
                { "kull", vendor.ApiKey},
                { "sifre", vendor.ApiPassword},
           };

        private Dictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount, Vendor vendor, Reservation reservation)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList) : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);

            var totalPrice = vendor.RentalWorkingType == VendorWorkingTypes.Commission ? reservation.TotalPrice : reservation.APITotalAmount;
            var deliveryPrice = vendor.RentalWorkingType == VendorWorkingTypes.Commission ? totalPrice - paidAmount : totalPrice;

            return new Dictionary<string, object>()
            {
                { "pick_location", additionalInformation.APIPickupLocationCode },
                { "pick_date",  additionalInformation.PickupDateTime.ToString("dd.MM.yyyy") },
                { "pick_hour",  additionalInformation.PickupDateTime.Hour },
                { "pick_min",  additionalInformation.PickupDateTime.Minute },
                { "return_location",  additionalInformation.APIReturnLocationCode },
                { "return_date",  additionalInformation.ReturnDateTime.ToString("dd.MM.yyyy") },
                { "return_hour",  additionalInformation.ReturnDateTime.Hour },
                { "return_min",  additionalInformation.ReturnDateTime.Minute },
                { "car_id",  reservationToken.APIReferenceCode },
                { "name",  postReservationRequest.CustomerName },
                { "surname", postReservationRequest.CustomerSurname},
                { "email",  postReservationRequest.CustomerEmail },
                { "tel",  postReservationRequest.CustomerTelephone },
                { "airline",  string.Empty},
                { "flight_no",  postReservationRequest.FlightNumberArrival },
                { "reservasyon_kodu", reservationNumber},
                { "ulke", string.Empty },
                { "tc_pass",  postReservationRequest.CustomerPersonalNumber },
                { "adres",   postReservationRequest.CustomerAddress },
                { "mesaj",  postReservationRequest.CustomerNote },
                { "bebek_koltuk",  selectedExtraCodes != null ? selectedExtraCodes.Contains("bebek_koltuk") ? "Evet" : "Hayır" : "Hayır" },
                { "bebek_koltuk_adet",  selectedExtraCodes != null ? selectedExtraCodes.Contains("bebek_koltuk") ? "1" : "0" : "0"},
                { "lcf_sigorta",  selectedExtraCodes != null ? selectedExtraCodes.Contains("lcf_sigorta") ? "Evet" : "Hayır" : "Hayır" },
                { "lcf_sigorta_adet", selectedExtraCodes != null ? selectedExtraCodes.Contains("lcf_sigorta") ? "1" : "0" : "0" },
                { "ek_sofor", selectedExtraCodes != null ? selectedExtraCodes.Contains("ek_sofor") ? "Evet" : "Hayır" : "Hayır" },
                { "ek_sofor_adet", selectedExtraCodes != null ? selectedExtraCodes.Contains("ek_sofor") ? "1" : "0" : "0" },
                { "navigasyon", selectedExtraCodes != null ? selectedExtraCodes.Contains("navigasyon") ? "Evet" : "Hayır" : "Hayır" },
                { "navigasyon_adet",  selectedExtraCodes != null ? selectedExtraCodes.Contains("navigasyon") ? "1" : "0" : "0" },
                { "superkasko", selectedExtraCodes != null ? selectedExtraCodes.Contains("superkasko") ? "Evet" : "Hayır" : "Hayır" },
                { "superkasko_adet",  selectedExtraCodes != null ? selectedExtraCodes.Contains("superkasko") ? "1" : "0" : "0"},
                { "ekkm500", selectedExtraCodes != null ? selectedExtraCodes.Contains("ekkm500") ? "Evet" : "Hayır" : "Hayır" },
                { "ekkm500_adet",  selectedExtraCodes != null ? selectedExtraCodes.Contains("ekkm500_adet") ? "1" : "0" : "0"},
                { "ekkm1000", selectedExtraCodes != null ? selectedExtraCodes.Contains("ekkm1000") ? "Evet" : "Hayır" : "Hayır" },
                { "ekkm1000_adet",  selectedExtraCodes != null ? selectedExtraCodes.Contains("ekkm1000_adet") ? "1" : "0" : "0"},
                { "kull", additionalInformation.Vendor.ApiKey },
                { "sifre",  additionalInformation.Vendor.ApiPassword },
                { "full_credit",  false },
                { "total_price_tl", totalPrice},
                { "teslimat_tl", deliveryPrice},
            };
        }
    }
}
