using KolayCAR.Broker.API.Helpers;
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
using GreenMotionProvider = KolayCAR.Broker.API.Providers.GreenMotion;

namespace KolayCAR.Broker.API.Providers.GreenMotion
{
    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;
        RestManager RestManager { get; set; }
        public ExtraProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var extras = new List<Extra>
              {

                    new Extra
                    {
                        ExtraId = 1,
                        ExtraCode = "2",
                        ExtraName = "Ek Sürücü",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false,
                    },
                    new Extra
                    {
                        ExtraId = 2,
                        ExtraCode = "1",
                        ExtraName = "Bebek Koltuğu",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 3,
                        ExtraCode = "3",
                        ExtraName = "Çocuk Koltuğu",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 4,
                        ExtraCode = "44",
                        ExtraName = "Mini Hasar Sigortası",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 5,
                        ExtraCode = "17",
                        ExtraName = "Bireysel Kaza Sigortası",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 7,
                        ExtraCode = "45",
                        ExtraName = "Süper Mini Hasar",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 6,
                        ExtraCode = "46",
                        ExtraName = "Süper Güvence",
                        ExtraRentalType = ExtraRentalTypes.Daily,
                        ExtraQuantityIncreasable = false
                    },
                    new Extra
                    {
                        ExtraId = 7,
                        ExtraCode = "47",
                        ExtraName = "Artan Mali Mesuliyet",
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
            vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == additionalInformation.ReservationToken.VehicleCode);

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
