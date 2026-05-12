using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Helpers.Avis;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.AvisRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.AvisResponseBase;

namespace KolayCAR.Broker.API.Providers.Avis
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly ExtraProvider _extraProvider;
        private readonly AuthProvider _authProvider;
        private readonly IConfigurationService _configurationService;
        private readonly ILocationProvider _locationProvider;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _configurationService = configurationService;
            _extraProvider = new ExtraProvider();
            _authProvider = new AuthProvider(apiBaseUrl);
            _locationProvider = new LocationProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Domain.Models.Reservation localReservation)
        {
            var token = await _authProvider.GetTokenAsync(vendor);
            var cancelRequestEntity = GetCancelEntity(postCancelReservationRequest, vendor, localReservation);
            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(cancelRequestEntity),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@AvisPostCancelReservationsRequestParameters}", localReservation.APIReferenceCode);

            var result = await _httpManager.PostAsync2<AvisCancelReservationRequest, AvisCancelReservationResponse>(
            requestPath: @"/STReservationApp/Cancel",
            headers: QueryHelper<AvisCancelReservationRequest>.GetHeaders(token, cancelRequestEntity, vendor.SecretKey),
            entity: cancelRequestEntity,
            brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
            }, isReservationRequest: true);

            Serilog.Log.Error("{@AvisPostCancelReservationsResponse}", result);

            if (result != null && (bool)result?.Data?.Result)
            {
                localReservation.APIReservationCancel = true;
                return new ServiceResponseBase(localReservation, true);
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisi rezervasyon iptali başarısız!");
        }

        private AvisCancelReservationRequest GetCancelEntity(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Domain.Models.Reservation localReservation)
        {
            return new AvisCancelReservationRequest
            {
                CountryCode = "TR",
                Brand = vendor.VendorName,
                ReservationNo = localReservation.APIReservationNumber,
                LastName = localReservation.CustomerSurname,
                TransactionId = localReservation.APIReferenceCode2,
                IsRefund = false,
                LogUserId = vendor.ApiClientId.Split("-")[1].ToIntNullSafe(),
                ReturnAmount = 0.0,
                ExternalRefund = 0.0
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Domain.Models.Reservation localReservation, List<Extra> apiExtras)
        {
            var token = await _authProvider.GetTokenAsync(vendor);

            var rateTotal = JsonConvert.DeserializeObject<AvisResponseBase.RateTotals>(reservationToken.APIReferenceCode3);

            var entity = vendor.VendorName == "Budget" ?
                    GetEntityBudget(vendor, reservationToken, postReservationRequest, additionalInformation, localReservation, rateTotal) :
                    GetEntityAvis(vendor, reservationToken, postReservationRequest, additionalInformation, localReservation, rateTotal);

            var result = new HttpResult<AvisPostReservationResponse>();

            if (vendor.VendorName == "Budget")
            {
                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(entity),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                Serilog.Log.Error("{@BudgetPostReservationRequestParameters}", entity);

                result = await _httpManager.PostAsync2<AvisPostReservationRequest, AvisPostReservationResponse>(
                          requestPath: "/STReservationApp/Create",
                          headers: QueryHelper<AvisPostReservationRequest>.GetHeaders(token, entity, vendor.SecretKey),
                          entity: entity,
                          brokerLogModel: new BrokerLogModel
                          {
                              LogKey = localReservation.ReservationNumber,
                              LogType = BrokerLogTypes.ReservationVendorAPIResponse
                          },
                          isReservationRequest: true
                     );
            }
            else
            {
                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(entity),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                Serilog.Log.Error("{@AvisPostReservationRequestParameters}", entity);

                result = await _httpManager.PostAsync2<AvisPostReservationRequest, AvisPostReservationResponse>(
                    requestPath: "/STReservationApp/Create",
                    headers: QueryHelper<AvisPostReservationRequest>.GetHeaders(token, entity, vendor.SecretKey),
                    entity: entity,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                    isReservationRequest: true
               );

            }

            if ((bool)(result?.Data?.Result))
            {
                if (result.Data.Data != null)
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.Data.Data.reservation.confirmation.number;
                    localReservation.APIReferenceCode2 = result.Data.Data.transaction.transaction_id;
                    return new ServiceResponseBase(localReservation, true);
                }
                else
                {
                    Serilog.Log.Error("{AvisReservationError}", "rezervasyon oluşturulamadı");
                    localReservation.APIMessage = "rezervasyon oluşturulamadı";
                }
            }
            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisinden veri alınamadı!");
        }
        private AvisPostReservationRequest GetEntityBudget(Vendor vendor, ReservationToken reservationToken, PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Domain.Models.Reservation localReservation, AvisResponseBase.RateTotals rateTotals)
        {
            var extras = _extraProvider.GetExtrasListFromExcel();
            var additionalProducts = new List<AdditionalProduct>();
            var paymentAmount = Math.Round(rateTotals.pay_now.original_reservation_total - rateTotals.rate.coupon_discount_amount, 2);

            if (postReservationRequest.PostReservationRequestV2 != null)
            {
                if (postReservationRequest.PostReservationRequestV2.Extras.Count > 0)
                {
                    foreach (var extra in postReservationRequest.PostReservationRequestV2.Extras)
                    {
                        additionalProducts.Add(new AdditionalProduct
                        {
                            ProductNo = extras.FirstOrDefault(e => e.ExtraCode == extra.ExtraCode).ExtraId,
                            ProductCount = 1
                        });
                    }
                }
            }

            if (postReservationRequest.ExtraList != "" && postReservationRequest.ExtraList != null)
            {
                if (postReservationRequest.ExtraList.Contains("|"))
                {
                    string[] extraArray = postReservationRequest.ExtraList.Split("|");
                    foreach (var item in extraArray)
                    {
                        additionalProducts.Add(new AdditionalProduct
                        {
                            ProductNo = extras.Where(x => x.ExtraCode == item.Split("~")[0]).Select(x => x.ExtraId).First(),
                            ProductCount = 1
                        });
                    }
                }
                else
                {
                    additionalProducts.Add(new AdditionalProduct
                    {
                        ProductNo = extras.Where(x => x.ExtraCode == postReservationRequest.ExtraList.Split("~")[0]).Select(x => x.ExtraId).First(),
                        ProductCount = 1,
                    });
                }

            }

            var entity = new AvisPostReservationRequest()
            {
                tempest = new Tempest
                {
                    product = new Product { brand = vendor.VendorName },
                    transaction = new Transaction { transaction_id = reservationToken.APIReferenceCode },
                    reservation = new AvisRequestBase.Reservation
                    {
                        pickup_date = additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        pickup_location = additionalInformation.APIPickupLocationCode,
                        dropoff_date = additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        dropoff_location = additionalInformation.APIReturnLocationCode,
                        vehicle_class_code = reservationToken.VehicleCode,
                        email_notification = false
                    },
                    rate_totals = new AvisRequestBase.RateTotals
                    {
                        rate = new AvisRequestBase.Rate
                        {
                            rate_code = rateTotals.rate.rate_code,
                            country_code = "TR",
                            discount = new Discount
                            {
                                code = vendor.ApiClientId.Contains("-") ? vendor.ApiClientId.Split('-')[0] : ""
                            },
                            loyalty = new Loyalty { },
                            membership = null,
                            coupon = null
                        }
                    },
                    passenger = new Passenger
                    {
                        contact = new Contact
                        {

                            first_name = postReservationRequest.CustomerName,
                            last_name = postReservationRequest.CustomerSurname,
                            telephone = postReservationRequest.CustomerTelephone.Replace(" ", ""),
                            email = postReservationRequest.CustomerEmail,
                            date_of_birth = postReservationRequest.CustomerBirthDay.ToDateTimeNullSafe().ToString("yyyy-MM-dd"),
                            age = DateTime.Now.Year - postReservationRequest.CustomerBirthDay.ToDateTimeNullSafe().Year,
                            title = "MR"
                        },
                        address = new AvisRequestBase.Address
                        {
                            address_line_1 = "test",
                            country_code = "TR"
                        },
                        driver = new Driver
                        {
                        }
                    },

                    arrival_flight = new ArrivalFlight
                    {
                        airline_code = "",
                        airline_number = ""
                    }
                },

                local = new Local
                {
                    PaymentAmount = paymentAmount,
                    DiscountAmount = rateTotals.rate.coupon_discount_amount,
                    DriverCity = 1,
                    DriverDistrict = 0,
                    TransmissionType = reservationToken.TransmissionTypeName,
                    VehicleCategory = reservationToken.ApiVehicleId.ToStringNullSafe(),
                    VehClassSize = reservationToken.VehicleClassSize.ToStringNullSafe(),
                    DriverTaxNumber = (postReservationRequest.CustomerPersonalNumber.Any(x => !char.IsLetter(x)) && postReservationRequest.CustomerPersonalNumber.Count() == 11) ? postReservationRequest.CustomerPersonalNumber : "",
                    DriverPassportNo = (postReservationRequest.CustomerPersonalNumber.Any(x => char.IsLetter(x)) || postReservationRequest.CustomerPersonalNumber.Count() < 11) ? postReservationRequest.CustomerPersonalNumber : "",
                    AdditinoalHour = 0,
                    RateCodeId = reservationToken.APIReferenceCode2,
                    PaymentAmountType = 0,
                    additionalProducts = additionalProducts ?? null,
                    CompanyName = "Obilet",
                    ReservationNumber = null,
                    TotalPrice = rateTotals.pay_now.original_reservation_total,
                    DeliveryAddress = "",
                    CollectionAddress = "",
                    OneWayAmount = reservationToken.OneWayFee,
                    NameOnCreditCard = "",
                    CreditCardNo = "",
                    CreditCardExpirationDate = ""
                },
                CountryCode = "TR",
                Brand = vendor.VendorName,
                LogUserId = vendor.ApiClientId.Contains("-") ? vendor.ApiClientId.Split('-')[1].ToIntNullSafe() : 0,
            };
            return entity;
        }

        public AvisPostReservationRequest GetEntityAvis(Vendor vendor, ReservationToken reservationToken, PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Domain.Models.Reservation reservation, AvisResponseBase.RateTotals rateTotals)
        {
            var paymentAmount = Math.Round(reservationToken.APIDailyPrice, 2) * reservation.RentalDuration;
            var extras = _extraProvider.GetExtrasListFromExcel();
            var totalPrice = Math.Round(reservationToken.DailyPrice, 2) * reservation.RentalDuration;
            var additionalProducts = new List<AdditionalProduct>();

            if (postReservationRequest.PostReservationRequestV2 != null)
            {
                if (postReservationRequest.PostReservationRequestV2.Extras.Count > 0)
                {
                    foreach (var extra in postReservationRequest.PostReservationRequestV2.Extras)
                    {
                        additionalProducts.Add(new AdditionalProduct
                        {
                            ProductNo = extras.FirstOrDefault(e => e.ExtraCode == extra.ExtraCode).ExtraId,
                            ProductCount = 1
                        });
                    }
                }
            }


            if (postReservationRequest.ExtraList != "" && postReservationRequest.ExtraList != null)
            {
                if (postReservationRequest.ExtraList.Contains("|"))
                {
                    string[] extraArray = postReservationRequest.ExtraList.Split("|");
                    foreach (var item in extraArray)
                    {
                        additionalProducts.Add(new AdditionalProduct
                        {
                            ProductNo = extras.Where(x => x.ExtraCode == item.Split("~")[0]).Select(x => x.ExtraId).First(),
                            ProductCount = 1
                        });
                    }
                }
                else
                {
                    additionalProducts.Add(new AdditionalProduct
                    {
                        ProductNo = extras.Where(x => x.ExtraCode == postReservationRequest.ExtraList.Split("~")[0]).Select(x => x.ExtraId).First(),
                        ProductCount = 1,
                        //ProductAmount = Convert.ToDouble(postReservationRequest.ExtraList.Split("~")[2], CultureInfo.InvariantCulture) 
                    });
                }
            }

            var entity = new AvisPostReservationRequest()
            {
                tempest = new Tempest
                {
                    product = new Product { brand = vendor.VendorName },
                    transaction = new Transaction { transaction_id = reservationToken.APIReferenceCode },
                    reservation = new AvisRequestBase.Reservation
                    {
                        pickup_date = additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        pickup_location = additionalInformation.APIPickupLocationCode,
                        dropoff_date = additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        dropoff_location = additionalInformation.APIReturnLocationCode,
                        vehicle_class_code = reservationToken.VehicleCode,
                        email_notification = false
                    },
                    rate_totals = new AvisRequestBase.RateTotals
                    {
                        rate = new AvisRequestBase.Rate
                        {
                            rate_code = rateTotals.rate.rate_code,
                            country_code = "TR",
                            discount = new Discount
                            {
                                code = vendor.ApiClientId.Contains("-") ? vendor.ApiClientId.Split('-')[0] : ""
                            },
                            loyalty = new Loyalty { },
                            membership = null,
                            coupon = null
                        }
                    },
                    passenger = new Passenger
                    {
                        contact = new Contact
                        {

                            first_name = postReservationRequest.CustomerName,
                            last_name = postReservationRequest.CustomerSurname,
                            telephone = postReservationRequest.CustomerTelephone.Replace(" ", ""),
                            email = postReservationRequest.CustomerEmail,
                            date_of_birth = postReservationRequest.CustomerBirthDay.ToDateTimeNullSafe().ToString("yyyy-MM-dd"),
                            age = DateTime.Now.Year - postReservationRequest.CustomerBirthDay.ToDateTimeNullSafe().Year,
                            title = "MR"
                        },
                        address = new AvisRequestBase.Address
                        {
                            address_line_1 = "test",
                            country_code = "TR"
                        },
                        driver = new Driver
                        {
                        }
                    },

                    arrival_flight = new ArrivalFlight
                    {
                        airline_code = "",
                        airline_number = ""
                    }
                },

                local = new Local
                {

                    DriverCity = 1,
                    DriverDistrict = 0,
                    TransmissionType = reservationToken.TransmissionTypeName,
                    VehicleCategory = reservationToken.ApiVehicleId.ToStringNullSafe(),
                    VehClassSize = reservationToken.VehicleClassSize.ToStringNullSafe(),
                    DriverTaxNumber = (postReservationRequest.CustomerPersonalNumber.Any(x => !char.IsLetter(x)) && postReservationRequest.CustomerPersonalNumber.Count() == 11) ? postReservationRequest.CustomerPersonalNumber : "",
                    DriverPassportNo = (postReservationRequest.CustomerPersonalNumber.Any(x => char.IsLetter(x)) || postReservationRequest.CustomerPersonalNumber.Count() < 11) ? postReservationRequest.CustomerPersonalNumber : "",
                    AdditinoalHour = 0,
                    RateCodeId = reservationToken.APIReferenceCode2,
                    PaymentAmount = rateTotals.pay_now.vehicle_total,
                    PaymentAmountType = 0,
                    additionalProducts = additionalProducts ?? null,
                    CompanyName = "Obilet",
                    ReservationNumber = null,
                    TotalPrice = rateTotals.pay_now.reservation_total,
                    DeliveryAddress = "",
                    CollectionAddress = "",
                    OneWayAmount = reservationToken.OneWayFee,
                    NameOnCreditCard = "",
                    CreditCardNo = "",
                    CreditCardExpirationDate = ""
                },
                CountryCode = "TR",
                Brand = vendor.VendorName,
                LogUserId = vendor.ApiClientId.Contains("-") ? vendor.ApiClientId.Split('-')[1].ToIntNullSafe() : 0,
            };
            return entity;
        }
    }
}
