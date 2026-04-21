using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Turmobil;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.TurmobilResponseBase;

namespace KolayCAR.Broker.API.Providers.Turmobil
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var result = await RestManager.PostAsync<object, vehicleTurmobil>(
                requestPath: "rest/dailyrezervation/vehicleTypeList",
                headers: new Dictionary<string, object>
                {
                        {"username" , vendor.ApiKey},
                        {"password" , vendor.ApiPassword},
                        {"Accept" , "*/*"}
                });

            if (result != null && result.data.Count > 0)
                return new ServiceResponseBase(result.data.Map(vendor), result != null);

            return new ServiceResponseBase(null, false, "Kimlik doğrulama işlemi başarısız!");
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var result = await RestManager.PostAsync<object, LocationVehiclesTurmobil>(
                requestPath: "rest/dailyrezervation/vehicleList",
                entity: GetVehiclesRequestParameters(additionalInformation, baseVendorRequestCurrencyType),
                headers: new Dictionary<string, object>
                {
                        {"username" , vendor.ApiKey},
                        {"password" , vendor.ApiPassword}
                });
            if (result?.data?.Count > 0)
            {
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.data, v => v.vehicleTypeId.ToString(), v => v.totalAmount);
                var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.vehicleTypeId.ToString()));
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
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.vehicleTypeId.ToString()).ToList();
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
                            VehicleCode = vehicle.value.vehicleTypeId.ToString(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId ?? null,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId ?? null,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            //APIDailyPrice = vehicle.value.dailyAmountLater.ToFloatNullSafe(),
                            APIDailyPrice = vehicle.value.hireDay != 0 ? vehicle.value.totalAmount / vehicle.value.hireDay : vehicle.value.dailyAmount,
                            APIDailyPricePayNow = vehicle.value.hireDay != 0 ? vehicle.value.totalAmount / vehicle.value.hireDay : vehicle.value.dailyAmount,
                            APIOneWayFee = vehicle.value.dropAmount.ToFloatNullSafe(),
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
                            BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
                            VehicleImageUrl = mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                            APIReferenceCode = vehicle.value.totalAmount.ToStringNullSafe(),
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
                return new ServiceResponseBase(mappedVehicleList, mappedVehicleList.Count > 0);
            }
            return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisinden araç bulunamadı!");
        }

        private Dictionary<string, object> GetVehiclesRequestParameters(ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrencyType)
        {
            return new Dictionary<string, object>()
            {
                { "deliveryDate", additionalInformation.PickupDateTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) },
                { "returnDate",  additionalInformation.ReturnDateTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) },
                { "deliveryLocationId",  additionalInformation.APIPickupLocationCode },
                { "returnLocationId",  additionalInformation.APIReturnLocationCode },
                { "curr", baseVendorRequestCurrencyType.ToString()}
            };
        }
    }
}
