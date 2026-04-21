using DailydriveWS;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers.Dailydrive;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Domain.Models.Response.Dailydrive;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Dailydrive
{

    public class ExtraProvider : IExtraProvider
    {
        IVehicleProvider vehicleProvider;
        RestManager HttpManager { get; set; }

        public ExtraProvider(string apiBaseUrl)
        {
            HttpManager = new RestManager(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetExtraList(Vendor vendor, CurrencyTypes currencyType, LanguageTypes languageType, int rentalDuration)
        {
            var tarifNos = string.IsNullOrEmpty(vendor.SecretKey) ? "-".Split('-') : vendor.SecretKey.Split('-');
            //return new ServiceResponseBase
            //{
            //    Success = false,
            //    Message = "Hata"
            //};
            var content = new StringContent(@$"
                <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:dto='http://ws.naryaz.com/model/dto'>
                    <soapenv:Header/> 
                    <soapenv:Body> 
                        <dto:ExtraProductsRequest> 
                            <dto:rentalDays></dto:rentalDays> 
                            <!--Zero or more repetitions:--> 
                            <dto:tariffNos>{tarifNos[0]}</dto:tariffNos> 
                            <dto:tariffNos>{tarifNos[1]}</dto:tariffNos> 
                            <dto:campaignNo>0</dto:campaignNo> 
                            <dto:classNo>170</dto:classNo> 
                        </dto:ExtraProductsRequest> 
                    </soapenv:Body> 
                </soapenv:Envelope>
            ", Encoding.UTF8, "text/xml");

            var result = await HttpManager.PostAsyncHttpContent<DailydriveExtraResponseBase>(
                 entity: content,
                 requestPath: "",
                 headers: GetLocationsRequestParameters(vendor));

            if (result?.SOAPENVEnvelope?.SOAPENVBody?.Ns2ExtraProductsResponse?.Ns2ExtraProducts?.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result.SOAPENVEnvelope.SOAPENVBody.Ns2ExtraProductsResponse.Ns2ExtraProducts.Count > 0,
                    Data = result.SOAPENVEnvelope.SOAPENVBody.Ns2ExtraProductsResponse.Ns2ExtraProducts.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Dailydrive ekstraları gelmiyor!"
            };
        }

        public async Task<ServiceResponseBase> GetExtras(GetExtrasRequest getExtrasRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors, bool addProfitMarkup = true, bool getAPIPrices = false)
        {
            vehicleProvider = new VehicleProvider(vendor, false);
            var reservationToken = additionalInformation.ReservationToken;
            var requestCurrencyType = getExtrasRequest.CurrencyCode.ToEnum<CurrencyTypes>();
            var requestLanguageType = getExtrasRequest.LanguageCode.ToEnum<LanguageTypes>();

            var result = await HttpManager.PostAsyncHttpContent<DailydriveExtraResponseBase>(
                 entity: GetContent(vendor, additionalInformation),
                 requestPath: "",
                 headers: GetLocationsRequestParameters(vendor));

            var extras = new List<Extra>();

            if (result?.SOAPENVEnvelope?.SOAPENVBody?.Ns2ExtraProductsResponse?.Ns2ExtraProducts?.Count > 0)
            {
                var parts = vendor.SecretKey.Split('-');
                var tarif1 = parts.ElementAtOrDefault(0) ?? "0";
                var tarif2 = parts.ElementAtOrDefault(1) ?? "0";

                var tarifNo = vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission && vendor.ProfitMarkupAdditionalProducts > 0 ? tarif1 : tarif2;
                extras = result.SOAPENVEnvelope.SOAPENVBody.Ns2ExtraProductsResponse.Ns2ExtraProducts.Where(x => x.Ns2TariffNo == tarifNo).ToList().Map();

                CalculationHelper.SetExtraPrices(extras, vendor, exchangeRates, requestCurrencyType, addProfitMarkup, getAPIPrices, reservationToken.BaseVendorRequestCurrencyType);
            }

            var getVehiclesRequest = ObjectHelper.GetVehiclesRequestEntity(getExtrasRequest, vendor);

            var getVehiclesResponse = await vehicleProvider.GetVehicles(getVehiclesRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors, reservationToken.BaseVendorRequestCurrencyType);

            var vehicles = getVehiclesResponse?.Data as List<Vehicle>;
            var selectedVehicle = vehicles?.FirstOrDefault(x => x.VehicleCode == reservationToken.VehicleCode);

            if (selectedVehicle == null)
                return new ServiceResponseBase(null, false);

            CalculationHelper.SetVehiclePrices(selectedVehicle, vendor, exchangeRates, requestCurrencyType, reservationToken, additionalInformation.RentalDuration);
                   
            var getExtrasResponse = new GetExtrasResponse(ReservationHelper.RemoveZeroPriceExtras(extras), selectedVehicle);
            return new ServiceResponseBase(getExtrasResponse, true);
        }

        private Dictionary<string, object> GetLocationsRequestParameters(Vendor vendor)
        {
            var authToken = Encoding.ASCII.GetBytes($"{vendor.ApiKey}:{vendor.ApiPassword}");
            var token = Convert.ToBase64String(authToken);

            return new Dictionary<string, object>()
            {
                { "Content-Type", "text/xml"  },
                //{ "Accept", "*/*"  },
                { "Authorization", $"Basic {token}" },
            };
        }

        private StringContent GetContent(Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation) 
        {
            var parts = vendor.SecretKey.Split('-');
            var tarif1 = parts.ElementAtOrDefault(0) ?? "0";
            var tarif2 = parts.ElementAtOrDefault(1) ?? "0";

            return new StringContent(@$"
                <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:dto='http://ws.naryaz.com/model/dto'>
                    <soapenv:Header/> 
                    <soapenv:Body> 
                        <dto:ExtraProductsRequest> 
                            <dto:rentalDays>{additionalInformation.RentalDuration}</dto:rentalDays>
                            <dto:tariffNos>{tarif1}</dto:tariffNos>
                            <dto:tariffNos>{tarif2}</dto:tariffNos>
                            <dto:campaignNo></dto:campaignNo> 
                            <dto:classNo>170</dto:classNo> 
                        </dto:ExtraProductsRequest> 
                    </soapenv:Body> 
                </soapenv:Envelope>
            ", Encoding.UTF8, "text/xml");
        }
    }
}
