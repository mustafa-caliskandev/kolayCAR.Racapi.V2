using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Helpers.Beto;
using KolayCAR.Broker.API.Mappers.Beto;
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
using static KolayCAR.Broker.Domain.Models.Requests.BetoRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.BetoResponseBase;

namespace KolayCAR.Broker.API.Providers.Beto
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
            if (auth != null && !string.IsNullOrEmpty(auth.access_token))
            {
                var result = await RestManager.GetAsync<AvaibilitiyVehicleRequest, List<AvaibilityVehicleResponse>>(
                    requestPath: $"{vendor.APIBaseUrl}api/GetCars/List_All",
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token)
                    );

                if (result != null)
                {
                    return new ServiceResponseBase
                    {
                        Success = result.Count > 0,
                        Data = result.Map()
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
                var result = await RestManager.PostAsyncWithUrlEncoded<AvaibilitiyVehicleRequest, List<AvaibilityVehicleResponse>>(
                requestPath: vendor.APIBaseUrl + $"api/GetCars/List",
                entity: GetAvailabilityRequestBodyEntity3(additionalInformation, vendor, baseVendorRequestCurrencyType),
                headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token));

                if (result != null)
                {
                    var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result, v => v.ARACNO, v => v.TUTAR.ToFloatNullSafe());
                    //var mappedVehicleList = result.Map(additionalInformation, vendor);
                    var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    if (vendor.VehicleMappingActive)
                    {
                        mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                        apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.ARACNO.ToString()));
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
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.ARACNO.ToString()).ToList();
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
                                VehicleCode = vehicle.value.ARACNO.ToString(),
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = vehicle.value.TUTAR.ToFloatNullSafe(),
                                APIDailyPricePayNow = vehicle.value.TUTAR.ToFloatNullSafe(),
                                APIOneWayFee = vehicle.value.DROPUCRET.ToFloatNullSafe(),
                                APIReferenceCode = vehicle.value.ARACNO.ToString(),
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
                                APIReferenceCode2 = vehicle.value.SINIF,
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
                    mappedVehicleList.RemoveAll(x => x.DailyPrice == 0 || x.IsAvailable == false); // Doğukan Selvi mailine binaen x.IsAvailable == false eklendi
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

        private AvaibilitiyVehicleRequest GetAvailabilityRequestBodyEntity(ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, CurrencyTypes baseVendorRequestCurrencyType) =>
     new AvaibilitiyVehicleRequest
     {
         pickup_date = additionalInformation.PickupDateTime.ToString("dd.MM.yyyy"),
         pickup_time = additionalInformation.PickupDateTime.ToString(" HH:mm"),
         dropoff_date = additionalInformation.ReturnDateTime.ToString("dd.MM.yyyy"),
         dropoff_time = additionalInformation.ReturnDateTime.ToString("HH:mm"),
         pickup_location = additionalInformation.APIPickupLocationCode,
         Drop_location = additionalInformation.APIReturnLocationCode,
         currency = CurrencyHelper.GetLongCurrencyType(baseVendorRequestCurrencyType).ToString()

     };

        private AvaibilitiyVehicleRequest GetAvailabilityRequestBodyEntity2(ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, CurrencyTypes baseVendorRequestCurrencyType) =>
     new AvaibilitiyVehicleRequest
     {
         pickup_date = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd"),
         pickup_time = additionalInformation.PickupDateTime.ToString(" HH:mm"),
         dropoff_date = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd"),
         dropoff_time = additionalInformation.ReturnDateTime.ToString("HH:mm"),
         pickup_location = additionalInformation.APIPickupLocationCode,
         Drop_location = additionalInformation.APIReturnLocationCode,
         currency = CurrencyHelper.GetLongCurrencyType(baseVendorRequestCurrencyType).ToString()

     };

        private List<KeyValuePair<string, string>> GetAvailabilityRequestBodyEntity3(ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, CurrencyTypes baseVendorRequestCurrencyType)
        {
            var list = new List<KeyValuePair<string, string>>();
            list.Add(new KeyValuePair<string, string>("pickup_date", additionalInformation.PickupDateTime.ToString("yyyy-MM-dd")));
            list.Add(new KeyValuePair<string, string>("pickup_time", additionalInformation.PickupDateTime.ToString("HH:mm:ss")));
            list.Add(new KeyValuePair<string, string>("pickup_location", additionalInformation.APIPickupLocationCode));
            list.Add(new KeyValuePair<string, string>("dropoff_date", additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd")));
            list.Add(new KeyValuePair<string, string>("dropoff_time", additionalInformation.ReturnDateTime.ToString("HH:mm:ss")));
            list.Add(new KeyValuePair<string, string>("Drop_location", additionalInformation.APIReturnLocationCode));
            //list.Add(new KeyValuePair<string, string>("currency", CurrencyHelper.GetLongCurrencyType(baseVendorRequestCurrencyType).ToString()));
            list.Add(new KeyValuePair<string, string>("currency", CurrencyHelper.GetLongCurrencyType(baseVendorRequestCurrencyType).ToString()));

            return list;
        }

        private CurrencyTypes GetCurrencyType(string currencyCode)
        {
            CurrencyTypes currencyType;
            switch (currencyCode)
            {
                case "TRY":
                    currencyType = CurrencyTypes.TRY; break;
                case "USD":
                    currencyType = CurrencyTypes.USD; break;
                case "EUR":
                    currencyType = CurrencyTypes.EUR; break;
                default:
                    currencyType = CurrencyTypes.EUR; break;
            }
            return currencyType;
        }

    }
}