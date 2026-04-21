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
using AvecHelper = KolayCAR.Broker.API.Helpers.Avec;
using RigorentProvider = KolayCAR.Broker.API.Providers.Rigorent;

namespace KolayCAR.Broker.API.Providers.Rigorent
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager HttpManager { get; set; }
        ILocationProvider locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            HttpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            locationProvider = new RigorentProvider.LocationProvider(apiBaseUrl);
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

            Serilog.Log.Error("{@RigorentPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await HttpManager.GetXmlAsync<RigorentResponseBase>(
                requestPath: "xml/xml_sonuc.Asp",
                parameters: postReservationRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                }, isReservationRequest: true);

            Serilog.Log.Error("{@RigorentPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result != null && result.Rez_Sonuc != null && result.Rez_Sonuc.Rezervasyon != null && result.Rez_Sonuc.Rezervasyon.Sonuc)
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = reservationToken.APIReferenceCode;

                var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@RigorentGetLocationsResponse}", location);
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
                    Success = localReservation != null && result != null && result.Rez_Sonuc.Rezervasyon.Sonuc,
                    Data = localReservation
                };
            }

            Serilog.Log.Error("Rigorent servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Rigorent servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Rigorent servisinden herhangi bir veri alınamadı!"
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

            Serilog.Log.Error("{@RigorentPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await HttpManager.GetXmlAsync<RigorentResponseBase>(
                requestPath: "xml/xml_rez_Iptal.Asp",
                parameters: postCancelReservationsRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }, isReservationRequest: true);

            Serilog.Log.Error("{@RigorentPostCancelReservationsResponse}", result);

            if (result != null && result.Rez_Sonuc.Rezervasyon.Sonuc)
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
                Message = "Rigorent servisi rezervasyon iptali başarısız!",
            };
        }

        private Dictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                 : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);

            return new Dictionary<string, object>()
            {
                { "Arac_id", reservationToken.VehicleCode },
                { "Rez_ID", additionalInformation.APIReferenceCode },
                { "Doviz", AvecHelper.CurrencyHelper.GetLongCurrencyType(reservationToken.BaseVendorRequestCurrencyType) },
                { "Kur", reservationToken.APIReferenceCode2 },
                { "Kiralayan_Ad", postReservationRequest.CustomerName },
                { "Kiralayan_Soyad", postReservationRequest.CustomerSurname },
                { "Kiralayan_Cep", postReservationRequest.CustomerTelephone },
                { "Kiralayan_Mail", postReservationRequest.CustomerEmail },
                { "Kiralayan_Ucus", $"{postReservationRequest.FlightNumberArrival} - {postReservationRequest.DepartureInfo}" },
                { "Cikis", additionalInformation.APIPickupLocationCode },
                { "Al_Gun",  additionalInformation.PickupDateTime.Day },
                { "Al_Ay",  additionalInformation.PickupDateTime.Month },
                { "Al_Yil",  additionalInformation.PickupDateTime.Year },
                { "Al_Saat",  additionalInformation.PickupDateTime.Hour },
                { "Al_Dakika",  additionalInformation.PickupDateTime.Minute },
                { "Donus",  additionalInformation.APIReturnLocationCode },
                { "Iade_Gun",  additionalInformation.ReturnDateTime.Day },
                { "Iade_Ay",  additionalInformation.ReturnDateTime.Month },
                { "Iade_Yil",  additionalInformation.ReturnDateTime.Year },
                { "Iade_Saat",  additionalInformation.ReturnDateTime.Hour },
                { "Iade_Dakika",  additionalInformation.ReturnDateTime.Minute },
                { "B_Koltuk", selectedExtraCodes != null ? selectedExtraCodes.Contains("B_Koltuk") ? "ON" : "" : "" },
                { "Navigasyon", selectedExtraCodes != null ? selectedExtraCodes.Contains("Navigasyon") ? "ON" : "": "" },
                { "User_Name",  additionalInformation.Vendor.ApiKey },
                { "User_Pass",  additionalInformation.Vendor.ApiPassword }
            };
        }

        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "Arac_id", reservation.VehicleId },
                { "Rez_ID", reservation.APIReservationNumber },
                { "User_Name",  vendor.ApiKey },
                { "User_Pass",  vendor.ApiPassword }
            };
    }
}
