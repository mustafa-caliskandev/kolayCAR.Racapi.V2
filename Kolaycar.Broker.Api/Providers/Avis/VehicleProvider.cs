using KolayCAR.Broker.API.Factories.Concrete;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Helpers.Avis;
using KolayCAR.Broker.API.Mappers.Avis;
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
using static KolayCAR.Broker.Domain.Models.Requests.AvisRequestBase;
using static KolayCAR.Broker.Domain.Models.Response.AvisResponseBase;

namespace KolayCAR.Broker.API.Providers.Avis
{
    public class VehicleProvider : IVehicleProvider
    {
        AuthProvider _authProvider;
        HttpManager _httpManager;
        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            _httpManager = new HttpManager(vendor.APIBaseUrl, timeout: disableTimeout ? 0 : vendor.APITimeout);
            _authProvider = new AuthProvider(vendor.APIBaseUrl);
        }
        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {
            var token = await _authProvider.GetTokenAsync(vendor);
            var entity = new AvisVehicleListRequest { Brand = vendor.VendorName, CountryCode = "TR" };
            var result = await _httpManager.PostAsync2<AvisVehicleListRequest, AvisVehicleListResponse>
                (
                requestPath: "/STVehicleApp/GetVehicleGroupList",
                headers: QueryHelper<AvisVehicleListRequest>.GetHeaders(token, entity, vendor.SecretKey),
                entity: entity
                );

            if (ObjectValidationHelper.CheckResponseObject(result))
                return new ServiceResponseBase(result.Data.Data.Map(vendor), true);
            return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisine ulaşılamadı!");
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Domain.Models.Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {
            var token = await _authProvider.GetTokenAsync(vendor);

            if (token == null)
                return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisinden token bilgisi alınamadı!");

            var entity = GetAvisAvailableVehicleRequestEntity(getVehiclesRequest, additionalInformation, vendor);
            var result = await _httpManager.PostAsync2<AvisAvailableVehicleRequest, AvisAvailableVehicleResponse>
                (
                requestPath: "/STReservationApp/GetPrices",
                entity: entity,
                headers: QueryHelper<AvisAvailableVehicleRequest>.GetHeaders(token, entity, vendor.SecretKey)
                );
            if (ObjectValidationHelper.CheckResponseObject(result))
            {
                string transactionId = result.Data.Data.transaction.transaction_id;
                var apiVehicleList = result.Data.Data.vehicles.Where(x => x.rate_totals.pay_now.reservation_total != 0).ToList();
                var mappedVehicleList = result.Data.Data.vehicles.Where(x => x.rate_totals.pay_now.reservation_total != 0).ToList().Map(vendor, additionalInformation);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, vendor.CurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == p.category.vehicle_class_code));
                }

                if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                    return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

                CalculationHelper.SetVehiclesPrices(mappedVehicleList, vendor, exchangeRates, requestCurrencyType, baseVendorRequestCurrencyType);
                VehicleHelper.SetVehiclesProperties(mappedVehicleList, vendor, additionalInformation.Agency, exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, profitMarkups);

                var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                {
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == vehicle.value.category.vehicle_class_code.ToString()).ToList();

                    for (int i = 0; i < tempMappedVehicleList.Count; i++)
                    {
                        var mappedVehicle = tempMappedVehicleList[i];

                        float apiDailyPrice, apiTotalPrice;
                        if (vendor.VendorName == "Budget")
                        {
                            apiDailyPrice = (float)(vehicle.value.rate_totals.pay_now.original_vehicle_total / vehicle.value.rate_totals.rate.rentdaycount);
                            apiTotalPrice = (float)vehicle.value.rate_totals.pay_now.original_reservation_total;
                        }
                        else
                        {
                            apiDailyPrice = (float)vehicle.value.rate_totals.pay_now.vehicle_total / vehicle.value.rate_totals.rate.rentdaycount;
                            apiTotalPrice = (float)vehicle.value.rate_totals.pay_now.reservation_total;
                        }

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
                            ApiVehicleId = vehicle.value.category.vehicle_class_category.ToIntNullSafe(),
                            VehicleCode = vehicle.value.category.vehicle_class_code.ToStringNullSafe(),
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
                            APIDailyPricePayNow = apiDailyPrice,
                            APIOneWayFee = (float)vehicle.value.rate_totals.pay_now.one_way_fee,
                            APITotalPrice = apiTotalPrice,
                            APIReferenceCode = transactionId,
                            APIReferenceCode2 = vehicle.value.category.RateCodeId,
                            //APIReferenceCode3 = vehicle.value.rate_totals.rate.rate_code,
                            APIReferenceCode3 = vehicle.value.rate_totals.ToJson(),
                            DepositPrice = mappedVehicle.DepositPrice,
                            VendorMinimumDriverAge = mappedVehicle.VendorMinimumDriverAge ?? 0,
                            VendorMinimumDrivingLicenseAge = mappedVehicle.VendorMinimumDrivingLicenseAge ?? 0,
                            ServiceCharge = mappedVehicle.ServiceCharge,
                            FuelType = mappedVehicle.FuelType,
                            TransmissionType = mappedVehicle.TransmissionType,
                            TransmissionTypeName = vehicle.value.category.vehicle_transmission,
                            DepositCreditCardRequired = mappedVehicle.DepositCreditCardRequired,
                            PickupLocationId = getVehiclesRequest.PickupLocationId,
                            ReturnLocationId = getVehiclesRequest.ReturnLocationId,
                            LanguageType = languageType,
                            PickupDateTime = pickupDateTime,
                            ReturnDateTime = returnDateTime,
                            VehicleName = mappedVehicle.VehicleName,
                            VehicleImageUrl = mappedVehicle.VehicleImages.Count > 0 ? mappedVehicle.VehicleImages[0].Url : string.Empty,
                            BaseVendorRequestCurrencyType = baseVendorRequestCurrencyType,
                            VehicleClassNo = mappedVehicle.VehicleClassNo,
                            VehicleGroupName = mappedVehicle.VehicleGroupName,
                            VehicleClassSize = vehicle.value.category.vehicle_class_size.ToIntNullSafe(),
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
            return new ServiceResponseBase(null, false, $"{vendor.VendorName} servisinden araç dönmedi!");
        }

        private AvisAvailableVehicleRequest GetAvisAvailableVehicleRequestEntity(GetVehiclesRequest getVehiclesRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor) =>
             new AvisAvailableVehicleRequest
             {
                 StartOfficeMnemoic = additionalInformation.APIPickupLocationCode,
                 StartDateTime = (Convert.ToDateTime(getVehiclesRequest.PickupDate) + TimeSpan.Parse(getVehiclesRequest.PickupTime)).ToString("yyyy-MM-ddTHH:mm:ss"),
                 EndOfficeMnemoic = additionalInformation.APIReturnLocationCode,
                 EndDateTime = (Convert.ToDateTime(getVehiclesRequest.ReturnDate) + TimeSpan.Parse(getVehiclesRequest.ReturnTime)).ToString("yyyy-MM-ddTHH:mm:ss"),
                 Brand = vendor.VendorName,
                 CountryCode = "TR",
                 IsMobile = false,
                 IsMotorcycle = false,
                 DiscountNo = vendor.ApiClientId.Split("-")[0],
                 DriverCustomerNo = 0,
                 CorporateCustomerNo = 0,
                 AdditinoalHour = 0,
                 LogUserId = vendor.ApiClientId.Split("-")[1].ToIntNullSafe(),
             };
    }
}
