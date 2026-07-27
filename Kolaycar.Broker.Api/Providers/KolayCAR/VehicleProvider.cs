using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.KolayCAR;

using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Resws;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using KolayCARHelper = KolayCAR.Broker.API.Helpers.KolayCAR;
namespace KolayCAR.Broker.API.Providers.KolayCAR
{
    public class VehicleProvider : IVehicleProvider
    {
        private ServiceSoapClient _kolayCARService;

        //public VehicleProvider(Vendor vendor)
        //{
        //    _kolayCARService = new ServiceSoapClient(EndpointConfiguration.ServiceSoap, Helpers.KolayCAR.ReservationHelper.GetSoapRemoteAddress(vendor.APIBaseUrl));
        //}

        public VehicleProvider(Vendor vendor, bool disableTimeout)
        {
            // _kolayCARService = new ServiceSoapClient(EndpointConfiguration.ServiceSoap, Helpers.KolayCAR.ReservationHelper.GetSoapRemoteAddress(vendor.APIBaseUrl), vendor.APITimeout);
        }
        private void SetTimeOut(int timeout)
        {
            _kolayCARService.Endpoint.Binding.CloseTimeout = new System.TimeSpan(0, 0, timeout);
            _kolayCARService.Endpoint.Binding.OpenTimeout = new System.TimeSpan(0, 0, timeout);
            _kolayCARService.Endpoint.Binding.ReceiveTimeout = new System.TimeSpan(0, 0, timeout);
            _kolayCARService.Endpoint.Binding.SendTimeout = new System.TimeSpan(0, 0, timeout);
        }

        public async Task<ServiceResponseBase> GetVehicleList(Vendor vendor)
        {

            // SetTimeOut(30);
            var client = CreateClient(vendor, 30);
            try
            {
                var getVehicleListResult = await client.GET_VEHICLES_ALLAsync(
                        vendor.ApiKey,
                        vendor.ApiPassword,
                        "TR",
                        vendor.CurrencyType.ToString(),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty);

                var getVehicleListResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(getVehicleListResult.Body.GET_VEHICLES_ALLResult);

                if (getVehicleListResponseObject.RETURNCODE == 0)
                    return new ServiceResponseBase(getVehicleListResponseObject.VEHICLES.Map(), true);

                return new ServiceResponseBase(null, false, "KolayCAR Servisinden araç listesi alınamadı");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@getVehiclesAllKolayCAR}", ex.Message);
                return new ServiceResponseBase(null, false, "KolayCAR Servisinden araç listesi alınamadı");
            }
        }

