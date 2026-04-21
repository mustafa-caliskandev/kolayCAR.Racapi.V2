using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Renteon;
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

namespace KolayCAR.Broker.API.Providers.Renteon
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager _restManager { get; set; }
        AuthProvider _authProvider { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _restManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            _authProvider = new AuthProvider();
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var vendorName = vendor.ApiClientId.Split("-")[1]; //connectorId-vendorName-countryCode
            var result = await _restManager.GetAsync<RenteonResponseBase.RenteonLocationResponseBase>(
                requestPath: $"/api/setup/provider/{vendorName}",
                headers: _authProvider.GetBasicAuth(vendor)
            );

            if (result != null)
            {
                var vehicles = result.CarCategories.Map(vendor);
                return new ServiceResponseBase { Data = vehicles, Success = true };

            }
            return new ServiceResponseBase { Data = null, Success = false, Message = $"{vendor.VendorName} servisine ulaşılamadı!" };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var auth = _authProvider.GetBasicAuth(vendor);

            var result = await _restManager.PostAsync<RenteonRequestBase.RenteonVehicleRequestBase, List<RenteonResponseBase.RenteonVehicleResponseBase>>(
                requestPath: $"api/bookings/availability",
                headers: _authProvider.GetBasicAuth(vendor),
                entity: CreateVehiclesRequest(vendor, additionalInformation, baseVendorRequestCurrencyType)
            );

            int durationInDays = (DateTime.Parse(getVehiclesRequest.ReturnDate) - DateTime.Parse(getVehiclesRequest.PickupDate)).Days;

            if (result != null && result.Count > 0)
            {
                var apiVehicleList = result;
                var mappedVehicleList = result.Map(additionalInformation, durationInDays, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.CarCategory.ToString()));
                }

                CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);


                foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                {
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.CarCategory).ToList();
                    for (int i = 0; i < tempMappedVehicleList.Count; i++)
                    {
                        var mappedVehicle = tempMappedVehicleList[i];
                        float apiDailyPrice = vehicle.value.Amount.ToFloatNullSafe() / mappedVehicle.RentalDuration;

                        var reservationToken = new ReservationToken
                        {
                            AgencyId = additionalInformation.Agency.AgencyId,
                            VendorId = vendor.VendorId,
                            //APIVendorId = vendor.VendorId,
                            APIVendorName = vendor.VendorName,
                            APIVendorPhone = vendor.VendorPhone,
                            APIVendorEmail = vendor.VendorEmail,
                            APIVendorLogo = vendor.Logo,
                            VehicleCode = vehicle.value.CarCategory,
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
                            //APIOneWayFee = vehicle.value.IncludedServices.Count > 0 ? vehicle.value.IncludedServices.Where(x => x.ServiceTypeId == 10).FirstOrDefault().AmountTotal.ToFloatNullSafe() : 0,
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
                            LanguageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>(),
                            PickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime),
                            ReturnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime),
                            VehicleName = mappedVehicle.VehicleName,
                            VehicleImageUrl = mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                            BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
                            APIReferenceCode2 = string.Join("|", vehicle.value.AvailableServices.Select(x => $"{x.Code}~{x.ServiceId}~{x.VatAmount}").ToList()),
                            ValueAddedTax = vehicle.value.VatAmount.ToFloatNullSafe(),
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
                    Data = mappedVehicleList,
                    Success = mappedVehicleList.Count > 0
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Renteon servisi ile bağlantı kurulamadı!",
            };
        }

        public RenteonRequestBase.RenteonVehicleRequestBase CreateVehiclesRequest(Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrencyType)
        {
            var vendorName = vendor.ApiClientId.Split("-")[1];
            var request = new RenteonRequestBase.RenteonVehicleRequestBase
            {
                Prepaid = true,
                IncludeOnRequest = false,
                Providers = new List<RenteonRequestBase.Provider>
                    {
                        new RenteonRequestBase.Provider
                        {
                            Code = vendorName,
                            DropOffOfficeIds = new List<int>(),
                            PickupOfficeIds = new List<int>(),
                            PricelistCodes = new List<string>(),
                        },
                    },
                CarCategories = new List<string>(),
                PickupLocation = additionalInformation.APIPickupLocationCode,
                DropOffLocation = additionalInformation.APIReturnLocationCode,
                PickupDate = additionalInformation.PickupDateTime.ToString("O"),
                DropOffDate = additionalInformation.ReturnDateTime.ToString("O"),
                Currency = baseVendorRequestCurrencyType.ToString(),
                HasDelivery = false,
                HasCollection = false,
            };

            if (additionalInformation.APIPickupLocationId != null)
                request.Providers[0].PickupOfficeIds.Add(additionalInformation.APIPickupLocationId.Value);
            if (additionalInformation.APIReturnLocationId != null)
                request.Providers[0].DropOffOfficeIds.Add(additionalInformation.APIReturnLocationId.Value);

            return request;
        }
    }
}
