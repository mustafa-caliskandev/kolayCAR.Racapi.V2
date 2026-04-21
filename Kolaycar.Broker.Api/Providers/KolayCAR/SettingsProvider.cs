using KolayCAR.Broker.API.Mappers.KolayCAR;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Resws;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Resws.ServiceSoapClient;

namespace KolayCAR.Broker.API.Providers.KolayCAR
{
    public class SettingsProvider : ISettingsProvider
    {
        private readonly ServiceSoapClient _kolayCARService;
        public SettingsProvider(Vendor vendor)
        {
            _kolayCARService = new ServiceSoapClient(EndpointConfiguration.ServiceSoap, Helpers.KolayCAR.ReservationHelper.GetSoapRemoteAddress(vendor.APIBaseUrl));
        }

        public async Task<ServiceResponseBase> GetSettings(GetSettingsRequest getSettingsRequest, Vendor vendor, ReservationToken reservationToken, List<ExchangeRates> exchangeRates)
        {
            var settings = new Settings
            {
                AgencyCommissionType = AgencyCommissionTypes.CommissionCalculatedOnTotalPrice,
                AgencyRentalFeeType = FeeTypes.ToAPIOwner,
                ServiceCharge = CalculationHelper.CurrencyExchange(exchangeRates, vendor, vendor.ServiceCharge, vendor.ServiceChargeCurrencyType, (CurrencyTypes)Enum.Parse(typeof(CurrencyTypes), getSettingsRequest.CurrencyCode)),
                ServiceChargeCurrencyType = vendor.ServiceChargeCurrencyType,
                Vendor = vendor
            };

            return new ServiceResponseBase
            {
                Success = true,
                Data = settings
            };
        }

        public async Task<ServiceResponseBase> GetSettingsFromVendorApi(Vendor vendor)
        {
            KOLAYCARSETTINGS getSettingsResponse = null;
            try
            {
                var result = await _kolayCARService.GET_SETTINGSAsync(
                    vendor.ApiKey,
                    vendor.ApiPassword,
                    "TR");

                if (result != null)
                {
                    getSettingsResponse = JsonConvert.DeserializeObject<KOLAYCARSETTINGS>(result.Body.GET_SETTINGSResult);

                    return new ServiceResponseBase
                    {
                        Success = true,
                        ServiceCode = "1",
                        ServiceMessage = "OK",
                        Data = getSettingsResponse.Map()
                    };
                }

                return new ServiceResponseBase
                {
                    Success = false,
                    ServiceCode = "-1",
                    ServiceMessage = "KolayCAR servisine ulaşılamadı.",
                    Data = null
                };

            }
            catch (Exception ex)
            {
                return new ServiceResponseBase
                {
                    Success = false,
                    ServiceCode = "-1",
                    ServiceMessage = "KolayCAR servisinde bir hata oluştu.",
                    Data = null
                };
            }
        }
    }
}
