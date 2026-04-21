using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OtocarProvider = KolayCAR.Broker.API.Providers.Otocar;

namespace KolayCAR.Broker.API.Providers.Otocar
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var extras = new List<Extra>
              {

                    new Extra
                    {
                        ExtraId = 1,
                        ExtraCode = "bebek_koltuk",
                        ExtraName = "Bebek koltuğu",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "lcf_sigorta",
                        ExtraName = "Lcf Sigortası",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 3,
                        ExtraCode = "ek_sofor",
                        ExtraName = "Ek şoför",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 4,
                        ExtraCode = "navigasyon",
                        ExtraName = "Navigasyon Cihazı",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 5,
                        ExtraCode = "superkasko",
                        ExtraName = "Süper Kasko",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false
                    }
              };
            return new ServiceResponseBase
            {
                Success = true,
                Data = extras
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            vehicleProvider = new VehicleProvider(vendor, false);

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleId == additionalInformation.VehicleId);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            var extras = selectedVehicle.Extras;

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
            CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
    }
}

