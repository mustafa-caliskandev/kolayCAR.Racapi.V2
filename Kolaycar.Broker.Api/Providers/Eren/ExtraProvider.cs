using Kolaycar.Broker.Api.Providers.Eren;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Eren;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Responses.Eren;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Eren;

public class ExtraProvider : IExtraProvider
{
    private readonly IVehicleProvider _vehicleProvider;
    private readonly HttpManager _httpManager;
    private readonly AuthProvider _authProvider;
    public ExtraProvider(Vendor vendor)
    {
        _vehicleProvider = new VehicleProvider(vendor, false);
        _authProvider = new AuthProvider(vendor.APIBaseUrl);
        _httpManager = new HttpManager(vendor.APIBaseUrl);
    }
    public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
    {
        var accessToken = await _authProvider.GetTokenAsync(vendor);

        var result = await _httpManager.GetAsync2<ErenExtraListResponse>
            (
              requestPath: "/v1/extras",
              headers: accessToken
            );

        if (result.Data?.extras?.Any() ?? false)
        {
            return new ServiceResponseBase
            {
                Data = result.Data.extras.Map(),
                Success = true
            };
        }
        return new ServiceResponseBase
        {
            Data = null,
            Success = false
        };
    }

    public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
    {
        var reservationToken = additionalInformation.ReservationToken;
        var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

        var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

        var getVehiclesResponse = await _vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

        var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
        var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleId == additionalInformation.VehicleId);

        if (selectedVehicle == null)
            return new ServiceResponseBase(null, false);

        var extras = selectedVehicle.Extras;

        CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
        CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

        var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle, vehicles);
        return new ServiceResponseBase(getExtrasResponse, true);
    }
}
