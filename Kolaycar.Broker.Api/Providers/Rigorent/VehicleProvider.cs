using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Rigorent;
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

namespace KolayCAR.Broker.API.Providers.Rigorent
{
    public class VehicleProvider : IVehicleProvider
    {
        HttpManager HttpManager { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            HttpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var result = await HttpManager.GetXmlAsync<RigorentResponseBase>(
            requestPath: "xml/xml_rez.Asp",
            parameters: GetVehiclesRequestParameters(additionalInformation),
            culture: "tr-TR");

            if (result != null && result.Mycarturassist != null)
            {
                var apiVehicleList = VehicleHelper.SelectCheapestByGroup(result.Mycarturassist.Arac, v => v.Araclar_Kayit_No, v => v.Gunluk_Kira.ToFloatNullSafe());
                //var mappedVehicleList = result.Mycarturassist.Arac.Map(additionalInformation, vendor);
                var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.Arac_Kayit_No.ToString()));
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
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.Arac_Kayit_No.ToString()).ToList();
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
                            VehicleId = vehicle.value.Arac_Kayit_No.ToIntNullSafe(),
                            VehicleCode = vehicle.value.Arac_Kayit_No.ToStringNullSafe(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = vehicle.value.Gunluk_Kira.ToFloatNullSafe(),
                            APIDailyPricePayNow = vehicle.value.Gunluk_Kira.ToFloatNullSafe(),
                            APIOneWayFee = vehicle.value.Drop_Bedeli.ToFloatNullSafe(),
                            APIReferenceCode = vehicle.value.Rez_ID.ToString(),
                            APIReferenceCode2 = vehicle.value.Gunluk_Kur.ToString(),
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

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Rigorent servisinden araç bulunamadı!"
            };

        }

        private Dictionary<string, object> GetVehiclesRequestParameters(ResponseReservationStepsAdditionalInformation additionalInformation)
        {
            return new Dictionary<string, object>()
            {
                { "Cikis", additionalInformation.APIPickupLocationCode },
                { "Al_Gun",  additionalInformation.PickupDateTime.Day },
                { "Al_Ay",  additionalInformation.PickupDateTime.Month },
                { "Al_Yil",  additionalInformation.PickupDateTime.Year },
                { "Al_Saat",  additionalInformation.PickupDateTime.Hour },
                { "Al_Dakika",  additionalInformation.PickupDateTime.Minute },
                { "Donus",  additionalInformation.APIReturnLocationCode },
                { "Iade_Gun",  additionalInformation.ReturnDateTime.Day },
                { "Iade_Ay",  additionalInformation.ReturnDateTime.Month },
                { "Iade_Yil",  additionalInformation.ReturnDateTime.Year },
                { "Iade_Saat",  additionalInformation.ReturnDateTime.Hour },
                { "Iade_Dakika",  additionalInformation.ReturnDateTime.Minute },
                { "User_Name",  additionalInformation.Vendor.ApiKey },
                { "User_Pass",  additionalInformation.Vendor.ApiPassword }
            };
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var result = await HttpManager.GetXmlAsync<RigorentResponseBase>(
            requestPath: "xml/xml_arac.asp",
            culture: "tr-TR");

            if (result != null && result.MyCarRent != null && result.MyCarRent.Arac != null && result.MyCarRent.Arac.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.MyCarRent.Arac.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Rigorent servisine ulaşılamadı!"
            };
        }
    }
}
