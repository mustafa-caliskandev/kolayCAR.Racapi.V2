using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenMotionHelper = KolayCAR.Broker.API.Helpers.GreenMotion;
using GreenMotionProvider = KolayCAR.Broker.API.Providers.GreenMotion;

namespace KolayCAR.Broker.API.Providers.GreenMotion
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager { get; set; }
        ILocationProvider locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            locationProvider = new GreenMotionProvider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var postCancelReservationEntity = CreatePostBookingCancelRequestBodyEntity(vendor, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = PostBookingCancelRequestBodyEntity(vendor, localReservation),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@GreenMotionPostCancelReservationRequestParameters}", postCancelReservationEntity);

            var result = await RestManager.PostAsyncXMLRestClient<GreenMotionResponseBase>(
                requestPath: string.Empty,
                parameterType: RestSharp.ParameterType.RequestBody,
                headers: GreenMotionHelper.RequestHelper.GetGreenMotionRequestHeader(),
                entity: postCancelReservationEntity,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@GreenMotionPostCancelReservationResult}", result);

            if (result != null &&
                result.gm_webservice != null &&
                result.gm_webservice.response != null &&
                !string.IsNullOrEmpty(result.gm_webservice.response.booking_ref))
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
                Message = "GreenMotion servisi rezervasyon iptali başarısız!",
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postBookingSaveRequestBodyEntity = CreatePostBookingSaveRequestBodyEntity(postReservationRequest, additionalInformation, reservationToken, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = PostBookingSaveRequestBodyEntity(postReservationRequest, additionalInformation, reservationToken, localReservation),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@GreenMotionPostReservationRequestParameters}", postBookingSaveRequestBodyEntity);

            var result = await RestManager.PostAsyncXMLRestClient<GreenMotionResponseBase>(
                requestPath: string.Empty,
                parameterType: RestSharp.ParameterType.RequestBody,
                headers: GreenMotionHelper.RequestHelper.GetGreenMotionRequestHeader(),
                entity: postBookingSaveRequestBodyEntity,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true
                );

            Serilog.Log.Error("{@GreenMotionPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;

            if (result != null &&
                result.gm_webservice != null &&
                result.gm_webservice.response != null &&
                !string.IsNullOrEmpty(result.gm_webservice.response.booking_ref))
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.gm_webservice.response.booking_ref;

                var pickupLocation = await locationProvider.GetLocationDetail(vendor, (int)localReservation.LanguageType + 1, reservationToken.APIPickupLocationCode);
                Serilog.Log.Error("{@GreenMotionGetPickupLocationsResponse}", pickupLocation.Data);

                if (pickupLocation.Success)
                {
                    if (pickupLocation.Data != null)
                    {
                        var reservationLocation = pickupLocation.Data as Domain.Models.Location;

                        localReservation.APIVendorPickupAddress = reservationLocation.Address;
                        localReservation.APIVendorPickupPhone = reservationLocation.PhoneNumber;
                    }
                }

                var returnLocation = await locationProvider.GetLocationDetail(vendor, (int)localReservation.LanguageType + 1, reservationToken.APIPickupLocationCode);
                Serilog.Log.Error("{@GreenMotionGetReturnLocationsResponse}", returnLocation.Data);

                if (returnLocation.Success)
                {
                    if (returnLocation.Data != null)
                    {
                        var reservationLocation = returnLocation.Data as Domain.Models.Location;

                        localReservation.APIVendorReturnAddress = reservationLocation.Address;
                        localReservation.APIVendorReturnPhone = reservationLocation.PhoneNumber;
                    }
                }

                return new ServiceResponseBase
                {
                    Success = true,
                    Data = localReservation
                };
            }

            Serilog.Log.Error("GreenMotion servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "GreenMotion servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "GreenMotion servisinden herhangi bir veri alınamadı!"
            };
        }

        public string PostBookingSaveRequestBodyEntity(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, Reservation localReservation)
        {
            var options = localReservation.ReservationExtras.Count > 0 ? localReservation.ReservationExtras.Select(x => new GreenMotionRequestBase.GreenMotionReservationOption
            {
                id = x.ExtraCode.ToIntNullSafe(),
                option_qty = x.Piece.ToIntNullSafe(),
                //option_total = x.APIPrice.ToStringNullSafe().Replace(",", ".")
                option_total = x.ApiPrice.ToStringNullSafe().Replace(",", ".")
            }).ToList() : new List<GreenMotionRequestBase.GreenMotionReservationOption>();

            string paymentTypeString = string.Empty;

            if (postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery)
            {
                paymentTypeString = "POA";
            }

            var postReservationPayload = new GreenMotionRequestBase.GreenMotionPostReservationRequest
            {
                full_credit = CreditHelper.ResolveTokenCreditType(reservationToken) == CreditType.FullCredit ? "Yes" : "",
                location_id = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
                dropoff_location_id = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
                start_date = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd"),
                start_time = additionalInformation.PickupDateTime.ToString("HH:mm"),
                end_date = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd"),
                end_time = additionalInformation.ReturnDateTime.ToString("HH:mm"),
                vehicle_id = reservationToken.VehicleId.ToStringNullSafe(),
                vehicle_total = reservationToken.APIReferenceCode2.ToStringNullSafe().Replace(",", "."),
                currency = reservationToken.BaseVendorRequestCurrencyType.ToString(),
                options = new GreenMotionRequestBase.GreenMotionGreenMotionReservationOptionList
                {
                    option = options
                },
                grand_total = (reservationToken.APIReferenceCode2.ToFloatNullSafe() + localReservation.APIExtraAmount).ToString().Replace(",", "."),
                cust_info = new GreenMotionRequestBase.GreenMotionCustomerInfo
                {
                    firstname = postReservationRequest.CustomerName,
                    lastname = postReservationRequest.CustomerSurname,
                    age = 30,
                    telephone = postReservationRequest.CustomerTelephone,
                    mobile = postReservationRequest.CustomerTelephone,
                    email = postReservationRequest.CustomerEmail,
                    flight_no = $"{postReservationRequest.FlightNumberArrival} - {postReservationRequest.DepartureInfo} - {postReservationRequest.CustomerNote}",
                    verification_response = localReservation.ReservationNumber,
                    dvlacheckcode = localReservation.ReservationNumber,
                },
                quoteid = reservationToken.APIReferenceCode.Split('|')[0],
                type = "MakeReservation",
                payment_type = paymentTypeString
            };

            var body = GreenMotionHelper.RequestHelper.GetGreenMotionRequestObject(additionalInformation.Vendor, postReservationPayload);

            return ObjectHelper.ObjectToXML(body);
        }

        public Dictionary<string, object> CreatePostBookingSaveRequestBodyEntity(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, Reservation localReservation) =>
            new Dictionary<string, object>()
            {
                { "application/xml", PostBookingSaveRequestBodyEntity(postReservationRequest,additionalInformation,reservationToken,localReservation) },
            };

        public string PostBookingCancelRequestBodyEntity(Vendor vendor, Reservation localReservation)
        {
            var postCancelReservationPayload = new GreenMotionRequestBase.GreenMotionPostCancelReservationRequest
            {
                location_id = localReservation.APIReferenceCode.Split('|')[1],
                booking_ref = localReservation.APIReservationNumber,
                type = "CancelReservation"
            };

            var body = GreenMotionHelper.RequestHelper.GetGreenMotionRequestObject(vendor, postCancelReservationPayload);

            return ObjectHelper.ObjectToXML(body);
        }

        public Dictionary<string, object> CreatePostBookingCancelRequestBodyEntity(Vendor vendor, Reservation localReservation) =>
            new Dictionary<string, object>()
            {
                { "application/xml", PostBookingCancelRequestBodyEntity(vendor, localReservation) },
            };
    }
}
