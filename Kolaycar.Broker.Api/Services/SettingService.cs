using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Mappers.KolayCAR;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using KolayCARProvider = KolayCAR.Broker.API.Providers.KolayCAR;

namespace KolayCAR.Broker.API.Services
{
    public interface ISettingService
    {
        Task<ServiceResponseBase> GetSettings(GetSettingsRequest getSettingsRequest);
    }
    public class SettingService : ISettingService
    {
        private readonly BrokerContext _context;
        private readonly IVendorService _vendorService;
        private readonly IAgencyService _agencyService;
        private readonly IReservationStepsService _reservationStepsService;
        private readonly IResTokenService _resTokenService;

        public SettingService(BrokerContext context, IConfigurationService configurationService, IVendorService vendorService, IAgencyService agencyService, IReservationStepsService reservationStepsService, IResTokenService resTokenService)
        {
            _context = context;
            _agencyService = agencyService;
            _vendorService = vendorService;
            _reservationStepsService = reservationStepsService;
            _resTokenService = resTokenService;
        }

        public async Task<ServiceResponseBase> GetSettings(GetSettingsRequest getSettingsRequest)
        {
            var reservationToken = await _resTokenService.GetReservationTokenByUniqueId(getSettingsRequest.ReservationToken);
            if (reservationToken != null)
            {
                var agency = await _agencyService.GetAgency(reservationToken.AgencyId.ToLongNullSafe());
                if (agency != null)
                {
                    var vendor = await _vendorService.GetVendorById(reservationToken.VendorId, agency, subVendorId: reservationToken.APIVendorId);
                    if (vendor != null)
                    {
                        await _agencyService.SetAgencyPaymentOptions(agency, vendor);
                        ISettingsProvider settingsProvider;

                        var exchangeRates = await _context.Exchangerates.ToListAsync();
                        var mappedExchangeRates = exchangeRates.Map();

                        getSettingsRequest.ApiKey = vendor.ApiKey;
                        getSettingsRequest.ApiPassword = vendor.ApiPassword;
                        getSettingsRequest.VendorType = vendor.VendorType;

                        switch (getSettingsRequest.VendorType)
                        {
                            default:
                                return new ServiceResponseBase
                                {
                                    Success = false,
                                    Message = "vendorType parametresine uygun tedarikçi bulunamadı!",
                                    ServiceCode = null,
                                    ServiceMessage = null
                                };
                            case VendorTypes.KolayCAR:
                                {
                                    settingsProvider = new KolayCARProvider.SettingsProvider(vendor);
                                    break;
                                }
                        }

                        var settings = await settingsProvider.GetSettings(getSettingsRequest, vendor, reservationToken, mappedExchangeRates);

                        if (settings != null)
                        {
                            var settingsData = settings.Data as Settings;
                            settingsData.ReservationSources = await _reservationStepsService.GetReservationSources();
                        }

                        return settings;
                    }
                    else
                        return new ServiceResponseBase
                        {
                            Success = false,
                            Message = "Tedarikçi bilgisine ulaşılamadı!",
                            ServiceCode = null,
                            ServiceMessage = null
                        };
                }
                else
                    return new ServiceResponseBase
                    {
                        Success = false,
                        Message = "Acente bilgisine ulaşılamadı!",
                        ServiceCode = null,
                        ServiceMessage = null
                    };
            }
            return new ServiceResponseBase
            {
                Success = false,
                Message = "reservationToken hatalı!",
                ServiceCode = null,
                ServiceMessage = null
            };

        }
    }
}
