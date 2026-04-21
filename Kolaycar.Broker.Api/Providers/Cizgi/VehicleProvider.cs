using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Cizgi;
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

namespace KolayCAR.Broker.API.Providers.Cizgi
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            AuthProvider = new AuthProvider(vendor.APIBaseUrl);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (!string.IsNullOrEmpty(vendor.SecretKey))// TODO: silinecek
            {
                Serilog.Log
                    .ForContext("Token", auth?.access_token.ToStringNullSafe())
                    .Error("{@CizgiAuthLog}", auth);
            }
            if (auth != null && !string.IsNullOrEmpty(auth.access_token))
            {
                var result = await RestManager.GetAsync<CizgiResponseBase.VehicleResponse>(
                       requestPath: $"list-cars",
                       headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token));

                if (!string.IsNullOrEmpty(vendor.SecretKey))
                {
                    Serilog.Log
                        .ForContext("Fleet", result?.fleet.ToStringNullSafe())
                        .Error("{@CizgiResultLog}", result);
                }

                if (result != null && result.status == 1 && result.fleet != null && result.fleet.Count > 0)
                {
                    return new ServiceResponseBase
                    {
                        Success = result != null,
                        Data = result.fleet.Map()
                    };
                }
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (auth != null && !string.IsNullOrEmpty(auth.access_token))
            {
                var result = await RestManager.PostAsync<CizgiRequestBase.AvailabilityRequest, CizgiResponseBase>(
                 requestPath: $"reservation/search",
                 entity: GetAvailabilityRequestBodyEntity(additionalInformation, vendor),
                 headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token));

                if (result?.results?.Count > 0 && result?.query?.duration > 0)
                {
                    var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.results, v => v.car_id, v => v.per_day.ToFloatNullSafe());
                    //var mappedVehicleList = result.results.Map(additionalInformation, result, vendor);
                    var mappedVehicleList = apiVehicleList.Map(additionalInformation, result, vendor);
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    if (vendor.VehicleMappingActive)
                    {
                        mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                        apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.car_id.ToString()));
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
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.car_id.ToString()).ToList();
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
                                VehicleCode = vehicle.value.car_id.ToString(),
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = vehicle.value.per_day.ToFloatNullSafe(),
                                APIDailyPricePayNow = vehicle.value.per_day.ToFloatNullSafe(),
                                APIOneWayFee = vehicle.value.oneway_fee.ToFloatNullSafe(),
                                APIReferenceCode = vehicle.value.car_id.ToString(),
                                APIReferenceCode2 = result.search_id,
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

            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };
        }

        private CizgiRequestBase.AvailabilityRequest GetAvailabilityRequestBodyEntity(ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
       new CizgiRequestBase.AvailabilityRequest
       {

           pickup_location = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
           pickup_date = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
           dropoff_location = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
           dropoff_date = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
           rate_codes = null
       };
    }
}
