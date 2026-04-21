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
using CizgiProvider = KolayCAR.Broker.API.Providers.Cizgi;

namespace KolayCAR.Broker.API.Providers.Cizgi
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager { get; set; }
        ILocationProvider locationProvider { get; set; }
        AuthProvider AuthProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            AuthProvider = new AuthProvider(apiBaseUrl);
            locationProvider = new CizgiProvider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (auth != null && !string.IsNullOrEmpty(auth.access_token))
            {
                var postCancelRequestBodyEntity = PostCancelRequestBodyEntity(localReservation, postCancelReservationRequest);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(postCancelRequestBodyEntity),
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
                });

                Serilog.Log.Error("{@CizgiPostCancelReservationsRequestParameters}", postCancelRequestBodyEntity);

                var result = await RestManager.PostAsync<CizgiRequestBase.CancelReservationRequest, CizgiResponseBase.CommonResponse>(
                    requestPath: $"reservation/cancel",
                    entity: postCancelRequestBodyEntity,
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    },
                    isReservationRequest: true);

                Serilog.Log.Error("{@CizgiPostCancelReservationsResponse}", result);

                if (result != null && result.status == 1)
                {
                    localReservation.APIReservationCancel = true;

                    return new ServiceResponseBase
                    {
                        Success = true,
                        Data = localReservation
                    };
                }
            }
            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Cizgi servisi rezervasyon iptali başarısız!",
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);

            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (auth != null && !string.IsNullOrEmpty(auth.access_token))
            {
                var postBookingSaveRequestBodyEntity = PostReservationRequestBodyEntity(postReservationRequest, additionalInformation, vendor, reservationNumber, reservationToken, localReservation, apiPaidAmount);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(postBookingSaveRequestBodyEntity),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                Serilog.Log.Error("{@CizgiPostReservationRequestParameters}", postBookingSaveRequestBodyEntity);

                var result = await RestManager.PostAsync<CizgiRequestBase.PostReservationRequest, CizgiResponseBase.PostReservationResponse>(
                    requestPath: $"reservation/book",
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token),
                    entity: postBookingSaveRequestBodyEntity,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    }, isReservationRequest: true);

                Serilog.Log.Error("{@CizgiPostReservationResult}", result);

                if (result != null && !string.IsNullOrEmpty(result.reference_no) && result.status == 1)
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.reference_no;

                    var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                    Serilog.Log.Error("{@CizgiGetLocationsResponse}", location);
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
                        Success = true,
                        Data = localReservation
                    };
                }
            }
            Serilog.Log.Error("Cizgi servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Cizgi servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Cizgi servisinden herhangi bir veri alınamadı!"
            };
        }

        private CizgiRequestBase.PostReservationRequest PostReservationRequestBodyEntity(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, string reservationNumber, ReservationToken reservationToken, Reservation localReservation, float apiPaidAmount)
        {
            var packages = new Dictionary<string, int>();
            foreach (var extra in localReservation.ReservationExtras)
            {
                packages.Add(extra.ExtraCode, extra.Piece);
            }

            return new CizgiRequestBase.PostReservationRequest
            {
                pickup_location = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
                pickup_date = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                dropoff_location = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
                dropoff_date = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                reference_no = reservationNumber,
                reference_id = 0,
                name = postReservationRequest.CustomerName,
                surname = postReservationRequest.CustomerSurname,
                email = postReservationRequest.CustomerEmail,
                phone = postReservationRequest.CustomerTelephone,
                notes = "",
                //netrentacar için kaldırıldı
                //notes = postReservationRequest.CustomerNote,
                birth_date = !string.IsNullOrEmpty(postReservationRequest.CustomerBirthDay) ? postReservationRequest.CustomerBirthDay.ToDateTimeNullSafe().ToString("yyyy-MM-dd") : string.Empty,
                passport_no = !string.IsNullOrEmpty(postReservationRequest.CustomerPersonalNumber) ? postReservationRequest.CustomerPersonalNumber : string.Empty,
                citizenship_no = !string.IsNullOrEmpty(postReservationRequest.CustomerPersonalNumber) ? postReservationRequest.CustomerPersonalNumber : string.Empty,
                driver2_name = string.Empty,
                driver2_surname = string.Empty,
                driver2_email = string.Empty,
                driver2_phone = string.Empty,
                driver2_passport_no = string.Empty,
                driver2_citizenship_no = string.Empty,
                car_id = reservationToken.VehicleCode.ToIntNullSafe(),
                flight_number = postReservationRequest.FlightNumberArrival,
                payment_made = 0,
                packages = packages,
                search_id = reservationToken.APIReferenceCode2
            };
        }

        private CizgiRequestBase.CancelReservationRequest PostCancelRequestBodyEntity(Reservation localReservation, PostCancelReservationRequest postCancelReservationRequest) =>
       new CizgiRequestBase.CancelReservationRequest
       {
           reference_no = localReservation.APIReservationNumber,
           reason = !string.IsNullOrEmpty(postCancelReservationRequest.CancelNote) ? postCancelReservationRequest.CancelNote : "Rezervasyon iptal edilmiştir."
       };
    }
}
