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
using static KolayCAR.Broker.Domain.Models.Requests.BesSRequestBase;

namespace KolayCAR.Broker.API.Providers._5S
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager;
        private readonly IConfigurationService _configurationService;
        ILocationProvider LocationProvider { get; set; }
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            LocationProvider = new LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var cancelReservationRequestBody = CreateCancelReservationRequestBody(localReservation, postCancelReservationRequest);

            await _configurationService.WriteLog(new BrokerLogModel(localReservation.ReservationNumber, cancelReservationRequestBody.ToJson(), BrokerLogTypes.ReservationCancelVendorAPIRequest));

            Serilog.Log.Error("{@5SPostCancelReservationsRequestParameters}", cancelReservationRequestBody);

            var result = await RestManager.GetAsyncResult<BesSResponseBase.ReservationResponse>(
                requestPath: "extservice/cancel",
                headers: Configuration.CreateHeaderWithAuth(vendor.ApiKey),
                parameters: cancelReservationRequestBody,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true
                );

            Serilog.Log.Error("{@5SPostCancelReservationsResponse}", result);

            if (result?.Data?.status ?? false)
            {
                localReservation.APIReservationCancel = true;
                return new ServiceResponseBase(localReservation, true);
            }
            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "rezervasyon iptalinde hata oluştu!");
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount;

            if (postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery)
                apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            else
                apiPaidAmount = reservationToken.APIDailyPrice * reservationToken.RentalDuration;

            var reservationRequestBody = CreateReservationRequestBody(postReservationRequest, reservationToken, localReservation, apiPaidAmount, vendor);

            await _configurationService.WriteLog(new BrokerLogModel(localReservation.ReservationNumber, reservationRequestBody.ToJson(), BrokerLogTypes.ReservationVendorAPIRequest));
            Serilog.Log.Error("{@5SReservationRequestBody}", reservationRequestBody);

            var result = await RestManager.PostAsyncResult<BesSRequestBase.BesSPostReservationRequest, BesSResponseBase.ReservationResponse>(
                requestPath: "extservice/create",
                headers: Configuration.CreateHeaderWithAuth(vendor.ApiKey),
                entity: reservationRequestBody,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                ignoreNull: true,
                isReservationRequest: true
                );

            Serilog.Log.Error("{@5SReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if ((result?.Data?.status ?? false) && !string.IsNullOrEmpty(result?.Data?.voucher))
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Data.voucher;

                var location = await LocationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@AssistGetLocationsResponse}", location);
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
                return new ServiceResponseBase(localReservation, result.Data.status);
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisine ulaşılamadı!");
        }
        private Dictionary<string, object> CreateCancelReservationRequestBody(Reservation localReservation, PostCancelReservationRequest postCancelReservationRequest) =>
            new Dictionary<string, object>
            {
                { "code", localReservation.ReservationNumber },
                { "reason", postCancelReservationRequest.CancelNote ?? "iptal" }
            };
        private BesSRequestBase.BesSPostReservationRequest CreateReservationRequestBody(PostReservationRequest postReservationRequest, ReservationToken reservationToken, Reservation localReservation, float apiPaidAmount, Vendor vendor)
        {
            var amountVehicle = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery
                ? reservationToken.DailyPrice * reservationToken.RentalDuration
                : reservationToken.APIDailyPrice * reservationToken.RentalDuration;

            //var amountFinal = (reservationToken.APIDailyPrice * reservationToken.RentalDuration) + reservationToken.OneWayFee + postReservationRequest.ExtraAmount.ToFloatNullSafe(); // gkursad 12.06.2024
            var amountFinal = amountVehicle + reservationToken.OneWayFee + postReservationRequest.ExtraAmount.ToFloatNullSafe();
            var isPayOnDelivery = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery;

            return new BesSRequestBase.BesSPostReservationRequest
            {
                search_vehicle_key = reservationToken.APIReferenceCode,
                code = localReservation.ReservationNumber,
                name = postReservationRequest.CustomerName + " " + postReservationRequest.CustomerSurname,
                phone = postReservationRequest.CustomerTelephone,
                email = postReservationRequest.CustomerEmail,
                description = postReservationRequest.CustomerNote + ", " + postReservationRequest.FlightNumberArrival,
                flight_company = postReservationRequest.FlightNumberDeparture,
                departure_airport = postReservationRequest.FlightNumberDeparture,
                landing_airport = string.Empty,
                flight_number = postReservationRequest.FlightNumberArrival,
                payment_method_code = isPayOnDelivery ? null : "office",
                payment_method_id = isPayOnDelivery ? null : 1,
                currency = GetCurrency(reservationToken.BaseVendorRequestCurrencyType),
                amount_extras = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? postReservationRequest.ExtraAmount.ToFloatNullSafe() : localReservation.APIExtraAmount.ToFloatNullSafe(),
                amount_vehicle = amountVehicle,
                amount_drop = isPayOnDelivery ? reservationToken.OneWayFee : reservationToken.APIOneWayFee,
                amount_paid = isPayOnDelivery ? 0 : apiPaidAmount,
                amount_final = amountFinal,
                extra = localReservation.ReservationExtras.Count > 0 ? localReservation.ReservationExtras.Select(x => new BesSExtra
                {
                    id = x.ExtraCode.ToIntNullSafe(),
                    //price = isPayOnDelivery ? reservationToken.CurrencyType == reservationToken.BaseVendorRequestCurrencyType ? localReservation.APIExtraAmount : CalculateExtraPrice(x.APIPrice * reservationToken.RentalDuration, vendor) : x.APIPrice * reservationToken.RentalDuration
                    price = isPayOnDelivery ? reservationToken.CurrencyType == reservationToken.BaseVendorRequestCurrencyType ? localReservation.APIExtraAmount : CalculateExtraPrice(x.ApiPrice * reservationToken.RentalDuration, vendor) : x.ApiPrice * reservationToken.RentalDuration
                }).ToList() : new List<BesSExtra>()
            };
        }
        private float GetPaymentMethod(PostReservationRequest postReservationRequest)//todo: parametre değerleri 5s tarafında nasıl bilinmiyor.
        {
            if (postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery)
                return 4.0f;
            else
                return 3.0f;
        }
        private float CalculateExtraPrice(float price, Vendor vendor) => vendor.AdditionalProductWorkingType == VendorWorkingTypes.ProfitMarkup ? ((price / 100) * vendor.ProfitMarkupAdditionalProducts) + price : price;
        private string GetCurrency(CurrencyTypes baseVendorRequestCurrencyType)
        {
            return baseVendorRequestCurrencyType switch
            {
                CurrencyTypes.TRY => "TRY",
                CurrencyTypes.USD => "USD",
                CurrencyTypes.EUR => "EUR",
                CurrencyTypes.GBP => "GBP",
                CurrencyTypes.CHF => "CHF",
                _ => ""
            };
        }
        private Dictionary<string, object> CreateReservationRequestBody2(PostReservationRequest postReservationRequest, ReservationToken reservationToken, Reservation localReservation, float apiPaidAmount, Vendor vendor)
        {
            var amountVehicle = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery
                ? reservationToken.DailyPrice * reservationToken.RentalDuration
                : reservationToken.APIDailyPrice * reservationToken.RentalDuration;

            //var amountFinal = (reservationToken.APIDailyPrice * reservationToken.RentalDuration) + reservationToken.OneWayFee + postReservationRequest.ExtraAmount.ToFloatNullSafe(); // gkursad 12.06.2024
            var amountFinal = amountVehicle + reservationToken.OneWayFee + postReservationRequest.ExtraAmount.ToFloatNullSafe();

            return new Dictionary<string, object>
            {
                { "search_vehicle_key", reservationToken.APIReferenceCode },
                { "code", localReservation.ReservationNumber },
                { "name", postReservationRequest.CustomerName + " " + postReservationRequest.CustomerSurname },
                { "phone", postReservationRequest.CustomerTelephone },
                { "email", postReservationRequest.CustomerEmail },
                { "description", postReservationRequest.CustomerNote + ", " + postReservationRequest.FlightNumberArrival },
                { "flight_company", postReservationRequest.FlightNumberDeparture },
                { "departure_airport", postReservationRequest.FlightNumberDeparture },
                { "landing_airport", string.Empty },
                { "flight_number", postReservationRequest.FlightNumberArrival },
                { "payment_method_id", GetPaymentMethod(postReservationRequest) },// ????????
                { "currency", reservationToken.BaseVendorRequestCurrencyType },
                //{ "amount_extras", postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? localReservation.ExtraPrice : postReservationRequest.ExtraAmount.ToFloatNullSafe() },
                { "amount_extras", postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? postReservationRequest.ExtraAmount.ToFloatNullSafe() : localReservation.APIExtraAmount.ToFloatNullSafe() },
                //{ "amount_vehicle", postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? localReservation.DailyPrice * localReservation.RentalDuration : localReservation.APIDailyPrice * localReservation.RentalDuration },
                { "amount_vehicle",  amountVehicle},
                { "amount_drop", postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? reservationToken.OneWayFee : reservationToken.APIOneWayFee },
                { "amount_paid", apiPaidAmount },// ???????????
                //{ "amount_final", postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? amountFinal : localReservation.APITotalAmount },
                { "amount_final", postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? amountFinal : amountFinal },
                { "extra", localReservation.ReservationExtras.Count > 0 ? localReservation.ReservationExtras.Select(x => new Dictionary<string, object>{
                    { "id", x.ExtraCode.ToIntNullSafe() },
                    //{ "price", 34 },
                    //{ "price", postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? reservationToken.CurrencyType == reservationToken.BaseVendorRequestCurrencyType ? localReservation.APIExtraAmount : CalculateExtraPrice(x.APIPrice * reservationToken.RentalDuration, vendor) : x.APIPrice * reservationToken.RentalDuration }
                    { "price", postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? reservationToken.CurrencyType == reservationToken.BaseVendorRequestCurrencyType ? localReservation.APIExtraAmount : CalculateExtraPrice(x.ApiPrice * reservationToken.RentalDuration, vendor) : x.ApiPrice * reservationToken.RentalDuration }
                    //x.Price * reservationToken.RentalDuration : x.APIPrice * reservationToken.RentalDuration }
                }).ToList() : new List<Dictionary<string, object>>() }
            };
        }

    }
}
