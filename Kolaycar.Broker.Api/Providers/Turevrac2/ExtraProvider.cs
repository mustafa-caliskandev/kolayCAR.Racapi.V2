using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Turevrac2
{
    public class ExtraProvider : IExtraProvider
    {
        VehicleProvider _vehicleProvider;

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var extras = new List<Extra>
              {
                    new Extra
                    {
                        ExtraId = 1,
                        ExtraCode = "CDW",
                        ExtraName = "CDW Güvencesi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false,
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "Young_Driver",
                        ExtraName = "Genç Sürücü",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 3,
                        ExtraCode = "PAI",
                        ExtraName = "PAI Güvencesi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 4,
                        ExtraCode = "LCF",
                        ExtraName = "LCF Güvencesi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 5,
                        ExtraCode = "IMM",
                        ExtraName = "IMM Güvencesi",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 6,
                        ExtraCode = "Exemption",
                        ExtraName = "Muafiyetsiz Güvence",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 7,
                        ExtraCode = "Baby_Seat",
                        ExtraName = "Bebek Koltuğu",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 8,
                        ExtraCode = "Addition_Drive",
                        ExtraName = "Ek Sürücü",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 9,
                        ExtraCode = "PKH1",
                        ExtraName = "Premium Güvence",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 10,
                        ExtraCode = "PKH2",
                        ExtraName = "Premium Plus Güvence",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 11,
                        ExtraCode = "Winter_Tire",
                        ExtraName = "Kış Lastiği",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 12,
                        ExtraCode = "Navigation",
                        ExtraName = "Navigasyon",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 13,
                        ExtraCode = "SCDW",
                        ExtraName = "Süper Hasar Sorumluluk Sigortası",
                        ExtraRentalType = ExtraRentalTypes.PerRental,
                        ExtraQuantityIncreasable = false
                    },
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

            _vehicleProvider = new VehicleProvider(vendor, false);

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);
            getVehiclesRequest.ReservationToken = reservationToken.ToJson();

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
}
