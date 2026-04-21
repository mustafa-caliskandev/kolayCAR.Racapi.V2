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
using HaraProvider = KolayCAR.Broker.API.Providers.Hara;

namespace KolayCAR.Broker.API.Providers.Hara
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager HttpManager { get; set; }
        ILocationProvider LocationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            HttpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            LocationProvider = new HaraProvider.LocationProvider(apiBaseUrl);
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

            Serilog.Log.Error("{@HaraPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await HttpManager.GetXmlAsync<HaraResponseBase>(
                requestPath: "xml_rez_save.Asp",
                parameters: postReservationRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@HaraPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result != null && result.Rez_Result != null && result.Rez_Result.Reservation != null && result.Rez_Result.Reservation.Result_)
            {
                localReservation.APIReservationSuccessfully = result != null && result.Rez_Result.Reservation.Result_;
                localReservation.APIReservationNumber = result.Rez_Result.Reservation.ID;

                var location = await LocationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@HaraGetLocationsResponse}", location);
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
                    Success = localReservation != null && result != null && result.Rez_Result.Reservation.Result_,
                    Data = localReservation
                };
            }
            else if (result != null && result.Rez_Result != null && result.Rez_Result.Reservation != null && !string.IsNullOrEmpty(result.Rez_Result.Reservation.False_Desc))
            {
                Serilog.Log.Error("{HaraReservationError}", result.Rez_Result.Reservation.False_Desc);
                localReservation.APIMessage = result.Rez_Result.Reservation.False_Desc;
            }

            Serilog.Log.Error("Hara servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Hara servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Hara servisinden herhangi bir veri alınamadı!"
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

            Serilog.Log.Error("{@HaraPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await HttpManager.GetXmlAsync<HaraResponseBase>(
                requestPath: "xml_rez_Cancel.Asp",
                parameters: postCancelReservationsRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }, isReservationRequest: true);

            Serilog.Log.Error("{@HaraPostCancelReservationsResponse}", result);

            if (result != null && result.Rez_Sonuc.Reservation.Result_)
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
                Message = "Hara servisi rezervasyon iptali başarısız!",
            };
        }

        private Dictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                 : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);

            var parameters = new Dictionary<string, object>()
            {
                { "Car_ID", reservationToken.APIReferenceCode2},
                { "Rez_ID", reservationNumber},
                { "Currency", AvecHelper.CurrencyHelper.GetLongCurrencyType(reservationToken.BaseVendorRequestCurrencyType) },
                { "Name", postReservationRequest.CustomerName},
                { "Sur_Name", postReservationRequest.CustomerSurname},
                { "Mobile", postReservationRequest.CustomerTelephone},
                { "Mail_Adress", postReservationRequest.CustomerEmail},
                { "Flight_Number", $"{postReservationRequest.FlightNumberArrival} - {postReservationRequest.DepartureInfo}" },
                { "Pickup_ID", additionalInformation.APIPickupLocationCode},
                { "Pickup_Day",  additionalInformation.PickupDateTime.Day},
                { "Pickup_Month",  additionalInformation.PickupDateTime.Month},
                { "Pickup_Year",  additionalInformation.PickupDateTime.Year},
                { "Pickup_Hour",  additionalInformation.PickupDateTime.Hour},
                { "Pickup_Min",  additionalInformation.PickupDateTime.Minute},
                { "Drop_Off_ID",  additionalInformation.APIReturnLocationCode},
                { "Drop_Off_Day",  additionalInformation.ReturnDateTime.Day},
                { "Drop_Off_Month",  additionalInformation.ReturnDateTime.Month},
                { "Drop_Off_Year",  additionalInformation.ReturnDateTime.Year},
                { "Drop_Off_Hour",  additionalInformation.ReturnDateTime.Hour},
                { "Drop_Off_Min",  additionalInformation.ReturnDateTime.Minute},
                //{ "Private_Driver", selectedExtraCodes != null ? selectedExtraCodes.Contains("Private_Driver") ? "ON" : "OFF" : "OFF"},
                //{ "Baby_Seat", selectedExtraCodes != null ? selectedExtraCodes.Contains("Baby_Seat") ? "ON" : "OFF": "OFF"},
                //{ "Child_Seat", selectedExtraCodes != null ?selectedExtraCodes.Contains("Child_Seat") ? "ON" : "OFF": "OFF"},
                //{ "Navigation",  selectedExtraCodes != null ?selectedExtraCodes.Contains("Navigation") ? "ON" : "OFF": "OFF"},
                //{ "TGI", selectedExtraCodes != null ? selectedExtraCodes.Contains("TGI") ? "ON" : "OFF": "OFF"},
                //{ "CDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("CDW") ? "ON" : "OFF": "OFF"},
                //{ "SCDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("SCDW") ? "ON" : "OFF": "OFF"},
                //{ "PAI", selectedExtraCodes != null ? selectedExtraCodes.Contains("PAI") ? "ON" : "OFF": "OFF"},
                //{ "Additional_Driver",selectedExtraCodes != null ? selectedExtraCodes.Contains("Additional_Driver") ? "ON" : "OFF": "OFF"},
                { "User_Name",  additionalInformation.Vendor.ApiKey},
                { "User_Pass",  additionalInformation.Vendor.ApiPassword}
            };

            if (selectedExtraCodes != null && selectedExtraCodes.Count > 0)
                foreach (var extraCode in selectedExtraCodes)
                    parameters.Add(extraCode, "ON");

            return parameters;
        }

        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "Car_ID", reservation.APIReferenceCode2},
                { "Rez_ID", reservation.ReservationNumber},
                { "User_Name",  vendor.ApiKey},
                { "User_Pass",  vendor.ApiPassword}
            };
    }
}
