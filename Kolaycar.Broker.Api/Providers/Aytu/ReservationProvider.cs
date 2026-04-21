using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Akkor;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AkkorProvider = KolayCAR.Broker.API.Providers.Akkor;
using AvecHelper = KolayCAR.Broker.API.Helpers.Avec;

namespace KolayCAR.Broker.API.Providers.Aytu
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager HttpManager { get; set; }
        ILocationProvider locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            HttpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            locationProvider = new AkkorProvider.LocationProvider(apiBaseUrl);
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

            Serilog.Log.Error("{@AkkorPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await HttpManager.GetXmlAsync<AkkorResponseBase>(
                requestPath: "XML_Rez_Save.Asp",
                parameters: postReservationRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@AkkorPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result != null && result.Sistemrent != null && result.Sistemrent.AkkorReservation != null && !string.IsNullOrEmpty(result.Sistemrent.AkkorReservation.ID))
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Sistemrent.AkkorReservation.ID;

                var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@AkkorGetLocationsResponse}", location.Data);
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

            Serilog.Log.Error("Akkor servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Akkor servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Akkor servisinden herhangi bir veri alınamadı!"
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

            Serilog.Log.Error("{@AkkorPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await HttpManager.GetXmlAsync<AkkorResponseBase>(
                requestPath: "XML_Rez_Cancel.Asp",
                parameters: postCancelReservationsRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }, isReservationRequest: true);

            Serilog.Log.Error("{@AkkorPostCancelReservationsResponse}", result);

            if (result != null && result.Sistemrent.AkkorReservation.Status)
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
                Message = "Akkor servisi rezervasyon iptali başarısız!",
            };
        }

        private Dictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                  : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);
            return new Dictionary<string, object>()
            {
                { "Key_Hack",  additionalInformation.Vendor.ApiClientId},
                { "User_Name",  additionalInformation.Vendor.ApiKey},
                { "User_Pass",  additionalInformation.Vendor.ApiPassword},
                { "Name", postReservationRequest.CustomerName},
                { "SurName", postReservationRequest.CustomerSurname},
                { "MobilePhone", postReservationRequest.CustomerTelephone},
                { "Mail_Adress", postReservationRequest.CustomerEmail},
                { "Rental_ID", postReservationRequest.CustomerPersonalNumber},
                { "Cars_Park_ID", reservationToken.APIReferenceCode2},
                { "Group_ID", reservationToken.VehicleCode},
                { "Rez_ID", reservationNumber},
                { "Pickup_ID", additionalInformation.APIPickupLocationCode},
                { "Pickup_Day",  additionalInformation.PickupDateTime.ToString("dd") },
                { "Pickup_Month",  additionalInformation.PickupDateTime.ToString("MM") },
                { "Pickup_Year",  additionalInformation.PickupDateTime.ToString("yyyy") },
                { "Pickup_Hour",  additionalInformation.PickupDateTime.ToString("HH") },
                { "Pickup_Min",  additionalInformation.PickupDateTime.ToString("mm") },
                { "Drop_Off_ID",  additionalInformation.APIReturnLocationCode},
                { "Drop_Off_Day",  additionalInformation.ReturnDateTime.ToString("dd") },
                { "Drop_Off_Month",  additionalInformation.ReturnDateTime.ToString("MM") },
                { "Drop_Off_Year",  additionalInformation.ReturnDateTime.ToString("yyyy") },
                { "Drop_Off_Hour",  additionalInformation.ReturnDateTime.ToString("HH") },
                { "Drop_Off_Min",  additionalInformation.ReturnDateTime.ToString("mm") },

                { "Adress", postReservationRequest.CustomerAddress},
                { "District", string.Empty},
                { "City", string.Empty},
                { "Country", string.Empty},
                { "Flight_Number", $"{postReservationRequest.FlightNumberArrival}" },
                { "Description", postReservationRequest.CustomerNote },

                { "Currency", AvecHelper.CurrencyHelper.GetLongCurrencyType(reservationToken.BaseVendorRequestCurrencyType)},
                { "Payment", additionalInformation.Vendor.SecretKey == VendorTypes.Akkor.ToString() ? paidAmount : 0},

                { "CDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("CDW") ? "ON" : "" : ""},
                { "SCDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("SCDW") ? "ON" : "": ""},
                { "LCF", selectedExtraCodes != null ?selectedExtraCodes.Contains("LCF") ? "ON" : "": ""},
                { "PAI",  selectedExtraCodes != null ?selectedExtraCodes.Contains("PAI") ? "ON" : "": ""},
                { "Baby_Seat", selectedExtraCodes != null ? selectedExtraCodes.Contains("Baby_Seat") ? "ON" : "": ""},
                { "Navigation",  selectedExtraCodes != null ?selectedExtraCodes.Contains("Navigation") ? "ON" : "": ""},
                { "Additional_Driver", selectedExtraCodes != null ? selectedExtraCodes.Contains("Additional_Driver") ? "ON" : "": ""},
            };
        }

        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "Key_Hack", vendor.ApiClientId},
                { "Rez_ID", reservation.ReservationNumber},
                { "ID",  reservation.APIReservationNumber},
                { "User_Name",  vendor.ApiKey},
                { "User_Pass",  vendor.ApiPassword},
            };
    }
}
