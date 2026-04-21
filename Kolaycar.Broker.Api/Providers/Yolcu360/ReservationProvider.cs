using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yolcu360Provider = KolayCAR.Broker.API.Providers.Yolcu360;

namespace KolayCAR.Broker.API.Providers.Yolcu360
{

    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager;
        AuthProvider AuthProvider;
        ILocationProvider LocationProvider { get; set; }
        private readonly IConfigurationService _configurationService;
        private readonly IMemoryCache _memoryCache;
        const string BLOCKED_VENDORS_CACHE_KEY = "BLOCKED_VENDORS_CACHE_KEY_{0}";
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            AuthProvider = new AuthProvider(apiBaseUrl);
            LocationProvider = new Yolcu360Provider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService, IMemoryCache memoryCache)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            AuthProvider = new AuthProvider(apiBaseUrl, memoryCache);
            LocationProvider = new Yolcu360Provider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
            _memoryCache = memoryCache;
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var cookie = await AuthProvider.Login(vendor.ApiKey, vendor.ApiPassword);

            if (!string.IsNullOrEmpty(cookie))
            {
                var reservationCancelRequestBody = CreateCancelReservationBody(localReservation, vendor);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(reservationCancelRequestBody),
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
                });

                Serilog.Log.Error("{@Yolcu360CanselReservationRequestBody}", reservationCancelRequestBody);

                var result = await RestManager.PostAsync<Yolcu360RequestBase.ReservationCancelRequestBody.RentalOrderParameters, Yolcu360CancelReservationResponseBase.RentalOrderCancellationResponse>(
                    headers: AuthProvider.CreateHeaderWithCookie(cookie),
                    requestPath: "car/order/cancel/",
                    entity: reservationCancelRequestBody,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    },
                    isReservationRequest: true);

                Serilog.Log.Error("{@Yolcu360CancelReservationResult}", result);

                if (result != null && result.cancelled)
                {
                    localReservation.APIReservationCancel = true;

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
                Data = localReservation,
                Message = "Yolcu360 login servisine ulaşılamadı."
            };
        }
        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var cookie = AuthProvider.Login(vendor.ApiKey, vendor.ApiPassword).Result;

            if (!string.IsNullOrEmpty(cookie))
            {
                float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
                var reservationRequestBody = CreateReservationRequestBody(reservationToken, localReservation, postReservationRequest, vendor, additionalInformation, apiExtras);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(reservationRequestBody),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                Serilog.Log.Error("{@Yolcu360ReservationRequestBody}", reservationRequestBody);

                var result = await RestManager.PostAsync<object, YolcuReservationResponseBase.Root>(
                    requestPath: "car/order/credit/",
                    headers: AuthProvider.CreateReservationHeaderWithSessionId(cookie),
                    entity: reservationRequestBody,
                    ignoreNull: true,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                    isReservationRequest: true);

                Serilog.Log.Error("{@Yolcu360ReservationResult}", result);

                localReservation.ReservationPostedToAPI = true;
                localReservation.APIVendorName = vendor.VendorName;

                if (result?.id != null)
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = !string.IsNullOrEmpty(result.vendorReservationId) ? result.vendorReservationId : result.id;
                    localReservation.APIReferenceCode2 = result.id;


                    if (result.listing != null
                        && result.listing.office != null
                        && result.listing.office.address != null
                        && !string.IsNullOrEmpty(result.listing.office.address.line)
                        && result.listing.dropoffLocation != null
                        && result.listing.dropoffLocation.address != null
                        && !string.IsNullOrEmpty(result.listing.dropoffLocation.address.line)
                        )
                    {
                        try
                        {
                            localReservation.PickupOfficeWorkingHours = GetOfficeWorkingHours(result.listing.office.openingHours, localReservation.PickupDate);
                            localReservation.ReturnOfficeWorkingHours = GetOfficeWorkingHours(result.listing.dropoffLocation.openingHours, localReservation.ReturnDate);
                            localReservation.APIVendorPickupAddress = result.listing.office.address.line;
                            localReservation.APIVendorPickupPhone = result.listing.office.phones?[0].dialCode + result.listing.office.phones?[0].number;

                            localReservation.APIVendorReturnAddress = result.listing.dropoffLocation.address.line;
                            localReservation.APIVendorReturnPhone = result.listing.dropoffLocation.phones?[0].dialCode + result.listing.dropoffLocation.phones?[0].number;
                        }
                        catch (Exception ex)
                        {
                            Serilog.Log.Error("{@Yolcu360ContactInformationError}", ex.ToJson());
                        }

                        var deliveryType = result.listing.office.deliveryType;
                        localReservation.IsOffice = deliveryType == "meetAndGreet" ? false : deliveryType == "inTerminalOffice" ? true : false;
                    }
                    else
                    {
                        #region Reservasyon lokasyon bilgileri responsta geliyor. Bu yüzden iptal edildi.
                        var location = await LocationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                        Serilog.Log.Error("{@Yolcu360GetLocationsResponse}", location);
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
                        #endregion
                    }

                    return new ServiceResponseBase
                    {
                        Success = true,
                        Data = localReservation
                    };
                }
                else
                {
                    #region Yolcu servisinde hata durumu için farklı senaryo çalışacak. Tekrar rezervasyon isteği veya araç müsait değil hatası işlemleri.
                    //try
                    //{
                    //    var subject = result != null && result.errorMessage != null && result.errorCode != null ? result.errorCode + " - " + result.errorMessage : "Hata";
                    //    var body = subject + "<br/><h4>Yolcu360 Response</h4></br>" + vendor.VendorName + "</br>" + JsonConvert.SerializeObject(result);
                    //    _configurationService.SendErrorMail("Yolcu360 Rezervasyon Kayıt Hatası", subject, body);

                    //    /// 
                    //    /// "30011" hatasında rezervasyon kayıt tekrar denenmekte.
                    //    ///
                    //    //if (!postReservationRequest.IsRetry && result != null && result.errorCode != null && result.errorCode == "30011")
                    //    //{
                    //    //    postReservationRequest.IsRetry = true;
                    //    //    await PostReservation(postReservationRequest, vendor, additionalInformation, reservationNumber, reservationToken, exchangeRates, localReservation, apiExtras);
                    //    //}

                    //    /// 
                    //    /// Bu bölümde hataya düşen tedarikçiyi cache'e kaydetme ve araç sorgulama sayfasına yönlendirmede bu tedarikçi araçlarını gizleme işlemi. cache süresi 30 dk olarak konuşuldu. bir ayarlamaya bağlanacak.
                    //    /// 
                    //    if (_memoryCache != null && result != null && result.errorCode != null)
                    //    {
                    //        var cacheKey = string.Format(BLOCKED_VENDORS_CACHE_KEY, reservationToken.APIVendorName);

                    //        _memoryCache.Set(cacheKey, reservationToken.APIVendorName, new MemoryCacheEntryOptions
                    //        {
                    //            AbsoluteExpiration = DateTime.Now.AddMinutes(vendor.VendorBanTime),
                    //            Priority = CacheItemPriority.Normal
                    //        });
                    //    }

                    //    if (result?.errorCode != null)
                    //    {
                    //        return new ServiceResponseBase
                    //        {
                    //            Success = false,
                    //            Message = "Yolcu360 rezervasyon servisine değişiklik veya hata oluştu.",
                    //            Data = localReservation,
                    //            ServiceCode = "7"
                    //        };
                    //    }1793

                    //}
                    //catch (Exception ex)
                    //{

                    //}
                    #endregion
                }
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Yolcu360 login servisine ulaşılamadı.",
                Data = localReservation
            };
        }

        private Yolcu360RequestBase.ReservationRequestBody.OrderRequest CreateReservationRequestBody(ReservationToken reservationToken, Reservation localReservation, PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<Extra> apiExtras, bool isErrorNumber = false)
        {
            if (postReservationRequest.PostReservationRequestV2 != null)
            {
                var extraList = new List<Yolcu360RequestBase.ReservationRequestBody.OrderedProduct>();

                if (postReservationRequest.PostReservationRequestV2?.Extras?.Count > 0)
                {
                    extraList = postReservationRequest.PostReservationRequestV2?.Extras?
                        .Where(e => e.VendorExtraExists == null || e.VendorExtraExists == true)?
                        .Select(e => new Yolcu360RequestBase.ReservationRequestBody.OrderedProduct
                        {
                            product = new Yolcu360RequestBase.ReservationRequestBody.Product
                            {
                                productType = e.ExtraCode,
                                label = e.ApiExtraCode
                            },
                            count = e.Piece
                        })
                        .ToList();
                }

                var regex = new System.Text.RegularExpressions.Regex("[^A-Za-z0-9]");
                var specialChar = regex.IsMatch(postReservationRequest.PostReservationRequestV2.Customer.PersonalNumber.ReplaceWhitespace("").ToStringNullSafe().Trim());
                postReservationRequest.PostReservationRequestV2.Customer.PersonalNumber = specialChar ? "R123456" : postReservationRequest.PostReservationRequestV2.Customer.PersonalNumber;

                bool cLetter = System.Text.RegularExpressions.Regex.IsMatch(postReservationRequest.PostReservationRequestV2.Customer.PersonalNumber, "[a-zA-Z]");

                var cBirthday = !string.IsNullOrEmpty(postReservationRequest.PostReservationRequestV2.Customer.BirthDay) && postReservationRequest.PostReservationRequestV2.Customer.BirthDay.Length == 10 ? DateTime.ParseExact(postReservationRequest.PostReservationRequestV2.Customer.BirthDay, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.ParseExact("01.01.2000", "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

                var cPersonalNoType = !string.IsNullOrEmpty(postReservationRequest.PostReservationRequestV2.Customer.PersonalNumber) && !cLetter && postReservationRequest.PostReservationRequestV2.Customer.PersonalNumber.Trim().Length == 11 ? "identityNumber" : "passportNo";

                //var converted = birthday.ToString("yyyy-MM-dd");

                var requestBodyV2 = new Yolcu360RequestBase.ReservationRequestBody.OrderRequest
                {
                    carRentalSearchId = reservationToken.APIReferenceCode.Split('|')[0].ToStringNullSafe(),
                    //listingId = reservationToken.APIReferenceCode.Split('|')[1].ToStringNullSafe(),
                    listingId = localReservation.VehicleCode,
                    referenceID = postReservationRequest.PostReservationRequestV2.SendAgencyReservationNumber ? postReservationRequest.PostReservationRequestV2.AgencyReservationReference : "",
                    renter = new Yolcu360RequestBase.ReservationRequestBody.Renter
                    {
                        user = new Yolcu360RequestBase.ReservationRequestBody.User
                        {
                            firstName = postReservationRequest.PostReservationRequestV2.Customer.Name,
                            lastName = postReservationRequest.PostReservationRequestV2.Customer.Surname,
                            email = postReservationRequest.PostReservationRequestV2.Customer.Email,
                            identityNumber = cPersonalNoType == "identityNumber" ? postReservationRequest.PostReservationRequestV2.Customer.PersonalNumber.Trim().Length > 11 ? "22222222220" : postReservationRequest.PostReservationRequestV2.Customer.PersonalNumber.Trim() : null,
                            passportNo = cPersonalNoType == "passportNo" ? postReservationRequest.PostReservationRequestV2.Customer.PersonalNumber.Trim().Length >= 7 && postReservationRequest.PostReservationRequestV2.Customer.PersonalNumber.Trim().Length <= 12 ? postReservationRequest.PostReservationRequestV2.Customer.PersonalNumber.Trim() : "R123456" : null,
                            birthDate = cBirthday.ToString("yyyy-MM-dd"),
                            purchaseCount = 0
                        },
                        contactInformation = new Yolcu360RequestBase.ReservationRequestBody.ContactInformation
                        {
                            phone = new Yolcu360RequestBase.ReservationRequestBody.Phone
                            {
                                number = postReservationRequest.PostReservationRequestV2.Customer.PhoneNumber,
                                country = this.GetYolcu360CountryCode(postReservationRequest.PostReservationRequestV2.Customer.PhoneNumber.Replace("+", "").Substring(0, 2))
                            },
                            address = new Yolcu360RequestBase.ReservationRequestBody.Address
                            {
                                line = "Istanbul",
                                //line = "Test",
                                city = "İstanbul",
                                //city = postReservationRequest.CustomerAddress,
                                country = "TR", // düzenleme yapılacak
                                zip = 0
                            }
                        }
                    },
                    orderedProducts = extraList,
                    isFullCredit = postReservationRequest.PostReservationRequestV2.FullCredit.ToBoolNullSafe()
                    //isFullCredit = reservationToken.APIFullCredit.ToBoolNullSafe()
                };

                return requestBodyV2;
            }

            var rgx = new System.Text.RegularExpressions.Regex("[^A-Za-z0-9]");
            var hasSpecialChar = rgx.IsMatch(postReservationRequest.CustomerPersonalNumber.ReplaceWhitespace("").ToStringNullSafe().Trim());
            postReservationRequest.CustomerPersonalNumber = hasSpecialChar ? "R123456" : postReservationRequest.CustomerPersonalNumber;

            bool containsLetter = System.Text.RegularExpressions.Regex.IsMatch(postReservationRequest.CustomerPersonalNumber, "[a-zA-Z]");

            var birthday = !string.IsNullOrEmpty(postReservationRequest.CustomerBirthDay) && postReservationRequest.CustomerBirthDay.Length == 10 ? DateTime.ParseExact(postReservationRequest.CustomerBirthDay, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.ParseExact("01.01.2000", "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var customerPersonalNoType = !string.IsNullOrEmpty(postReservationRequest.CustomerPersonalNumber) && !containsLetter && postReservationRequest.CustomerPersonalNumber.Trim().Length == 11 ? "identityNumber" : "passportNo";


            localReservation.ReservationExtras.RemoveAll(e => e.ExtraType == AdditionalProductTypes.Insurance);

            //var converted = birthday.ToString("yyyy-MM-dd");

            var requestBody = new Yolcu360RequestBase.ReservationRequestBody.OrderRequest
            {
                carRentalSearchId = reservationToken.APIReferenceCode.Split('|')[0].ToStringNullSafe(),
                //listingId = reservationToken.APIReferenceCode.Split('|')[1].ToStringNullSafe(),
                listingId = localReservation.VehicleCode,
                referenceID = postReservationRequest.SendAgencyReservationNumber ? postReservationRequest.AgencyReservationReference : "",
                renter = new Yolcu360RequestBase.ReservationRequestBody.Renter
                {
                    user = new Yolcu360RequestBase.ReservationRequestBody.User
                    {
                        firstName = postReservationRequest.CustomerName,
                        lastName = postReservationRequest.CustomerSurname,
                        email = postReservationRequest.CustomerEmail,
                        identityNumber = customerPersonalNoType == "identityNumber" ? postReservationRequest.CustomerPersonalNumber.Trim().Length > 11 ? "22222222220" : postReservationRequest.CustomerPersonalNumber.Trim() : null,
                        passportNo = customerPersonalNoType == "passportNo" ? postReservationRequest.CustomerPersonalNumber.Trim().Length >= 7 && postReservationRequest.CustomerPersonalNumber.Trim().Length <= 12 ? postReservationRequest.CustomerPersonalNumber.Trim() : "R123456" : null,
                        birthDate = birthday.ToString("yyyy-MM-dd"),
                        purchaseCount = 0
                    },
                    contactInformation = new Yolcu360RequestBase.ReservationRequestBody.ContactInformation
                    {
                        phone = new Yolcu360RequestBase.ReservationRequestBody.Phone
                        {
                            number = postReservationRequest.CustomerTelephone,
                            country = this.GetYolcu360CountryCode(postReservationRequest.CustomerTelephone.Replace("+", "").Substring(0, 2))
                        },
                        address = new Yolcu360RequestBase.ReservationRequestBody.Address
                        {
                            line = "Istanbul",
                            //line = "Test",
                            city = "İstanbul",
                            //city = postReservationRequest.CustomerAddress,
                            country = "TR", // düzenleme yapılacak
                            zip = 0
                        }
                    }
                },
                orderedProducts = localReservation.ReservationExtras.Count > 0 ? localReservation.ReservationExtras.Select(x => new Yolcu360RequestBase.ReservationRequestBody.OrderedProduct
                {
                    count = 1,
                    product = new Yolcu360RequestBase.ReservationRequestBody.Product
                    {
                        productType = x.ExtraCode,
                        label = apiExtras.Where(p => p.ExtraCode == x.ExtraCode)?.FirstOrDefault()?.Label?.ToStringNullSafe(),
                        //price = x.APIPrice.ToIntNullSafe(),
                        price = x.ApiPrice.ToIntNullSafe(),
                        maxAmount = 1,
                        //damageInsuranceType = string.Empty,
                        //damageInsuranceCategory = !string.IsNullOrEmpty(x.DamageInsuranceCategory) ? x.DamageInsuranceCategory : null,
                        extraRangeAmount = 1
                    }
                }).ToList() : new List<Yolcu360RequestBase.ReservationRequestBody.OrderedProduct>(),
                isFullCredit = postReservationRequest.FullCredit.ToBoolNullSafe()
                //isFullCredit = reservationToken.APIFullCredit.ToBoolNullSafe()
            };

            return requestBody;
        }
        #region Ülke telfon kodu - Gerekli olursa düzenleme yapılacak.(Tamamlanmadı!)
        private string GetYolcu360CountryCode(string countryCode)
        {
            string code = "";
            switch (countryCode)
            {
                case "49":
                    code = "DE";
                    break;
                case "90":
                    code = "TR";
                    break;
                case "31":
                    code = "NL";
                    break;
                case "32":
                    code = "BE";
                    break;
                case "33":
                    code = "FR";
                    break;
                case "44":
                    code = "GB";
                    break;
                case "43":
                    code = "AT";
                    break;
                case "41":
                    code = "CH";
                    break;
                case "39":
                    code = "IT";
                    break;
                case "45":
                    code = "DK";
                    break;
                case "46":
                    code = "SE";
                    break;
                case "47":
                    code = "NO";
                    break;
                case "48":
                    code = "PL";
                    break;
                case "34":
                    code = "ES";
                    break;
                default:
                    code = "TR"; break;
            }
            ;
            return code;
            //return countryCode.StartsWith("49")
            //                                 ? "DE" : "TR";
            //return countryCode.StartsWith("01") 
            //       || countryCode.StartsWith("+1") 
            //       || countryCode.StartsWith("+49")
            //       || countryCode.StartsWith("49") ? "DE" : "TR";
        }

        public enum Country
        {
            AD, AE, AF, AG, AI, AL, AM, AN, AO, AQ, AR, AS, AT, AU, AW, AZ, BA, BB, BD, BE, BF, BG, BH, BI, BJ, BL, BM, BN, BO, BR, BS, BT, BW, BY, BZ, CA, CC, CD, CF, CG, CH, CI, CK, CL, CM, CN, CO, CR, CU, CV, CW, CX, CY, CZ, DE, DJ, DK, DM, DO, DZ, EC, EE, EG, EH, ER, ES, ET, FI, FJ, FK, FM, FO, FR, GA, GB, GD, GE, GG, GH, GI, GL, GM, GN, GQ, GR, GT, GU, GW, GY, HK, HN, HR, HT, HU, ID, IE, IL, IM, IN, IO, IQ, IR, IS, IT, JE, JM, JO, JP, KE, KG, KH, KI, KM, KN, KP, KR, KW, KY, KZ, LA, LB, LC, LI, LK, LR, LS, LT, LU, LV, LY, MA, MC, MD, ME, MF, MG, MH, MK, ML, MM, MN, MO, MP, MR, MS, MT, MU, MV, MW, MX, MY, MZ, NA, NC, NE, NG, NI, NL, NO, NP, NR, NU, NZ, OM, PA, PE, PF, PG, PH, PK, PL, PM, PN, PR, PS, PT, PW, PY, QA, RE, RO, RS, RU, RW, SA, SB, SC, SD, SE, SG, SH, SI, SJ, SK, SL, SM, SN, SO, SR, SS, ST, SV, SX, SY, SZ, TC, TD, TG, TH, TJ, TK, TL, TM, TN, TO, TR, TT, TV, TW, TZ, UA, UG, US, UY, UZ, VA, VC, VE, VG, VI, VN, VU, WF, WS, XK, YE, YT, ZA, ZM, ZW
        }
        #endregion
        private Yolcu360RequestBase.ReservationCancelRequestBody.RentalOrderParameters CreateCancelReservationBody(Reservation localReservation, Vendor vendor)
        {
            return new Yolcu360RequestBase.ReservationCancelRequestBody.RentalOrderParameters
            {
                carRentalOrderId = localReservation.APIReferenceCode2,
                email = vendor.ApiKey,
                key = vendor.ApiPassword
            };
        }
        private string GetOfficeWorkingHours(YolcuReservationResponseBase.OpeningHours office, DateTime tarih) => office != null ? tarih.DayOfWeek switch
        {
            DayOfWeek.Monday => office.monday != null ? office.monday.open + " - " + office.monday.close : "",
            DayOfWeek.Tuesday => office.tuesday != null ? office.tuesday.open + " - " + office.tuesday.close : "",
            DayOfWeek.Wednesday => office.wednesday != null ? office.wednesday.open + " - " + office.wednesday.close : "",
            DayOfWeek.Thursday => office.thursday != null ? office.thursday.open + " - " + office.thursday.close : "",
            DayOfWeek.Friday => office.friday != null ? office.friday.open + " - " + office.friday.close : "",
            DayOfWeek.Saturday => office.saturday != null ? office.saturday.open + " - " + office.saturday.close : "",
            DayOfWeek.Sunday => office.sunday != null ? office.sunday.open + " - " + office.sunday.close : "",
            _ => ""
        } : "";
    }
}