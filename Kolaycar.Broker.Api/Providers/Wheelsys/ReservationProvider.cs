using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Wheelsys
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager _httpManager { get; set; }
        ILocationProvider _locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _locationProvider = new LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
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

            Serilog.Log.Error("{@" + vendor.VendorName + "PostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await _httpManager.GetXmlAsync<WheelsysResponseBase.Root>(
               requestPath: $"{vendor.ApiKey}/link/v3/cancel-res_{vendor.ApiPassword.Split('-')[0]}.html",
               parameters: postCancelReservationsRequestParameters,
               brokerLogModel: new BrokerLogModel
               {
                   LogKey = localReservation.ReservationNumber,
                   LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
               },
               isReservationRequest: true);

            Serilog.Log.Error("{@" + vendor.VendorName + "PostCancelReservationsResponse}", result);

            if (result != null && result.response != null && result.response.reservation != null && result.response.reservation.status == "OK")
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
                Message = $"{vendor.VendorName} servisi rezervasyon iptali başarısız!",
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postReservationsRequestParameters = PostReservationsRequestParameters(vendor, postReservationRequest, additionalInformation, reservationNumber, reservationToken, apiPaidAmount);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationsRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@" + vendor.VendorName + "PostReservationRequestParameters}", postReservationsRequestParameters);

            var result = await _httpManager.GetXmlAsync<WheelsysResponseBase.Root>(
            requestPath: $"{vendor.ApiKey}/link/v3/new-res_{vendor.ApiPassword.Split('-')[0]}.html",
            parameters: postReservationsRequestParameters,
            brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationVendorAPIResponse
            }, isReservationRequest: true);
            Serilog.Log.Error("{@" + vendor.VendorName + "PostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;
            localReservation.APIMessage = result?.response?.reservation?.status;
            if (result != null && result.response != null && result.response.reservation != null && (result.response.reservation.status == "REZ" || result.response.reservation.status == "OK"))
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.response.reservation.irn;

                //var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);

                //var reservationLocation = new List<Domain.Models.Location>();
                //if (location.Success)
                //    reservationLocation = location.Data as List<Domain.Models.Location>;

                //var reservationPickupLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIPickupLocationCode).FirstOrDefault();
                //var reservationReturnLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIReturnLocationCode).FirstOrDefault();

                //localReservation.APIVendorPickupAddress = reservationPickupLocation.Address;
                //localReservation.APIVendorReturnAddress = reservationReturnLocation.Address;
                //localReservation.APIVendorPickupPhone = reservationPickupLocation.PhoneNumber;
                //localReservation.APIVendorReturnPhone = reservationReturnLocation.PhoneNumber;

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
                Message = $"{vendor.VendorName} servisi rezervasyonu başarısız!",
            };
        }
        private Dictionary<string, object> PostReservationsRequestParameters(Vendor vendor, PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList) : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);

            var parameters = new Dictionary<string, object>()
            {
                { "AGENT",  vendor.ApiPassword.Split('-')[1] },
                { "DATE_FROM", additionalInformation.PickupDateTime.ToString("dd/MM/yyyy").Replace('.','/')},
                { "TIME_FROM", additionalInformation.PickupDateTime.ToString("HH:mm") },
                { "DATE_TO", additionalInformation.ReturnDateTime.ToString("dd/MM/yyyy").Replace('.','/') },
                { "TIME_TO", additionalInformation.ReturnDateTime.ToString("HH:mm") },
                { "PICKUP_STATION", reservationToken.APIPickupLocationCode },
                { "RETURN_STATION", reservationToken.APIReturnLocationCode },
                { "GROUP", reservationToken.VehicleCode }, //TODO: Vehiclecode geliştirmesi yapılacak!
                { "CUSTOMER_NAME", $"{postReservationRequest.CustomerName} {postReservationRequest.CustomerSurname}" },
                { "CUSTOMER_EMAIL", postReservationRequest.CustomerEmail },
                { "CUSTOMER_PHONE", postReservationRequest.CustomerTelephone },
                { "VOUCHERNO", reservationNumber },
                { "CUSTOMER_BIRTHDATE", postReservationRequest.CustomerBirthDay.Replace('.','/') },
                { "REMARKS", postReservationRequest.FlightNumberArrival + "-" + postReservationRequest.CustomerNote },
                { "CUSTOMER_IDENTITYNUMBER", postReservationRequest.CustomerPersonalNumber },
                { "CUSTCOUNTRY", (Regex.Matches(postReservationRequest.CustomerPersonalNumber, @"[a-zA-Z]").Count > 0 || postReservationRequest.CustomerPersonalNumber.Length != 11) == true ? "UK" : "TR"}


            };

            if (selectedExtraCodes != null)
                foreach (var extraCode in selectedExtraCodes)
                    parameters.Add(extraCode, "1");

            return parameters;
        }
        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
          new Dictionary<string, object>()
         {
                { "AGENT",  vendor.ApiPassword.Split('-')[1] },
                { "IRN",  reservation.APIReservationNumber},
                { "REFNO",  reservation.ReservationNumber},
         };
    }
}
