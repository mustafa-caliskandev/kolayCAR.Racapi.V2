using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErboycarProvider = KolayCAR.Broker.API.Providers.Erboycar;

namespace KolayCAR.Broker.API.Providers.Erboycar
{
    public class ReservationProvider : IReservationProvider
    {
        ILocationProvider locationProvider { get; set; }
        RestManager RestManager { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            locationProvider = new ErboycarProvider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var postCancelReservationRequestBodyEntity = PostCancelReservationRequestBodyEntity(vendor, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postCancelReservationRequestBodyEntity),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@ErboycarPostCancelReservationRequestParameters}", postCancelReservationRequestBodyEntity);

            var result = await RestManager.DeleteAsync<ErboycarRequestBase.ReservationCancel, ErboycarResponseBase.ReservationCancel>(
                requestPath: "reservation-cancel",
                entity: postCancelReservationRequestBodyEntity,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@ErboycarPostCancelReservationResult}", result);

            if (result != null)
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
                Message = "Erboycar servisi rezervasyon iptali başarısız!",
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var postReservationRequestBodyEntity = PostReservationRequestBodyEntity(postReservationRequest, additionalInformation, vendor, localReservation, reservationToken);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestBodyEntity),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@ErboycarPostReservationRequestParameters}", postReservationRequestBodyEntity);

            var result = await RestManager.PostAsync<ErboycarRequestBase.Reservation, ErboycarResponseBase.Reservation>(
                requestPath: "save",
                entity: postReservationRequestBodyEntity,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@ErboycarPostReservationResult}", result);

            if (result != null && !string.IsNullOrEmpty(result.reservation_status) && result.reservation_status.ToLower() == "success" && result.summary != null)
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.summary.res_no;

                var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@ErboycarGetLocationsResponse}", location);
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
            else if (result != null && !string.IsNullOrEmpty(result.error))
            {
                Serilog.Log.Error($"Erboycar servisinden herhangi bir veri alınamadı! error:{result.error} - code:{result.code}");
                localReservation.APIMessage = $"error:{result.error}";
            }
            else
            {
                Serilog.Log.Error("Erboycar servisinden herhangi bir veri alınamadı!");
                localReservation.APIMessage = "Erboycar servisinden herhangi bir veri alınamadı!";
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Erboycar servisinden herhangi bir veri alınamadı!"
            };
        }

        private ErboycarRequestBase.Reservation PostReservationRequestBodyEntity(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, Reservation localReservation, ReservationToken reservationToken)
        {
            var body = new ErboycarRequestBase.Reservation
            {
                _token = reservationToken.APIReferenceCode,
                extra = new List<ErboycarRequestBase.Extra>(),
                customer = new ErboycarRequestBase.Customer
                {
                    first_name = localReservation.CustomerName,
                    last_name = localReservation.CustomerSurname,
                    phone_mobile = localReservation.CustomerPhone,
                    email = localReservation.CustomerMail
                },
                customer_type = "individual",
                payment_type = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 0 :
                    postReservationRequest.PaymentType == PaymentTypes.PayAll ? 1 :
                    postReservationRequest.PaymentType == PaymentTypes.AdvancePayment ? 2 : 0,
                partial_amount = postReservationRequest.PaymentType == PaymentTypes.AdvancePayment ? localReservation.APIPaidAmount : 0
            };

            if (localReservation.ReservationExtras != null && localReservation.ReservationExtras.Any())
            {
                foreach (var extra in localReservation.ReservationExtras)
                {
                    body.extra.Add(new ErboycarRequestBase.Extra
                    {
                        _id = extra.ExtraCode,
                        qty = extra.Piece
                    });
                }
            }

            return body;
        }

        private ErboycarRequestBase.ReservationCancel PostCancelReservationRequestBodyEntity(Vendor vendor, Reservation localReservation) =>
            new ErboycarRequestBase.ReservationCancel
            {
                reservation_number = localReservation.APIReservationNumber
            };
    }
}
