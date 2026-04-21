using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avec2Helper = KolayCAR.Broker.API.Helpers.Avec;
using Avec2Provider = KolayCAR.Broker.API.Providers.Avec2;

namespace KolayCAR.Broker.API.Providers.Avec2
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager HttpManager { get; set; }
        ILocationProvider LocationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            HttpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            LocationProvider = new Avec2Provider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = localReservation.APIPaidAmount; //CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postReservationRequestParameters = PostReservationRequestParameters(postReservationRequest, additionalInformation, reservationNumber, reservationToken, apiPaidAmount);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@AvecPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await HttpManager.GetXmlAsync<Avec2ResponseBase>(
            requestPath: "xml_rez_Save.asp",
            parameters: postReservationRequestParameters,
            brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationVendorAPIResponse
            }, isReservationRequest: true);

            Serilog.Log.Error("{@AvecPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result != null && result.rez_Result != null && result.rez_Result.Reservation != null && result.rez_Result.Reservation.Result_)
            {
                localReservation.APIReservationSuccessfully = result != null && result.rez_Result.Reservation.Result_;
                localReservation.APIReservationNumber = result.rez_Result.Reservation.ID.ToStringNullSafe();

                var location = await LocationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@AvecGetLocationsResponse}", location);
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
                    Success = localReservation != null && result != null && result.rez_Result.Reservation.Result_,
                    Data = localReservation
                };
            }
            else if (result != null && result.rez_Result != null && result.rez_Result.Reservation != null && !string.IsNullOrEmpty(result.rez_Result.Reservation.False_Desc))
            {
                Serilog.Log.Error("{AvecReservationError}", result.rez_Result.Reservation.False_Desc);
                localReservation.APIMessage = result.rez_Result.Reservation.False_Desc;
            }

            Serilog.Log.Error("Avec servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Avec servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Avec servisinden herhangi bir veri alınamadı!"
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

            Serilog.Log.Error("{@AvecPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await HttpManager.GetXmlAsync<Avec2ResponseBase>(
            requestPath: "xml_rez_Cancel.Asp",
            parameters: postCancelReservationsRequestParameters,
            brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
            }, isReservationRequest: true);

            Serilog.Log.Error("{@AvecPostCancelReservationsResponse}", result);

            if (result != null && result.rez_Sonuc.Reservation.Result_)
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
                Message = "Avec servisi rezervasyon iptali başarısız!",
            };
        }

        private Dictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                 : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);

            return new Dictionary<string, object>()
            {
                { "Rez_No", reservationNumber},
                { "Car_ID", reservationToken.APIReferenceCode2},
                { "Rez_ID", reservationToken.APIReferenceCode},
                { "Currency", Avec2Helper.CurrencyHelper.GetLongCurrencyType(reservationToken.BaseVendorRequestCurrencyType)},
                { "Name", postReservationRequest.CustomerName},
                { "Sur_Name", postReservationRequest.CustomerSurname},
                { "Mobile", postReservationRequest.CustomerTelephone},
                { "Mail_Adress", postReservationRequest.CustomerEmail},
                { "Flight_Number", $"{postReservationRequest.FlightNumberArrival} - {postReservationRequest.DepartureInfo}"},
                { "Pickup_ID", additionalInformation.APIPickupLocationCode},
                { "Pickup_Day", additionalInformation.PickupDateTime.Day},
                { "Pickup_Month", additionalInformation.PickupDateTime.Month},
                { "Pickup_Year", additionalInformation.PickupDateTime.Year},
                { "Pickup_Hour", additionalInformation.PickupDateTime.Hour},
                { "Pickup_Min", additionalInformation.PickupDateTime.Minute},
                { "Drop_Off_ID", additionalInformation.APIReturnLocationCode},
                { "Drop_Off_Day", additionalInformation.ReturnDateTime.Day},
                { "Drop_Off_Month", additionalInformation.ReturnDateTime.Month},
                { "Drop_Off_Year", additionalInformation.ReturnDateTime.Year},
                { "Drop_Off_Hour", additionalInformation.ReturnDateTime.Hour},
                { "Drop_Off_Min", additionalInformation.ReturnDateTime.Minute},
                { "Payment", paidAmount},
                { "Private_Driver", selectedExtraCodes != null ? selectedExtraCodes.Contains("Private_Driver") ? "ON" : "" : ""},
                { "Baby_Seat", selectedExtraCodes != null ? selectedExtraCodes.Contains("Baby_Seat") ? "ON" : "": ""},
                { "Child_Seat", selectedExtraCodes != null ?selectedExtraCodes.Contains("Child_Seat") ? "ON" : "": ""},
                { "Navigation", selectedExtraCodes != null ?selectedExtraCodes.Contains("Navigation") ? "ON" : "": ""},
                { "Young_Drive", selectedExtraCodes != null ?selectedExtraCodes.Contains("Young_Drive") ? "ON" : "": ""},
                { "SCDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("SCDW") ? "ON" : "": ""},
                { "Additional_Driver",selectedExtraCodes != null ? selectedExtraCodes.Contains("Additional_Driver") ? "ON" : "": ""},
                { "Additional_KM",selectedExtraCodes != null ? selectedExtraCodes.Contains("Additional_KM") ? "ON" : "": ""},
                { "CDW",selectedExtraCodes != null ? selectedExtraCodes.Contains("CDW") ? "ON" : "": ""},
                { "PAI",selectedExtraCodes != null ? selectedExtraCodes.Contains("PAI") ? "ON" : "": ""},
                { "TGI",selectedExtraCodes != null ? selectedExtraCodes.Contains("TGI") ? "ON" : "": ""},
                { "KM_150",selectedExtraCodes != null ? selectedExtraCodes.Contains("KM_150") ? "ON" : "": ""},
                { "KM_400",selectedExtraCodes != null ? selectedExtraCodes.Contains("KM_400") ? "ON" : "": ""},
                { "Tablet_Navigation",selectedExtraCodes != null ? selectedExtraCodes.Contains("Tablet_Navigation") ? "ON" : "": ""},
                { "User_Name", additionalInformation.Vendor.ApiKey},
                { "User_Pass", additionalInformation.Vendor.ApiPassword}
            };
        }

        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "Car_ID", reservation.VehicleId},
                { "Rez_ID", reservation.APIReferenceCode},
                { "User_Name", vendor.ApiKey},
                { "User_Pass", vendor.ApiPassword}
            };
    }
}
