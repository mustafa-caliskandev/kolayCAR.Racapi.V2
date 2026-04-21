using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Helpers.Renticar;
using KolayCAR.Broker.API.Mappers.Renticar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Renticar.Request;
using KolayCAR.Broker.Domain.Models.Renticar.Response;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KolayCAR.Broker.API.Providers.Renticar
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        public VehicleProvider(Vendor vendor, IMemoryCache memoryCache, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            AuthProvider = new AuthProvider(vendor.APIBaseUrl, memoryCache);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var auth = await AuthProvider.GetToken(vendor);

            if (auth != null && auth.status == "success")
            {
                var result = await RestManager.GetAsync<List<CarsResponseBase>>(
                requestPath: "cars",
                headers: AuthProvider.CreateHeader(auth.token));

                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result?.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Renticar kullanıcı girişi yapılamadı!"
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var auth = await AuthProvider.GetToken(vendor, getVehiclesRequest.IsReservationRequest);

            if (auth != null && auth.status == "success")
            {
                var result = await RestManager.PostAsync<SearchRequestBody, SearchResponseBase>(
                    requestPath: "search",
                    entity: CreateBody(getVehiclesRequest, additionalInformation),
                    headers: AuthProvider.CreateHeader(auth.token));

                if (!getVehiclesRequest.IsReservationRequest && result != null && result.status == "error")
                {
                    getVehiclesRequest.IsReservationRequest = true;
                    await GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, baseVendorRequestCurrencyType);
                }

                if (result != null && result.offers != null && result.offers.Count > 0)
                {
                    var apiVehicleList = result.offers;
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
                    var mappedVehicleList = result.offers.Map(additionalInformation, requestCurrencyType, vendor);

                    if (vendor.VehicleMappingActive)
                    {
                        mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                        apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.car.carId));
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
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.car.carId.ToString() && x.ApiVendorName == vehicle.value.companySlug).ToList();
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
                                VehicleCode = vehicle.value.car.carId.ToStringNullSafe(),
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = RenticarHelper.GetPrice(vehicle.value, requestCurrencyType, 1),
                                APIDailyPricePayNow = RenticarHelper.GetPrice(vehicle.value, requestCurrencyType, 1),
                                APIOneWayFee = RenticarHelper.GetPrice(vehicle.value, requestCurrencyType, 3),
                                APIReferenceCode = vehicle.value.offerId.ToStringNullSafe(),
                                APIReferenceCode2 = mappedVehicle.ApiVendorName,
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
                    mappedVehicleList.RemoveAll(x => x.FullCredit == false);
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
                Message = "Renticar kullanıcı girişi yapılamadı!"
            };
        }

        private static SearchRequestBody CreateBody(GetVehiclesRequest getVehiclesRequest, ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            //"yyyy-MM-ddTHH:mm:ssZ"
            return new SearchRequestBody
            {
                pickup_datetime = DateTime.ParseExact(getVehiclesRequest.PickupDate + " " + getVehiclesRequest.PickupTime, "dd.MM.yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                return_datetime = DateTime.ParseExact(getVehiclesRequest.ReturnDate + " " + getVehiclesRequest.ReturnTime, "dd.MM.yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                pickup_location = additionalInformation.APIPickupLocationCode,
                return_location = additionalInformation.APIReturnLocationCode
            };
        }
    }
}
