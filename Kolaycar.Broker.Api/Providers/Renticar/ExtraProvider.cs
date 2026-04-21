using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Renticar;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Renticar.Request;
using KolayCAR.Broker.Domain.Models.Renticar.Response;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Renticar
{
    public class ExtraProvider : IExtraProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        IVehicleProvider VehicleProvider { get; set; }

        public ExtraProvider()
        {

        }
        public ExtraProvider(Vendor vendor, IMemoryCache memoryCache)
        {
            RestManager = new RestManager(vendor.APIBaseUrl);
            AuthProvider = new AuthProvider(vendor.APIBaseUrl, memoryCache);
            VehicleProvider = new VehicleProvider(vendor, memoryCache, false);
        }
        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var extras = new List<Domain.Models.Extra>
            {
                //new Domain.Models.Extra
                //{
                //    ExtraId = 1,
                //    ExtraCode = "Bebek Koltuğu",
                //    ExtraName = "Bebek Koltuğu",
                //    ExtraRentalType = ExtraRentalTypes.Daily,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 2,
                //    ExtraCode = "Ek Sürücü",
                //    ExtraName = "Ek Sürücü(Günlük)",
                //    ExtraRentalType = ExtraRentalTypes.Daily,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 3,
                //    ExtraCode = "Genç Sürücü Paketi",
                //    ExtraName = "Genç Sürücü Paketi",
                //    ExtraRentalType = ExtraRentalTypes.Daily,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 4,
                //    ExtraCode = "İnternet Paketi",
                //    ExtraName = "İnternet Paketi",
                //    ExtraRentalType = ExtraRentalTypes.Daily,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 5,
                //    ExtraCode = "Kapsamlı Mini Hasar Güvencesi",
                //    ExtraName = "Kapsamlı Mini Hasar Güvencesi",
                //    ExtraRentalType = ExtraRentalTypes.Daily,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 6,
                //    ExtraCode = "Mini Hasar Güvencesi",
                //    ExtraRentalType = ExtraRentalTypes.Daily,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 7,
                //    ExtraCode = "Araç Marka Garantisi ",
                //    ExtraName = "Araç Marka Garantisi ",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 8,
                //    ExtraCode = "Kış Lastiği",
                //    ExtraName = "Kış Lastiği",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 9,
                //    ExtraCode = "Yol Yardım Assistans",
                //    ExtraName = "Yol Yardım Assistans",
                //    ExtraRentalType = ExtraRentalTypes.Daily,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 10,
                //    ExtraCode = "Premium Tam Kapsamlı Sigorta",
                //    ExtraName= "Premium Tam Kapsamlı Sigorta",
                //    ExtraRentalType = ExtraRentalTypes.Daily,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 11,
                //    ExtraCode = "(LCF) Lastik,Cam,Far sigortası",
                //    ExtraName = "(LCF) Lastik,Cam,Far sigortası",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 12,
                //    ExtraCode = "Çocuk Koltuğu",
                //    ExtraName = "Çocuk Koltuğu",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 13,
                //    ExtraCode = "Araç Üstü Çadır",
                //    ExtraName = "Araç Üstü Çadır",
                //    ExtraRentalType = ExtraRentalTypes.Daily,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 14,
                //    ExtraCode = "Ek Sürücü",
                //    ExtraName = "Ek Sürücü(Kiralama Başına)",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 15,
                //    ExtraCode = "Mini Hasar Sigortası 1500 TL Kapsamlı",
                //    ExtraName = "Mini Hasar Sigortası 1500 TL Kapsamlı",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 16,
                //    ExtraCode = "Mini Hasar Sigortası 2500 TL Kapsamlı",
                //    ExtraName = "Mini Hasar Sigortası 2500 TL Kapsamlı",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 17,
                //    ExtraCode = "Mini Hasar Sigortası 3500 TL Kapsamlı",
                //    ExtraName = "Mini Hasar Sigortası 3500 TL Kapsamlı",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 18,
                //    ExtraCode = "Güvence Paketi",
                //    ExtraName = "Güvence Paketi",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 19,
                //    ExtraCode = "Muafiyetsiz Kaza Güvencesi",
                //    ExtraName = "Muafiyetsiz Kaza Güvencesi",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 20,
                //    ExtraCode = "Bireysel Kaza Güvencesi",
                //    ExtraName = "Bireysel Kaza Güvencesi",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
                //new Domain.Models.Extra
                //{
                //    ExtraId = 21,
                //    ExtraCode = "3. Şahıs Sorumluluk Güvencesi",
                //    ExtraName = "3. Şahıs Sorumluluk Güvencesi",
                //    ExtraRentalType = ExtraRentalTypes.PerRental,
                //    ExtraQuantityIncreasable = false
                //},
            };

            return new ServiceResponseBase
            {
                Success = true,
                Data = extras
            };
        }

        private static ExtrasRequestBase CreateBody(string offerId) => new ExtrasRequestBase { offerId = offerId };
        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var requestLanguageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();
            var extras = new List<Domain.Models.Extra>();

            var auth = await AuthProvider.GetToken(vendor, getExtrasRequest.IsReservationRequest);

            if (auth?.status == "success")
            {
                var result = await RestManager.PostAsync<ExtrasRequestBase, ExtrasResponseBase>(
                    requestPath: "extras",
                    headers: AuthProvider.CreateHeader(auth.token),
                    entity: CreateBody(reservationToken.APIReferenceCode));

                if (!getExtrasRequest.IsReservationRequest && result?.status == "error")
                {
                    getExtrasRequest.IsReservationRequest = true;
                    await GetExtras(getExtrasRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, addProfitMarkup, getAPIPrices);
                }

                if (result?.status == "success" && result?.extras?.Count > 0)
                {
                    extras = result.extras.Map(requestCurrencyType);
                    CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);
                }
            }

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await VehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode && x.ApiVendorName == reservationToken.APIReferenceCode2);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);

            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }
    }
}
