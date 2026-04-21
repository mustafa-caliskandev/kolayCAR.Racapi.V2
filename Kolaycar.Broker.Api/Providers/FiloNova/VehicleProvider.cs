using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Helpers.FiloNova;
using KolayCAR.Broker.API.Mappers.FiloNova;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.FiloNova
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        LocationProvider LocationProvider { get; set; }
        private readonly IMemoryCache _memoryCache;

        public VehicleProvider(Vendor vendor, IMemoryCache memoryCache, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            AuthProvider = new AuthProvider();
            _memoryCache = memoryCache;
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var result = await RestManager.PostAsync<FiloNovaRequestBase.MasterRequest, FiloNovaResponseBase>(
                     requestPath: $"getmasterdata",
                     entity: GetMasterRequestBodyEntity(vendor),
                     headers: AuthProvider.CreateAuthHeaderWithContentType(vendor));
            if (result != null && result.groupCodeInformation != null && result.groupCodeInformation.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.groupCodeInformation.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            try
            {
                //FiloNova servisinde sorgulanan lokasyon için mesai saatleri kontrol edilir
                LocationProvider = new LocationProvider(vendor.APIBaseUrl, _memoryCache);
                var apiPickupAndReturnLocations = new List<string>
            {
                additionalInformation.APIPickupLocationCode,
                additionalInformation.APIReturnLocationCode
            };

                var workingHours = await LocationProvider.GetAPIWorkingHours(vendor);

                if (workingHours != null)
                {

                    var checkLocationIsAvailable = VehicleQueryHelper.CheckLocationIsAvailable(
                    workingHours,
                    additionalInformation.APIPickupLocationCode,
                    additionalInformation.APIReturnLocationCode,
                    ReservationHelper.GetDateTimeToDateAndTimeStrings(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime),
                    ReservationHelper.GetDateTimeToDateAndTimeStrings(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime));

                    if (checkLocationIsAvailable || workingHours.Count == 0)
                    {
                        var result = await RestManager.PostAsync<FiloNovaRequestBase.AvailabilityRequest, FiloNovaResponseBase>(
                                     requestPath: $"calculateAvailability",
                                     entity: GetAvailabilityRequestBodyEntity(additionalInformation, vendor),
                                     headers: AuthProvider.CreateAuthHeaderWithContentType(vendor));

                        if (result?.availabilityData != null && result?.responseResult?.result == true)
                        {
                            var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.availabilityData, v => v.groupCodeId, v => v.payAmount.ToFloatNullSafe());
                            //var mappedVehicleList = result.availabilityData.Map(additionalInformation, result, vendor);
                            var mappedVehicleList = apiVehicleList.Map(additionalInformation, result, vendor);
                            var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                            if (vendor.VehicleMappingActive)
                            {
                                mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                                apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.groupCodeId.ToString()));
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
                                var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.groupCodeId.ToString()).ToList();
                                for (int i = 0; i < tempMappedVehicleList.Count; i++)
                                {
                                    var mappedVehicle = tempMappedVehicleList[i];
                                    var apiDailyPrice = vehicle.value.payAmount.ToFloatNullSafe() / mappedVehicle.RentalDuration;

                                    var reservationToken = new ReservationToken
                                    {
                                        AgencyId = additionalInformation.Agency.AgencyId,
                                        VendorId = vendor.VendorId,
                                        APIVendorId = vendor.VendorId,
                                        APIVendorName = vendor.VendorName,
                                        APIVendorPhone = vendor.VendorPhone,
                                        APIVendorEmail = vendor.VendorEmail,
                                        APIVendorLogo = vendor.Logo,
                                        VehicleId = mappedVehicle.VehicleId.ToIntNullSafe(),
                                        VehicleCode = vehicle.value.groupCodeId.ToString(),
                                        APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                        APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                        APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                        APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                        CurrencyType = requestCurrencyType,
                                        RentalDuration = mappedVehicle.RentalDuration,
                                        DailyPrice = mappedVehicle.DailyPrice,
                                        OneWayFee = mappedVehicle.OneWayFee.ToFloatNullSafe(),
                                        DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                        //APIDailyPrice = mappedVehicle.DailyPrice.ToFloatNullSafe(),
                                        APIDailyPrice = apiDailyPrice,
                                        //APIDailyPricePayNow = mappedVehicle.DailyPricePayNow.ToFloatNullSafe(),
                                        APIDailyPricePayNow = apiDailyPrice,
                                        APIOneWayFee = result.oneWayFeeAmount.ToFloatNullSafe(),
                                        APIReferenceCode = result.trackingNumber,
                                        DepositPrice = mappedVehicle.DepositPrice,
                                        VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                                        VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                                        ServiceCharge = mappedVehicle.ServiceCharge,
                                        FuelType = mappedVehicle.FuelType,
                                        TransmissionType = mappedVehicle.TransmissionType,
                                        DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                                        PickupLocationId = getVehiclesRequest.PickupLocationId,
                                        ReturnLocationId = getVehiclesRequest.ReturnLocationId,
                                        LanguageType = languageType,
                                        PickupDateTime = pickupDateTime,
                                        ReturnDateTime = returnDateTime,
                                        VehicleName = mappedVehicle.VehicleName,
                                        VehicleImageUrl = mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                                        BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
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
                            return new ServiceResponseBase
                            {
                                Success = mappedVehicleList.Count > 0,
                                Data = mappedVehicleList
                            };
                        }
                    }
                }
                else
                {
                    return new ServiceResponseBase
                    {
                        Success = false,
                        Message = "FiloNova mesai saati uygun değil!",
                    };
                }

                return new ServiceResponseBase
                {
                    Success = false,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return default;
            }
        }


        private FiloNovaRequestBase.AvailabilityRequest GetAvailabilityRequestBodyEntity(ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
         new FiloNovaRequestBase.AvailabilityRequest
         {
             brokerCode = vendor.ApiClientId,
             langId = vendor.SecretKey.ToIntNullSafe(),
             queryParameters = new FiloNovaRequestBase.AvailabilityQueryParameters
             {
                 pickupBranchId = additionalInformation.APIPickupLocationCode,
                 dropoffBranchId = additionalInformation.APIReturnLocationCode,
                 pickupDateTime = additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "+03:00",
                 dropoffDateTime = additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "+03:00"
             },
             channelCode = 70,
         };

        private FiloNovaRequestBase.MasterRequest GetMasterRequestBodyEntity(Vendor vendor) =>
         new FiloNovaRequestBase.MasterRequest
         {
             brokerCode = vendor.ApiClientId,
             langId = vendor.SecretKey.ToIntNullSafe()
         };
    }


}
