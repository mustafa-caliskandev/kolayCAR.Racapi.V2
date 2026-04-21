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
using ElitcarProvider = KolayCAR.Broker.API.Providers.Elitcar;

namespace KolayCAR.Broker.API.Providers.Elitcar
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
            AuthProvider = new AuthProvider();
            locationProvider = new ElitcarProvider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postBookingSaveRequestBodyEntity = PostReservationRequestBodyEntity(postReservationRequest, additionalInformation, vendor, reservationNumber, reservationToken, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postBookingSaveRequestBodyEntity),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@ElitcarPostReservationRequestParameters}", postBookingSaveRequestBodyEntity);

            var result = await RestManager.PostAsync<ElitcarRequestBase.PostReservationRequest, ElitcarResponseBase>(
                requestPath: $"reservation/create",
                headers: AuthProvider.CreateHeaderWithContentType(),
                entity: postBookingSaveRequestBodyEntity,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                }, isReservationRequest: true);

            Serilog.Log.Error("{@ElitcarPostReservationResult}", result);

            if (result != null && result.id > 0 && !string.IsNullOrEmpty(result.pnr))
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.pnr;
                localReservation.APIReferenceCode2 = result.id.ToStringNullSafe();

                var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@ElitcarGetLocationsResponse}", location);
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

            Serilog.Log.Error("Elitcar servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Elitcar servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Elitcar servisinden herhangi bir veri alınamadı!"
            };
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var postCancelRequestBodyEntity = PostCancelRequestBodyEntity(localReservation, vendor);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postCancelRequestBodyEntity),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@ElitcarPostCancelReservationsRequestParameters}", postCancelRequestBodyEntity);

            var result = await RestManager.PutAsync<ElitcarRequestBase.CancelReservation>(
                requestPath: $"reservation/cancel",
                entity: postCancelRequestBodyEntity,
                headers: AuthProvider.CreateHeaderWithContentType(),
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }, isReservationRequest: true);

            Serilog.Log.Error("{@ElitcarPostCancelReservationsResponse}", result);

            if (result != null && result.id > 0 && !string.IsNullOrEmpty(result.pnr))
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
                Message = "Elitcar servisi rezervasyon iptali başarısız!",
            };
        }

        private ElitcarRequestBase.CancelReservation PostCancelRequestBodyEntity(Reservation localReservation, Vendor vendor) =>
        new ElitcarRequestBase.CancelReservation
        {
            username = vendor.ApiKey,
            password = vendor.ApiPassword,
            pnr = localReservation.APIReservationNumber,
            id = localReservation.APIReferenceCode2.ToIntNullSafe()
        };

        private ElitcarRequestBase.PostReservationRequest PostReservationRequestBodyEntity(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, string reservationNumber, ReservationToken reservationToken, Reservation localReservation) =>
       new ElitcarRequestBase.PostReservationRequest
       {
           username = vendor.ApiKey,
           password = vendor.ApiPassword,
           pick_id = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
           pick_date_time = additionalInformation.PickupDateTime.ToString("dd.MM.yyyy HH:mm"),
           drop_id = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
           drop_date_time = additionalInformation.ReturnDateTime.ToString("dd.MM.yyyy HH:mm"),
           country_code = "tr",
           collected_amount = reservationToken.APIDailyPrice * reservationToken.RentalDuration,
           payment_method = "credit-card",
           birth_date = !string.IsNullOrEmpty(postReservationRequest.CustomerBirthDay) ? postReservationRequest.CustomerBirthDay : "01.01.1990",
           phone = postReservationRequest.CustomerTelephone,
           id = reservationToken.VehicleCode,
           first_name = postReservationRequest.CustomerName,
           last_name = postReservationRequest.CustomerSurname,
           rental_amount = reservationToken.APIDailyPrice * reservationToken.RentalDuration,
           services = localReservation.ReservationExtras.Count > 0 ? localReservation.ReservationExtras.Select(x => new ElitcarRequestBase.ExtraList
           {
               id = x.ExtraCode.ToIntNullSafe(),
               piece = x.Piece
           }).ToList()
                : new List<ElitcarRequestBase.ExtraList>(),

       };
    }
}
