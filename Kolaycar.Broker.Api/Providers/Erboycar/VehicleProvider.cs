using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Erboycar;
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

namespace KolayCAR.Broker.API.Providers.Erboycar
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var result = await RestManager.GetAsync<ErboycarResponseBase.VehicleResponse>(
            requestPath: "search",
            parameters: GetVehiclesRequestParameters(additionalInformation));

            if ((bool)result?.availableVehicles?.Any()
                && (int)Math.Ceiling((additionalInformation.ReturnDateTime - additionalInformation.PickupDateTime).TotalDays) >= result.rentalDays)
            {
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.availableVehicles, v => v._id, v => v.discounted_daily_price.ToFloatNullSafe());
                //var mappedVehicleList = result.availableVehicles.Map(additionalInformation, result, vendor);
                var mappedVehicleList = apiVehicleList.Map(additionalInformation, result, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p._id.ToString()));
                }

                CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);

                var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                {
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value._id.ToString()).ToList();
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
                            VehicleId = mappedVehicle.VehicleId.ToIntNullSafe(),
                            VehicleCode = vehicle.value._id.ToString(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = vehicle.value.discounted_daily_price.ToFloatNullSafe(),
                            APIDailyPricePayNow = vehicle.value.discounted_daily_price.ToFloatNullSafe(),
                            APIOneWayFee = result.oneWayPrice.ToFloatNullSafe(),
                            APIReferenceCode = vehicle.value._token.ToStringNullSafe(),
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
                Message = "Erboycar servisinden araç bulunamadı!"
            };
        }

        private Dictionary<string, object> GetVehiclesRequestParameters(ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            return new Dictionary<string, object>()
            {
                { "station", additionalInformation.APIPickupLocationCode },
                { "drop_station", additionalInformation.APIReturnLocationCode },
                { "pickupDateTime", additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm") },
                { "dropDateTime", additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm") },
                { "token", additionalInformation.Vendor.ApiKey },
            };
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var result = await RestManager.GetAsync<List<ErboycarResponseBase.VehicleListItem>>(
                    requestPath: "vehicle-groups");
            if (result != null && result.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.Map()?.OrderBy(x => x.VehicleName).ToList()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };
        }
    }
}
