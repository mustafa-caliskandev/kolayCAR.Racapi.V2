using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Enuygun;
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

namespace KolayCAR.Broker.API.Providers.Enuygun
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        ICacheService _cacheService { get; set; }
        public VehicleProvider(Vendor vendor, bool disableTimeout, ICacheService cacheService)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            AuthProvider = new AuthProvider(vendor.APIBaseUrl);
            _cacheService = cacheService;
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            return new ServiceResponseBase
            {
                Success = false,
                Message = $"{vendor.VendorName} servisine ulaşılamadı."
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var auth = await _cacheService.GetOrCreateAsync($"EnUygun{vendor.VendorName}Token", () => AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword), TimeSpan.FromMinutes(30));

            if (auth?.Status != "OK")
                return new(null, false, "Enuygun servisinden token alınamadı!");

            var result = await RestManager.PostAsync<EnuygunRequest.Search.Vehicles, EnuygunResponse.Search.Root>(
                requestPath: "/api/v1/search",
                entity: getSearchRequest(getVehiclesRequest, additionalInformation, baseVendorRequestCurrencyType, vendor),
                headers: AuthProvider.CreateHeaderWithToken(auth.Data.Token)
                );

            if (result?.status != "OK")
                return new(null, false, "Enuygun servisinden araçlar alınamadı!");

            if (result?.data?.reservations?.Count > 0)
            {
                var requestId = result.data.requestId;
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.data.reservations, v => $"{v.company.name}-{v.vehicle.matchCode}",
                    GetReservationGroupPrice);

                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();
                var mappedVehicleList = apiVehicleList.Map(additionalInformation, requestCurrencyType, vendor, requestId, exchangeRates);

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => p?.vehicle == null || !localVehicles.Any(e => e.VehicleCode == p.vehicle.matchCode));
                }

                CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);
                if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                    return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

                var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                foreach (var vehicle in apiVehicleList)
                {
                    var reservationBreakdown = vehicle.breakdowns?.FirstOrDefault(e => e?.type == "reservation");
                    var dropBreakdown = vehicle.breakdowns?.FirstOrDefault(e => e?.type == "drop");
                    var apiDailyPrice = Math.Round(reservationBreakdown.totalPrice / vehicle.days, 2).ToFloatNullSafe();

                    var mappedVehicle = mappedVehicleList.FirstOrDefault(e => e.VehicleCode == vehicle.vehicle.matchCode && e.ApiVendorName == vehicle.company.name);

                    if (mappedVehicle == null)
                        continue;

                    var adress = $"{vehicle.company.name} | pickUpOffice ~ {vehicle.pickUpOffice?.address} - {vehicle.pickUpOffice?.phoneNumber} | dropOffice ~ {vehicle.dropOffOffice?.address} - {vehicle.dropOffOffice?.phoneNumber}";

                    var reservationToken = new ReservationToken
                    {
                        AgencyId = additionalInformation.Agency.AgencyId,
                        VendorId = vendor.VendorId,
                        APIVendorId = additionalInformation.Vendor.VendorId,
                        APIVendorName = vehicle.company?.name ?? "",
                        APIVendorEmail = vendor.VendorEmail,
                        APIVendorLogo = vehicle.company?.logoUri ?? string.Empty,
                        APIVendorPhone = vendor.VendorPhone,
                        VehicleId = mappedVehicle.VehicleId,
                        VehicleCode = vehicle.vehicle.matchCode,
                        APIPickupLocationId = additionalInformation.APIPickupLocationId,
                        APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                        APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                        APIReturnLocationId = additionalInformation.APIReturnLocationId,
                        CurrencyType = requestCurrencyType,
                        RentalDuration = mappedVehicle.RentalDuration,
                        DailyPrice = mappedVehicle.DailyPrice,
                        OneWayFee = mappedVehicle.OneWayFee,
                        DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                        APIDailyPrice = Math.Round(apiDailyPrice, 2).ToFloatNullSafe(),
                        APIDailyPricePayNow = Math.Round(vehicle.price?.dailyPrice > 0 ? vehicle.price.dailyPrice : apiDailyPrice, 2).ToFloatNullSafe(),
                        APITotalPrice = vehicle.price.totalPrice,
                        APITotalPricePayNow = vehicle.price.totalPrice,
                        APIReferenceCode = requestId,
                        APIReferenceCode2 = vehicle.referenceId,
                        APIOneWayFee = dropBreakdown?.totalPrice.ToFloatNullSafe() ?? 0,
                        DepositPrice = mappedVehicle.DepositPrice,
                        VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                        VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                        ServiceCharge = mappedVehicle.ServiceCharge,
                        FuelType = mappedVehicle.FuelType,
                        TransmissionType = mappedVehicle.TransmissionType,
                        DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                        PickupLocationId = mappedVehicle.PickupLocationId,
                        ReturnLocationId = mappedVehicle.ReturnLocationId,
                        LanguageType = languageType,
                        PickupDateTime = pickupDateTime,
                        ReturnDateTime = returnDateTime,
                        VehicleName = mappedVehicle.VehicleName,
                        VehicleImageUrl = mappedVehicle.VehicleImages?.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                        BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
                        APIFullCredit = mappedVehicle.FullCredit.ToBoolNullSafe(),
                        ApiVendorType = VendorTypes.EnUygun,
                        APIReferenceCode3 = adress, //search kısmında dönen pickup ve drop office bilgileri içermektedir.
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
                return new(mappedVehicleList, true);
            }
            return new(null, false, "Maalesef, girdiğiniz bilgilere uygun bir araç bulunamadı.");
        }

        private EnuygunRequest.Search.Vehicles getSearchRequest(GetVehiclesRequest getVehiclesRequest, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrencyType, Vendor vendor)
        {
            DateTime pickupDate = DateTime.Parse(getVehiclesRequest.PickupDate);
            DateTime returnDate = DateTime.Parse(getVehiclesRequest.ReturnDate);

            var requestSearch = new EnuygunRequest.Search.Vehicles
            {
                pickUpDate = pickupDate.Date.ToString("yyyy-MM-dd"),
                pickUpTime = getVehiclesRequest.PickupTime,
                dropOffDate = returnDate.Date.ToString("yyyy-MM-dd"),
                dropOffTime = getVehiclesRequest.ReturnTime,
                pickUpLocation = additionalInformation.APIPickupLocationCode,
                dropOffLocation = additionalInformation.APIReturnLocationCode,
                currency = baseVendorRequestCurrencyType.ToString(),
                brokerParameters = new EnuygunRequest.Search.BrokerParameters
                {
                    ratio = vendor.ProfitMarkupDailyPrice.ToIntNullSafe(),
                    contractType = vendor.ApiClientId
                }
            };
            return requestSearch;
        }

        private static float GetReservationGroupPrice(EnuygunResponse.Search.Reservation reservation)
        {
            var reservationBreakdown = reservation?.breakdowns?.FirstOrDefault(e => e?.type == "reservation");
            var totalPrice = reservation.price.totalPrice;
            return totalPrice > 0 ? totalPrice : float.MaxValue;
        }
    }
}
