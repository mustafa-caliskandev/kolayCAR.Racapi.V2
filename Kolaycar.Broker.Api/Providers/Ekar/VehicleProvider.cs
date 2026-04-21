using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Ekar;
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
using AvecHelper = KolayCAR.Broker.API.Helpers.Avec;

namespace KolayCAR.Broker.API.Providers.Ekar
{
    public class VehicleProvider : IVehicleProvider
    {
        HttpManager HttpManager { get; set; }
        bool GetLongVehicleName { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            HttpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var result = await HttpManager.GetXmlAsync<EkarResponseBase>(
            requestPath: "xml_Musait.asp",
            parameters: GetVehiclesRequestParameters(additionalInformation, baseVendorRequestCurrencyType),
            culture: "tr-TR");

            if (result?.EkarSistemrent?.Musaitlik?.Count > 0)
            {
                GetLongVehicleName = true;
                var apiAllVehicleListResponse = await GetVehicleList(vendor);

                if (apiAllVehicleListResponse != null && apiAllVehicleListResponse.Success && apiAllVehicleListResponse.Data != null)
                {
                    var apiAllVehicleList = apiAllVehicleListResponse.Data as List<Vehicle>;
                    var apiVehicleList = result.EkarSistemrent.Musaitlik;
                    var mappedVehicleList = result.EkarSistemrent.Musaitlik.Map(additionalInformation, vendor);
                    var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                    //Ekar entegrasyonuna özel olarak yazılmıştır.
                    mappedVehicleList.ForEach(x =>
                    {
                        var vehicle = apiAllVehicleList.Where(v => v.VehicleCode == x.VehicleCode).FirstOrDefault();

                        x.VehicleName = vehicle.VehicleName;
                        x.FuelType = vehicle.FuelType;
                        x.TransmissionType = vehicle.TransmissionType;
                    });

                    if (vendor.VehicleMappingActive)
                    {
                        mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                        apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.Kayit_No.ToString()));
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
                        var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.Kayit_No.ToString()).ToList();
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
                                VehicleCode = vehicle.value.Kayit_No.ToString(),
                                APIPickupLocationId = additionalInformation.APIPickupLocationId,
                                APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                                APIReturnLocationId = additionalInformation.APIReturnLocationId,
                                APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                                CurrencyType = requestCurrencyType,
                                RentalDuration = mappedVehicle.RentalDuration,
                                DailyPrice = mappedVehicle.DailyPrice,
                                OneWayFee = mappedVehicle.OneWayFee,
                                DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                                APIDailyPrice = vehicle.value.G_Fiyat.ToFloatNullSafe(),
                                APIDailyPricePayNow = vehicle.value.T_Fiyat.ToFloatNullSafe(),
                                APIOneWayFee = vehicle.value.Drop_Mesafe.ToFloatNullSafe(),
                                APIReferenceCode = vehicle.value.Rez_ID.ToString(),
                                APIReferenceCode2 = vehicle.value.Kayit_No.ToString(),
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
                Message = "Ekar servisinden araç bulunamadı!"
            };
        }

        private Dictionary<string, object> GetVehiclesRequestParameters(ResponseReservationStepsAdditionalInformation additionalInformation, CurrencyTypes baseVendorRequestCurrencyType)
        {
            return new Dictionary<string, object>()
            {
                { "Sube_Kodu", additionalInformation.APIPickupLocationCode },
                { "Bas_Gun", StringHelper.GetTwoCharacterDateItem(additionalInformation.PickupDateTime.Day) },
                { "Bas_Ay", StringHelper.GetTwoCharacterDateItem(additionalInformation.PickupDateTime.Month) },
                { "Bas_Yil", StringHelper.GetTwoCharacterDateItem(additionalInformation.PickupDateTime.Year) },
                { "Bas_Saat",StringHelper.GetTwoCharacterDateItem(additionalInformation.PickupDateTime.Hour) },
                { "Bas_Dakika",StringHelper.GetTwoCharacterDateItem(additionalInformation.PickupDateTime.Minute) },
                { "Donus_Sube_Kodu",  additionalInformation.APIReturnLocationCode },
                { "Bit_Gun", StringHelper.GetTwoCharacterDateItem(additionalInformation.ReturnDateTime.Day) },
                { "Bit_Ay", StringHelper.GetTwoCharacterDateItem(additionalInformation.ReturnDateTime.Month) },
                { "Bit_Yil", StringHelper.GetTwoCharacterDateItem(additionalInformation.ReturnDateTime.Year) },
                { "Bit_Saat", StringHelper.GetTwoCharacterDateItem(additionalInformation.ReturnDateTime.Hour) },
                { "Bit_Dakika", StringHelper.GetTwoCharacterDateItem(additionalInformation.ReturnDateTime.Minute) },
                { "Currency", AvecHelper.CurrencyHelper.GetLongCurrencyType(baseVendorRequestCurrencyType) },
                { "User_Name",  additionalInformation.Vendor.ApiKey },
                { "User_Pass",  additionalInformation.Vendor.ApiPassword },
                { "Key_Hack",  additionalInformation.Vendor.ApiClientId }
            };
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var result = await HttpManager.GetXmlAsync<EkarResponseBase>(
            requestPath: "xml_Arac.asp",
            parameters: GetVehicleListRequestParameters(vendor),
            culture: "tr-TR");

            if (result != null && result.EkarSistemrent.Arac != null && result.EkarSistemrent.Arac.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.EkarSistemrent.Arac.Map(getLongVehicleName: GetLongVehicleName)
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Ekar servisine ulaşılamadı!"
            };
        }

        private Dictionary<string, object> GetVehicleListRequestParameters(Vendor vendor)
        {
            return new Dictionary<string, object>()
            {
                { "Key_Hack", vendor.ApiClientId },
            };
        }
    }
}
