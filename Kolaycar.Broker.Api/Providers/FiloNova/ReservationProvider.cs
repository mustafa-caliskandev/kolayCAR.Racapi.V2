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
using FiloNovaProvider = KolayCAR.Broker.API.Providers.FiloNova;

namespace KolayCAR.Broker.API.Providers.FiloNova
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
            locationProvider = new FiloNovaProvider.LocationProvider(apiBaseUrl);
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

            Serilog.Log.Error("{@FiloNovaPostReservationRequestParameters}", postBookingSaveRequestBodyEntity);

            var result = await RestManager.PostAsyncResult<FiloNovaRequestBase.PostReservationRequest, FiloNovaResponseBase>(
                requestPath: $"createReservation",
                headers: AuthProvider.CreateAuthHeaderWithContentType(vendor),
                entity: postBookingSaveRequestBodyEntity,
                ignoreNull: true,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@FiloNovaPostReservationResult}", result);

            if (result?.Data != null && !string.IsNullOrEmpty(result.Data.pnrNumber) && result.Data.responseResult != null && result.Data.responseResult.result)
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Data.pnrNumber;
                localReservation.APIReferenceCode2 = result.Data.reservationId;

                var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@FiloNovaGetLocationsResponse}", location);
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

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisinden herhangi bir veri alınamadı!");

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

            Serilog.Log.Error("{@FiloNovaPostCancelReservationsRequestParameters}", postCancelRequestBodyEntity);

            var result = await RestManager.PostAsyncResult<FiloNovaRequestBase.PostCancelReservation, FiloNovaResponseBase.CancelReservationResult>(
                requestPath: $"cancelReservation",
                entity: postCancelRequestBodyEntity,
                headers: AuthProvider.CreateAuthHeaderWithContentType(vendor),
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@FiloNovaPostCancelReservationsResponse}", result);

            if (result?.Data != null && result.Data.responseResult != null && result.Data.responseResult.result)
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

        private FiloNovaRequestBase.PostReservationRequest PostReservationRequestBodyEntity(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, string reservationNumber, ReservationToken reservationToken, Reservation localReservation) =>
            new FiloNovaRequestBase.PostReservationRequest
            {
                reservationCustomerParameters = new FiloNovaRequestBase.ReservationCustomerParameters
                {
                    brokerCode = vendor.ApiClientId,
                    dummyContactData = new FiloNovaRequestBase.DummyContactData
                    {
                        name = postReservationRequest.CustomerName,
                        surname = postReservationRequest.CustomerSurname,
                        fullName = $"{postReservationRequest.CustomerName} {postReservationRequest.CustomerSurname}",
                        email = postReservationRequest.CustomerEmail,
                        phoneNumber = postReservationRequest.CustomerTelephone,
                        governmentId = postReservationRequest.CustomerPersonalNumber,
                        referenceNumber = reservationNumber
                    }
                },
                reservationQueryParameters = new FiloNovaRequestBase.AvailabilityQueryParameters
                {
                    pickupBranchId = additionalInformation.APIPickupLocationCode,
                    dropoffBranchId = additionalInformation.APIReturnLocationCode,
                    pickupDateTime = additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "+03:00",
                    dropoffDateTime = additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "+03:00"

                },
                reservationEquimentParameters = new FiloNovaRequestBase.ReservationEquimentParameters
                {
                    groupCodeId = reservationToken.VehicleCode,
                    billingType = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 10 : 20
                },
                reservationAdditionalProducts =
                localReservation.ReservationExtras.Count > 0 ? localReservation.ReservationExtras.Select(x => new FiloNovaRequestBase.ReservationAdditionalProduct
                {
                    productId = x.ExtraCode,
                    value = x.Piece,
                    billingType = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery || additionalInformation.Agency.AdditionalProductAmountDeliveryPayment || postReservationRequest.ExtraPricePayToDelivery ? 10 : 20
                }).ToList()
                    : new List<FiloNovaRequestBase.ReservationAdditionalProduct>(),

                reservationPriceParameters = new FiloNovaRequestBase.ReservationPriceParameters
                {
                    //20.10.2023 novacar araç ücretini brokerın tahsil ettiği ek ürün ücretinin ise teslimatta ödeneceği senaryo için 20 - 50 olarak göndermemiz gerektiğini söyledi.
                    paymentType = 20,
                    trackingNumber = reservationToken.APIReferenceCode,
                    paymentMethodCode = 50
                    //paymentType = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 20 : 10,
                    //trackingNumber = reservationToken.APIReferenceCode,
                    //paymentMethodCode = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 60 : localReservation.ReservationExtras.Count > 0 ? 30 
                    //: postReservationRequest.FullCredit.ToBoolNullSafe() ? 40 : 50
                    //paymentType = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 60 : postReservationRequest.FullCredit.ToBoolNullSafe() ? 40 : 50,
                    //trackingNumber = reservationToken.APIReferenceCode,
                    //paymentMethodCode = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 60 : postReservationRequest.FullCredit.ToBoolNullSafe() ? 40 : 50
                },
                langId = vendor.SecretKey.ToIntNullSafe(),
                channelCode = 70
            };


        private FiloNovaRequestBase.PostCancelReservation PostCancelRequestBodyEntity(Reservation localReservation, Vendor vendor) =>
        new FiloNovaRequestBase.PostCancelReservation
        {
            reservationId = localReservation.APIReferenceCode2,
            cancellationReason = 100000006,
            pnrNumber = localReservation.APIReservationNumber,
            langId = vendor.SecretKey.ToIntNullSafe(),
            channelCode = 70
        };

    }
}
