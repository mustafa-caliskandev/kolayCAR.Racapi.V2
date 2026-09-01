using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Enuygun;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Enuygun
{
    public class ExtraProvider : IExtraProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        IVehicleProvider vehicleProvider { get; set; }
        ICacheService _cacheService { get; set; }

        public ExtraProvider(string apiBaseUrl, ICacheService cacheService)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl);
            _cacheService = cacheService;
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var result = ReadExtraList();

            if (result != null)
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = result.Map(vendor)
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = $"{vendor.VendorName} extra list servisine ulaşılamadı!"
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            vehicleProvider = new VehicleProvider(vendor, false, _cacheService);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var extras = new List<Extra>();

            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);

            if (auth?.Status == "OK")
            {
                var result = await RestManager.PostAsync<EnuygunRequest.Extra.Extras, EnuygunResponse.Extra.Root>(
                    requestPath: "/api/v1/extra-services",
                    headers: AuthProvider.CreateHeaderWithToken(auth.Data.Token),
                    entity: new EnuygunRequest.Extra.Extras
                    {
                        requestId = reservationToken.APIReferenceCode,
                        referenceId = reservationToken.APIReferenceCode2
                    }
                    );

                if (result?.status == "OK")
                {
                    extras = result.data.extraServices.Map(vendor);
                    CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices,
                           reservationToken.BaseVendorRequestCurrencyType);
                }
            }
            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var companyName = reservationToken.APIReferenceCode3.Split("|")[0];
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode && x.ApiVendorName == companyName.TrimEnd());

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        private List<EnuygunResponse.Extra.ExtraService> ReadExtraList()
        {
            var data = ExcelHelper.ReadExcel(@"Docs\Enuygun\extra_service.csv");
            var result = new List<EnuygunResponse.Extra.ExtraService>();

            if (data?.Rows?.Count > 0)
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    result.Add(new EnuygunResponse.Extra.ExtraService
                    {
                        context = data.Rows[i]["slug"].ToStringNullSafe(),
                        name = data.Rows[i]["name"].ToStringNullSafe()
                    });
                }
            }

            return result;
        }
    }
}
