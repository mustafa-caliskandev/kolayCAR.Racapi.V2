using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EuropcarProvider = KolayCAR.Broker.API.Providers.Europcar;

namespace KolayCAR.Broker.API.Providers.Europcar
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager HttpManager { get; set; }
        ILocationProvider locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            HttpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            locationProvider = new EuropcarProvider.LocationProvider(apiBaseUrl);
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

            Serilog.Log.Error("{@EuropcarPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            //var result = await HttpManager.GetXmlAsync<EuropcarResponseBase>(
            //    requestPath: $"{vendor.ApiKey.Split('|')[0]}/link/v3/cancel-res_{vendor.ApiPassword.Split('-')[0]}.html",
            //    parameters: postCancelReservationsRequestParameters,
            //    brokerLogModel: new BrokerLogModel
            //    {
            //        LogKey = localReservation.ReservationNumber,
            //        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
            //    });
            //var result = await HttpManager.GetXmlAsync<EuropcarResponseBase>(
            //   requestPath: $"{vendor.ApiKey.Split('|')[0]}/link/v3/cancel_{vendor.ApiPassword.Split('-')[0]}.html",
            //   parameters: postCancelReservationsRequestParameters,
            //   brokerLogModel: new BrokerLogModel
            //   {
            //       LogKey = localReservation.ReservationNumber,
            //       LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
            //   });
            var result = await HttpManager.GetXmlAsync<EuropcarResponseBase>(
               requestPath: $"/{vendor.ApiClientId}/link/v3/cancel_{vendor.ApiKey}.html",
               parameters: postCancelReservationsRequestParameters,
               brokerLogModel: new BrokerLogModel
               {
                   LogKey = localReservation.ReservationNumber,
                   LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
               }, isReservationRequest: true);

            Serilog.Log.Error("{@EuropcarPostCancelReservationsResponse}", result);

            if (result != null && result.response != null && result.response.reservation != null && result.response.reservation.status == "CNC")
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
                Message = "Europcar servisi rezervasyon iptali başarısız!",
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

            Serilog.Log.Error("{@EuropcarPostReservationRequestParameters}", postReservationsRequestParameters);

            //var result = await HttpManager.GetXmlAsync<EuropcarResponseBase>(
            //    requestPath: $"{vendor.ApiKey.Split('|')[0]}/link/v3/new-res_{vendor.ApiPassword.Split('-')[0]}.html",
            //    parameters: postReservationsRequestParameters,
            //    brokerLogModel: new BrokerLogModel
            //    {
            //        LogKey = localReservation.ReservationNumber,
            //        LogType = BrokerLogTypes.ReservationVendorAPIResponse
            //    });
            //var result = await HttpManager.GetXmlAsync<EuropcarResponseBase>(
            //requestPath: $"{vendor.ApiKey.Split('|')[0]}/link/v3/new-res_{vendor.ApiPassword.Split('-')[0]}.html",
            //parameters: postReservationsRequestParameters,
            //brokerLogModel: new BrokerLogModel
            //{
            //    LogKey = localReservation.ReservationNumber,
            //    LogType = BrokerLogTypes.ReservationVendorAPIResponse
            //});
            var result = await HttpManager.GetXmlAsync<EuropcarResponseBase>(
            requestPath: $"/{vendor.ApiClientId}/link/v3/new-res_{vendor.ApiKey}.html",
            parameters: postReservationsRequestParameters,
            brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationVendorAPIResponse
            },
            isReservationRequest: true);

            Serilog.Log.Error("{@EuropcarPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;
            localReservation.APIMessage = result?.response?.reservation?.status;
            if (result != null && result.response != null && result.response.reservation != null && result.response.reservation.status == "REZ")
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
                Message = "Europcar servisi rezervasyonu başarısız!",
            };
        }

        private Dictionary<string, object> PostReservationsRequestParameters(Vendor vendor, PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                  : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);

            //var parameters = new Dictionary<string, object>()
            //{
            //    { "AGENT",  vendor.ApiPassword.Split('-')[1] },
            //    { "DATE_FROM", additionalInformation.PickupDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) },
            //    { "TIME_FROM", additionalInformation.PickupDateTime.ToString("HH:mm") },
            //    { "DATE_TO", additionalInformation.ReturnDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) },
            //    { "TIME_TO", additionalInformation.ReturnDateTime.ToString("HH:mm") },
            //    { "PICKUP_STATION", reservationToken.APIPickupLocationCode },
            //    { "RETURN_STATION", reservationToken.APIReturnLocationCode },
            //    { "PICKUP_INFO", "" },
            //    { "RETURN_INFO", "" },
            //    { "GROUP", reservationToken.VehicleCode }, //TODO: Vehiclecode geliştirmesi yapılacak!
            //    { "CUSTOMER_NAME", $"{postReservationRequest.CustomerName} {postReservationRequest.CustomerSurname}" },
            //    { "CUSTOMER_EMAIL", postReservationRequest.CustomerEmail },
            //    { "CUSTOMER_PHONE", postReservationRequest.CustomerTelephone },
            //    { "VOUCHERNO", reservationNumber },
            //    { "QUOTEREF", "" },
            //    { "PICKUP_POINT", "" },
            //    { "DROPOFF_POINT", "" },
            //    { "REMARKS", postReservationRequest.FlightNumberArrival + "-" + postReservationRequest.CustomerNote },
            //    { "CDP",  !string.IsNullOrEmpty(vendor.ApiClientId) ? vendor.ApiClientId : ""}
            //};

            var parameters = new Dictionary<string, object>()
            {
                { "Password",  vendor.ApiPassword },
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
              //{ "CDP",  !string.IsNullOrEmpty(vendor.ApiClientId) ? vendor.ApiClientId : ""},

            };
            bool a = Decimal.TryParse(postReservationRequest.CustomerPersonalNumber, out _);
            if (selectedExtraCodes != null)
                foreach (var extraCode in selectedExtraCodes)
                    parameters.Add(extraCode, "1");

            return parameters;
        }

        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "Password",  vendor.ApiPassword },
                { "IRN",  reservation.APIReservationNumber},
                { "CANCELDESCRIPTION",  reservation.ReservationStatusNote},
              //{ "CDP",  !string.IsNullOrEmpty(vendor.ApiClientId) ? vendor.ApiClientId : ""}
            };
    }
}
