using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IConfigurationService
    {
        Task<Configurations> GetConfigurations();
        Task<T> GetConfigurationValueByFieldName<T>(string filedName);
        Task<BaseAgencies> GetBaseAgencies();
        Task<string> GetLabel(int labelId, LanguageTypes languageType);
        Task<string> GetFormContent(int contentId, LanguageTypes languageType);
        Task<string> GetContentUrl(int contentId, LanguageTypes languageType);
        Task WriteLog(BrokerLogModel brokerLogModel);
        Task<Parametre> GetConfigurationByDegisken(string degisken);
        string GetConnectionString();
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly BrokerContext _context;
        private readonly LoggingDbContext _loggingDbContext;
        private readonly ICacheService _cacheService;
        private readonly IConfigurationRepository _configurationRepository;

        public ConfigurationService(BrokerContext context, LoggingDbContext loggingDbContext, ICacheService cacheService, IConfigurationRepository configurationRepository)
        {
            _context = context;
            _loggingDbContext = loggingDbContext;
            _cacheService = cacheService;
            _configurationRepository = configurationRepository;
        }

        public async Task<T> GetConfigurationValueByFieldName<T>(string filedName)
        {
            var configuration = await _context.Parametre.Where(x => x.Degisken == filedName).FirstOrDefaultAsync();
            return configuration != null ? (T)Convert.ChangeType(configuration.Deger, typeof(T)) : default(T);
        }

        public async Task<Configurations> GetConfigurations()
        {
            var configurations = await GetConfigurationList();

            var configurationDto = new Configurations
            {
                SMTPServer = configurations.Where(x => x.Degisken == "SMTPServer").FirstOrDefault().Deger,
                SMTPPort = configurations.Where(x => x.Degisken == "SMTPPort").FirstOrDefault().Deger.ToIntNullSafe(),
                SMTPSSL = configurations.Where(x => x.Degisken == "SMTPSSL").FirstOrDefault().Deger.ToBoolNullSafe(),
                SenderEmail = configurations.Where(x => x.Degisken == "SenderMail").FirstOrDefault().Deger,
                SenderPassword = configurations.Where(x => x.Degisken == "SenderPass").FirstOrDefault().Deger,
                AdministratorEmail = configurations.Where(x => x.Degisken == "AdministratorMail").FirstOrDefault().Deger,
                SendInvoiceToCustomerActive = configurations.Where(x => x.Degisken == "SendInvoiceActive").FirstOrDefault().Deger.ToBoolNullSafe(),
                SendInvoiceToAgencyActive = configurations.Where(x => x.Degisken == "SendMailInvoice").FirstOrDefault().Deger.ToBoolNullSafe(),
                SendReservationMailToCustomerActive = configurations.Where(x => x.Degisken == "SendReservationMailToCustomerActive").FirstOrDefault().Deger.ToBoolNullSafe(),
                ExternalInvoiceType = (ExternalInvoiceType)configurations.Where(x => x.Degisken == "ExternalInvoiceType").FirstOrDefault().Deger.ToIntNullSafe(),
                ExternalLoginType = (ExternalLoginType)configurations.Where(x => x.Degisken == "ExternalLoginType").FirstOrDefault().Deger.ToIntNullSafe(),
                ReservationMailTemplateId = configurations.Where(x => x.Degisken == "ReservationMailTemplate").FirstOrDefault().Deger.ToIntNullSafe(),
                ReservationStatusNameLabelId = configurations.Where(x => x.Degisken == "ReservationStatusNameLabelId").FirstOrDefault().Deger.ToIntNullSafe(),
                ReservationStatusMessageLabelId = configurations.Where(x => x.Degisken == "ReservationStatusMessageLabelId").FirstOrDefault().Deger.ToIntNullSafe(),
                ReservationDetailUrlTemplate = configurations.Where(x => x.Degisken == "ReservationDetailUrlTemplate").FirstOrDefault().Deger,
                PortalOwnerTitle = configurations.Where(x => x.Degisken == "POTITLE").FirstOrDefault().Deger,
                PortalOwnerAddress = configurations.Where(x => x.Degisken == "POADDRESS").FirstOrDefault().Deger,
                PortalOwnerPhone = configurations.Where(x => x.Degisken == "POPHONE").FirstOrDefault().Deger,
                PortalOwnerFax = configurations.Where(x => x.Degisken == "POFAX").FirstOrDefault().Deger,
                PortalOwnerEmail = configurations.Where(x => x.Degisken == "POEMAIL").FirstOrDefault().Deger,
                PortalOwnerDomain = configurations.Where(x => x.Degisken == "PODOMAIN").FirstOrDefault().Deger,
                PortalOwnerLogoPath = configurations.Where(x => x.Degisken == "POLOGO").FirstOrDefault().Deger,
                NearestRentalTime = configurations.Where(x => x.Degisken == "NearestRentalTime").FirstOrDefault().Deger.ToIntNullSafe(),
                EmergencyPhone = configurations.Where(x => x.Degisken == "POEMERGENCYPHONE").FirstOrDefault().Deger,
                KolayCARPaymentAPIKey = configurations.Where(x => x.Degisken == "KolayCARPaymentAPIKey").FirstOrDefault().Deger,
                KolayCARPaymentAPIPassword = configurations.Where(x => x.Degisken == "KolayCARPaymentAPIPassword").FirstOrDefault().Deger,
                KolayCARPaymentAPIVendorId = configurations.Where(x => x.Degisken == "KolayCARPaymentAPIVendorId").FirstOrDefault().Deger.ToIntNullSafe(),
                OnlyAvailableVehicles = configurations.Where(x => x.Degisken == "OnlyAvailableVehicles").FirstOrDefault().Deger.ToBoolNullSafe(),
                AdministratorEmailSending = configurations.Where(x => x.Degisken == "AdministratorEmailSending").FirstOrDefault().Deger.ToBoolNullSafe(),
                LocationEmailSending = configurations.Where(x => x.Degisken == "LocationEmailSending").FirstOrDefault().Deger.ToBoolNullSafe(),
                DefaultCustomerMailAddress = configurations.Where(x => x.Degisken == "DefaultMailAddress").FirstOrDefault().Deger.ToStringNullSafe(),
                SendSmsToCustomer = configurations.Where(x => x.Degisken == "SendSmsToCustomer").FirstOrDefault().Deger.ToBoolNullSafe(),
                KolayCARPaymentAPIDomainId = configurations.Where(x => x.Degisken == "KolayCARPaymentAPIDomainId").FirstOrDefault().Deger.ToIntNullSafe(),
                KolayCARRentwsAPIKey = configurations.Where(x => x.Degisken == "KolayCARRentwsAPIKey").FirstOrDefault().Deger.ToStringNullSafe(),
                KolayCARRentwsAPIPassword = configurations.Where(x => x.Degisken == "KolayCARRentwsAPIPassword").FirstOrDefault().Deger.ToStringNullSafe(),
                KolayCARBankVendorId = configurations.Where(x => x.Degisken == "KolayCARBankVendorId").FirstOrDefault().Deger.ToIntNullSafe(),
                PaymentRefundActive = configurations.Where(x => x.Degisken == "PaymentRefundActive").FirstOrDefault().Deger.ToBoolNullSafe(),
                PenaltyInformationEmailActive = configurations.Where(x => x.Degisken == "PenaltyInformationEmailActive").FirstOrDefault().Deger.ToBoolNullSafe(),
                TimeoutLog = configurations.Where(x => x.Degisken == "TimeoutLog").FirstOrDefault() != null ? configurations.Where(x => x.Degisken == "TimeoutLog").FirstOrDefault().Deger.ToBoolNullSafe() : false,
                NewSetting = configurations.Where(x => x.Degisken == "NewSetting").FirstOrDefault() != null ? configurations.Where(x => x.Degisken == "NewSetting").FirstOrDefault().Deger.ToBoolNullSafe() : true,
                RezIlaveSMSAktif = configurations.Where(x => x.Degisken == "RezIlaveSMSAktif").FirstOrDefault() != null ? configurations.Where(x => x.Degisken == "RezIlaveSMSAktif").FirstOrDefault().Deger.ToBoolNullSafe() : false,
                RezIlaveSMS = configurations.Where(x => x.Degisken == "RezIlaveSMS").FirstOrDefault().Deger.ToStringNullSafe(),
                KazanKazanSmsAktif = configurations.Where(x => x.Degisken == "KazanKazanSmsAktif").FirstOrDefault() != null ? configurations.Where(x => x.Degisken == "KazanKazanSmsAktif").FirstOrDefault().Deger.ToBoolNullSafe() : false,
                IyzicoRefundActive = configurations.Where(x => x.Degisken == "IyzicoRefundActive").FirstOrDefault() != null ? configurations.Where(x => x.Degisken == "IyzicoRefundActive").FirstOrDefault().Deger.ToBoolNullSafe() : false,
                CustomIpAddress = configurations.Where(x => x.Degisken == "CustomIpAddress").FirstOrDefault() != null ? configurations.Where(x => x.Degisken == "CustomIpAddress").FirstOrDefault().Deger.ToStringNullSafe() : "",
                DefaultLanguageType = configurations.Where(x => x.Degisken == "defaultLanguageID").FirstOrDefault() != null ? (LanguageTypes)configurations.Where(x => x.Degisken == "defaultLanguageID").FirstOrDefault().Deger.ToIntNullSafe() : LanguageTypes.TR,
                GetPaymentSettingsFromBroker = configurations.Where(x => x.Degisken == "GetPaymentSettingsFromBroker").FirstOrDefault() != null
                        ? configurations.Where(x => x.Degisken == "GetPaymentSettingsFromBroker").FirstOrDefault().Deger.ToBoolNullSafe() : false,
                VendorScoreActive = configurations.Where(x => x.Degisken == "VendorScoreActive").FirstOrDefault() != null ? configurations.Where(x => x.Degisken == "VendorScoreActive").FirstOrDefault().Deger.ToBoolNullSafe() : true,
                NoPriceVoucherSending = configurations.Where(x => x.Degisken == "NoPriceVoucherSending").FirstOrDefault() != null ? configurations.Where(x => x.Degisken == "NoPriceVoucherSending").FirstOrDefault().Deger.ToBoolNullSafe() : true,
                MaxAllowedAdvanceReservationDays = configurations.Where(x => x.Degisken == "MaxAllowedAdvanceReservationDays").FirstOrDefault() != null ? configurations.Where(x => x.Degisken == "MaxAllowedAdvanceReservationDays").FirstOrDefault().Deger.ToIntNullSafe() : 0,
                GetPremiumPacketsOnAvailabilityRequest = configurations.Where(x => x.Degisken == "GetPremiumPacketsOnAvailabilityRequest").FirstOrDefault() != null ? configurations.Where(x => x.Degisken == "GetPremiumPacketsOnAvailabilityRequest").FirstOrDefault().Deger.ToBoolNullSafe() : false,
                IsCancelledOnTheApiFirst = configurations.Where(x => x.Degisken == "IsCancelledOnTheApiFirst").FirstOrDefault() != null ?
                configurations.Where(x => x.Degisken == "IsCancelledOnTheApiFirst").FirstOrDefault().Deger.ToBoolNullSafe() : false,
                AutoCancel = configurations.Where(x => x.Degisken == "Otomatikİptal").FirstOrDefault() != null ?
                configurations.Where(x => x.Degisken == "Otomatikİptal").FirstOrDefault().Deger.ToBoolNullSafe() : false,
                CheckCouponActive = configurations.Where(x => x.Degisken == "CheckCouponActive").FirstOrDefault() != null ?
                configurations.Where(x => x.Degisken == "CheckCouponActive").FirstOrDefault().Deger.ToBoolNullSafe() : false,
                ShowLocationAddressOnPayment = configurations.Where(x => x.Degisken == "ShowLocationAddressOnPayment").FirstOrDefault() != null ?
                configurations.Where(x => x.Degisken == "ShowLocationAddressOnPayment").FirstOrDefault().Deger.ToBoolNullSafe() : false
            };
            return configurationDto;
        }

        public async Task<BaseAgencies> GetBaseAgencies()
        {
            var admin = await _context.Agency.Where(x => x.Roleid == (int)UserRoles.Admin).FirstOrDefaultAsync();
            var webSite = await _context.Agency.Where(x => x.Roleid == (int)UserRoles.User).FirstOrDefaultAsync();
            var mobileAPP = await _context.Agency.Where(x => x.Roleid == (int)UserRoles.MobileAPP).FirstOrDefaultAsync();

            return new BaseAgencies
            {
                Admin = admin.Map(),
                WebSite = webSite.Map(),
                MobileAPP = mobileAPP.Map()
            };
        }

        public async Task<string> GetLabel(int labelId, LanguageTypes languageType)
        {
            if (CacheSettings.UseCache)
            {
                var labels = await _cacheService.GetOrCreateAsync($"Label-{labelId}-{languageType.ToString()}", () => _context.Label.Where(x => x.Labelid == labelId && x.Dilid == ((int)languageType) + 1).FirstOrDefaultAsync(), TimeSpan.FromDays(1));
                return labels?.Labeladi.ToStringNullSafe();
            }

            var label = await _context.Label.Where(x => x.Labelid == labelId && x.Dilid == ((int)languageType) + 1).FirstOrDefaultAsync();
            return label?.Labeladi.ToStringNullSafe();
        }

        public async Task<string> GetFormContent(int contentId, LanguageTypes languageType)
        {
            var content = await _context.Icerikdil.Where(x => x.Icerikid == contentId && x.Dilid == ((int)languageType) + 1).FirstOrDefaultAsync();
            return content?.Editor;
        }

        public async Task<string> GetContentUrl(int contentId, LanguageTypes languageType)
        {
            var defaultLanguageId = (await _context.Parametre.FirstOrDefaultAsync(p => p.Degisken == "defaultLanguageID"))?.Deger?.ToIntNullSafe();
            var languageCode =
                ((int)languageType + 1) == defaultLanguageId
                ? string.Empty
                : $"{languageType}/";

            var content = await _context.Icerikdil.Where(x => x.Icerikid == contentId && x.Dilid == ((int)languageType) + 1).FirstOrDefaultAsync();
            return $"{languageCode}{content?.Contenturl}";
        }

        public async Task WriteLog(BrokerLogModel brokerLogModel)
        {
            try
            {
                if (brokerLogModel != null && !string.IsNullOrEmpty(brokerLogModel.Content))
                {
                    await _loggingDbContext.BrokerLogs.AddAsync(new Brokerlog
                    {
                        Logkey = brokerLogModel.LogKey,
                        Content = brokerLogModel.Content,
                        Logtype = (int)brokerLogModel.LogType,
                        Logtypekey = brokerLogModel.LogType.ToString()
                    });

                    await _loggingDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@WriteLogError}", ex.Message);
            }
        }

        public string GetConnectionString() => DbConnectionHelper.Instance().ConnectionString/*_appSettings.ConnectionString*/;

        public async Task<Parametre> GetConfigurationByDegisken(string degisken)
        {
            if (CacheSettings.UseCache)
            {
                var parameters = await GetConfigurationList();
                return parameters.FirstOrDefault(x => x.Degisken.ToLower() == degisken.ToLower()) ?? new Parametre();
            }
            else
            {
                var result = await _context.Parametre.Where(x => x.Degisken.ToLower() == degisken.ToLower()).FirstOrDefaultAsync();
                return result ?? new Parametre();
            }
        }
        public async Task<IEnumerable<Parametre>> GetConfigurationList()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{CacheSettings.ConfigurationKey}-ConfigurationList", _configurationRepository.GetAllAsync);

            return await _configurationRepository.GetAllAsync();
        }
    }
}
