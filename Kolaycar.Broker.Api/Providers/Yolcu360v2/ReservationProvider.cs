using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Yolcu360v2;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.Yolcu360v2RequestBase;
using static KolayCAR.Broker.Domain.Models.Response.Yolcu360v2.Yolcu360v2ResponseBaseDeleted;

namespace KolayCAR.Broker.API.Providers.Yolcu360v2
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly AuthProvider _authProvider;
        private readonly IConfigurationService _configurationService;
        public ReservationProvider(string apibaseUrl, IConfigurationService configurationService, ICacheService cacheService)
        {
            _httpManager = new HttpManager(apibaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _authProvider = new AuthProvider(apibaseUrl, cacheService);
            _configurationService = configurationService;
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var auth = await _authProvider.GetToken(vendor);
            if (auth == null)
                return new(localReservation, false, $"{vendor.VendorName} token bilgisi alınamadı!");

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = localReservation.APIReservationNumber,
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            var cancellableResult = await _httpManager.PostAsyncWithModel<Yolcu360v2ReservationResponseBase.CancellableResponse>
            (
                requestPath: $"/api/v1/order/{localReservation.APIReferenceCode2}/cancel_eligibility",
                headers: auth
            );

            Serilog.Log.Error("{@Yolcu360v2CheckCancellableResponse}", $"{localReservation.ReservationNumber} - {cancellableResult.ToJson()}");

            if (!cancellableResult.cancellable || !cancellableResult.refundable)
            {
                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = cancellableResult.ToJson(),
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                });
                return new(localReservation, false, $"{vendor.VendorName} rezervasyon iptale uygun değil!");
            }

            var result = await _httpManager.PostAsyncWithModel<Yolcu360v2ReservationResponseBase.CancelReservationResponse>
            (
                requestPath: $"/api/v1/order/{localReservation.APIReferenceCode2}/cancel",
                headers: auth,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }
            );

            Serilog.Log.Error("{@Yolcu360v2CancelReservationResponse}", $"{localReservation.ReservationNumber} - {cancellableResult.ToJson()}");

            if (result.success && result.status == "success")
            {
                localReservation.APIReservationCancel = true;
                return new(localReservation, true);
            }

            return new(localReservation, false, $"{vendor.VendorName} servisi rezervasyon iptali başarısız!");
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var auth = await _authProvider.GetToken(vendor);
            if (auth == null)
                return new(localReservation, false, $"{vendor.VendorName} token bilgisi alınamadı!");

            var entity = GetEntity(postReservationRequest, reservationToken, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(entity),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@Yolcu360v2ReservationRequestBody}", entity.ToJson());

            var result = await _httpManager.PostAsyncWithModel<Yolcu360v2PostReservationRequest, Yolcu360v2ReservationResponseBase.Root>
            (
                requestPath: "/api/v1/order",
                entity: entity,
                headers: GetHeaders(auth, reservationToken.BaseVendorRequestCurrencyType),
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true
            );

            Serilog.Log.Error("{@Yolcu360v2ReservationResult}", result.ToJson());

            if (string.IsNullOrEmpty(result?.id))
                return new(localReservation, false, $"{vendor.VendorName} rezervasyon isteği başarısız!");

            var payEntity = new Yolcu360v2PostPayRequest { orderID = result.id, paymentType = "limit" };

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(payEntity),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            var resultPayment = await _httpManager.PostAsyncWithModel<Yolcu360v2PostPayRequest, Yolcu360v2PaymentResponseBase.Root>
            (
                requestPath: "/api/v1/payment/pay ",
                entity: payEntity,
                headers: GetHeaders(auth, reservationToken.BaseVendorRequestCurrencyType),
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true
            );

            var isSuccess = resultPayment?.orderedCarProduct != null
                && resultPayment.orderedCarProduct.status?.ToLower() == "reserved"
                && resultPayment.orderedCarProduct.vendorCancelled == false;

            if (!isSuccess)
            {
                Serilog.Log.Error("{@Yolcu360v2PaymentError}", resultPayment.ToJson());
                return new(localReservation, false, $"{vendor.VendorName} rezervasyon isteği başarısız!");
            }

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;
            localReservation.APIReservationSuccessfully = true;
            localReservation.APIReservationNumber = !string.IsNullOrEmpty(resultPayment.orderedCarProduct.vendorReservationID) ? resultPayment.orderedCarProduct.vendorReservationID : resultPayment.id;
            localReservation.APIReferenceCode2 = resultPayment.id;

            if (result?.orderedCarProduct?.car?.appointment?.checkInOffice != null && result?.orderedCarProduct?.car?.appointment?.checkOutOffice != null)
            {
                localReservation.PickupOfficeWorkingHours = GetOfficeWorkingHours(result.orderedCarProduct.car.appointment.checkInOffice, localReservation.PickupDate);
                localReservation.ReturnOfficeWorkingHours = GetOfficeWorkingHours(result.orderedCarProduct.car.appointment.checkOutOffice, localReservation.ReturnDate);
                localReservation.APIVendorPickupAddress = $"{result.orderedCarProduct.car.appointment.checkInOffice.address.adm1} - {result.orderedCarProduct.car.appointment.checkInOffice.address.adm2 + result.orderedCarProduct.car.appointment.checkInOffice.address.street}";
                localReservation.APIVendorPickupPhone = result.orderedCarProduct.car.appointment.checkInOffice.phones.FirstOrDefault();
                localReservation.APIVendorReturnAddress = $"{result.orderedCarProduct.car.appointment.checkOutOffice.address.adm1} - {result.orderedCarProduct.car.appointment.checkOutOffice.address.adm2 + result.orderedCarProduct.car.appointment.checkOutOffice.address.street}";
                localReservation.APIVendorReturnPhone = result.orderedCarProduct.car.appointment.checkOutOffice.phones.FirstOrDefault();

                var deliveryType = result.orderedCarProduct.car.appointment.checkInOffice.deliveryType.id;
                localReservation.IsOffice = deliveryType == (3 | 5) ? true : false;
            }
            return new(localReservation, true);
        }

        private Yolcu360v2PostReservationRequest GetEntity(PostReservationRequest postReservationRequest, ReservationToken reservationToken, Reservation localReservation)
        {
            List<Yolcu360v2RequestBase.ExtraProduct> extras = null;
            if (!string.IsNullOrEmpty(postReservationRequest.ApiExtras) && !string.IsNullOrEmpty(postReservationRequest.ExtraList) && localReservation.ReservationExtras?.Count > 0)
            {
                postReservationRequest.ApiExtras.Trim('{', '}');
                var parts = postReservationRequest.ApiExtras.Split('|', StringSplitOptions.RemoveEmptyEntries);

                var apiExtras = parts.Select(p =>
                    {
                        var split = p.Split('~', StringSplitOptions.RemoveEmptyEntries);
                        return new ExtraItem
                        {
                            Code = split.ElementAtOrDefault(0),
                            ApiCode = split.ElementAtOrDefault(1)
                        };
                    }).Where(x => !string.IsNullOrEmpty(x.Code) && !string.IsNullOrEmpty(x.ApiCode)).ToList();

                extras = localReservation.ReservationExtras.Select(item => new Yolcu360v2RequestBase.ExtraProduct { code = apiExtras.FirstOrDefault(e => e.Code == item.ExtraCode).ApiCode, quantity = 1 }).ToList();
            }
            if (postReservationRequest.PostReservationRequestV2?.Extras?.Count > 0)
                extras = postReservationRequest.PostReservationRequestV2.Extras?.Where(e => e.VendorExtraExists)?.Select(item => new Yolcu360v2RequestBase.ExtraProduct { code = item.ApiExtraCode, quantity = item.Piece }).ToList();

            var entity = new Yolcu360v2PostReservationRequest
            {
                paymentType = "limit",
                searchID = reservationToken.APIReferenceCode,
                code = reservationToken.VehicleCode,
                extraProducts = extras,
                passenger = new Yolcu360v2RequestBase.Passenger
                {
                    firstName = postReservationRequest.CustomerName,
                    lastName = postReservationRequest.CustomerSurname,
                    email = postReservationRequest.CustomerEmail,
                    nationality = "TR",
                    phone = postReservationRequest.CustomerTelephone.Replace(" ", string.Empty),
                    identityNumber = postReservationRequest.CustomerPersonalNumber.Any(char.IsLetter) ? null : postReservationRequest.CustomerPersonalNumber,
                    passportNo = postReservationRequest.CustomerPersonalNumber.Any(char.IsLetter) ? postReservationRequest.CustomerPersonalNumber : null,
                    birthDate = postReservationRequest.CustomerBirthDay.ToDateTimeNullSafe().ToString("yyyy-MM-dd")
                },
                isFullCredit = reservationToken.FullCredit,
                isLimitedCredit = false,
                trackingID = postReservationRequest.SendAgencyReservationNumber ? postReservationRequest.AgencyReservationReference : ""
            };
            return entity;
        }

        private string GetOfficeWorkingHours(Yolcu360v2ReservationResponseBase.CheckInOffice office, DateTime tarih)
        {
            if (office == null) return "";

            var day = ((int)tarih.DayOfWeek + 6) % 7 + 1; // Pazartesi=1 ... Pazar=7
            var hours = office.openingHours.FirstOrDefault(e => e.dayOfWeek == day);

            return hours != null ? $"{hours.open} - {hours.close}" : "";
        }
        private string GetOfficeWorkingHours(Yolcu360v2ReservationResponseBase.CheckOutOffice office, DateTime tarih)
        {
            if (office == null) return "";

            var day = ((int)tarih.DayOfWeek + 6) % 7 + 1; // Pazartesi=1 ... Pazar=7
            var hours = office.openingHours.FirstOrDefault(e => e.dayOfWeek == day);

            return hours != null ? $"{hours.open} - {hours.close}" : "";
        }
        private IDictionary<string, object> GetHeaders(IDictionary<string, object> token, CurrencyTypes baseVendorRequestCurrencyType)
        {
            var headers = new Dictionary<string, object>(token ?? new Dictionary<string, object>());

            headers["X-Currency"] = GetYolcu360v2CurrencyTypes(baseVendorRequestCurrencyType.ToString());

            return headers;
        }
        private string GetYolcu360v2CurrencyTypes(string baseVendorRequestCurrencyType) => baseVendorRequestCurrencyType switch
        {
            "TRY" => Yolcu360v2CurrencyTypes.TRY.ToString(),
            "USD" => Yolcu360v2CurrencyTypes.USD.ToString(),
            "EUR" => Yolcu360v2CurrencyTypes.EUR.ToString(),
            _ => "Error"
        };
        class ExtraItem
        {
            public string Code { get; set; }
            public string ApiCode { get; set; }
        }
    }
}
