using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerReservationHelper = KolayCAR.Broker.API.Helpers.ReservationHelper;
using CirucularProvider = KolayCAR.Broker.API.Providers.Circular;

namespace KolayCAR.Broker.API.Providers.Circular
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager { get; set; }
        ILocationProvider locationProvider { get; set; }
        AuthProvider AuthProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _configurationService = configurationService;
            locationProvider = new CirucularProvider.LocationProvider(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var postCancelReservationRequestBody = GetCancelParametersForUrlEncoded(postCancelReservationRequest, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postCancelReservationRequestBody),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@CircularPostCancelReservationRequest}", postCancelReservationRequestBody);

            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);

            var result = await RestManager.PostAsyncWithUrlEncoded<List<KeyValuePair<string, string>>, CircularResponseBase.PostCancelReservationResponseBody>(
                //requestPath: $"reservation/save?token={auth.token}&size=500&referralagent={vendor.ApiPassword}",
                requestPath: $"reservation/cancel?token={auth.token}",
                headers: AuthProvider.CreateHeaderWithContentType(),
                entity: postCancelReservationRequestBody,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@CircularPostCancelReservationResponse}", result);

            if (result != null && result.result != null && result.result.status.ToLower() == "canceled")
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
                Message = "Circular Servisine Ulaşılamadı",
                Data = localReservation
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);


            if (auth != null && !string.IsNullOrEmpty(auth.token))
            {
                var postReservationRequestBody = GetParametersForUrlEncoded(postReservationRequest, localReservation, additionalInformation, vendor, reservationToken, apiExtras);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(postReservationRequestBody),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                Serilog.Log.Error("{@CircularPostReservationRequestParameters}", postReservationRequestBody);

                float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);

                var result = await RestManager.PostAsyncWithUrlEncoded<List<KeyValuePair<string, string>>, CircularResponseBase.PostReservationResponseModel>(
                    //requestPath: $"reservation/save?token={auth.token}&size=500&referralagent={vendor.ApiPassword}",
                    requestPath: $"reservation/save?token={auth.token}",
                    headers: AuthProvider.CreateHeaderWithContentType(),
                    entity: postReservationRequestBody,
                    brokerLogModel: new BrokerLogModel()
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                      isReservationRequest: true
                    );

                Serilog.Log.Error("{@CircularPostReservationResult}", result);

                if (result != null && result.success && result.data != null)
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.data.id.ToString();

                    var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);

                    Serilog.Log.Error("{@CircularGetLocationsResponse}", location);

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

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Circular servisine erişim engellendi.",
                Data = localReservation
            };
        }

        private List<KeyValuePair<string, string>> GetParametersForUrlEncoded(PostReservationRequest postReservationRequest, Reservation localReservation, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, ReservationToken reservationToken, List<Extra> apiExtras)
        {
            var isSpecialPrice = GetIsReservationSpecialPrice(vendor, postReservationRequest, localReservation, reservationToken, additionalInformation.Agency, apiExtras) != -1;

            var extras = localReservation.ReservationExtras.Count > 0 ? JsonConvert.SerializeObject(localReservation.ReservationExtras.Select(x => new CircularRequestBase.CircularRequestExtra()
            {
                id = x.ExtraCode.ToIntNullSafe(),
                //value = isSpecialPrice ? x.APIPrice : x.Price,
                value = isSpecialPrice ? x.ApiPrice : x.Price,
                include = 1
            }).ToList()) : string.Empty;

            var parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("id", string.Empty));
            parameters.Add(new KeyValuePair<string, string>("currencycode", reservationToken.BaseVendorRequestCurrencyType.ToString()));
            parameters.Add(new KeyValuePair<string, string>("reservatedate", DateTime.Now.ToString("dd.MM.yyyy")));
            parameters.Add(new KeyValuePair<string, string>("startdate", postReservationRequest.PickupDate));
            parameters.Add(new KeyValuePair<string, string>("starttime", postReservationRequest.PickupTime));
            parameters.Add(new KeyValuePair<string, string>("expectedenddate", postReservationRequest.ReturnDate));
            parameters.Add(new KeyValuePair<string, string>("expectedendtime", postReservationRequest.ReturnTime));
            parameters.Add(new KeyValuePair<string, string>("cargroup", reservationToken.APIReferenceCode2));
            parameters.Add(new KeyValuePair<string, string>("driver", postReservationRequest.CustomerName + " " + postReservationRequest.CustomerSurname));
            parameters.Add(new KeyValuePair<string, string>("paytype", postReservationRequest.PaymentType == PaymentTypes.AdvancePayment || postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? "GCC" : "G/A"));
            parameters.Add(new KeyValuePair<string, string>("pricing", isSpecialPrice ? "special" : "daily"));
            parameters.Add(new KeyValuePair<string, string>("specialprice", isSpecialPrice ? localReservation.TotalPrice.ToFloatNullSafe().ToString() : localReservation.APITotalAmount.ToFloatNullSafe().ToString()));
            parameters.Add(new KeyValuePair<string, string>("dailyprice", isSpecialPrice ? localReservation.DailyPrice.ToString() : localReservation.APIDailyPrice.ToString()));
            parameters.Add(new KeyValuePair<string, string>("outlocation", additionalInformation.APIPickupLocationCode));
            parameters.Add(new KeyValuePair<string, string>("returnlocation", additionalInformation.APIReturnLocationCode));
            parameters.Add(new KeyValuePair<string, string>("referralagent", vendor.ApiClientId.ToIntNullSafe().ToString()));
            parameters.Add(new KeyValuePair<string, string>("referralno", localReservation.ReservationNumber));
            parameters.Add(new KeyValuePair<string, string>("prepayment", isSpecialPrice ? localReservation.PaidAmount.ToString() : "0"));
            parameters.Add(new KeyValuePair<string, string>("flightarrivalnumber", postReservationRequest.FlightNumberArrival));
            parameters.Add(new KeyValuePair<string, string>("dropprice", localReservation.OneWayFee.ToString()));
            parameters.Add(new KeyValuePair<string, string>("extra", extras));

            return parameters;
        }

        private List<KeyValuePair<string, string>> GetCancelParametersForUrlEncoded(PostCancelReservationRequest postCancelReservationRequest, Reservation localReservation)
        {
            var parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("referralno", postCancelReservationRequest.ReservationNumber));
            parameters.Add(new KeyValuePair<string, string>("info", postCancelReservationRequest.CancelNote));
            parameters.Add(new KeyValuePair<string, string>("id", localReservation.APIReservationNumber));

            return parameters;
        }

        #region Özel Fiyat Kontrolü (Tamamlanmadı!)
        public static float GetIsReservationSpecialPrice(Vendor vendor, PostReservationRequest postReservationRequest, Reservation localReservation, ReservationToken reservationToken, Agency agency, List<Extra> apiExtras)
        {
            switch (postReservationRequest.PaymentType)
            {
                default:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayToAgency:
                case PaymentTypes.PayAll:
                    {
                        if (postReservationRequest.PaidAmount >= localReservation.APITotalAmount ||
                        postReservationRequest.ExtraPricePayToDelivery ||
                        postReservationRequest.OneWayFeePayToDelivery ||
                        (!vendor.SellingBelowCostForCouponCode && !string.IsNullOrEmpty(postReservationRequest.CouponCode) && postReservationRequest.PaidAmount < localReservation.APITotalAmount))
                        {
                            if (agency.FreePriceShowActive)
                            {
                                if (postReservationRequest.SpecialDailyPrice != -1)
                                {
                                    if (postReservationRequest.SpecialDailyPrice < reservationToken.APIDailyPrice)
                                    {
                                        return postReservationRequest.SpecialDailyPrice;
                                    }
                                    else
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            return reservationToken.DailyPrice;
                        }
                    }
                case PaymentTypes.PayOnDelivery:
                    {
                        if (agency.FreePriceShowActive)
                        {
                            return postReservationRequest.SpecialDailyPrice != -1 ? postReservationRequest.SpecialDailyPrice : reservationToken.DailyPrice;
                        }
                        else
                        {
                            return reservationToken.DailyPrice;
                        }
                    }
                case PaymentTypes.AdvancePayment:
                    {
                        var isSpecialExtraPriceUse = BrokerReservationHelper.IsSpecialExtraPriceUse(postReservationRequest.ExtraList, apiExtras, vendor.ProfitMarkupAdditionalProducts, postReservationRequest.PaymentType, postReservationRequest, agency, vendor);

                        if (agency.FreePriceShowActive && postReservationRequest.SpecialDailyPrice != -1)
                        {
                            return postReservationRequest.SpecialDailyPrice;
                        }
                        else if (agency.AdvancePaymentAmountByAgencyCommissionActive || isSpecialExtraPriceUse)
                        {
                            return reservationToken.DailyPrice;
                        }
                        else
                        {
                            return -1;
                        }
                    }
            }
        }
        #endregion
    }
}
