using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.KolayCAR
{
    public static class SettingsMapper
    {
        //TODO: Kolaycar > serviceporvider yazılınca buradaki vendor parametresi kaldırılacak.
        public static Settings Map(this KolayCARResponseBase settings, Vendor vendor) =>
            settings != null ? new Settings
            {
                Active = settings.ISACTIVE,
                AgencyName = settings.AGENTNAME,
                AgencyMailAddress = settings.AGENTMAILADDRESS,
                ParentMailAddress = settings.PARENTMAILADDRESS,
                AgencyAddress = settings.AGENTADDRESS,
                AgencyTelephone = settings.AGENTTELEPHONE,
                AgencyGsm = settings.AGENTGSM,
                AgencyLogoUrl = settings.AGENTLOGO,
                AgencyAuthorizedPersonName = settings.AGENTAUTHORIZED,
                AgencyServiceActive = settings.AGENTSERVICEACTIVE,
                DefaultVendorId = settings.DEFAULTVENDORID,
                DefaultDomainId = settings.DEFAULTDOMAINID,
                DefaultVendorLogoUrl = settings.DEFAULTVENDORLOGO,
                DefaultVendorWhatsAppTelephone = settings.DEFAULTVENDORWHATSAPPTELEPHONE,
                DefaultVendorRentalContract = settings.DEFAULTVENDORRENTCONTRACT,
                DefaultVendorSpecialArea = settings.DEFAULTVENDORSPECIALAREA1,
                DefaultVendorSpecialArea2 = settings.DEFAULTVENDORSPECIALAREA2,
                DefaultVendorSpecialArea3 = settings.DEFAULTVENDORSPECIALAREA3,
                AgencyDetailLogActive = settings.AGENTDETAILLOGACTIVE,
                AgencyCommissionType = (AgencyCommissionTypes)settings.AGENCYCOMMISSIONTYPE,
                AgencyReservationEmail = settings.AGENCYRESEMAIL,
                Languages = settings.LANGUAGES.Map(),
                Currencies = settings.CURRENCY.Map(),
                AgencyRentalFeeType = (FeeTypes)settings.AGENCYRENTALFEETYPE,
                AgencyExtrasFeeType = (FeeTypes)settings.AGENCYEXTRASFEETYPE,
                AgencyOneWayFeeType = (FeeTypes)settings.AGENCYONEWAYFEETYPE,
                RequestTimeOutTime = settings.SQLSERVERTIMEOUT,
                LoadBalanceServer = settings.LOADBALANCESERVER,
                APICounterValue = settings.APICOUNTERVALUE,
                CustomerMailSending = settings.CUSTOMERMAILSENDING,
                CustomerSMSSending = settings.CUSTOMERSMSSENDING,
                ReservationSourceTypes = settings.RESSOURCETYPE.Map(),
                ServiceCharge = vendor.ServiceCharge,
                ServiceChargeCurrencyType = vendor.ServiceChargeCurrencyType
            }
            : null;

        public static List<Language> Map(this List<SETTINGS_LANGUAGE> languages)
        {
            var _languages = new List<Language>();

            if (languages != null && languages.Count != 0)
                foreach (var language in languages)
                    _languages.Add(new Language
                    {
                        LanguageId = language.LANGUAGEID,
                        LanguageISOCode = language.LANGUAGEISOCODE,
                        IsDefaultLanguage = language.DEFAULTLANGUAGE
                    });

            return _languages;
        }

        public static List<Currency> Map(this List<SETTINGS_CURRENCY> currencies)
        {
            var _currencies = new List<Currency>();

            if (currencies != null && currencies.Count != 0)
                foreach (var currency in currencies)
                    _currencies.Add(new Currency
                    {
                        CurrencyId = currency.CURRENCYID,
                        CurrencyISOCode = currency.CURRENCYISOCODE,
                        IsDefaultCurrency = currency.DEFAULTCURRENCY
                    });

            return _currencies;
        }

        public static List<ReservationSourceType> Map(this List<RESSOURCETYPES> reservationSourceTypes)
        {
            var _reservationSourceTypes = new List<ReservationSourceType>();

            if (reservationSourceTypes != null && reservationSourceTypes.Count != 0)
                foreach (var reservationSourceType in reservationSourceTypes)
                    _reservationSourceTypes.Add(new ReservationSourceType
                    {
                        ReservationSourceId = reservationSourceType.RESSOURCEID,
                        ReservationSource = reservationSourceType.RESSOURCE
                    });

            return _reservationSourceTypes;
        }

        public static Settings Map(this KOLAYCARSETTINGS settings)
        {
            return settings != null ? new Settings
            {
                FullCredit = settings.FULLCREDITACTIVE
            } : null;
        }
    }
}
