using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Central;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Central
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        LocationProvider LocationProvider { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var getVehicleListRequestParameters = GetVehicleListRequestParameters(vendor);

            var result = await RestManager.GetAsync<List<CentralResponseBase.CentralVehicle>>(
               requestPath: $"operation/API/QueryReservation.php",
               parameters: getVehicleListRequestParameters);

            if (result != null && result.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.Map()
                };
            }

            return new ServiceResponseBase();
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            ////Central servisinde sorgulanan lokasyon için mesai saatleri kontrol edilir
            //LocationProvider = new LocationProvider(vendor.APIBaseUrl);
            //var apiPickupAndReturnLocations = new List<string>
            //{
            //    additionalInformation.APIPickupLocationCode,
            //    additionalInformation.APIReturnLocationCode
            //};
            //var apiLocation = await LocationProvider.GetAPILocation(vendor, apiPickupAndReturnLocations);

            //if (apiLocation != null && apiLocation.Count > 0)
            //{
            //    var checkCentralLocationIsAvailable = VehicleQueryHelper.CheckCentralLocationIsAvailable(
            //        apiLocation,
            //        ReservationHelper.GetDateTimeToDateAndTimeStrings(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime),
            //        ReservationHelper.GetDateTimeToDateAndTimeStrings(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime));

            //    if (checkCentralLocationIsAvailable)
            //    {
            var getVehicleListRequestParameters = GetVehiclesRequestParameters(vendor, additionalInformation);

            var result = await RestManager.GetAsync<List<CentralResponseBase.CentralVehicle>>(
               requestPath: "operation/API/QueryReservation.php",
               parameters: getVehicleListRequestParameters);

            if (result != null && result.Count > 0)
            {
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result, v => v.SubGroupShortName, v => v.DiscountedDailyPrice.ToFloatNullSafe());
                var mappedVehicleList = result.Map(additionalInformation, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.SubGroupShortName));
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
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.SubGroupShortName.ToString()).ToList();
                    for (int i = 0; i < tempMappedVehicleList.Count; i++)
                    {
                        var mappedVehicle = tempMappedVehicleList[i];

                        var reservationToken = new ReservationToken
                        {
                            AgencyId = additionalInformation.Agency.AgencyId,
                            VendorId = vendor.VendorId,
                            APIVendorId = vendor.VendorId,
                            APIVendorName = vendor.VendorName,
                            APIVendorPhone = vendor.VendorPhone,
                            APIVendorEmail = vendor.VendorEmail,
                            APIVendorLogo = vendor.Logo,
                            VehicleId = mappedVehicle.VehicleId,
                            VehicleCode = vehicle.value.SubGroupShortName.ToStringNullSafe(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = vehicle.value.DiscountedDailyPrice.ToFloatNullSafe(),
                            APIDailyPricePayNow = vehicle.value.DiscountedDailyPrice.ToFloatNullSafe(),
                            APIOneWayFee = vehicle.value.DropPrice.ToFloatNullSafe(),
                            APIReferenceCode = vehicle.value.SubGroupId.ToStringNullSafe(),
                            APIReferenceCode2 = vehicle.value.MainRulesId.ToStringNullSafe(),
                            APIReferenceCode3 = vehicle.value.GroupId.ToStringNullSafe(),
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
            else
            {
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "Central servisinden araç bulunamadı!",
                };
            }
            //    }
            //    else
            //    {
            //        return new ServiceResponseBase
            //        {
            //            Success = false,
            //            Message = "Central mesai saati uygun değil!",
            //        };
            //    }
            //}

            //return new ServiceResponseBase
            //{
            //    Success = false,
            //    Message = "Central servisinden lokasyon bilgisine ulaşılamadı!",
            //};
        }

        private Dictionary<string, object> GetVehiclesRequestParameters(Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation) =>
            new Dictionary<string, object>()
            {
                { "login",  vendor.ApiKey},
                { "passwd",  vendor.ApiPassword},
                { "alis_yeri_kodu", additionalInformation.APIPickupLocationCode},
                { "iade_yeri_kodu", additionalInformation.APIReturnLocationCode},
                { "alis_tarihi", additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mm")},
                { "iade_tarihi", additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mm")},
                { "liste",  2},
            };

        private Dictionary<string, object> GetVehicleListRequestParameters(Vendor vendor) =>
           new Dictionary<string, object>()
           {
                { "login",  vendor.ApiKey},
                { "passwd",  vendor.ApiPassword},
                { "liste",  1},
           };
    }
}
