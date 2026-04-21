using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Yolcu360;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.Yolcu360RequestBase;

namespace KolayCAR.Broker.API.Providers.Yolcu360
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        private readonly Services.IVendorService _vendorService;
        private readonly IVendorVendorService _vendorVendorService;
        const string BLOCKED_VENDORS_CACHE_KEY = "BLOCKED_VENDORS_CACHE_KEY_{0}";
        //private readonly Models.BrokerContext _context;

        #region Cache
        private readonly IMemoryCache _memoryCache;
        const string VendorVendors = "VendorVendors";
        #endregion

        //public VehicleProvider(string apiBaseUrl)
        //{
        //    RestManager = new RestManager(apiBaseUrl);
        //    AuthProvider = new AuthProvider(apiBaseUrl);
        //}
        public VehicleProvider(Vendor vendor, IMemoryCache memoryCache, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            AuthProvider = new AuthProvider(vendor.APIBaseUrl, memoryCache);
            _memoryCache = memoryCache;
        }
        public async Task<ServiceResponseBase> GetVehicleList(Domain.Models.Vendor vendor)
        {
            return new ServiceResponseBase
            {
                Success = false,
                Message = "Yolcu servisine ulaşılamadı."
            };
        }
        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Domain.Models.Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var cookie = AuthProvider.Login(vendor.ApiKey, vendor.ApiPassword).Result;

            if (!string.IsNullOrEmpty(cookie))
            {
                //Araç arama sayfası ise 'car/listing/agency/' değilse 'car/listing/search/' ikinci durum yolcu cache
                var isNewSearch = getVehiclesRequest.IsReservationRequest || isVehicleSearchPage(additionalInformation);

                var result = isNewSearch ?
                    await RestManager.PostAsync<CarListingAgencyRequestBody, Yolcu360CarListingAgencyResponseBase.CarListingAgencyResponse>(
                    requestPath: "car/listing/agency/",
                    entity: CreateRequestBody(additionalInformation, vendor),
                    headers: AuthProvider.CreateHeaderWithCookie(cookie),
                    timeout: 30
                    ) :
                    await RestManager.PostAsync<CarListingAgencyRequestBody, Yolcu360CarListingAgencyResponseBase.CarListingAgencyResponse>(
                    requestPath: "car/listing/search/" + additionalInformation.ReservationToken.APIReferenceCode.Split('|')[0],
                    headers: AuthProvider.CreateHeaderWithCookie(cookie),
                    timeout: 30
                    );

                
                if (result != null && !string.IsNullOrEmpty(result.errorCode))
                {
                    if (!isNewSearch && result.errorCode == "20002")//yolcu servisi cacheden araç listelemede boş dönerse tekrardan istek atılacak şekilde ayarlandı.
                    {
                        result = await RestManager.PostAsync<CarListingAgencyRequestBody, Yolcu360CarListingAgencyResponseBase.CarListingAgencyResponse>(
                        requestPath: "car/listing/agency/",
                        entity: CreateRequestBody(additionalInformation, vendor),
                        headers: AuthProvider.CreateHeaderWithCookie(cookie)
                        );
                    }
                    else if (result.errorCode == "40016" || result.errorMessage == "Invalid user type")// session süresi dolmuş işe burada session yenilenecek şekilde ayarlandı.
                    {
                        cookie = AuthProvider.Login(vendor.ApiKey, vendor.ApiPassword, true).Result;

                        result = await RestManager.PostAsync<CarListingAgencyRequestBody, Yolcu360CarListingAgencyResponseBase.CarListingAgencyResponse>(
                        requestPath: "car/listing/agency/",
                        entity: CreateRequestBody(additionalInformation, vendor),
                        headers: AuthProvider.CreateHeaderWithCookie(cookie)
                        );
                    }
                }

                if (result?.data?.Count > 0)
                {
                    #region Yeni tedarikçi var ise otomatik kayıt işlemi için düzenleme
                    //await InsertNewVendors(result, vendor);
                    InsertVendorVendors(result, vendor, getVehiclesRequest, additionalInformation);
                    #endregion

                    var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.data, v => v.listingId, v => v.pricing.agencyPrice.ToFloatNullSafe());
                    var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor, additionalInformation.Agency);
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                    VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);
                    if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                        return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

                    var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                    var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                    var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                    foreach (var apiVehicle in apiVehicleList)
                    {
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == apiVehicle.listingId).ToList();
                       
                        for (int i = 0; i < tempMappedVehicleList.Count; i++)
                        {
                            var mappedVehicle = tempMappedVehicleList[i];
                            var apiDailyPrice = (apiVehicle.pricing.agencyPrice / 100) / mappedVehicle.RentalDuration;
                            bool vendorFlightPassRequired = false;

                            if (vendor?.FlightNumberRequired == true)
                            {
                                var flightCardMandatory = vendor.VendorVendors?
                                    .FirstOrDefault(e => e?.Active == true && e?.VendorName == apiVehicle?.vendor?.name)?
                                    .FlightCardMandatory;

                                vendorFlightPassRequired = flightCardMandatory.ToBoolNullSafe() == true;
                            }

                            var reservationToken = new ReservationToken
                            {
                                AgencyId = additionalInformation.Agency.AgencyId,
                                VendorId = vendor.VendorId,
                                //APIVendorId = mappedVehicle.VendorId == 0 ? vendor.VendorId : mappedVehicle.VendorId,
                                APIVendorId = apiVehicle.vendor.id * 100,
                                APIVendorName = apiVehicle.vendor.name,
                                APIVendorPhone = apiVehicle.office.phones.Count > 0 ? apiVehicle.office.phones[0].dialCode.ToString() + apiVehicle.office.phones[0].number.ToString() : string.Empty,
                                APIVendorEmail = apiVehicle.office.email,
                                APIVendorLogo = mappedVehicle.VendorLogo,
                                VehicleId = 1,
                                VehicleCode = mappedVehicle.VehicleCode,
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = apiDailyPrice.ToFloatNullSafe(),
                                APIDailyPricePayNow = apiDailyPrice.ToFloatNullSafe(),
                                APIOneWayFee = apiVehicle.pricing.oneWayPrice.ToFloatNullSafe() / 100 + apiVehicle.pricing.deliveryFee.ToFloatNullSafe() / 100,
                                APIReferenceCode = result.carRentalSearchId + "|" + apiVehicle.listingId,
                                DepositPrice = mappedVehicle.DepositPrice.ToFloatNullSafe(),
                                //VendorMinimumDriverAge = apiVehicle.value.rules.driverAge,
                                VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                                //VendorMinimumDrivingLicenseAge = apiVehicle.value.VendorMinimumDrivingLicenseAge,
                                VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                                ServiceCharge = mappedVehicle.ServiceCharge,
                                FuelType = mappedVehicle.FuelType,
                                TransmissionType = mappedVehicle.TransmissionType,
                                VehicleCategoryType = mappedVehicle.VehicleCategoryType,
                                IsOffice = mappedVehicle.IsOffice,
                                //IsAirport = apiVehicle.value.IsAirport,
                                DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                                PickupLocationId = getVehiclesRequest.PickupLocationId,
                                ReturnLocationId = getVehiclesRequest.ReturnLocationId,
                                LanguageType = languageType,
                                PickupDateTime = pickupDateTime,
                                ReturnDateTime = returnDateTime,
                                VehicleName = mappedVehicle.VehicleName,
                                VehicleImageUrl = mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                                BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
                                APIFullCredit = mappedVehicle.FullCredit.ToBoolNullSafe(),
                                ApiVendorType = VendorTypes.Yolcu360,
                                SpecialProfitApplied = mappedVehicle.SpecialProfitApplied,
                                BaggageQuantityType = mappedVehicle.BaggageQuantityType,
                                PassangerQuantityType = mappedVehicle.PassangerQuantityType,
                                TotalKmLimit = mappedVehicle.TotalKMLimit ?? 0,
                                VehicleType = mappedVehicle.VehicleType,
                                VendorFlightPassRequired = vendorFlightPassRequired,
                                FullCredit = mappedVehicle.FullCredit,
                                SippCode = mappedVehicle.SippCode
                            };
                            mappedVehicle.ReservationToken = reservationToken.ToJson();
                         
                            if (additionalInformation.Agency.SpecialParameters)
                            {
                                mappedVehicle.SpecialVendorId = vendor.VendorId.ToString();
                                //mappedVehicle.SpecialVendorName = vendor.VendorName;
                                mappedVehicle.SpecialVendorLogo = vendor.Logo;
                            }
                        }
                    }

                    // Aktif olmayan tedarikçi tedarikçi araçlarını çıkarma
                    #region Kapalı tedarikçi araçlarını yansıtmama
                    if (mappedVehicleList.Count > 0 && vendor.VendorVendors?.Count > 0)
                    {
                        var sameNames = vendor.VendorVendors.Where(vv => vv.Active == false && vv.VendorId == vendor.VendorId).Select(mv => mv.VendorName).ToList();
                        if (_memoryCache != null)
                        {
                            foreach (var item in result.totalStatistics.vendor)
                            {
                                if (_memoryCache.TryGetValue(string.Format(BLOCKED_VENDORS_CACHE_KEY, item.value), out string name))
                                {
                                    sameNames.Add(name);
                                }
                            }
                        }
                        mappedVehicleList.RemoveAll(mv => sameNames.Contains(mv.VendorName));
                        //mappedVehicleList.RemoveAll(mv => !vendor.VendorVendors.Any(vv => vv.VENDORNAME.ToLower() == mv.VendorName.ToLower() && vv.ACTIVE));
                    }
                    #endregion
                    #region Full-Credit olamayanları listelememek için kontrol
                    //if (vendor.CreditType == CreditType.FullCredit)
                    //{
                    //    mappedVehicleList.RemoveAll(p => p.FullCredit == false);
                    //}
                    #endregion

                    return new ServiceResponseBase
                    {
                        Success = mappedVehicleList.Count > 0,
                        Data = mappedVehicleList,
                        Data2 = vendor.NewVendors
                    };
                }

                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "Yolcu360 araç servisine ulaşılamadı."
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Yolcu360 kullanıcı girişi yapılamadı."
            };
        }
        private static bool GetYolcu360DeliveryType(string type)
        {
            switch (type)
            {
                case "nonTerminalMeetAndGreet": return false;
                case "meetAndGreet": return false;
                case "inTerminalOffice": return true;
                case "deliveredToAddress": return false;
                case "fromOffice": return true;
                case "nonTerminalValet": return false;
                default: return false;
            }
        }
        private CarListingAgencyRequestBody CreateRequestBody(ResponseReservationStepsAdditionalInformation additionalInformation, Domain.Models.Vendor vendor)
        {
            return new CarListingAgencyRequestBody
            {
                pickupLocationId = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
                dropoffLocationId = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
                pickupDatetime = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                dropoffDatetime = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                //requestedCommissionType = vendor.RentalWorkingType == VendorWorkingTypes.Commission ? 2 : 1,
                //requestedCommissionAmount = vendor.ProfitMarkupDailyPrice.ToIntNullSafe()
            };
        }
        private async Task InsertVendorVendors(Yolcu360CarListingAgencyResponseBase.CarListingAgencyResponse result, Domain.Models.Vendor vendor, GetVehiclesRequest getVehiclesRequest, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            if (result.totalStatistics != null && result.totalStatistics.vendor != null && result.totalStatistics.vendor.Count > 0)
            {
                var vendorVendors = vendor.VendorVendors;

                var yolcuVendorList = result.totalStatistics.vendor.Select(x => new Domain.Models.VendorVendor
                {
                    Active = true,
                    VendorId = vendor.VendorId,
                    VendorName = x.display
                }).ToList();

                //checkAvisAndBudget(yolcuVendorList, getVehiclesRequest, additionalInformation);//TODO: Avis ve Budget zaman zaman yansıyor. Var mı kontrolü için eklendi. Silinecek.

                yolcuVendorList.RemoveAll(v => vendorVendors.Any(vv => vv.VendorName == v.VendorName));

                if (yolcuVendorList.Count > 0)
                {
                    #region veri tabanına eklenecek liste
                    vendor.NewVendors = yolcuVendorList;
                    #endregion
                }
            }
        }
        private bool isVehicleSearchPage(ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            if (additionalInformation.ReservationToken == null)
                return true;
            return string.IsNullOrEmpty(additionalInformation.ReservationToken.APIReferenceCode);
        }
        private async Task checkAvisAndBudget(List<Domain.Models.VendorVendor> vendorVendors, GetVehiclesRequest getVehiclesRequest, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            try
            {
                var check = vendorVendors.Where(x => x.VendorName == "Avis" || x.VendorName == "Budget").ToList().Count > 0;
                if (check)
                {
                    Serilog.Log
                        .ForContext("PickupLocation", getVehiclesRequest.PickupLocationId)
                        .ForContext("ReturnLocation", getVehiclesRequest.ReturnLocationId)
                        .ForContext("PickupDate", getVehiclesRequest.PickupDate.ToString() + " " + getVehiclesRequest.PickupTime.ToString())
                        .ForContext("ReturnDate", getVehiclesRequest.ReturnDate.ToString() + " " + getVehiclesRequest.ReturnTime.ToString())
                        .ForContext("Agency", additionalInformation.Agency.ToString())
                        .Fatal("{@AvisAndBudget}", getVehiclesRequest);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private IDictionary<string, object> CreateBrokerSendMailRequest(string subject, string body, string mail, string replyTo)
        {
            return new Dictionary<string, object> {
            { "fromTitle", "Hatalı "},
            { "toMailAddress", mail},
            { "subject", subject},
            { "body", body},
            { "replyTo", replyTo}
        };
        }
    }
}
