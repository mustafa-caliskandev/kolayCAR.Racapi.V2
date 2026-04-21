using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Helpers.Garenta;
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
using GarentaProvider = KolayCAR.Broker.API.Providers.Garenta;

namespace KolayCAR.Broker.API.Providers.Garenta
{
    public class ReservationProvider : IReservationProvider
    {
        ILocationProvider locationProvider { get; set; }
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            AuthProvider = new AuthProvider();
            locationProvider = new GarentaProvider.LocationProvider(apiBaseUrl);
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

            Serilog.Log.Error("{@GarentaPostCancelReservationRequestParameters}", postCancelReservationRequestBodyEntity);

            var result = await RestManager.PostAsync<GarentaRequestBase, GarentaResponseBase>(
                requestPath: string.Empty,
                entity: postCancelReservationRequestBodyEntity,
                headers: AuthProvider.CreateAuthHeaderWithContentType(vendor),
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@GarentaPostCancelReservationResult}", result);

            if (result != null &&
                result.EXPORT != null &&
                result.EXPORT.ES_OUTPUT != null &&
                result.EXPORT.ES_OUTPUT.SUCCESS != null &&
                !string.IsNullOrEmpty(result.EXPORT.ES_OUTPUT.SUCCESS.SUCCESS) &&
                result.EXPORT.ES_OUTPUT.SUCCESS.SUCCESS == "X")
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
                Message = "Garenta servisi rezervasyon iptali başarısız!",
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

            Serilog.Log.Error("{@GarentaPostReservationRequestParameters}", postReservationRequestBodyEntity);

            var result = await RestManager.PostAsync<GarentaRequestBase, GarentaResponseBase>(
                requestPath: string.Empty,
                entity: postReservationRequestBodyEntity,
                headers: AuthProvider.CreateAuthHeaderWithContentType(vendor),
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@GarentaPostReservationResult}", result);

            if (result != null &&
                result.EXPORT != null &&
                result.EXPORT.ES_OUTPUT != null &&
                result.EXPORT.ES_OUTPUT.RESERV_INFO != null &&
                !string.IsNullOrEmpty(result.EXPORT.ES_OUTPUT.RESERV_INFO.PNR_CODE))
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.EXPORT.ES_OUTPUT.RESERV_INFO.PNR_CODE;

                var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@GarentaGetLocationsResponse}", location);
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
            else if (result != null &&
                result.EXPORT != null &&
                result.EXPORT.ES_OUTPUT != null &&
                result.EXPORT.ES_OUTPUT.MESSAGE != null &&
                result.EXPORT.ES_OUTPUT.MESSAGE.Any() &&
                !string.IsNullOrEmpty(result.EXPORT.ES_OUTPUT.MESSAGE[0].MESSAGE))
            {
                Serilog.Log.Error("Garenta servisinden herhangi bir veri alınamadı!");
                localReservation.APIMessage = result.EXPORT.ES_OUTPUT.MESSAGE[0].MESSAGE;
            }
            else
            {
                Serilog.Log.Error("Garenta servisinden herhangi bir veri alınamadı!");
                localReservation.APIMessage = "Garenta servisinden herhangi bir veri alınamadı!";
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Garenta servisinden herhangi bir veri alınamadı!"
            };
        }

        private GarentaRequestBase PostReservationRequestBodyEntity(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, Reservation localReservation, ReservationToken reservationToken)
        {
            var body = new GarentaRequestBase
            {
                sap_props = RequestHelper.GetSapProps(GarentaRequestBase.ServiceTypes.CREATE_BROKER_RESERV, vendor.ApiKey, vendor.ApiPassword),
                import = new GarentaRequestBase.Import
                {
                    IS_INPUT = new GarentaRequestBase.ISINPUT_RESERVATION
                    {
                        RESERVATION = new GarentaRequestBase.RESERVATION
                        {
                            NAME = postReservationRequest.CustomerName,
                            SURNAME = postReservationRequest.CustomerSurname,
                            TEL_NO = postReservationRequest.CustomerTelephone,
                            EMAIL = postReservationRequest.CustomerEmail,
                            NATIO = string.Empty,
                            SIPP_CODE = reservationToken.VehicleCode,
                            PICKUP_DATE = postReservationRequest.PickupDate,
                            PICKUP_TIME = postReservationRequest.PickupTime + ":00",
                            DROPOFF_DATE = postReservationRequest.ReturnDate,
                            DROPOFF_TIME = postReservationRequest.ReturnTime + ":00",
                            PICKUP_OFFICE = additionalInformation.APIPickupLocationCode,
                            RETURN_OFFICE = additionalInformation.APIReturnLocationCode,
                            BROKER_RESERV_NO = localReservation.ReservationNumber,
                            CAMPAIGN_ID = string.Empty,
                            SEARCH_REFERENCE = reservationToken.APIReferenceCode3.ToStringNullSafe()
                        },
                        EXTRA = new List<GarentaRequestBase.EXTRA>(),
                        BROKER_CODE = vendor.ApiKey,
                        LANGU = GarentaRequestBase.LanguageTypes.T.ToString()
                    }
                }
            };

            if (localReservation.ReservationExtras != null && localReservation.ReservationExtras.Any())
            {
                foreach (var extra in localReservation.ReservationExtras)
                {
                    body.import.IS_INPUT.EXTRA.Add(new GarentaRequestBase.EXTRA
                    {
                        PRODUCT_ID = extra.ExtraCode,
                        COUNT = extra.Piece.ToString()
                    });
                }
            }

            return body;
        }

        private GarentaRequestBase PostCancelReservationRequestBodyEntity(Vendor vendor, Reservation localReservation) =>
            new GarentaRequestBase
            {
                sap_props = RequestHelper.GetSapProps(GarentaRequestBase.ServiceTypes.CANCEL_BROKER_RESERV, vendor.ApiKey, vendor.ApiPassword),
                import = new GarentaRequestBase.Import
                {
                    IS_INPUT = new GarentaRequestBase.ISINPUT_CANCEL_RESERVATION
                    {
                        PNR_NO = localReservation.APIReservationNumber,
                        BROKER_CODE = vendor.ApiKey,
                        LANGU = GarentaRequestBase.LanguageTypes.T.ToString()
                    }
                }
            };
    }
}
