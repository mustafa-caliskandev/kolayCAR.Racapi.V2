using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Wheelsys;
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

namespace KolayCAR.Broker.API.Providers.Wheelsys
{
    public class VehicleProvider : IVehicleProvider
    {
        public HttpManager _httpManager;
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {

            var result = await _httpManager.GetXmlAsync<WheelsysResponseBase.Root>(
                requestPath: $"{vendor.ApiKey}/link/v3/groups_{vendor.ApiPassword.Split('-')[0]}.html",
                parameters: GetVehicleListRequestParameters(vendor));

            if (result != null && result.response != null)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.response.cargroup.Map(vendor)
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = $"{vendor.VendorName} servisine ulaşılamadı!"
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var result = await _httpManager.GetXmlAsync<WheelsysResponseBase.Root>(
               requestPath: $"/{vendor.ApiKey}/link/v3/price-quote_{vendor.ApiPassword.Split('-')[0]}.html",
               parameters: GetVehiclesRequestParameters(vendor, additionalInformation),
               logResult: false,
               vendorName: vendor.VendorName);

            if (result?.pricequote?.rates != null)
            {
                var apiReservationInfo = result.pricequote;
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.pricequote.rates.category, v => v.code, v => v.totalrate.ToFloatNullSafe());
                //var mappedVehicleList = result.pricequote.rates.category.Map(result.pricequote, additionalInformation, vendor);
                var mappedVehicleList = apiVehicleList.Map(result.pricequote, additionalInformation, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.code));
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
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.code.ToString()).ToList();
                    for (int i = 0; i < tempMappedVehicleList.Count; i++)
                    {
                        var mappedVehicle = tempMappedVehicleList[i];

                        float totalPrice = VehicleHelper.ConvertCommaFreePriceToFloat(vehicle.value.totalrate);
                        float oneWayFee = VehicleHelper.ConvertCommaFreePriceToFloat(vehicle.value.onewaycharge);
                        int rentalDuration = mappedVehicle.RentalDuration;
                        float dailyPrice = (totalPrice - oneWayFee) / rentalDuration;

                        var reservationToken = new ReservationToken
                        {
                            AgencyId = additionalInformation.Agency.AgencyId,
                            VendorId = vendor.VendorId,
                            //Ek ürün gelmeme sorunu için yapıldı
                            APIVendorId = vendor.VendorId,
                            APIVendorName = vendor.VendorName,
                            APIVendorPhone = vendor.VendorPhone,
                            APIVendorEmail = vendor.VendorEmail,
                            APIVendorLogo = vendor.Logo,
                            VehicleId = mappedVehicle.VehicleId,
                            VehicleCode = vehicle.value.code,
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = dailyPrice,
                            APIDailyPricePayNow = dailyPrice,
                            APIOneWayFee = oneWayFee,
                            APIReferenceCode = apiReservationInfo.id,
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
                            VehicleImageUrl = mappedVehicle.VehicleImages != null && mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
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
                    Success = true,
                    Data = mappedVehicleList
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = $"{vendor.VendorName} servisinden araç bulunamadı!"
            };
        }

        private Dictionary<string, object> GetVehiclesRequestParameters(Domain.Models.Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation) //=>
        {

            return new Dictionary<string, object>()
            {
                { "DATE_FROM",  additionalInformation.PickupDateTime.ToString("dd/MM/yyyy").Replace('.','/')},
                { "TIME_FROM", additionalInformation.PickupDateTime.ToString("HH:mm") },
                { "DATE_TO",  additionalInformation.ReturnDateTime.ToString("dd/MM/yyyy").Replace('.','/')},
                { "TIME_TO",  additionalInformation.ReturnDateTime.ToString("HH:mm") },
                { "PICKUP_STATION",  additionalInformation.APIPickupLocationCode },
                { "RETURN_STATION",  additionalInformation.APIReturnLocationCode },
                { "AGENT",  vendor.ApiPassword.Split('-')[1] },
            };
        }

        private Dictionary<string, object> GetVehicleListRequestParameters(Domain.Models.Vendor vendor) =>
         new Dictionary<string, object>()
         {
                { "AGENT",  vendor.ApiPassword.Split('-')[1] },
         };
    }
}
