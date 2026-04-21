namespace KolayCAR.Broker.Domain.Models
{
    public class Configurations
    {
        public string SMTPServer { get; set; }
        public int SMTPPort { get; set; }
        public bool SMTPSSL { get; set; }
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string AdministratorEmail { get; set; }
        public bool SendInvoiceToCustomerActive { get; set; }
        public bool SendInvoiceToAgencyActive { get; set; }
        public bool SendReservationMailToCustomerActive { get; set; }
        public ExternalInvoiceType ExternalInvoiceType { get; set; }
        public ExternalLoginType ExternalLoginType { get; set; }
        public int ReservationMailTemplateId { get; set; }
        public int ReservationStatusNameLabelId { get; set; }
        public int ReservationStatusMessageLabelId { get; set; }
        public string ReservationDetailUrlTemplate { get; set; }
        public string PortalOwnerTitle { get; set; }
        public string PortalOwnerAddress { get; set; }
        public string PortalOwnerPhone { get; set; }
        public string PortalOwnerFax { get; set; }
        public string PortalOwnerEmail { get; set; }
        public string PortalOwnerDomain { get; set; }
        public string PortalOwnerLogoPath { get; set; }
        public int NearestRentalTime { get; set; }
        public string EmergencyPhone { get; set; }
        public string KolayCARPaymentAPIKey { get; set; }
        public string KolayCARPaymentAPIPassword { get; set; }
        public int KolayCARPaymentAPIVendorId { get; set; }
        public bool OnlyAvailableVehicles { get; set; }
        public bool AdministratorEmailSending { get; set; }
        public bool LocationEmailSending { get; set; }
        public string DefaultCustomerMailAddress { get; set; }
        public bool SendSmsToCustomer { get; set; }
        public int KolayCARPaymentAPIDomainId { get; set; }
        public string KolayCARRentwsAPIKey { get; set; }
        public string KolayCARRentwsAPIPassword { get; set; }
        public int KolayCARBankVendorId { get; set; }
        public bool PaymentRefundActive { get; set; }
        public bool PenaltyInformationEmailActive { get; set; }
        public bool TimeoutLog { get; set; }
        public bool NewSetting { get; set; }
        //public bool SpecialExtrasIsActive { get; set; }

        public bool RezIlaveSMSAktif { get; set; }

        public string RezIlaveSMS { get; set; }
        public bool KazanKazanSmsAktif { get; set; }
        public bool IyzicoRefundActive { get; set; }
        public string CustomIpAddress { get; set; }
        public LanguageTypes DefaultLanguageType { get; set; }
        public bool GetPaymentSettingsFromBroker { get; set; }
        public bool VendorScoreActive { get; set; }
        public bool NoPriceVoucherSending { get; set; }
        public int MaxAllowedAdvanceReservationDays { get; set; }
        public bool GetPremiumPacketsOnAvailabilityRequest { get; set; }
        public bool IsCancelledOnTheApiFirst { get; set; }
        public bool AutoCancel { get; set; }
        public bool CheckCouponActive { get; set; }
        public bool ShowLocationAddressOnPayment { get; set; }
    }

    public enum ExternalInvoiceType
    {
        None,
        Netresys,
        KolayCAR
    }

    public enum ExternalLoginType
    {
        None,
        Cockpit
    }
}
