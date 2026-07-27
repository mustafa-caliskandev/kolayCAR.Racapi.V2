using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Pandora2;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Pandora2
{
    public class ReservationProvider : IReservationProvider
    {
        private const decimal CalculatedPriceTolerance = 0.10m;

        private RestManager RestManager { get; set; }
        private AuthProvider AuthProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(AuthProvider.NormalizeBaseUrl(apiBaseUrl), DbConnectionHelper.Instance().ConnectionString);
            AuthProvider = new AuthProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var auth = await AuthProvider.GetAccessToken(vendor);

            if (auth != null && !string.IsNullOrWhiteSpace(auth.access_token))
            {
                var request = new Pandora2CancelRequest
                {
                    Id = localReservation.APIReservationNumber.ToIntNullSafe()
                };

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(request),
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
                });

                var result = await RestManager.PostAsyncRestClientResult<Pandora2ResponseBase.CancelResponse>(
                    requestPath: "bookings/cancel",
                    parameterType: ParameterType.RequestBody,
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token, localReservation.LanguageType.ToString()),
                    entity: AuthProvider.CreateJsonBody(request),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    },
                    isReservationRequest: true);

                if (result?.Data?.Status.ToStringNullSafe().ToLowerInvariant() == "cancelled")
                {
                    localReservation.APIReservationCancel = true;
                    return new ServiceResponseBase(localReservation, true);
                }

                return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisi rezervasyon iptali basarisiz!");
            }

            return new ServiceResponseBase(localReservation, false, "Pandora2 servisi ile baglanti kurulamadi!");
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var auth = await AuthProvider.GetAccessToken(vendor);

            if (auth != null && !string.IsNullOrWhiteSpace(auth.access_token))
            {
                var additions = SummaryProvider.CreateAdditions(localReservation.ReservationExtras, apiExtras);

                var calculateRequest = SummaryProvider.CreateCalculateRequest(reservationToken, additionalInformation, additions);

                var calculateResult = await RestManager.PostAsyncRestClientResult<Pandora2ResponseBase.PriceResponse>(
                    requestPath: "bookings/calculate",
                    parameterType: ParameterType.RequestBody,
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token, postReservationRequest.LanguageCode),
                    entity: AuthProvider.CreateJsonBody(calculateRequest),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIRequest
                    },
                    isReservationRequest: true);

                if (calculateResult?.Success != true || calculateResult.Data == null)
                    return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, calculateResult, "servisi fiyat hesaplama basarisiz!");

                var priceValidationResult = ValidateCalculatedPrice(calculateResult.Data, localReservation);
                if (priceValidationResult != null)
                    return priceValidationResult;

                var createRequest = CreateBookingRequest(postReservationRequest, additionalInformation, reservationToken, localReservation, calculateResult.Data.NetTotal, additions);
                var authHeaderWithContentType = AuthProvider.CreateAuthHeaderWithContentType(auth.access_token, postReservationRequest.LanguageCode);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(createRequest),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                var result = await RestManager.PostAsyncRestClientResult<Pandora2ResponseBase.BookingCreateResponse>(
                    requestPath: "bookings/create",
                    parameterType: ParameterType.RequestBody,
                    headers: authHeaderWithContentType,
                    entity: AuthProvider.CreateJsonBody(createRequest),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                    isReservationRequest: true);

                if (result?.Data?.Id > 0)
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.Data.Id.ToString();
                    localReservation.APIReferenceCode3 = result.Data.Files?.ReservationForm;
                    ApplyVendorContactInfo(localReservation, result.Data);

                    return new ServiceResponseBase(localReservation, true);
                }

                return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisinden herhangi bir veri alinamadi!");
            }

            return new ServiceResponseBase(null, false, "Access denied!");
        }

        private static void ApplyVendorContactInfo(Reservation localReservation, Pandora2ResponseBase.BookingCreateResponse bookingResponse)
        {
            if (localReservation == null || bookingResponse == null)
                return;

            var pickupAddress = FirstNonEmpty(bookingResponse.PickupLocation?.Address, bookingResponse.Supplier?.Address);
            var pickupPhone = FirstNonEmpty(bookingResponse.PickupLocation?.Phone, bookingResponse.Supplier?.Phone);
            var returnAddress = FirstNonEmpty(bookingResponse.DropOffLocation?.Address, pickupAddress);
            var returnPhone = FirstNonEmpty(bookingResponse.DropOffLocation?.Phone, pickupPhone);

            if (!string.IsNullOrWhiteSpace(pickupAddress))
                localReservation.APIVendorPickupAddress = pickupAddress;

            if (!string.IsNullOrWhiteSpace(pickupPhone))
                localReservation.APIVendorPickupPhone = pickupPhone;

            if (!string.IsNullOrWhiteSpace(returnAddress))
                localReservation.APIVendorReturnAddress = returnAddress;

            if (!string.IsNullOrWhiteSpace(returnPhone))
                localReservation.APIVendorReturnPhone = returnPhone;
        }

        private static string FirstNonEmpty(params string[] values) =>
            values?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

        private static ServiceResponseBase ValidateCalculatedPrice(Pandora2ResponseBase.PriceResponse calculateResponse, Reservation localReservation)
        {
            var calculatedTotal = ToMoneyDecimal(calculateResponse.NetTotal);
            var localTotal = ToMoneyDecimal(localReservation.APITotalAmount);
            var priceDifference = System.Math.Abs(calculatedTotal - localTotal);

            if (priceDifference <= CalculatedPriceTolerance)
                return null;

            var localTotalWithExtras = ToMoneyDecimal(localReservation.APITotalAmount + localReservation.APIExtraAmount);
            var priceDifferenceWithExtras = System.Math.Abs(calculatedTotal - localTotalWithExtras);

            if (localReservation.APIExtraAmount > 0 && priceDifferenceWithExtras <= CalculatedPriceTolerance)
                return null;

            var message = $"Pandora fiyat kontrolu basarisiz! Calculate fiyati ile lokal rezervasyon toplam fiyati farkli. Calculate: {calculatedTotal:0.00}, Local: {localTotal:0.00}, LocalExtraDahil: {localTotalWithExtras:0.00}, Tolerans: {CalculatedPriceTolerance:0.00}";
            localReservation.APIMessage = message;

            return new ServiceResponseBase(
                localReservation,
                false,
                message,
                serviceMessage: message,
                serviceCode: "PriceMismatch");
        }

        private static decimal ToMoneyDecimal(string value) =>
            ToMoneyDecimal(Pandora2MapperHelper.ToMoney(value));

        private static decimal ToMoneyDecimal(float value) =>
            decimal.Round((decimal)value, 2, System.MidpointRounding.AwayFromZero);

        private static Pandora2CreateRequest CreateBookingRequest(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, Reservation localReservation, string netTotal, List<Pandora2AdditionRequest> additions) =>
            new Pandora2CreateRequest
            {
                VehicleId = reservationToken.VehicleCode,
                OfficeOutId = additionalInformation.APIPickupLocationCode,
                OfficeInId = additionalInformation.APIReturnLocationCode,
                DateOut = VehicleProvider.FormatApiDate(additionalInformation.PickupDateTime),
                DateIn = VehicleProvider.FormatApiDate(additionalInformation.ReturnDateTime),
                NetTotal = netTotal,
                Additions = additions,
                DriverInfo = new Pandora2DriverInfo
                {
                    FirstName = postReservationRequest.CustomerName,
                    LastName = postReservationRequest.CustomerSurname,
                    Birthdate = postReservationRequest.CustomerBirthDay,
                    Phone = postReservationRequest.CustomerTelephone,
                    Email = postReservationRequest.CustomerEmail,
                    FlightNo = postReservationRequest.FlightNumberArrival,
                    Address = postReservationRequest.CustomerAddress
                },
                CustomerNote = $"{postReservationRequest.CustomerNote} | Rezervasyon: {localReservation.ReservationNumber}"
            };
    }

    public class Pandora2CreateRequest : Pandora2CalculateRequest
    {
        public string NetTotal { get; set; }
        public int? CustomerId { get; set; }
        public Pandora2DriverInfo DriverInfo { get; set; }
        public string CustomerNote { get; set; }
    }

    public class Pandora2DriverInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Birthdate { get; set; }
        public string Phone { get; set; }
        public int? Id { get; set; }
        public string Email { get; set; }
        public string FlightNo { get; set; }
        public string Address { get; set; }
    }

    public class Pandora2CancelRequest
    {
        public int Id { get; set; }
    }
}
