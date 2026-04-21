using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models
{
    public class Settings
    {
        public bool Active { get; set; }
        public string AgencyName { get; set; }
        public string AgencyMailAddress { get; set; }
        public string ParentMailAddress { get; set; }
        public string AgencyAddress { get; set; }
        public string AgencyTelephone { get; set; }
        public string AgencyGsm { get; set; }
        public string AgencyLogoUrl { get; set; }
        public string AgencyAuthorizedPersonName { get; set; }
        public bool AgencyServiceActive { get; set; }
        public int DefaultVendorId { get; set; }
        public int DefaultDomainId { get; set; }
        public string DefaultVendorLogoUrl { get; set; }
        public string DefaultVendorWhatsAppTelephone { get; set; }
        public string DefaultVendorRentalContract { get; set; }
        public string DefaultVendorSpecialArea { get; set; }
        public string DefaultVendorSpecialArea2 { get; set; }
        public string DefaultVendorSpecialArea3 { get; set; }
        public bool AgencyDetailLogActive { get; set; }
        public AgencyCommissionTypes AgencyCommissionType { get; set; }
        public string AgencyReservationEmail { get; set; }
        public List<Language> Languages { get; set; }
        public List<Currency> Currencies { get; set; }
        public FeeTypes AgencyRentalFeeType { get; set; }
        public FeeTypes AgencyExtrasFeeType { get; set; }
        public FeeTypes AgencyOneWayFeeType { get; set; }
        public int RequestTimeOutTime { get; set; }
        public string LoadBalanceServer { get; set; }
        public string APICounterValue { get; set; }
        public string CustomerMailSending { get; set; }
        public string CustomerSMSSending { get; set; }
        public List<ReservationSourceType> ReservationSourceTypes { get; set; }
        public float ServiceCharge { get; set; }
        public CurrencyTypes ServiceChargeCurrencyType { get; set; }
        public Vendor Vendor { get; set; }
        public List<ReservationSource> ReservationSources { get; set; }
        public bool FullCredit { get; set; }
    }

    public class Language
    {
        public int LanguageId { get; set; }
        public string LanguageISOCode { get; set; }
        public bool IsDefaultLanguage { get; set; }
    }

    public class Currency
    {
        public int CurrencyId { get; set; }
        public string CurrencyISOCode { get; set; }
        public bool IsDefaultCurrency { get; set; }
    }

    public enum FeeTypes
    {
        ToCustomer,
        ToAPIOwner,
        All
    }

    public class ReservationSourceType
    {
        public int ReservationSourceId { get; set; }
        public string ReservationSource { get; set; }
    }
}
