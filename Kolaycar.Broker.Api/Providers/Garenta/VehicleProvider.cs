using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Helpers.Garenta;
using KolayCAR.Broker.API.Mappers.Garenta;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Garenta
{
    public class VehicleProvider : IVehicleProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            RestManager = new RestManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            AuthProvider = new AuthProvider();
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var dataTable = ExcelHelper.ReadExcel($@"Docs\Garenta\{vendor.VendorName}\VehicleList.xlsx");

            var result = new List<GarentaResponseBase.STATIC_VEHICLE>();

            if (dataTable.Rows.Count > 0)
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    result.Add(new GarentaResponseBase.STATIC_VEHICLE
                    {
                        CarGroup = dataTable.Rows[i]["Car Group"].ToStringNullSafe(),
                        NewSipp = dataTable.Rows[i]["New Sipp"].ToStringNullSafe(),
                        Brand = dataTable.Rows[i]["Brand"].ToStringNullSafe(),
                        Model = dataTable.Rows[i]["Model"].ToStringNullSafe(),
                        Type = dataTable.Rows[i]["Type"].ToStringNullSafe(),
                        Fuel = dataTable.Rows[i]["Fuel"].ToStringNullSafe(),
                        Transmission = dataTable.Rows[i]["Transmission"].ToStringNullSafe(),
                        Doors = dataTable.Rows[i]["Doors"].ToStringNullSafe(),
                        Seats = dataTable.Rows[i]["Seats"].ToStringNullSafe(),
                        Deposit = dataTable.Rows[i]["Deposit"].ToStringNullSafe(),
                        MinDriverAge = dataTable.Rows[i]["Age Restriction Min. Age"].ToStringNullSafe(),
                        YoungDriverAge = dataTable.Rows[i]["Young Driver Age"].ToStringNullSafe(),
                        MinDriverLicenseAge = dataTable.Rows[i]["Driver License Experience"].ToStringNullSafe(),
                        DailyKmLimit = dataTable.Rows[i]["Günlük KM Limiti"].ToStringNullSafe(),
                        TotalKmLimit = dataTable.Rows[i]["Aylık KM Limiti"].ToStringNullSafe()
                    });
                }

                result.RemoveAll(x => string.IsNullOrEmpty(x.NewSipp));
            }

            return new ServiceResponseBase
            {
                Success = true,
                Data = result.Map().OrderBy(x => x.VehicleName).ToList()
            };
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var result = await RestManager.PostAsync<GarentaRequestBase, GarentaResponseBase>(
                 requestPath: string.Empty,
                 entity: GetVehiclesRequestBody(getVehiclesRequest, additionalInformation, vendor),
                 headers: AuthProvider.CreateAuthHeaderWithContentType(vendor));

            if (result?.EXPORT?.ES_OUTPUT?.RETURN?.Count() > 0)
            {
                var apiVehicleList = result.EXPORT.ES_OUTPUT.RETURN;
                var apiCampaingVehicleList = result.EXPORT.ES_OUTPUT.RETURN.Where(x => x.IS_CAMPAIGN == "X").ToList();

                var mappedVehicleList = result.EXPORT.ES_OUTPUT.RETURN.Where(x => x.IS_CAMPAIGN != "X").ToList().Map(additionalInformation, result.EXPORT.ES_OUTPUT.ADD_PROD, vendor);
                var mappedCampaignVehicleList = apiCampaingVehicleList.Map(additionalInformation, result.EXPORT.ES_OUTPUT.ADD_PROD, vendor);

                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.SIPP_CODE.ToString()));

                    mappedCampaignVehicleList = VehicleHelper.MapLocalVehicleList(mappedCampaignVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit);
                    apiCampaingVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.SIPP_CODE.ToString()));
                }

                CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);
                vendor.ProfitMarkupDailyPrice = vendor.SecretKey.Replace(',', '.').ToFloatNullSafe();

                CalculationHelper.SetVehiclesPrices(mappedCampaignVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                VehicleHelper.SetVehiclesProperties(mappedCampaignVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);

                if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                    return new ServiceResponseBase(null, false, "Yanlış gün sayısı");
                if (mappedCampaignVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                    return new ServiceResponseBase(null, false, "Yanlış gün sayısı");
                mappedVehicleList.AddRange(mappedCampaignVehicleList);

                var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                {
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.SIPP_CODE.ToString()).ToList();
                    for (int i = 0; i < tempMappedVehicleList.Count; i++)
                    {
                        var mappedVehicle = tempMappedVehicleList[i];
                        var dailyPrice = (vehicle.value.NET_AMOUNT / mappedVehicle.RentalDuration).ToFloatNullSafe();

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
                            VehicleCode = vehicle.value.SIPP_CODE.ToString(),
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee.ToFloatNullSafe(),
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = dailyPrice,
                            APIDailyPricePayNow = dailyPrice,
                            APIOneWayFee = mappedVehicle.OneWayFee.ToFloatNullSafe(),
                            APIReferenceCode = vehicle.value.SIPP_CODE,
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
                            APIReferenceCode2 = vehicle.value.IS_CAMPAIGN,
                            SpecialProfitApplied = mappedVehicle.SpecialProfitApplied,
                            BaggageQuantityType = mappedVehicle.BaggageQuantityType,
                            PassangerQuantityType = mappedVehicle.PassangerQuantityType,
                            TotalKmLimit = mappedVehicle.TotalKMLimit ?? 0,
                            VehicleCategoryType = mappedVehicle.VehicleCategoryType,
                            VehicleType = mappedVehicle.VehicleType,
                            VendorFlightPassRequired = vendor.FlightNumberRequired ?? false,
                            FullCredit = mappedVehicle.FullCredit,
                            SippCode = mappedVehicle.SippCode,
                            APIReferenceCode3 = vehicle.value.SEARCH_REFERENCE
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
                Data = null
            };
        }

        private GarentaRequestBase GetVehiclesRequestBody(GetVehiclesRequest getVehiclesRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
            new GarentaRequestBase
            {
                sap_props = RequestHelper.GetSapProps(GarentaRequestBase.ServiceTypes.SEARCH_AVAILABILITY, vendor.ApiKey, vendor.ApiPassword),
                import = new GarentaRequestBase.Import
                {
                    IS_INPUT = new GarentaRequestBase.ISINPUT_SEARCH
                    {
                        SEARCH = new GarentaRequestBase.SEARCH
                        {
                            PICKUP_DATE = getVehiclesRequest.PickupDate,
                            PICKUP_TIME = getVehiclesRequest.PickupTime + ":00",
                            DROPOFF_DATE = getVehiclesRequest.ReturnDate,
                            DROPOFF_TIME = getVehiclesRequest.ReturnTime + ":00",
                            PICKUP_OFFICE = additionalInformation.APIPickupLocationCode,
                            RETURN_OFFICE = additionalInformation.APIReturnLocationCode
                        },
                        BROKER_CODE = vendor.ApiKey,
                        LANGU = RequestHelper.GetLanguageType(getVehiclesRequest.LanguageCode.ToEnum<LanguageTypes>())
                    }
                }
            };
    }
}
