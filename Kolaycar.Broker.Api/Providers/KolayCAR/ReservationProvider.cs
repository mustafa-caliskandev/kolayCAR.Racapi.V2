using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Resws;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Resws.ServiceSoapClient;
using KolayCARHelper = KolayCAR.Broker.API.Helpers.KolayCAR;

namespace KolayCAR.Broker.API.Providers.KolayCAR
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly ServiceSoapClient _kolayCARService;
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(Vendor vendor, IConfigurationService configurationService)
        {
            _kolayCARService = new ServiceSoapClient(EndpointConfiguration.ServiceSoap, KolayCARHelper.ReservationHelper.GetSoapRemoteAddress(vendor.APIBaseUrl));
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            try
            {
                var postCancelKolayCARReservationRequest = new KolayCARRequest.PostCancelKolayCARReservationRequest
                {
                    APIKEY = vendor.ApiKey,
                    APIPASSWORD = vendor.ApiPassword,
                    LANGISOCODE = postCancelReservationRequest.LanguageCode,
                    RESERVATIONNO = localReservation.APIReservationNumber,
                    COMMENT = postCancelReservationRequest.CancelNote ?? string.Empty,
                    TOKEN = string.Empty,
                    PARAM1 = string.Empty,
                    PARAM2 = string.Empty,
                    PARAM3 = string.Empty,
                    PARAM4 = string.Empty,
                    PARAM5 = string.Empty,
                    PARAM6 = string.Empty
                };

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(postCancelKolayCARReservationRequest),
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
                });

                var postCancelReservationResult = await _kolayCARService.POST_RESERVATION_CANCEL1Async(
                    postCancelKolayCARReservationRequest.APIKEY,
                    postCancelKolayCARReservationRequest.APIPASSWORD,
                    postCancelKolayCARReservationRequest.LANGISOCODE,
                    postCancelKolayCARReservationRequest.RESERVATIONNO,
                    postCancelKolayCARReservationRequest.COMMENT,
                    postCancelKolayCARReservationRequest.TOKEN,
                    postCancelKolayCARReservationRequest.PARAM1,
                    postCancelKolayCARReservationRequest.PARAM2,
                    postCancelKolayCARReservationRequest.PARAM3,
                    postCancelKolayCARReservationRequest.PARAM4,
                    postCancelKolayCARReservationRequest.PARAM5,
                    postCancelKolayCARReservationRequest.PARAM6);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = postCancelReservationResult.POST_RESERVATION_CANCEL_V2Result,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                });

                Serilog.Log.Error("{@KolayCARPostCancelReservationsResponse}", postCancelReservationResult.POST_RESERVATION_CANCEL_V2Result);
                var postCancelReservationResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(postCancelReservationResult.POST_RESERVATION_CANCEL_V2Result);

                if (postCancelReservationResponseObject.RETURNCODE == 0)
                {
                    localReservation.APIReservationCancel = true;

                    return new ServiceResponseBase
                    {
                        Success = postCancelReservationResponseObject.RETURNCODE == 0,
                        ServiceCode = postCancelReservationResponseObject.RETURNCODE.ToString(),
                        Data = localReservation
                    };
                }
                if (postCancelReservationResponseObject.RETURNCODE == 2049)
                {
                    localReservation.APIReservationCancel = true;

                    return new ServiceResponseBase
                    {
                        Success = postCancelReservationResponseObject.RETURNCODE == 2049,
                        ServiceCode = postCancelReservationResponseObject.RETURNCODE.ToString(),
                        Data = localReservation
                    };
                }

                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "KolayCAR servisine ulaşılamadı!",
                    Data = localReservation,
                    ServiceCode = postCancelReservationResponseObject.RETURNCODE.ToString(),
                    ServiceMessage = postCancelReservationResponseObject.MESSAGE
                };
            }
            #region Soap Exceptions Catches
            catch (System.Net.WebException ex)
            {
                Serilog.Log.Fatal("{@KolayCarPostReservationError}", ex.Message);
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "KolayCar servisinde bir hata meydana geldi.",
                    Data = localReservation
                };
            }
            catch (TimeoutException ex)
            {
                Serilog.Log.Fatal("{@KolayCarPostReservationError}", ex.Message);
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "KolayCar servisinde bir hata meydana geldi.",
                    Data = localReservation
                };
            }
            catch (System.ServiceModel.FaultException ex)
            {
                Serilog.Log.Fatal("{@KolayCarPostReservationError}", ex.Message);
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "KolayCar servisinde bir hata meydana geldi.",
                    Data = localReservation
                };
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                Serilog.Log.Fatal("{@KolayCarPostReservationError}", ex.Message);
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "KolayCar servisinde bir hata meydana geldi.",
                    Data = localReservation
                };
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal("{@KolayCarPostReservationError}", ex.Message);
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "KolayCar servisinde bir hata meydana geldi.",
                    Data = localReservation
                };
            }
            #endregion
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            try
            {
                string fotmatExtraString = string.Empty;
                var formattedExtra = ObjectHelper.ParseFormattedExtra(postReservationRequest.ExtraList);
                formattedExtra.RemoveAll(e => e.ApiCode == "PRMPKT-1");

                if (postReservationRequest.PostReservationRequestV2?.Extras?.Count > 0)
                    fotmatExtraString = string.Join(",", postReservationRequest.PostReservationRequestV2.Extras.Select(e => $"{e.ExtraCode}-{e.Piece}" + (postReservationRequest.PostReservationRequestV2.Payment.PaymentType == PaymentTypes.AdvancePayment ? $"-{e.Price}" : "")));


                //var formattedExtraListString = GetFormattedExtraListStrig(postReservationRequest.PostReservationRequestV2.Extras);
                var formattedExtraList = KolayCARHelper.ExtraHelper.FormatExtrasWithAPIPrices(postReservationRequest.ExtraList, apiExtras, formattedExtra, additionalInformation.Agency.FreePriceActive, additionalInformation.Agency.FreePriceShowActive, vendor.ProfitMarkupAdditionalProducts, postReservationRequest.PaymentType, exchangeRates, vendor, postReservationRequest, additionalInformation.Agency, reservationToken.BaseVendorRequestCurrencyType);
                int vehicleId = reservationToken.VehicleCode.Split('-')[0].ToIntNullSafe();

                var extras = postReservationRequest?.PostReservationRequestV2?.Extras;

                postReservationRequest.ExtraAmount = extras?.Count > 0 ? extras.Sum(e => e.ExtraRentalType == ExtraRentalTypes.PerRental ? e.ApiPrice : e.ApiPrice * reservationToken.RentalDuration) : KolayCARHelper.ExtraHelper.GetTotalExtraPrice(formattedExtraList, apiExtras, formattedExtra, reservationToken.RentalDuration);

                float specialDailyPrice = KolayCARHelper.ReservationHelper.GetKolayCARSpecialDailyPrice(additionalInformation.Agency, reservationToken, postReservationRequest, apiExtras, vendor, localReservation);
                float specialOneWayFee = KolayCARHelper.ReservationHelper.GetKolayCARSpecialOneWayFee(additionalInformation.Agency, reservationToken, postReservationRequest, apiExtras, vendor, localReservation);

                float apiPaidAmount = localReservation.PaymentType == PaymentTypes.AdvancePayment && localReservation.RentalWorkingType == VendorWorkingTypes.ProfitMarkup && localReservation.AdditionalProductWorkingType == VendorWorkingTypes.ProfitMarkup && localReservation.OneWayFeeWorkingType == VendorWorkingTypes.ProfitMarkup && specialDailyPrice == -1 && specialOneWayFee == -1 ? 0 : localReservation.APIPaidAmount;
                //CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);

                if (postReservationRequest.PaymentType != PaymentTypes.PayOnDelivery && postReservationRequest.PaymentType != PaymentTypes.AdvancePayment)
                {
                    apiPaidAmount = 0;

                    if (postReservationRequest.SpecialDailyPrice != -1 && specialDailyPrice != -1)
                    {
                        var specialPrice = specialDailyPrice * reservationToken.RentalDuration;
                        apiPaidAmount = apiPaidAmount + specialPrice;
                    }
                    else
                    {
                        var vehiclePrice = reservationToken.APIDailyPrice * reservationToken.RentalDuration;
                        apiPaidAmount = apiPaidAmount + vehiclePrice;
                    }

                    if (!postReservationRequest.ExtraPricePayToDelivery)
                    {
                        var extaPrice = localReservation.APIExtraAmount;
                        apiPaidAmount = apiPaidAmount + extaPrice;
                    }

                    if (!postReservationRequest.OneWayFeePayToDelivery)
                    {
                        var oneWayFee = localReservation.APIOneWayFee;
                        apiPaidAmount = apiPaidAmount + oneWayFee;
                    }
                }

                if (localReservation.DailyPrice * localReservation.RentalDuration <= localReservation.CouponDiscountValue)
                {
                    apiPaidAmount = localReservation.DailyPrice * localReservation.RentalDuration;
                }

                //v1 de aşağıdaki kod var testlerden sonra kontrol edilecek
                if (postReservationRequest.CouponCode != "" && postReservationRequest.CouponCode != null)
                {
                    #region gkursad - 17.05.2024
                    //if (localReservation.CouponDiscountType == CouponDiscountTypes.ByPercent)
                    //    specialDailyPrice = CalculationHelper.RoundPrice((reservationToken.DailyPrice * (100 - localReservation.CouponDiscountValue) / 100), (int)vendor.PriceRoundingType);
                    //if (localReservation.CouponDiscountType == CouponDiscountTypes.ByPrice)
                    //    specialDailyPrice = CalculationHelper.RoundPrice((reservationToken.DailyPrice * reservationToken.RentalDuration - localReservation.CouponDiscountValue) / reservationToken.RentalDuration, (int)vendor.PriceRoundingType); 
                    #endregion
                }
                ;
                if (!string.IsNullOrEmpty(postReservationRequest.CouponCode) && postReservationRequest.PaymentType == PaymentTypes.PayAll)
                {
                    specialDailyPrice = -1;
                }

                if (!string.IsNullOrEmpty(postReservationRequest.CouponCode) && postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery)
                {
                    if (localReservation.CouponDiscountType == CouponDiscountTypes.ByPercent)
                        specialDailyPrice = CalculationHelper.RoundPrice((reservationToken.DailyPrice * (100 - localReservation.CouponDiscountValue) / 100), (int)vendor.PriceRoundingType);
                    if (localReservation.CouponDiscountType == CouponDiscountTypes.ByPrice)
                        specialDailyPrice = CalculationHelper.RoundPrice((reservationToken.DailyPrice * reservationToken.RentalDuration - localReservation.CouponDiscountValue) / reservationToken.RentalDuration, (int)vendor.PriceRoundingType);
                }

                string paymentInfo = GetPaymentInfo(postReservationRequest, additionalInformation);

                var postKolayCARReservationRequest = new KolayCARRequest.PostKolayCARReservationRequest
                {
                    APIKEY = vendor.ApiKey,
                    APIPASSWORD = vendor.ApiPassword,
                    LANGISOCODE = postReservationRequest.LanguageCode,
                    CURRENCYISOCODE = reservationToken.BaseVendorRequestCurrencyType.ToString(),
                    PICKUPLOCATIONID = reservationToken.APIPickupLocationCode.ToString(),
                    RETURNLOCATIONID = reservationToken.APIReturnLocationCode.ToString(),
                    PICKUPDATE = postReservationRequest.PickupDate,
                    RETURNDATE = postReservationRequest.ReturnDate,
                    PICKUPTIME = postReservationRequest.PickupTime,
                    RETURNTIME = postReservationRequest.ReturnTime,
                    VENDORID = reservationToken.APIVendorId.ToString(),
                    VEHICLEID = vehicleId.ToString(),
                    EXTRAIDLIST = string.IsNullOrEmpty(fotmatExtraString) ? formattedExtraList : fotmatExtraString,
                    CUSTOMERINSTUTIONTYPENO = postReservationRequest.CustomerInstutionTypeNumber.ToStringNullSafe(),
                    CUSTOMERNAME = postReservationRequest.CustomerName,
                    CUSTOMERSURNAME = postReservationRequest.CustomerSurname,
                    CUSTOMERTELEPHONE = postReservationRequest.CustomerTelephone ?? string.Empty,
                    CUSTOMEREMAIL = postReservationRequest.CustomerEmail,
                    CUSTOMERPERSONALNUMBER = postReservationRequest.CustomerPersonalNumber ?? string.Empty,
                    CUSTOMERNOTE = postReservationRequest.CustomerNote ?? string.Empty,
                    CUSTOMEREXPLANATION = postReservationRequest.CustomerExplanation ?? string.Empty,
                    COMPANYTITLE = postReservationRequest.CompanyTitle ?? string.Empty,
                    COMPANYADDRESS = postReservationRequest.CustomerAddress ?? string.Empty,
                    COMPANYTAXOFFICE = postReservationRequest.CompanyTaxOffice ?? string.Empty,
                    COMPANYTAXNO = postReservationRequest.CompanyTaxNumber ?? string.Empty,
                    FLIGHTNOARRIVAL = $"{postReservationRequest.FlightNumberArrival ?? string.Empty} | {postReservationRequest.DepartureInfo}",
                    FLIGHTNODEPARTURE = postReservationRequest.FlightNumberDeparture ?? string.Empty,
                    CUSTOMERIP = postReservationRequest.CustomerIPAddress,
                    PAIDAMOUNT = apiPaidAmount.ToStringNullSafe().Replace(",", "."),
                    SENDMAIL = "false",
                    UPDATERESNO = string.Empty,
                    CREDITCARDPAYMENTTYPEACTIVE = false,
                    ADVANCEPAYMENTTYPEACTIVE = false,
                    BANKID = string.Empty,
                    BANKVENDORID = string.Empty,
                    CREDITCARDHOLDER = string.Empty,
                    CREDITCARDNO = string.Empty,
                    EXPIREDYEAR = string.Empty,
                    EXPIREDMONTH = string.Empty,
                    SECURITYCODE = string.Empty,
                    INSTALLMENTCOUNT = "",
                    THREEDPAYMENTACTIVE = "false",
                    PARAM1VALUE = string.Empty,
                    PARAM2VALUE = string.Empty,
                    PARAM3VALUE = string.Empty,
                    PARAM4VALUE = string.Empty,
                    PARAM5VALUE = string.Empty,
                    PARAM6VALUE = string.Empty,
                    //ek ürünleri tahsil etmişse 111 gönderilecek
                    //PARAM7VALUE = postReservationRequest.FullCredit.ToBoolNullSafe() ? "1111" : string.Empty,
                    PARAM7VALUE = (postReservationRequest.PaymentType == PaymentTypes.AdvancePayment && vendor.RentalWorkingType == VendorWorkingTypes.Commission) ? "" : paymentInfo,
                    //PARAM8VALUE = reservationNumber ?? string.Empty,
                    //reservationNumber yerine boş değer gidiyordu, eğer ResAgencyNameSending false ise reservationNumber gönderiliyor
                    PARAM8VALUE = vendor.ResAgencyNameSending ? additionalInformation.Agency.AgencyName : reservationNumber,
                    PARAM9VALUE = string.Empty,
                    PARAM10VALUE = string.Empty,
                    PARAM11VALUE = specialDailyPrice != -1 ? specialDailyPrice.ToString() : string.Empty,
                    PARAM12VALUE = specialOneWayFee != -1 ? specialOneWayFee.ToString() : string.Empty,
                    PARAM13VALUE = vendor.ResAgencyNameSending ? "0,10" : "0",
                    PARAM14VALUE = string.Empty
                };

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(postKolayCARReservationRequest),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                Serilog.Log.Error("{@KolayCARPostReservationRequest}", postKolayCARReservationRequest);

                var postReservationResult = await _kolayCARService.POST_RESERVATIONAsync(
                    postKolayCARReservationRequest.APIKEY,
                    postKolayCARReservationRequest.APIPASSWORD,
                    postKolayCARReservationRequest.LANGISOCODE,
                    postKolayCARReservationRequest.CURRENCYISOCODE,
                    postKolayCARReservationRequest.PICKUPLOCATIONID,
                    postKolayCARReservationRequest.RETURNLOCATIONID,
                    postKolayCARReservationRequest.PICKUPDATE,
                    postKolayCARReservationRequest.RETURNDATE,
                    postKolayCARReservationRequest.PICKUPTIME,
                    postKolayCARReservationRequest.RETURNTIME,
                    postKolayCARReservationRequest.VENDORID,
                    postKolayCARReservationRequest.VEHICLEID,
                    postKolayCARReservationRequest.EXTRAIDLIST,
                    postKolayCARReservationRequest.CUSTOMERINSTUTIONTYPENO,
                    postKolayCARReservationRequest.CUSTOMERNAME,
                    postKolayCARReservationRequest.CUSTOMERSURNAME,
                    postKolayCARReservationRequest.CUSTOMERTELEPHONE,
                    postKolayCARReservationRequest.CUSTOMEREMAIL,
                    postKolayCARReservationRequest.CUSTOMERPERSONALNUMBER,
                    postKolayCARReservationRequest.CUSTOMERNOTE,
                    postKolayCARReservationRequest.CUSTOMEREXPLANATION,
                    postKolayCARReservationRequest.COMPANYTITLE,
                    postKolayCARReservationRequest.COMPANYADDRESS,
                    postKolayCARReservationRequest.COMPANYTAXOFFICE,
                    postKolayCARReservationRequest.COMPANYTAXNO,
                    postKolayCARReservationRequest.FLIGHTNOARRIVAL,
                    postKolayCARReservationRequest.FLIGHTNODEPARTURE,
                    postKolayCARReservationRequest.CUSTOMERIP,
                    postKolayCARReservationRequest.PAIDAMOUNT,
                    postKolayCARReservationRequest.SENDMAIL,
                    postKolayCARReservationRequest.UPDATERESNO,
                    postKolayCARReservationRequest.CREDITCARDPAYMENTTYPEACTIVE,
                    postKolayCARReservationRequest.ADVANCEPAYMENTTYPEACTIVE,
                    postKolayCARReservationRequest.BANKID,
                    postKolayCARReservationRequest.BANKVENDORID,
                    postKolayCARReservationRequest.CREDITCARDHOLDER,
                    postKolayCARReservationRequest.CREDITCARDNO,
                    postKolayCARReservationRequest.EXPIREDYEAR,
                    postKolayCARReservationRequest.EXPIREDMONTH,
                    postKolayCARReservationRequest.SECURITYCODE,
                    postKolayCARReservationRequest.INSTALLMENTCOUNT,
                    postKolayCARReservationRequest.THREEDPAYMENTACTIVE,
                    postKolayCARReservationRequest.PARAM1VALUE,
                    postKolayCARReservationRequest.PARAM2VALUE,
                    postKolayCARReservationRequest.PARAM3VALUE,
                    postKolayCARReservationRequest.PARAM4VALUE,
                    postKolayCARReservationRequest.PARAM5VALUE,
                    postKolayCARReservationRequest.PARAM6VALUE,
                    postKolayCARReservationRequest.PARAM7VALUE,
                    postKolayCARReservationRequest.PARAM8VALUE,
                    postKolayCARReservationRequest.PARAM9VALUE,
                    postKolayCARReservationRequest.PARAM10VALUE,
                    postKolayCARReservationRequest.PARAM11VALUE,
                    postKolayCARReservationRequest.PARAM12VALUE,
                    postKolayCARReservationRequest.PARAM13VALUE,
                    postKolayCARReservationRequest.PARAM14VALUE);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = postReservationResult.Body.POST_RESERVATIONResult,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                });

                var postReservationResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(postReservationResult.Body.POST_RESERVATIONResult);

                Serilog.Log.Error("{@KolayCARPostReservationResponse}", postReservationResponseObject);

                if (postReservationResponseObject.MESSAGE == "Herhangi bir veri bulunamadı" && !postReservationRequest.IsRetry)
                {
                    postReservationRequest.IsRetry = true;

                    return await PostReservation(postReservationRequest, vendor, additionalInformation, reservationNumber, reservationToken, exchangeRates, localReservation, apiExtras);
                }

                localReservation.ReservationPostedToAPI = true;

                if (postReservationResponseObject.RETURNCODE == 0)
                {
                    var reservation = postReservationResponseObject.RESERVATION[0];
                    var apiVendor = postReservationResponseObject.VENDOR[0];

                    localReservation.APIReservationSuccessfully = postReservationResponseObject.RETURNCODE == 0;

                    localReservation.APIReservationNumber = reservation.RESERVATIONNO;
                    localReservation.APIVendorName = apiVendor.VENDORNAME;

                    localReservation.APIVendorPickupAddress = postReservationResponseObject.VENDOR[0].VENDORPICKUPLOCATIONADDRESS;
                    localReservation.APIVendorReturnAddress = postReservationResponseObject.VENDOR[0].VENDORRETURNLOCATIONADDRESS;
                    localReservation.APIVendorPickupPhone = postReservationResponseObject.VENDOR[0].VENDORPICKUPLOCATIONPHONE;
                    localReservation.APIVendorReturnPhone = postReservationResponseObject.VENDOR[0].VENDORRETURNLOCATIONPHONE;

                    return new ServiceResponseBase
                    {
                        Success = localReservation != null && postReservationResponseObject.RETURNCODE == 0,
                        ServiceCode = postReservationResponseObject.RETURNCODE.ToString(),
                        Data = localReservation
                    };
                }

                Serilog.Log.Error("KolayCAR rezervasyonu başarısız!");
                localReservation.APIMessage = $"{postReservationResponseObject.MESSAGE} - CODE: {postReservationResponseObject.RETURNCODE}";
                return new ServiceResponseBase
                {
                    Success = false,
                    Data = localReservation,
                    Message = "KolayCAR servisine ulaşılamadı!",
                    ServiceCode = postReservationResponseObject.RETURNCODE.ToString(),
                    ServiceMessage = postReservationResponseObject.MESSAGE
                };
            }
            #region Soap Exceptions Catches
            catch (System.Net.WebException ex)
            {
                Serilog.Log.Fatal("{@KolayCarPostReservationError}", ex.Message);
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "KolayCar servisinde bir hata meydana geldi.",
                    Data = localReservation
                };
            }
            catch (TimeoutException ex)
            {
                Serilog.Log.Fatal("{@KolayCarPostReservationError}", ex.Message);
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "KolayCar servisinde bir hata meydana geldi.",
                    Data = localReservation
                };
            }
            catch (System.ServiceModel.FaultException ex)
            {
                Serilog.Log.Fatal("{@KolayCarPostReservationError}", ex.Message);
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "KolayCar servisinde bir hata meydana geldi.",
                    Data = localReservation
                };
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                Serilog.Log.Fatal("{@KolayCarPostReservationError}", ex.Message);
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "KolayCar servisinde bir hata meydana geldi.",
                    Data = localReservation
                };
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal("{@KolayCarPostReservationError}", ex.Message);
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "KolayCar servisinde bir hata meydana geldi.",
                    Data = localReservation
                };
            }
            #endregion
        }

        private string GetFormattedExtraListStrig(List<Extra> extras)
        {
            return string.Join(",", extras.Select(e =>
                       $"{e.ExtraCode}-{e.Piece}"
                   ));
        }

        public string GetPaymentInfo(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            string returnValue;

            if (CreditHelper.ResolveTokenCreditType(additionalInformation.ReservationToken) == CreditType.FullCredit)
                return "1111";

            if (postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery)
                return "000";

            string firsth = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? "0" : "1";
            string second = postReservationRequest.ExtraPricePayToDelivery ? "0" : "1";
            string thirth = postReservationRequest.OneWayFeePayToDelivery ? "0" : "1";

            returnValue = firsth + second + thirth;

            return returnValue;
        }
    }
}
