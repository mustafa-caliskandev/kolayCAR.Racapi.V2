using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Enuygun;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Enuygun
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        ICacheService _cacheService { get; set; }
        public VehicleProvider(Vendor vendor, bool disableTimeout, ICacheService cacheService)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            AuthProvider = new AuthProvider(vendor.APIBaseUrl);
            _cacheService = cacheService;
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            return new ServiceResponseBase
            {
                Success = false,
                Message = $"{vendor.VendorName} servisine ulaşılamadı."
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var userName = vendor.ApiKey;
            var password = vendor.ApiPassword;

            var auth = await _cacheService.GetOrCreateAsync($"EnUygun{vendor.VendorName}Token", () => AuthProvider.GetToken(userName, password), TimeSpan.FromMinutes(30));

            if (auth?.Status == "OK")
            {
                var result = await RestManager.PostAsync<EnuygunRequest.Search.Vehicles, EnuygunResponse.Search.Root>(
                    requestPath: "/api/v1/search",
                    entity: getSearchRequest(getVehiclesRequest, additionalInformation, baseVendorRequestCurrencyType, vendor),
                    headers: AuthProvider.CreateHeaderWithToken(auth.Data.Token)
                    );

                if (result?.status == "OK")
                {
                    if (result?.data?.reservations?.Count > 0)
                    {
                        var requestId = result.data.requestId;
                        var apiVehicleList = result.data.reservations;
                        //apiVehicleList.RemoveAll(x => x.breakdowns[0].officePrice > 0);

                        var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
                        var mappedVehicleList = apiVehicleList.Map(additionalInformation, requestCurrencyType, vendor, requestId, exchangeRates);

                        if (vendor.VehicleMappingActive)
                        {
                            mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                            apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.vehicle.matchCode));
                        }

                        CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                        VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);
                        if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                            return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

                        var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                        var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                        var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                        foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                        {
                            //var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.vehicle.matchCode && x.ApiDailyPrice == vehicle.value.breakdowns[0].chargePrice / vehicle.value.days).ToList();    
                            var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.vehicle.matchCode && x.ApiVendorName == vehicle.value.company.name).ToList();
                            //var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.vehicle.matchCode).ToList();   
                            for (int i = 0; i < tempMappedVehicleList.Count; i++)
                            {
                                var mappedVehicle = tempMappedVehicleList[i];
                                var adress = vehicle.value.company.name + "|" + "pickUpOffice" + " ~ " +
                                    vehicle.value.pickUpOffice.address + " ~ " +
                                    vehicle.value.pickUpOffice.phoneNumber + " | " +
                                    "dropOffice " + " ~ " +
                                    vehicle.value.dropOffOffice.address + " ~ " +
                                    vehicle.value.dropOffOffice.phoneNumber;

                                //var TotalPrice = vehicle.value.breakdowns[0].type == "reservation" ? vehicle.value.breakdowns[0].chargePrice.ToFloatNullSafe() : 0;

                                var TotalPrice = vehicle.value.breakdowns[0].type == "reservation" ? vehicle.value.breakdowns[0].totalPrice : 0;

                                string vendorName = vendor.ShowSubVendorLogo == true ? vehicle.value.company.name : vendor.VendorName;
                                string vendorLogo = vendor.ShowSubVendorLogo == true ? vehicle.value.company.logoUri : vendor.Logo;

                                var reservationToken = new ReservationToken
                                {

                                    AgencyId = additionalInformation.Agency.AgencyId,
                                    VendorId = vendor.VendorId,
                                    APIVendorId = additionalInformation.Vendor.VendorId,
                                    //APIVendorName = vendor.VendorName,
                                    //APIVendorName = mappedVehicle.VendorName,
                                    APIVendorName = vendorName,
                                    APIVendorEmail = vendor.VendorEmail,
                                    APIVendorLogo = vendor.Logo,
                                    //APIVendorLogo = mappedVehicle.VendorLogo,
                                    //APIVendorLogo = vendorLogo,
                                    APIVendorPhone = vendor.VendorPhone,
                                    VehicleId = mappedVehicle.VehicleId,
                                    VehicleCode = vehicle.value.vehicle.matchCode,
                                    APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                    APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                    APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                    APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                    CurrencyType = requestCurrencyType,
                                    RentalDuration = mappedVehicle.RentalDuration,
                                    DailyPrice = mappedVehicle.DailyPrice,
                                    OneWayFee = mappedVehicle.OneWayFee,
                                    DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                    APIDailyPrice = Math.Round(TotalPrice / mappedVehicle.RentalDuration, 2).ToFloatNullSafe(),
                                    APITotalPrice = Math.Round(TotalPrice, 2).ToFloatNullSafe(),
                                    APITotalPricePayNow = Math.Round(TotalPrice / mappedVehicle.RentalDuration, 2).ToFloatNullSafe(),
                                    APIReferenceCode = requestId,
                                    APIReferenceCode2 = vehicle.value.referenceId,
                                    APIOneWayFee = vehicle.value.breakdowns[1].type == "drop" ? vehicle.value.breakdowns[1].raw.totalPrice.ToFloatNullSafe() : 0,
                                    DepositPrice = mappedVehicle.DepositPrice,
                                    APIDailyPricePayNow = Math.Round(vehicle.value.price.dailyPrice, 2).ToFloatNullSafe(),
                                    VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                                    VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                                    ServiceCharge = mappedVehicle.ServiceCharge,
                                    FuelType = mappedVehicle.FuelType,
                                    TransmissionType = mappedVehicle.TransmissionType,
                                    DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                                    PickupLocationId = mappedVehicle.PickupLocationId,
                                    ReturnLocationId = mappedVehicle.ReturnLocationId,
                                    LanguageType = languageType,
                                    PickupDateTime = pickupDateTime,
                                    ReturnDateTime = returnDateTime,
                                    VehicleName = mappedVehicle.VehicleName,
                                    VehicleImageUrl = mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                                    BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
                                    APIFullCredit = mappedVehicle.FullCredit.ToBoolNullSafe(),
                                    ApiVendorType = VendorTypes.EnUygun,
                                    APIReferenceCode3 = adress, //search kısmında dönen pickup ve drop office bilgileri içermektedir.
                                    SpecialProfitApplied = mappedVehicle.SpecialProfitApplied,
                                    BaggageQuantityType = mappedVehicle.BaggageQuantityType,
                                    PassangerQuantityType = mappedVehicle.PassangerQuantityType,
                                    TotalKmLimit = mappedVehicle.TotalKMLimit ?? 0,
                                    VehicleCategoryType = mappedVehicle.VehicleCategoryType,
                                    VehicleType = mappedVehicle.VehicleType,
                                    VendorFlightPassRequired = vendor.FlightNumberRequired ?? false,
                                    FullCredit = mappedVehicle.FullCredit,
                                    SippCode = mappedVehicle.SippCode
                                };

                                mappedVehicle.ReservationToken = reservationToken.ToJson();

                                if (additionalInformation.Agency.SpecialParameters)
                                {
                                    mappedVehicle.SpecialVendorId = vendor.VendorId.ToString();
                                    mappedVehicle.SpecialVendorName = vendor.VendorName;
                                    mappedVehicle.SpecialVendorLogo = vendor.Logo;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(getVehiclesRequest.Guid))
                        {
                            Serilog.Log.Error("{@Step8}", mappedVehicleList.ToJson());
                        }
                        return new ServiceResponseBase
                        {
                            Success = mappedVehicleList.Count > 0,
                            Data = mappedVehicleList
                        };
                    }
                    return new ServiceResponseBase
                    {
                        Success = false,
                        Message = "Maalesef, girdiğiniz bilgilere uygun bir araç bulunamadı."
                    };
                }

                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "EnUygun servisine ulaşılamadı (Message = " + result?.userMessage + ")"
                };
            }
            return null;
        }

        private EnuygunRequest.Search.Vehicles getSearchRequest(GetVehiclesRequest getVehiclesRequest, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrencyType, Vendor vendor)
        {
            DateTime pickupDate = DateTime.Parse(getVehiclesRequest.PickupDate);
            DateTime returnDate = DateTime.Parse(getVehiclesRequest.ReturnDate);

            var requestSearch = new EnuygunRequest.Search.Vehicles
            {
                pickUpDate = pickupDate.Date.ToString("yyyy-MM-dd"),
                pickUpTime = getVehiclesRequest.PickupTime,
                dropOffDate = returnDate.Date.ToString("yyyy-MM-dd"),
                dropOffTime = getVehiclesRequest.ReturnTime,
                pickUpLocation = additionalInformation.APIPickupLocationCode,
                dropOffLocation = additionalInformation.APIReturnLocationCode,
                currency = baseVendorRequestCurrencyType.ToString(),
                brokerParameters = new EnuygunRequest.Search.BrokerParameters
                {
                    ratio = vendor.ProfitMarkupDailyPrice.ToIntNullSafe(),
                    contractType = vendor.ApiClientId
                }


            };
            return requestSearch;
        }
    }
}
