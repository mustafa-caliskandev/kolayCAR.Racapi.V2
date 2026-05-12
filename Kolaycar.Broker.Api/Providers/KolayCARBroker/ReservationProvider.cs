using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.KolayCARBroker
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager _httpManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        private readonly IConfigurationService _configurationService;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public ReservationProvider(string apiBaseUrl, ICacheService cacheService, IConfigurationService configurationService, IConfiguration configuration)
        {
            _httpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            AuthProvider = new AuthProvider(apiBaseUrl);
            _configurationService = configurationService;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var auth = await _cacheService.GetOrCreateAsync($"kolayCarBroker{vendor.VendorName}Token", () => AuthProvider.GetJWT(vendor.ApiKey, vendor.ApiPassword, EncryptionHelper.Encrypt(vendor.ApiPassword)), TimeSpan.FromMinutes(30));

            if (auth != null)
            {
                var user = auth.Data as User;
                var postCancelReservationsRequestParameters = CreateBrokerPostCancelReservationsRequestParameters(postCancelReservationRequest, localReservation, vendor);

                await _configurationService.WriteLog(new BrokerLogModel(localReservation.ReservationNumber, postCancelReservationsRequestParameters.ToJson(), BrokerLogTypes.ReservationCancelVendorAPIRequest));

                Serilog.Log.Error("{@BrokerPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

                var result = await _httpManager.PostAsync<PostCancelReservationRequest, Reservation>(
                    requestPath: "reservations/cancel",
                    parameters: postCancelReservationsRequestParameters,
                    headers: AuthProvider.CreateAuthHeader(user.Token),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    }, isReservationRequest: true);

                Serilog.Log.Error("{@BrokerPostCancelReservationsResponse}", result);

                if (result?.Data != null && (bool)result?.Success)
                {
                    localReservation.APIReservationCancel = result.Data.APIReservationCancel;
                    return new ServiceResponseBase(localReservation, result.Success, "", result.ServiceMessage ?? result.Message);
                }
                return new ServiceResponseBase(localReservation, result.Success, result.Message, result.ServiceMessage ?? result.Message);
            }
            return new ServiceResponseBase(localReservation, auth.Success, "Kimlik doğrulama işlemi başarısız!", auth.Message);
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            if (postReservationRequest.PostReservationRequestV2 != null)
            {
                try
                {
                    var auth = await _cacheService.GetOrCreateAsync($"kolayCarBroker{vendor.VendorName}Token", () => AuthProvider.GetJWT(vendor.ApiKey, vendor.ApiPassword, EncryptionHelper.Encrypt(vendor.ApiPassword)), TimeSpan.FromMinutes(30));

                    if (auth == null)
                        return new ServiceResponseBase(null, false, "Kimlik doğrulama işlemi başarısız!");

                    var entity = GetEntity(postReservationRequest.PostReservationRequestV2, additionalInformation, reservationToken, vendor, localReservation);
                    var extras = new List<Extra>();
                    if (vendor.UseBrokerConfigurations)
                    {
                        foreach (var extra in localReservation.ReservationExtras)
                        {
                            var apiExtra = apiExtras.FirstOrDefault(e => e.ExtraId == extra.ExtraId);
                            extras.Add(new Extra { Code = apiExtra.Code, Piece = extra.Piece, Price = extra.ApiPrice });
                            //extras.Add(new Extra { Code = apiExtra.Code, Piece = extra.Piece, Price = extra.APIPrice });
                        }
                    }
                    else
                    {
                        foreach (var extra in localReservation.ReservationExtras)
                        {
                            var apiExtra = apiExtras.FirstOrDefault(e => e.ExtraCode == extra.ExtraCode);
                            extras.Add(new Extra { Code = apiExtra.ApiExtraCode, Piece = extra.Piece, Price = extra.ApiPrice });
                        }
                    }
                    entity.Extras = extras;
                    await _configurationService.WriteLog(new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        Content = JsonConvert.SerializeObject(entity),
                        LogType = BrokerLogTypes.ReservationVendorAPIRequest
                    });

                    var user = auth.Data as User;
                    var authHeader = AuthProvider.CreateAuthHeader(user.Token);

                    Serilog.Log.Error("{@BrokerPostReservationRequest}", entity.ToJson());
                    Serilog.Log.Error("{@BrokerPostReservationRequestHeaders}", authHeader);

                    var result = await _httpManager.PostAsync<PostReservationRequestV2, Reservation>(
                        requestPath: "/reservations/save",
                        headers: authHeader,
                        entity: entity,
                        brokerLogModel: new BrokerLogModel
                        {
                            LogKey = localReservation.ReservationNumber,
                            LogType = BrokerLogTypes.ReservationVendorAPIResponse
                        },
                        isReservationRequest: true);
                    Serilog.Log.Error("{@BrokerPostReservationResult}", result);

                    localReservation.ReservationPostedToAPI = true;
                    localReservation.APIMessage = result.ServiceMessage ?? result.Message;

                    if (result.Data != null && result.Success)
                    {
                        var reservation = result.Data as Reservation;
                        localReservation.APIReservationSuccessfully = result.Success;

                        localReservation.APIReservationNumber = !vendor.UseBrokerConfigurations ? reservation.ReservationNumber : reservation.APIReservationNumber;

                        if (vendor.UseBrokerConfigurations)
                        {
                            localReservation.APIVendorName = reservation.APIVendorName;
                            localReservation.APIPhoneActive = reservation.APIPhoneActive;
                        }
                        else
                        {
                            localReservation.APIVendorName = reservationToken.APIVendorName;
                            if (reservationToken.ApiVendorType == VendorTypes.Yolcu360)
                                localReservation.APIPhoneActive = reservation.APIPhoneActive;
                        }
                        localReservation.PickupOfficeWorkingHours = reservation.PickupOfficeWorkingHours;
                        localReservation.ReturnOfficeWorkingHours = reservation.ReturnOfficeWorkingHours;
                        localReservation.APIVendorPickupAddress = reservation.APIVendorPickupAddress;
                        localReservation.APIVendorReturnAddress = reservation.APIVendorReturnAddress;
                        localReservation.APIVendorPickupPhone = reservation.APIVendorPickupPhone;
                        localReservation.APIVendorReturnPhone = reservation.APIVendorReturnPhone;

                        return new ServiceResponseBase(localReservation, result.Success, "", result.ServiceMessage ?? result.Message);
                    }
                    return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} servisine ulaşılamadı!", result.ServiceMessage ?? result.Message);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error("{@KolayCarBrokerReservationError}", ex.ToJson());
                    return new ServiceResponseBase(null, false, "Rezervasyon isteği başarısız!");
                }

            }

            try
            {
                var auth = await _cacheService.GetOrCreateAsync($"kolayCarBroker{vendor.VendorName}Token", () => AuthProvider.GetJWT(vendor.ApiKey, vendor.ApiPassword, EncryptionHelper.Encrypt(vendor.ApiPassword)), TimeSpan.FromMinutes(30));

                if (auth != null)
                {
                    float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest, exchangeRates);
                    var user = auth.Data as User;

                    var postReservationsRequestParameters = CreateBrokerPostReservationsRequestParameters(postReservationRequest, localReservation, apiPaidAmount, additionalInformation, exchangeRates, vendor, reservationToken);
                    var authHeader = AuthProvider.CreateAuthHeader(user.Token);

                    await _configurationService.WriteLog(new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        Content = JsonConvert.SerializeObject(postReservationsRequestParameters),
                        LogType = BrokerLogTypes.ReservationVendorAPIRequest
                    });

                    Serilog.Log.Error("{@BrokerPostReservationRequestParameters}", postReservationsRequestParameters);
                    Serilog.Log.Error("{@BrokerPostReservationRequestHeaders}", authHeader);

                    var result = await _httpManager.PostAsync<Reservation, Reservation>(
                        requestPath: "reservations",
                        parameters: postReservationsRequestParameters,
                        headers: authHeader,
                        brokerLogModel: new BrokerLogModel
                        {
                            LogKey = localReservation.ReservationNumber,
                            LogType = BrokerLogTypes.ReservationVendorAPIResponse
                        },
                        isReservationRequest: true);

                    Serilog.Log.Error("{@BrokerPostReservationResult}", result);

                    localReservation.ReservationPostedToAPI = true;
                    localReservation.APIMessage = result.ServiceMessage ?? result.Message;

                    if (result.Data != null && result.Success)
                    {
                        var reservation = result.Data as Reservation;
                        localReservation.APIReservationSuccessfully = result.Success;

                        localReservation.APIReservationNumber = !vendor.UseBrokerConfigurations ? reservation.ReservationNumber : reservation.APIReservationNumber; //Broker'ın tedarikçisinin rezervasyon numarası alınıyor.

                        if (vendor.UseBrokerConfigurations)
                        {
                            localReservation.APIVendorName = reservation.APIVendorName;
                            localReservation.APIPhoneActive = reservation.APIPhoneActive;
                        }
                        else
                        {
                            localReservation.APIVendorName = reservationToken.APIVendorName;
                            if (reservationToken.ApiVendorType == VendorTypes.Yolcu360)
                                localReservation.APIPhoneActive = reservation.APIPhoneActive;
                        }
                        localReservation.PickupOfficeWorkingHours = reservation.PickupOfficeWorkingHours;
                        localReservation.ReturnOfficeWorkingHours = reservation.ReturnOfficeWorkingHours;
                        localReservation.APIVendorPickupAddress = reservation.APIVendorPickupAddress;
                        localReservation.APIVendorReturnAddress = reservation.APIVendorReturnAddress;
                        localReservation.APIVendorPickupPhone = reservation.APIVendorPickupPhone;
                        localReservation.APIVendorReturnPhone = reservation.APIVendorReturnPhone;

                        return new ServiceResponseBase(localReservation, result.Success, "", result.ServiceMessage ?? result.Message);
                    }
                    return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} servisine ulaşılamadı!", result.ServiceMessage ?? result.Message);
                }
                Serilog.Log.Error("{@KolayCarBrokerReservationAuthError}", auth.ToJson());
                return new ServiceResponseBase(null, auth.Success, "Kimlik doğrulama işlemi başarısız!", auth.Message);
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@KolayCarBrokerReservationAuthError}", ex.Message);
                return new ServiceResponseBase(localReservation, false, $"{vendor.VendorName} servisine ulaşılamadı!");
            }
        }

        private float GetPaidAmount(ReservationToken reservationToken, PostReservationRequestV2 postReservationRequestV2, Vendor vendor, Reservation reservation)
        {
            float apiPaidAmount = 0f;
            switch (postReservationRequestV2.Payment.PaymentType)
            {
                case PaymentTypes.PayOnDelivery:
                    {
                        apiPaidAmount = 0f; break;
                    }
                case PaymentTypes.AdvancePayment:
                    apiPaidAmount = postReservationRequestV2.Pricing.PaidAmount;
                    break;
                case PaymentTypes.PayToAgency:
                case PaymentTypes.CommissionFree:
                case PaymentTypes.PayAll:
                    {
                        apiPaidAmount = reservationToken.APIDailyPrice * reservationToken.RentalDuration;

                        if (reservationToken.OneWayFee > 0 && !postReservationRequestV2.Payment.OneWayFeePayToDelivery)
                            apiPaidAmount += reservation.APIOneWayFee;

                        if (postReservationRequestV2.Extras?.Where(e => !(e.ExtraType == AdditionalProductTypes.Premium && !e.VendorExtraExists)).ToList().Count > 0 && !postReservationRequestV2.Payment.ExtraPricePayToDelivery)
                            apiPaidAmount += reservation.APIExtraAmount;
                        break;
                    }

                default:
                    break;
            }
            return apiPaidAmount;
        }

        private Dictionary<string, object> CreateBrokerPostReservationsRequestParameters(PostReservationRequest postReservationRequest, Reservation localReservation, float paidAmount, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, Vendor vendor, ReservationToken reservationToken)
        {
            //ReservationToken reservationToken = JsonConvert.DeserializeObject<ReservationToken>(EncryptionHelper.DecryptAES256(postReservationRequest.ReservationToken));

            //if (additionalInformation.Vendor.UseBrokerConfigurations)
            //    postReservationRequest.ExtraList = ReservationHelper.ChangeExtraCodeAndExtraId(postReservationRequest.ExtraList);

            //if (additionalInformation.Vendor.AdditionalProductWorkingType != VendorWorkingTypes.Commission && localReservation.PaymentType != PaymentTypes.PayOnDelivery)
            //    postReservationRequest.ExtraList = ReservationHelper.ChangeExtraLocalPriceToAPIPrice(postReservationRequest.ExtraList, localReservation.ReservationExtras, useBrokerConfigurations: additionalInformation.Vendor.UseBrokerConfigurations);

            var brokerName = _configuration["AppSettings:BrokerName"].ToStringNullSafe();

            var paymentType = localReservation.PaymentType;

            if (localReservation.PaymentType == PaymentTypes.AdvancePayment)
                paymentType = PaymentTypes.PayOnDelivery;

            if (localReservation.PaymentType == PaymentTypes.AdvancePayment && additionalInformation.Vendor.RentalWorkingType == VendorWorkingTypes.ProfitMarkup)
                paidAmount = 0;
            else if (localReservation.PaymentType == PaymentTypes.AdvancePayment && additionalInformation.Vendor.RentalWorkingType == VendorWorkingTypes.Commission)
                paidAmount += localReservation.CouponDiscountAmount;
            else if (paymentType == PaymentTypes.PayAll)
            {
                paymentType = PaymentTypes.PayToAgency;
                if (localReservation.ExtraPricePayToDelivery)
                    paidAmount = paidAmount - localReservation.ExtraPrice;
                if (localReservation.OneWayFeePayToDelivery)
                    paidAmount = paidAmount - localReservation.OneWayFee;
            }

            var specialDailyPrice = localReservation.PaymentType == PaymentTypes.PayOnDelivery ? localReservation.DailyPrice : -1;
            var specialOneWayFee = localReservation.PaymentType == PaymentTypes.PayOnDelivery ? localReservation.OneWayFee : -1;
            if (!string.IsNullOrEmpty(postReservationRequest.ExtraList))
            {
                List<string> extraListBuilder = new List<string>();
                string[] extraData = postReservationRequest.ExtraList.Split('|');

                foreach (string extraDataItem in extraData)
                {
                    string[] extraParts = extraDataItem.Split('~');
                    if (extraParts.Length == 6 && extraParts[4] != "PRMPKT-1")
                    {
                        var (extraID, extraPiece, rawExtraPrice, extraName, extraCode) =
                              (extraParts[0], extraParts[1], extraParts[2], extraParts[3], extraParts[4]);

                        extraListBuilder.Add(string.Join("~", new[]
                                  {
                                            extraID,
                                            extraPiece,
                                            rawExtraPrice.Replace(',', '.'),
                                            extraName,
                                            extraCode
                                      }));

                    }
                    if (extraParts.Length > 7 && extraParts[4] != "PRMPKT-1")
                    {
                        var apiExtraPrice = extraParts[6].ToFloatNullSafe();

                        var (extraID, extraPiece, rawExtraPrice, extraName, extraCode, extraRentalTypeStr, extraApiPriceStr, extraDescription) =
                              (extraParts[0], extraParts[1], apiExtraPrice.ToString(), extraParts[3], extraParts[4], extraParts[5], apiExtraPrice, extraParts[7]);

                        if (reservationToken.BaseBaseVendorRequestCurrencyType != reservationToken.CurrencyType)
                        {
                            apiExtraPrice = CalculationHelper.CurrencyExchange(exchangeRates, vendor, apiExtraPrice, reservationToken.CurrencyType, reservationToken.BaseBaseVendorRequestCurrencyType);

                        }
                        extraListBuilder.Add(string.Join("~", new[]
                                  {
                                            extraID,
                                            extraPiece,
                                            rawExtraPrice.Replace(',', '.'),
                                            extraName,
                                            extraCode,
                                            extraRentalTypeStr,
                                            apiExtraPrice.ToString().Replace(',','.'),
                                            extraDescription
                                      }));

                    }
                }
                postReservationRequest.ExtraList = string.Join("|", extraListBuilder);
            }

            return new Dictionary<string, object>()
            {
                //{ "memberId", postReservationRequest.MemberId},
                { "languageCode", postReservationRequest.LanguageCode},
                { "currencyCode", reservationToken.BaseVendorRequestCurrencyType.ToString()},
                { "pickupLocationId", additionalInformation.APIPickupLocationCode},
                { "returnLocationId", additionalInformation.APIReturnLocationCode},
                { "pickupDate", postReservationRequest.PickupDate},
                { "returnDate", postReservationRequest.ReturnDate},
                { "pickupTime", postReservationRequest.PickupTime},
                { "returnTime", postReservationRequest.ReturnTime},
                { "vehicleName", postReservationRequest.VehicleName},
                { "extraList", postReservationRequest.ExtraList},
                { "reservationToken", reservationToken.APIReferenceCode},
                { "userToken", string.Empty},
                { "couponCode", string.Empty},
                { "customerInstutionTypeNumber", postReservationRequest.CustomerInstutionTypeNumber},
                { "customerName", postReservationRequest.CustomerName},
                { "customerSurname", postReservationRequest.CustomerSurname},
                { "customerTelephone", postReservationRequest.CustomerTelephone},
                { "customerEmail", postReservationRequest.CustomerEmail},
                { "customerPersonalNumber", postReservationRequest.CustomerPersonalNumber},
                { "customerNote", postReservationRequest.CustomerNote},
                { "customerExplanation", postReservationRequest.CustomerExplanation},
                { "companyTitle", postReservationRequest.CompanyTitle},
                { "companyAddress", postReservationRequest.CustomerAddress},
                { "companyTaxOffice", postReservationRequest.CompanyTaxOffice},
                { "companyTaxNumber", postReservationRequest.CompanyTaxNumber},
                { "flightNumberArrival", $"{postReservationRequest.FlightNumberArrival}"},
                { "flightNumberDeparture", $"{postReservationRequest.FlightNumberDeparture}"},
                { "customerIPAddress", postReservationRequest.CustomerIPAddress},
                { "paidAmount", paidAmount},
                { "updateReservationNumber", string.Empty},
                { "creditCardPaymentTypeActive", false},
                { "advancePaymentTypeActive", null},
                { "bankId", null},
                { "bankVendorId",  null},
                { "creditCardHolder", string.Empty},
                { "creditCardNumber", string.Empty},
                { "expiredYear", null},
                { "expiredMonth", null},
                { "securityCode", string.Empty},
                { "installmentCount", 0},
                { "threeDPaymentActive", false},
                { "threeDStatus", string.Empty},
                { "threeDAuth", string.Empty},
                { "threeDLevel", string.Empty},
                { "threeDTxnId", string.Empty},
                { "threeDMd", string.Empty},
                { "threeDPnOrInfo", string.Empty},
                { "specialDailyPrice", specialDailyPrice},
                { "specialOneWayFee", specialOneWayFee},
                { "extraAmount", localReservation.APIExtraAmount},
                { "isCommissionFreePrice", false},
                { "sendReservationMail", false},
                { "customerBirthDay", postReservationRequest.CustomerBirthDay},
                { "departureInfo", postReservationRequest.DepartureInfo},
                { "vehicleImageURL", postReservationRequest.VehicleImageURL},
                { "agencyReservationReference", brokerName.ToLower() == "tatilburada" ? "" : localReservation.ReservationNumber},
                { "paymentType", paymentType},
                { "skyscannerRedirectID", string.Empty},
                { "commercialAllowance", postReservationRequest.CommercialAllowance},
                { "advancedPaymentWithoutPayment", postReservationRequest.PaymentType == PaymentTypes.AdvancePayment },
                { "fullCredit", postReservationRequest.FullCredit },
                { "countryCode", postReservationRequest.CountryCode },
                { "SendAgencyReservationNumber", vendor.ResAgencyNameSending},
                { "apiExtras", postReservationRequest.ApiExtras}
            };
        }

        private Dictionary<string, object> CreateBrokerPostCancelReservationsRequestParameters(PostCancelReservationRequest request, Reservation reservation, Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "reservationNumber", reservation.APIReservationNumber },
                { "cancelNote", request.CancelNote },
                { "languageCode", request.LanguageCode },
                { "customerEmail", !string.IsNullOrEmpty(reservation.DefaultCustomerMailAddress) && !vendor.UseBrokerConfigurations && vendor.SendDefaultMailAddress ? reservation.DefaultCustomerMailAddress : request.CustomerEmail },
                { "isBrokerReservation", vendor.UseBrokerConfigurations }
            };
        }
        public static string Encrypt(string str)
        {
            try
            {
                string password = ".web!758558";
                byte[] encryptedByteArray = System.Text.Encoding.Unicode.GetBytes(str);
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                byte[] encryptedData = Encrypt(encryptedByteArray, pdb.GetBytes(32), pdb.GetBytes(16));
                return Convert.ToBase64String(encryptedData);
            }
            catch
            {
                return string.Empty;
            }
        }
        private static byte[] Encrypt(byte[] unencryptedData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(unencryptedData, 0, unencryptedData.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }
        private PostReservationRequestV2 GetEntity(PostReservationRequestV2 postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, Vendor vendor, Reservation reservation)
        {
            var brokerName = _configuration["AppSettings:BrokerName"].ToStringNullSafe();
            var paidAmount = GetPaidAmount(reservationToken, postReservationRequest, vendor, reservation);
            return new PostReservationRequestV2
            {
                Customer = new Customer
                {
                    Name = postReservationRequest.Customer.Name,
                    Surname = postReservationRequest.Customer.Surname,
                    PhoneNumber = postReservationRequest.Customer.PhoneNumber,
                    Email = postReservationRequest.Customer.Email,
                    PersonalNumber = postReservationRequest.Customer.PersonalNumber,
                    Note = postReservationRequest.Customer.Note,
                    Explanation = postReservationRequest.Customer.Explanation,
                    InstutionTypeNumber = postReservationRequest.Customer.InstutionTypeNumber,
                    Address = postReservationRequest.Customer.Address,
                    BirthDay = postReservationRequest.Customer.BirthDay
                },
                PickupLocationId = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
                ReturnLocationId = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
                LanguageCode = postReservationRequest.LanguageCode,
                CurrencyCode = reservationToken.BaseVendorRequestCurrencyType.ToString(),
                PickupDate = postReservationRequest.PickupDate,
                ReturnDate = postReservationRequest.ReturnDate,
                PickupTime = postReservationRequest.PickupTime,
                ReturnTime = postReservationRequest.ReturnTime,
                VehicleName = postReservationRequest.VehicleName,
                ReservationToken = reservationToken.APIReferenceCode,
                Company = new Company
                {
                    Title = postReservationRequest.Company?.Title ?? "",
                    TaxOffice = postReservationRequest.Company?.TaxOffice ?? "",
                    TaxNumber = postReservationRequest.Company?.TaxNumber ?? ""
                },
                FlightNumberArrival = postReservationRequest.FlightNumberArrival,
                FlightNumberDeparture = postReservationRequest.FlightNumberDeparture,
                Pricing = new Pricing
                {
                    PaidAmount = paidAmount,
                    SpecialDailyPrice = postReservationRequest.Pricing.SpecialDailyPrice,
                    SpecialOneWayFee = postReservationRequest.Pricing.SpecialOneWayFee,
                    IsCommissionFreePrice = false,
                    PaidAmountAfterUsingCouponCode = postReservationRequest.Pricing.PaidAmountAfterUsingCouponCode,
                },
                SendReservationMail = false,
                DepartureInfo = postReservationRequest.DepartureInfo,
                VehicleImageURL = postReservationRequest.VehicleImageURL,
                AgencyReservationReference = brokerName.ToLower() == "tatilburada" ? "" : reservation.ReservationNumber,
                Payment = new Payment
                {
                    PaymentType = postReservationRequest.Payment.PaymentType,
                    AdvancedPaymentWithoutPayment = postReservationRequest.Payment.PaymentType == PaymentTypes.AdvancePayment
                },
                SkyscannerRedirectID = "",
                CommercialAllowance = postReservationRequest.CommercialAllowance,
                FullCredit = postReservationRequest.FullCredit ?? false,
                CountryCode = postReservationRequest.CountryCode,
                SendAgencyReservationNumber = vendor.ResAgencyNameSending,
            };
        }
    }
}
