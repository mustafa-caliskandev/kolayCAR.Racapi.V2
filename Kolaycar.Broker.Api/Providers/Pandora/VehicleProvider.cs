using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Pandora;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Pandora
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
            var auth = await AuthProvider.GetAccessToken(vendor);

            if (auth.access_token != null)
            {
                var result = await RestManager.GetAsync<List<PandoraResponseBase.CarCategories>>(
                    requestPath: "tr/api/carCategories",
                    headers: AuthProvider.CreateAuthHeader(auth.access_token));

                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Pandora servisine ulaşılamadı!"
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var auth = await AuthProvider.GetAccessToken(vendor);

            if (auth != null && !string.IsNullOrEmpty(auth?.access_token))
            {
                var result = await RestManager.PostAsyncRestClient<List<PandoraResponseBase.AvailableVehicle>>(
                    requestPath: $"tr/api/bookings/availability",
                    parameterType: RestSharp.ParameterType.RequestBody,
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token),
                    entity: CreateAvailabilityForm(additionalInformation, baseVendorRequestCurrencyType));

                if (result?.Count > 0)
                {
                    var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result, v => v.CarCategoryId, v => v.Amount.ToFloatNullSafe());
                    //var mappedVehicleList = result.Map(additionalInformation, vendor);
                    var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    if (vendor.VehicleMappingActive)
                    {
                        mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                        apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.CarCategoryId.ToString()));
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
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.CarCategoryId.ToString()).ToList();
                        for (int i = 0; i < tempMappedVehicleList.Count; i++)
                        {
                            var mappedVehicle = tempMappedVehicleList[i];
                            float apiDailyPrice = vehicle.value.Amount.ToFloatNullSafe() / mappedVehicle.RentalDuration;

                            var reservationToken = new ReservationToken
                            {
                                AgencyId = additionalInformation.Agency.AgencyId,
                                VendorId = vendor.VendorId,
                                APIVendorId = vendor.VendorId,
                                APIVendorName = vendor.VendorName,
                                APIVendorPhone = vendor.VendorPhone,
                                APIVendorEmail = vendor.VendorEmail,
                                APIVendorLogo = vendor.Logo,
                                VehicleId = vehicle.value.CarCategoryId,
                                VehicleCode = vehicle.value.CarCategoryId.ToString(),
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = apiDailyPrice,
                                APIDailyPricePayNow = vehicle.value.Amount.ToFloatNullSafe(),
                                APIOneWayFee = vehicle.value.IncludedServices?.FirstOrDefault(x => x.ServiceTypeId == 10)?.AmountTotal.ToFloatNullSafe() ?? 0f,
                                APIReferenceCode = vehicle.value.PricelistId.ToString(),
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
                Message = "Kimlik doğrulama işlemi başarısız!",
            };
        }

        public string GetAvailabilityRequestBodyEntity(ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrencyType) =>
            JsonConvert.SerializeObject(new PostAvailabilityRequets
            {
                BookAsCommissioner = false,
                OfficeOutId = Convert.ToInt32(additionalInformation.APIPickupLocationCode),
                OfficeInId = Convert.ToInt32(additionalInformation.APIReturnLocationCode),
                DateOut = additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                DateIn = additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                Currency = baseVendorRequestCurrencyType.ToString()
            });

        public Dictionary<string, object> CreateAvailabilityForm(ResponseReservationStepsAdditionalInformation additionalınformation, CurrencyTypes baseVendorRequestCurrencyType) =>
            new Dictionary<string, object>()
            {
                { "application/json", GetAvailabilityRequestBodyEntity(additionalınformation, baseVendorRequestCurrencyType) },
            };
    }

    public class PostAvailabilityRequets
    {
        public bool BookAsCommissioner { get; set; }
        public int OfficeOutId { get; set; }
        public int OfficeInId { get; set; }
        public string DateOut { get; set; }
        public string DateIn { get; set; }
        public string Currency { get; set; }
    }
}
