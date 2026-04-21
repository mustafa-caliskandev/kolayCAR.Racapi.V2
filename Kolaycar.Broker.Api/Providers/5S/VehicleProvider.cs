using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers._5S;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Requests.BesSRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.BesSResponseBase;

namespace KolayCAR.Broker.API.Providers._5S
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        HttpManager _httpManager { get; set; }
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);

            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            _httpManager = new HttpManager(vendor.APIBaseUrl, httpClientHandler: clientHandler);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var result = await _httpManager.GetAsync2<List<NewReponseBaseVehicleClass.Root>>(
                           requestPath: "extservice/vehicle",
                           headers: Configuration.CreateHeaderWithAuth(vendor.ApiKey));

            if (result?.Data?.Count > 0)
                return new ServiceResponseBase(result.Data.Map(), true);

            return new ServiceResponseBase(null, false);
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            //var result = await RestManager.PostAsync<object, VehicleSearchResponse>(
              //  requestPath: "extservice/search",
              //  entity: GetVehicleSearchRequestBody(additionalInformation, baseVendorRequestCurrencyType),
              //  headers: Configuration.CreateHeaderWithAuth(vendor.ApiKey)
              //  );
            var result = await RestManager.PostAsync<AvailableVehicleRequest, VehicleSearchResponse>(
                requestPath: "extservice/search",
                entity: GetVehicleSearchRequestBody(additionalInformation, baseVendorRequestCurrencyType),
                headers: Configuration.CreateHeaderWithAuth(vendor.ApiKey)
                );

            if (result?.data?.Count > 0)
            {
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(
                                            result.data,
                                            v => v.model.id.ToStringNullSafe() + "-" + v.brand.id.ToStringNullSafe() + "-" + v.sipp.ToStringNullSafe(),
                                            v => v.sale_price.ToFloatNullSafe());
                //var mappedVehicleList = result.Map(additionalInformation, vendor);
                var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == (p.model.id.ToStringNullSafe() + "-" + p.brand.id.ToStringNullSafe() + "-" + p.sipp.ToStringNullSafe())));
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
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.model.id.ToString() + "-" + vehicle.value.brand.id.ToStringNullSafe() + "-" + vehicle.value.sipp.ToStringNullSafe()).ToList();
                    for (int i = 0; i < tempMappedVehicleList.Count; i++)
                    {
                        var mappedVehicle = tempMappedVehicleList[i];
                        var apiDailyPrice = vehicle.value.sale_price.ToFloatNullSafe() / vehicle.value.day_range.ToIntNullSafe();

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
                            VehicleCode = vehicle.value.model.id.ToString() + "-" + vehicle.value.brand.id.ToStringNullSafe() + "-" + vehicle.value.sipp.ToStringNullSafe(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee.ToFloatNullSafe(),
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = apiDailyPrice,
                            APIDailyPricePayNow = apiDailyPrice,
                            APIOneWayFee = vehicle.value.drop_price.ToFloatNullSafe(),
                            APIReferenceCode = vehicle.value.search_vehicle_key,
                            DepositPrice = mappedVehicle.DepositPrice,
                            VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                            VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                            ServiceCharge = mappedVehicle.ServiceCharge,
                            FuelType = mappedVehicle.FuelType,
                            TransmissionType = mappedVehicle.TransmissionType,
                            BaggageQuantityType = mappedVehicle.BaggageQuantityType,
                            PassangerQuantityType = mappedVehicle.PassangerQuantityType,
                            TotalKmLimit = mappedVehicle.TotalKMLimit ?? 0,
                            VehicleCategoryType = mappedVehicle.VehicleCategoryType,
                            VehicleType = mappedVehicle.VehicleType,
                            VendorFlightPassRequired = vendor.FlightNumberRequired ?? false,
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

                var apiVehicles = mappedVehicleList.Where(v => v.DailyPrice > 0).ToList();

                return new ServiceResponseBase
                {
                    Success = apiVehicles.Count > 0,
                    Data = apiVehicles
                };
            }

            return new ServiceResponseBase
            {
                Data = null,
                Success = false,
                ServiceMessage = "Servise ulaşılamadı!"
            };
        }
        private AvailableVehicleRequest GetVehicleSearchRequestBody(ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrencyType)
        {
            return new AvailableVehicleRequest
            {
                begin_date = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd HH:mm:ss") ,
                 end_date = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                 begin_location = additionalInformation.APIPickupLocationCode,
                 end_location = additionalInformation.APIReturnLocationCode,
                 currency = GetCurrency(baseVendorRequestCurrencyType)
            };
        }
        private string GetCurrency(CurrencyTypes baseVendorRequestCurrencyType)
        {
            return baseVendorRequestCurrencyType switch
            {
                CurrencyTypes.TRY => "TRY",
                CurrencyTypes.USD => "USD",
                CurrencyTypes.EUR => "EUR",
                CurrencyTypes.GBP => "GBP",
                CurrencyTypes.CHF => "CHF",
                _ => ""
            };
        }
    }
}
