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
using System.Web;

namespace KolayCAR.Broker.API.Providers.AutoHome
{
    public class ReservationProvider : IReservationProvider
    {

        HttpManager _httpmanager { get; set; }
        ILocationProvider _locationprovider { get; set; }
        private IConfigurationService ConfigurationService { get; set; }

        public ReservationProvider(string apiBaseUrl, IConfigurationService configrationService)
        {
            _httpmanager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _locationprovider = new LocationProvider(apiBaseUrl);
            ConfigurationService = configrationService;
        }


        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Domain.Models.Vendor vendor, Reservation localReservation)
        {
            var postCancelReservationsRequestParameters = PostCancelReservationsRequestParameters(vendor, localReservation);

            await ConfigurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postCancelReservationsRequestParameters),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@AutoHomePostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await _httpmanager.PostAsync2<AutoHomeResponseBase.ReservationCancel, AutoHomeResponseBase.ReservationCancel>(
                requestPath: "/API/reservation/v2/ReservationCancel.php",
                parameters: postCancelReservationsRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }, isReservationRequest: true);


            Serilog.Log.Error("{@AutoHomePostCancelReservationsResponse}", result);

            if (result?.Data?.cevap != null)
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

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Domain.Models.Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postReservationRequestParameters = PostReservationRequestParameters(postReservationRequest, additionalInformation, reservationNumber, reservationToken, postReservationRequest.PaidAmount, vendor, localReservation);

            await ConfigurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@AutohomePostReservationRequestParameters}", postReservationRequestParameters);

            var result = await _httpmanager.PostAsync2<object, AutoHomeResponseBase.Reservation>(
                requestPath: "API/reservation/v2/Reservation.php" + postReservationRequestParameters,
                 brokerLogModel: new BrokerLogModel
                 {
                     LogKey = localReservation.ReservationNumber,
                     LogType = BrokerLogTypes.ReservationVendorAPIResponse
                 },
                isReservationRequest: true
                );
            //var result = new AutoHomeResponseBase.Reservation();
            Serilog.Log.Error("{@AutoHomePostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result?.Data?.cevap != null)
            {

                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Data.ReservationId;

                var location = await _locationprovider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@AutoHomeGetLocationsResponse}", location.Data);
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

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisinden herhangi bir veri alınamadı!");
        }
        private string PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount, Domain.Models.Vendor vendor, Reservation localReservation)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                 : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);
            var parameters = new Dictionary<string, object>()
            {
                { "login",  additionalInformation.Vendor.ApiKey},
                { "passwd",  additionalInformation.Vendor.ApiPassword},

                { "Name", postReservationRequest.CustomerName},
                { "LastName", postReservationRequest.CustomerSurname},
                { "Id", postReservationRequest.CustomerPersonalNumber},

                { "EMail", postReservationRequest.CustomerEmail},
                { "BirthDay", postReservationRequest.CustomerBirthDay},
                { "MobilePhone", postReservationRequest.CustomerTelephone},

                { "Country",  localReservation.CityOfPickupLocation },
                { "City",  localReservation.CityOfPickupLocation },

                { "ContractDepartureLocationNo", reservationToken.APIPickupLocationCode},
                { "ContractDepartureDate", reservationToken.PickupDateTime},
                { "ContractReturnLocationNo", $"{reservationToken.APIReturnLocationCode}" },
                { "ContractReturnDate", reservationToken.ReturnDateTime },
                { "Address","adres"},

                {"GroupId" , reservationToken.VehicleId},
                {"SubGroupId" ,reservationToken.APIReferenceCode},
                {"SubGroupShortName" , reservationToken.SippCode},
                {"sendMail" ,1},
                //{ "Full_Credit", vendor.CreditType == CreditType.FullCredit },
                { "Currency", reservationToken.BaseVendorRequestCurrencyType.ToString() }
            };

            if (vendor.CreditType == CreditType.FullCredit)
                parameters.Add("FullCredit", 1);

            if (postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery)
                parameters.Add("FullCredit", 2);

            // URL'yi oluşturma
            var returnParametre = parameters.ToQueryString();
            if (selectedExtraCodes?.Count > 0)
            {
                var url = "&" + BuildQueryString(selectedExtraCodes);
                returnParametre = returnParametre + url;
            }
            return returnParametre;
        }
        public static string BuildQueryString(List<string> extraCodes)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            foreach (var extraCode in extraCodes)
            {
                queryString.Add("Extras[]", extraCode);
            }
            return queryString.ToString();
        }
        private Dictionary<string, object> PostCancelReservationsRequestParameters(Domain.Models.Vendor vendor, Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "login", vendor.ApiKey },
                { "passwd",  vendor.ApiPassword},
                { "ReservationId",  reservation.APIReservationNumber},
            };
    }


}
