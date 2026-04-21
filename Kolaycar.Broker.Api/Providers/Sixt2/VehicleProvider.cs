using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Sixt2;
using KolayCAR.Broker.API.Services.Abstract;
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

namespace KolayCAR.Broker.API.Providers.Sixt2
{
    public class VehicleProvider : IVehicleProvider
    {
        public HttpManager _httpManager { get; set; }
        public AuthProvider _authProvider { get; set; }
        public VehicleProvider(Vendor vendor, ICacheService cacheService, bool disableTimeout)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            _authProvider = new AuthProvider(vendor.APIBaseUrl, cacheService);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var token = await _authProvider.GetBearerToken(vendor);

            if (token == null) return new(null, false, "Sixt token bilgisi alınamadı!");

            var vehicleResult = await _httpManager.GetAsyncWithModel<SixtResponseBase<SixtVehicleListResponse>>($"/api/v1/vehicles?type={Uri.EscapeDataString("external")}", headers: token);

            return vehicleResult?.result?.data?.Any() == true
                ? new(vehicleResult.result.data.Map(vendor), true)
                : new(null, false, "Sixt araç listesi alınamadı!");

        }

        private IDictionary<string, object> GetParameter()
        {
            return new Dictionary<string, object> { { "type", "external" } };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var token = await _authProvider.GetBearerToken(vendor);
            if (token == null) return new(null, false, "Sixt token bilgisi alınamadı!");

            var result = await _httpManager.PostAsyncWithModel<object, SixtResponseBase<SixtVehiclesResponse>>(
              requestPath: "/api/v1/vehicles/availables",
              parameters: GetVehiclesRequestParameters(additionalInformation, getVehiclesRequest),
              headers: token);

            if (result?.result?.vehicles?.Count > 0)
            {
                var apiVehicleList = result.result.vehicles;
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
                var mappedVehicleList = result.result.vehicles.Map(vendor, additionalInformation);

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    //apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.vehicle_group));
                    //apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == StringHelper.ToTurkishCharacterEscapeUpperCase(p.vehicle_group + "|" + (p.vehicle_brands.Contains(',') ? p.vehicle_brands.Split(',')[0].Replace(" ", "") : p.vehicle_brands.Replace(" ", "")))));
                    apiVehicleList.RemoveAll(p =>
                    {
                        var brandList = p.vehicle_brands
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(b => StringHelper.ToTurkishCharacterEscapeUpperCase(p.vehicle_group + "|" + b.Replace(" ", "")));

                        return !brandList.Any(code => localVehicles.Any(e => e.VehicleCode == code));
                    });
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
                    //var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == StringHelper.ToTurkishCharacterEscapeUpperCase(vehicle.value.vehicle_group + "|" + (vehicle.value.vehicle_brands.Contains(',') ? vehicle.value.vehicle_brands.Split(',')[0].Replace(" ", "") : vehicle.value.vehicle_brands.Replace(" ", "")))).ToList();

                    var brandCodes = vehicle.value.vehicle_brands.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(b => StringHelper.ToTurkishCharacterEscapeUpperCase(
                        vehicle.value.vehicle_group + "|" + b.Replace(" ", ""))).ToList();

                    foreach (var item in brandCodes)
                    {
                        var tempMappedVehicleList = mappedVehicleList.Where(x => item.Contains(x.VehicleCode)).ToList();
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
                                VehicleCode = item,
                                //VehicleCode = StringHelper.ToTurkishCharacterEscapeUpperCase(vehicle.value.vehicle_group + "|" + (vehicle.value.vehicle_brands.Contains(',') ? vehicle.value.vehicle_brands.Split(',')[0].Replace(" ", "") : vehicle.value.vehicle_brands.Replace(" ", ""))),
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = vehicle.value.daily_price.ToFloatNullSafe(),
                                APIDailyPricePayNow = vehicle.value.daily_price.ToFloatNullSafe(),
                                APIOneWayFee = vehicle.value.one_way_price.ToFloatNullSafe(),
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
                                APIReferenceCode = result.result.unid,
                                APIReferenceCode2 = token.ToJson(),
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
                }
                return new(mappedVehicleList, mappedVehicleList.Count > 0);
            }
            return new(null, false, $"{vendor.VendorName} servisine ulaşılamadı!");
        }
        public Dictionary<string, object> GetVehiclesRequestParameters(ResponseReservationStepsAdditionalInformation additionalInformation, GetVehiclesRequest getVehiclesRequest)
        {
            var parameters = new Dictionary<string, object> {
                { "pickup_station_id", additionalInformation.APIPickupLocationCode.Split("~")[0] },
                { "pickup_station", additionalInformation.APIPickupLocationCode.Split("~")[1] },
                { "return_station_id", additionalInformation.APIReturnLocationCode.Split("~")[0] },
                { "return_station", additionalInformation.APIReturnLocationCode.Split("~")[1] },
                { "pickup_date", getVehiclesRequest.PickupDate.ToDateTimeNullSafe().ToString("yyyy-MM-dd") },
                { "pickup_time", getVehiclesRequest.PickupTime },
                { "return_date", getVehiclesRequest.ReturnDate.ToDateTimeNullSafe().ToString("yyyy-MM-dd") },
                { "return_time", getVehiclesRequest.ReturnTime },
            };

            if (!string.IsNullOrEmpty(additionalInformation.LocationRateCode))
                parameters.Add("rate_key", additionalInformation.LocationRateCode);

            return parameters;
        }
    }
}