        public async Task<ServiceResponseBase> GetVehicles(GetVehiclesRequest getVehiclesRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, CurrencyTypes baseVendorRequestCurrencyType, List<ProfitMarkup> profitMarkups = null)
        {

            KolayCARResponseBase getVehiclesResponseObject = null;
            KOLAYCARSETTINGS getSettingsResponse = null;
            var client = CreateClient(vendor, vendor.APITimeout);

            #region Credit kontrolü.
            if (ShouldFetchCreditSettings(vendor, additionalInformation.Agency))
            {
                var getSettingsResult = await client.GET_SETTINGSAsync(
                        getVehiclesRequest.ApiKey,
                        getVehiclesRequest.ApiPassword,
                        getVehiclesRequest.LanguageCode
                    );

                if (getSettingsResult != null)
                    getSettingsResponse = JsonConvert.DeserializeObject<KOLAYCARSETTINGS>(getSettingsResult.Body.GET_SETTINGSResult);

            }
            #endregion
            var getVehiclesResult = new GET_VEHICLES_V21();
            try
            {
                getVehiclesResult = await client.GET_VEHICLES1Async(
                                 getVehiclesRequest.ApiKey,
                                 getVehiclesRequest.ApiPassword,
                                 getVehiclesRequest.LanguageCode,
                                 baseVendorRequestCurrencyType.ToString(),
                                 Convert.ToInt32(additionalInformation.APIPickupLocationCode),
                                 Convert.ToInt32(additionalInformation.APIReturnLocationCode),
                                 getVehiclesRequest.PickupDate,
                                 getVehiclesRequest.ReturnDate,
                                 getVehiclesRequest.PickupTime,
                                 getVehiclesRequest.ReturnTime,
                                 getVehiclesRequest.UserToken,
                                 getVehiclesRequest.CouponCode,
                                 string.Empty,
                                 string.Empty,
                                 string.Empty,
                                 string.Empty,
                                 string.Empty);
            }
            catch (Exception ex)
            {
                // Serilog.Log.Error("{@KolaycarAvailabilityError}", $"{ex.Message} - {getVehiclesRequest.ToJson()}");
                return new ServiceResponseBase(null, false, "KolayCAR service could not be reached!");
            }

            getVehiclesResponseObject = JsonConvert.DeserializeObject<KolayCARResponseBase>(getVehiclesResult.GET_VEHICLES_V2Result);

            if (getVehiclesResponseObject?.RETURNCODE == 0)
            {
                var apiVehicleList = getVehiclesResponseObject.VEHICLES
                    .GroupBy(v => $"{v.VEHICLEID}-{v.VENDORID}")
                    .Select(g => g.OrderBy(v => v.DAILYPRICE).First())
                    .ToList();
                //var mappedVehicleList = getVehiclesResponseObject.VEHICLES.Map(additionalInformation);
                var mappedVehicleList = apiVehicleList.Map(additionalInformation, vendor);
                var kolayCarCreditType = ResolveKolayCarCreditType(getSettingsResponse, vendor, additionalInformation.Agency);
                var requestCurrencyType = getVehiclesRequest.CurrencyCode.ToEnum<CurrencyTypes>();

                if (vendor.VehicleMappingActive)
                {
                    mappedVehicleList = VehicleHelper.MapLocalVehicleList(mappedVehicleList, localVehicles, exchangeRates: exchangeRates, baseVendorRequestCurrencyType, requestCurrencyType, useLocalDeposit: (bool)vendor.UseLocalDeposit, vendor: vendor);
                    apiVehicleList.RemoveAll(p => !localVehicles.Any(e => e.VehicleCode == $"{p.VEHICLEID}-{p.VENDORID}"));
                }

                var vehiclePriceSnapshots = new Dictionary<string, KolayCARHelper.VehiclePriceSnapshot>();
                foreach (var mappedVehicle in mappedVehicleList)
                {
                    var apiVehicle = apiVehicleList.FirstOrDefault(x => $"{x.VEHICLEID}-{x.VENDORID}" == mappedVehicle.VehicleCode);
                    if (apiVehicle == null)
                        continue;

                    vehiclePriceSnapshots[mappedVehicle.VehicleCode] = KolayCARHelper.MoneyHelper.ApplyVehiclePrices(
                        mappedVehicle,
                        apiVehicle,
                        vendor,
                        additionalInformation.Agency,
                        exchangeRates,
                        baseVendorRequestCurrencyType,
                        requestCurrencyType,
                        profitMarkups);
                }

                if (mappedVehicleList.Any(vehicle => Math.Abs(vehicle.RentalDuration - additionalInformation.RentalDuration) > 1))
                    return new ServiceResponseBase(null, false, "Yanlış gün sayısı");

                var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
                var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);
                var languageType = getVehiclesRequest.LanguageCode.TrimNullSafe().ToUpper().ToEnum<LanguageTypes>();

                foreach (var vehicle in apiVehicleList.Select((value, index) => new { value, index }))
                {
                    var tempMappedVehicleList = mappedVehicleList.Where(x => x.VehicleCode == $"{vehicle.value.VEHICLEID}-{vehicle.value.VENDORID}").ToList();

                    for (int i = 0; i < tempMappedVehicleList.Count; i++)
                    {
                        var mappedVehicle = tempMappedVehicleList[i];
                        mappedVehicle.CreditType = kolayCarCreditType;
                        mappedVehicle.FullCredit = kolayCarCreditType == CreditType.FullCredit;
                        vehiclePriceSnapshots.TryGetValue(mappedVehicle.VehicleCode, out var priceSnapshot);

                        var reservationToken = new ReservationToken
                        {
                            AgencyId = additionalInformation.Agency.AgencyId,
                            VendorId = vendor.VendorId,
                            APIVendorId = vehicle.value.VENDORID == 0 ? vendor.VendorId : vehicle.value.VENDORID,
                            APIVendorName = vendor.VendorName,
                            APIVendorPhone = vendor.VendorPhone,
                            APIVendorEmail = vendor.VendorEmail,
                            APIVendorLogo = vendor.Logo,
                            VehicleId = mappedVehicle.VehicleId,
                            VehicleCode = $"{vehicle.value.VEHICLEID}-{vehicle.value.VENDORID}",
                            APIPickupLocationId = additionalInformation.APIPickupLocationId,
                            APIPickupLocationCode = additionalInformation.APIPickupLocationCode,
                            APIReturnLocationId = additionalInformation.APIReturnLocationId,
                            APIReturnLocationCode = additionalInformation.APIReturnLocationCode,
                            CurrencyType = requestCurrencyType,
                            RentalDuration = mappedVehicle.RentalDuration,
                            DailyPrice = mappedVehicle.DailyPrice,
                            OneWayFee = mappedVehicle.OneWayFee,
                            DailyPricePayNow = mappedVehicle.DailyPricePayNow,
                            APIDailyPrice = KolayCARHelper.MoneyHelper.ToFloat(vehicle.value.DAILYPRICE),
                            APIDailyPricePayNow = KolayCARHelper.MoneyHelper.ToFloat(vehicle.value.DAILYPRICEPAYNOW),
                            APIOneWayFee = KolayCARHelper.MoneyHelper.ToFloat(vehicle.value.ONEWAYFEE),
                            APITotalPrice = KolayCARHelper.MoneyHelper.ToFloat(vehicle.value.TOTALPRICE),
                            KolayCarDailyPrice = priceSnapshot?.DailyPrice,
                            KolayCarOneWayFee = priceSnapshot?.OneWayFee,
                            KolayCarDailyPricePayNow = priceSnapshot?.DailyPricePayNow,
                            KolayCarAPIDailyPrice = priceSnapshot?.ApiDailyPrice,
                            KolayCarAPIDailyPricePayNow = priceSnapshot?.ApiDailyPricePayNow,
                            KolayCarAPIOneWayFee = priceSnapshot?.ApiOneWayFee,
                            KolayCarAPITotalPrice = priceSnapshot?.ApiTotalPrice,
                            KolayCarDepositPrice = priceSnapshot?.DepositPrice,
                            KolayCarServiceCharge = priceSnapshot?.ServiceCharge,
                            DepositPrice = mappedVehicle.DepositPrice,
                            VendorMinimumDriverAge = vehicle.value.VENDORMINDRIVERAGE,
                            VendorMinimumDrivingLicenseAge = vehicle.value.VENDORMINDRIVINGLICENSEAGE,
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
                            CreditType = mappedVehicle.CreditType,
                            APICreditType = mappedVehicle.CreditType,
                            FullCredit = mappedVehicle.FullCredit,
                            APIFullCredit = mappedVehicle.FullCredit,
                            SippCode = mappedVehicle.SippCode
                            //APITotalPrice = vehicle.value.TOTALPRICE
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

                return new ServiceResponseBase(mappedVehicleList, getVehiclesResponseObject.RETURNCODE == 0, "", getVehiclesResponseObject.MESSAGE, getVehiclesResponseObject.RETURNCODE.ToString());
            }
            return new ServiceResponseBase(null, false, "KolayCAR service could not be reached!", getVehiclesResponseObject.MESSAGE.ToStringNullSafe(), getVehiclesResponseObject.RETURNCODE.ToStringNullSafe());
        }

        private static bool ShouldFetchCreditSettings(Vendor vendor, Agency agency)
        {
            return (VendorAllowsCreditType(vendor, CreditType.FullCredit) || VendorAllowsCreditType(vendor, CreditType.LimitedCredit))
                && (CreditHelper.AgencyAllowsCreditType(agency, CreditType.FullCredit) || CreditHelper.AgencyAllowsCreditType(agency, CreditType.LimitedCredit));
        }

        private static CreditType ResolveKolayCarCreditType(KOLAYCARSETTINGS settings, Vendor vendor, Agency agency)
        {
            if (settings == null)
                return CreditType.Non;

            if (settings.FULLCREDITLIMITEDACTIVE
                && VendorAllowsCreditType(vendor, CreditType.LimitedCredit)
                && CreditHelper.AgencyAllowsCreditType(agency, CreditType.LimitedCredit))
                return CreditType.LimitedCredit;

            if (settings.FULLCREDITACTIVE
                && VendorAllowsCreditType(vendor, CreditType.FullCredit)
                && CreditHelper.AgencyAllowsCreditType(agency, CreditType.FullCredit))
                return CreditType.FullCredit;

            return CreditType.Non;
        }

        private static bool VendorAllowsCreditType(Vendor vendor, CreditType creditType)
        {
            if (creditType == CreditType.Non)
                return true;

            if (vendor == null)
                return false;

            if (vendor.CreditType == creditType)
                return true;

            return vendor.CreditType == CreditType.FullCredit && creditType == CreditType.LimitedCredit;
        }

        private ServiceSoapClient CreateClient(Vendor vendor, int timeoutSeconds)
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport) // HTTPS desteği
            {
                SendTimeout = TimeSpan.FromSeconds(timeoutSeconds),
                OpenTimeout = TimeSpan.FromSeconds(timeoutSeconds),
                CloseTimeout = TimeSpan.FromSeconds(timeoutSeconds),
                ReceiveTimeout = TimeSpan.FromSeconds(timeoutSeconds),
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max
            };

            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

            var endpoint = new EndpointAddress(Helpers.KolayCAR.ReservationHelper.GetSoapRemoteAddress(vendor.APIBaseUrl));

            return new ServiceSoapClient(binding, endpoint);
        }
    }
}
