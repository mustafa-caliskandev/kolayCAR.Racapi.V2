using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Eganis;
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

namespace KolayCAR.Broker.API.Providers.Eganis
{
    public class VehicleProvider : IVehicleProvider
    {

        HttpManager HttpManager { get; set; }
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            HttpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            AuthProvider = new AuthProvider(vendor.APIBaseUrl);
            RestManager = new RestManager(vendor.APIBaseUrl);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var getToken = await AuthProvider.getToken(vendor);

            if (getToken == null)
                return new ServiceResponseBase { Success = false, Message = "Token Başarılı Bir Şekilde Alınamadı.", };

            getToken.Add("Content-Type", "application/json");

            var result = await HttpManager.GetAsync<List<EganisResponseBase.VehicleResponse.VehicleRes>>
                (
                    requestPath: "/Api/GetVehicleGroups",
                    headers: getToken
                );



            if (result?.Data?.Count == 0)
            {
                return new ServiceResponseBase
                {
                    Success = false,
                    Message = "Vehicle listesi API'den başarıyla alınamadı.",
                };
            }

            return new ServiceResponseBase
            {
                Success = true,
                Data = result?.Data?.Map(vendor)
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var getToken = await AuthProvider.getToken(vendor);

            if (getToken == null)
                return new ServiceResponseBase { Success = false, Message = "Token Başarılı Bir Şekilde Alınamadı.", };

            var request = new EganisRequestBase.VehicleRequest
            {
                pickupLocationId = additionalInformation.APIPickupLocationId.ToIntNullSafe(),
                dropOffLocationId = additionalInformation.APIReturnLocationId.ToIntNullSafe(),
                pickupDay = additionalInformation.PickupDateTime.Day,
                pickupMonth = additionalInformation.PickupDateTime.Month,
                pickupYear = additionalInformation.PickupDateTime.Year,
                pickupHour = additionalInformation.PickupDateTime.Hour,
                pickupMin = additionalInformation.PickupDateTime.Minute,
                dropOffDay = additionalInformation.ReturnDateTime.Day,
                dropOffMonth = additionalInformation.ReturnDateTime.Month,
                dropOffYear = additionalInformation.ReturnDateTime.Year,
                dropOffHour = additionalInformation.ReturnDateTime.Hour,
                dropOffMin = additionalInformation.ReturnDateTime.Minute,
                currencyCode = getVehiclesRequest.CurrencyCode.ToString()
            };

            var result = await HttpManager.PostAsync<EganisRequestBase.VehicleRequest, List<EganisResponseBase.VehicleResponse.VehicleRes>>
                (
                    requestPath: "/Api/SearchVehicle",
                    headers: getToken,
                    entity: request 
                );

            if (result?.Data?.Count > 0)
            {
                var apiVehicleList = result.Data;
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                var mappedVehicleList = apiVehicleList.Map(vendor, additionalInformation);

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.vehGroupId.ToString()));
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
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.vehGroupId.ToStringNullSafe()).ToList();
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
                            VehicleCode = vehicle.value.vehGroupId.ToString(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = vehicle.value.dailyRentalFee.ToFloatNullSafe(),
                            APITotalPrice = vehicle.value.rentalFee.ToFloatNullSafe(),
                            APITotalPricePayNow = vehicle.value.rentalFee.ToFloatNullSafe(),
                            APIDailyPricePayNow = vehicle.value.dailyRentalFee.ToFloatNullSafe(),
                            APIOneWayFee = vehicle.value.dropFee.ToFloatNullSafe(),
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
            return new ServiceResponseBase
            {
                Success = false,
                Message = "Eganis servisine ulaşılamadı"
            };
        }
    }
}
