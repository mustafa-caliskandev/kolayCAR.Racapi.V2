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
using EkarProvider = KolayCAR.Broker.API.Providers.Ekar;

namespace KolayCAR.Broker.API.Providers.Ekar
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager HttpManager { get; set; }
        ILocationProvider LocationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            HttpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            LocationProvider = new EkarProvider.LocationProvider(apiBaseUrl);
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

            Serilog.Log.Error("{@EkarPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await HttpManager.GetXmlAsync<EkarResponseBase>(
                requestPath: "XML_Rez_Kaydet.Asp",
                parameters: postReservationRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@EkarPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result != null && result.EkarSistemrent != null && result.EkarSistemrent.Rezervasyon != null && !string.IsNullOrEmpty(result.EkarSistemrent.Rezervasyon.Kayit_No))
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.EkarSistemrent.Rezervasyon.Kayit_No;

                var location = await LocationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@EkarGetLocationsResponse}", location);
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
                    Success = false,
                    Data = localReservation
                };
            }

            Serilog.Log.Error("Ekar servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Ekar servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Ekar servisinden herhangi bir veri alınamadı!"
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

            Serilog.Log.Error("{@EkarPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await HttpManager.GetXmlAsync<EkarResponseBase>(
                requestPath: "XML_Rez_Cancel.Asp",
                parameters: postCancelReservationsRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@EkarPostCancelReservationsResponse}", result);

            if (result != null && result.EkarSistemrent != null && result.EkarSistemrent.Rezervation != null && !string.IsNullOrEmpty(result.EkarSistemrent.Rezervation.ID))
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
                Message = "Ekar servisi rezervasyon iptali başarısız!",
            };
        }

        private Dictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                   : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);

            return new Dictionary<string, object>()
            {
                { "Rez_Key_No", reservationToken.APIReferenceCode},
                { "Grup", reservationToken.APIReferenceCode2},
                { "Kiralayan_Ad", postReservationRequest.CustomerName},
                { "Kiralayan_Soyad", postReservationRequest.CustomerSurname},
                { "Kiralayan_Cep", postReservationRequest.CustomerTelephone},
                { "Kiralayan_Mail", postReservationRequest.CustomerEmail},
                { "Kiralayan_TC", !string.IsNullOrEmpty(postReservationRequest.CustomerPersonalNumber) ? postReservationRequest.CustomerPersonalNumber : "11111111111" },
                { "Sube_Kodu", additionalInformation.APIPickupLocationCode},
                { "Bas_Gun", StringHelper.GetTwoCharacterDateItem(additionalInformation.PickupDateTime.Day) },
                { "Bas_Ay", StringHelper.GetTwoCharacterDateItem(additionalInformation.PickupDateTime.Month) },
                { "Bas_Yil", StringHelper.GetTwoCharacterDateItem(additionalInformation.PickupDateTime.Year) },
                { "Bas_Saat", StringHelper.GetTwoCharacterDateItem(additionalInformation.PickupDateTime.Hour) },
                { "Bas_Dakika", StringHelper.GetTwoCharacterDateItem(additionalInformation.PickupDateTime.Minute) },
                { "Donus_Sube_Kodu",  additionalInformation.APIReturnLocationCode},
                { "Bit_Gun", StringHelper.GetTwoCharacterDateItem(additionalInformation.ReturnDateTime.Day) },
                { "Bit_Ay", StringHelper.GetTwoCharacterDateItem(additionalInformation.ReturnDateTime.Month) },
                { "Bit_Yil", StringHelper.GetTwoCharacterDateItem(additionalInformation.ReturnDateTime.Year) },
                { "Bit_Saat", StringHelper.GetTwoCharacterDateItem(additionalInformation.ReturnDateTime.Hour) },
                { "Bit_Dakika", StringHelper.GetTwoCharacterDateItem(additionalInformation.ReturnDateTime.Minute) },
                { "CDW_Hesap_Fiyat", selectedExtraCodes != null ? selectedExtraCodes.Contains("CDW_Hesap_Fiyat") ? "ON" : "" : ""},
                { "SCDW_Hesap_Fiyat", selectedExtraCodes != null ? selectedExtraCodes.Contains("SCDW_Hesap_Fiyat") ? "ON" : "": ""},
                { "LCF_Hesap_Fiyat", selectedExtraCodes != null ?selectedExtraCodes.Contains("LCF_Hesap_Fiyat") ? "ON" : "": ""},
                { "PAI_Hesap_Fiyat",  selectedExtraCodes != null ?selectedExtraCodes.Contains("PAI_Hesap_Fiyat") ? "ON" : "": ""},
                { "Navigasyon_Hesap_Fiyat", selectedExtraCodes != null ? selectedExtraCodes.Contains("Navigasyon_Hesap_Fiyat") ? "ON" : "": ""},
                { "Ek_Surucu_Hesap_Fiyat",  selectedExtraCodes != null ?selectedExtraCodes.Contains("Ek_Surucu_Hesap_Fiyat") ? "ON" : "": ""},
                { "Bebek_Koltuk_Hesap_Fiyat", selectedExtraCodes != null ? selectedExtraCodes.Contains("Bebek_Koltuk_Hesap_Fiyat") ? "ON" : "": ""},
                { "User_Name", additionalInformation.Vendor.ApiKey},
                { "User_Pass", additionalInformation.Vendor.ApiPassword},
                { "Key_Hack", additionalInformation.Vendor.ApiClientId}
            };
        }

        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "Key_Hack", vendor.ApiClientId },
                { "Rez_Key_No", reservation.APIReferenceCode},
                { "ID", reservation.APIReferenceCode2},
                { "User_Name",  vendor.ApiKey},
                { "User_Pass",  vendor.ApiPassword}
            };
    }
}
