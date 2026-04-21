using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.AutoHome;
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

namespace KolayCAR.Broker.API.Providers.AutoHome
{
    public class VehicleProvider : IVehicleProvider
    {
        HttpManager _httpmanager;
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _httpmanager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {

            var result = await _httpmanager.GetAsync2<List<AutoHomeResponseBase.Vehicle>>(
                requestPath: "/API/reservation/v2/QueryReservation.php",
                parameters: GetVehicleParameters(vendor)
                );

            if (result != null && result.Data != null && result.Data.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.Data.Map(vendor)
                };
            }

            return null;

        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var result = await _httpmanager.GetAsync2<List<AutoHomeResponseBase.Vehicle>>(
                requestPath: "/API/reservation/v2/QueryReservation.php",
                parameters: GetVehicleParameters2(vendor, getVehiclesRequest, additionalInformation),
                encode: false);

            if (result?.Data?.Count > 0 && (bool)result?.Success)
            {
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.Data, v => v.SubGroupShortName, v => v.DiscountedDailyPrice.ToFloatNullSafe());
                //var mappedVehicleList = result.Data.Map(additionalInformation, vendor);
                var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    //apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.GroupId.ToString()));
                }

                if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                    return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

                CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);

                var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                {
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.SubGroupShortName.ToStringNullSafe()).ToList();

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
                            VehicleId = vehicle.value.GroupId,
                            VehicleCode = mappedVehicle.VehicleCode,
                            APIReferenceCode = vehicle.value.SubGroupId.ToStringNullSafe(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            //APIDailyPrice = vehicle.value.WebPaymentPrice.ToFloatNullSafe(),
                            APIDailyPrice = vehicle.value.DiscountedDailyPrice.ToFloatNullSafe(),
                            //APIDailyPricePayNow = vehicle.value.WebPaymentPrice.ToFloatNullSafe(),
                            APIDailyPricePayNow = vehicle.value.DiscountedDailyPrice.ToFloatNullSafe(),
                            APIOneWayFee = vehicle.value.DropPrice.ToFloatNullSafe(),
                            DepositPrice = mappedVehicle.DepositPrice,
                            VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                            VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                            ServiceCharge = mappedVehicle.ServiceCharge,
                            FuelType = mappedVehicle.FuelType,
                            TransmissionType = mappedVehicle.TransmissionType,
                            SippCode = vehicle.value.SubGroupShortName,
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
                            FullCredit = mappedVehicle.FullCredit
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

            return new ServiceResponseBase
            {
                Success = false,
                Message = vendor.VendorName + " servisinden araç bulunamadı!"
            };
        }

        private IDictionary<string, object> GetVehicleParameters(Vendor vendor)
        {
            return new Dictionary<string, object>() { { "login", vendor.ApiKey }, { "passwd", vendor.ApiPassword }, { "alis_yeri_kodu", "1" }, { "iade_yeri_kodu ", "1" }, { "liste", "1" } };
        }

        private IDictionary<string, object> GetVehicleParameters2(Vendor vendor, GetVehiclesRequest getVehiclesRequest, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            var pickupdate = getVehiclesRequest.PickupDate;
            var pickuptime = getVehiclesRequest.PickupTime;
            var returndate = getVehiclesRequest.ReturnDate;
            var returntime = getVehiclesRequest.ReturnTime;
            var pickupdatetime = pickupdate + " " + pickuptime;
            var returndatetime = returndate + " " + returntime;
            return new Dictionary<string, object>()
            {
                    { "login", vendor.ApiKey },
                    { "passwd", vendor.ApiPassword },
                    { "alis_tarihi",pickupdatetime },
                    { "iade_tarihi", returndatetime },
                    { "alis_yeri_kodu" ,additionalInformation.APIPickupLocationCode },
                    { "iade_yeri_kodu",additionalInformation.APIReturnLocationCode },
                    { "liste", "2" },

            };
        }
    }
}
