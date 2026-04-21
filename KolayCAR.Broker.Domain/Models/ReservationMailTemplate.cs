namespace KolayCAR.Broker.Domain.Models
{
    public class ReservationMailTemplate
    {
        public ReservationMailSendToTypes ReservationMailSendToType { get; set; }
        public Reservation Reservation { get; set; }
        public Agency Agency { get; set; }
        public Vendor Vendor { get; set; }
        public CouponCode CouponCode { get; set; }
        public string ReservationStatusName { get; set; }
        public string ReservationStatusMessage { get; set; }
        public string ReservationDetailUrlTemplate { get; set; }
        public string PortalOwnerTitle { get; set; }
        public string PortalOwnerAddress { get; set; }
        public string PortalOwnerPhone { get; set; }
        public string PortalOwnerFax { get; set; }
        public string PortalOwnerEmail { get; set; }
        public string PortalOwnerDomain { get; set; }
        public string PortalOwnerLogo { get; set; }
        public string IsOfficeTrueLabel { get; set; }
        public string IsOfficeFalseLabel { get; set; }
        public string EmergencyPhone { get; set; }
        public string DocumentTitle { get; set; }
        public string FuelName { get; set; }
        public string TransmissionName { get; set; }
        public string AgencyMessage { get; set; }
        public string DepositMessage { get; set; }
        public string AdditionsTitleLabel { get; set; }
        public Location PickupLocation { get; set; }
        public Location ReturnLocation { get; set; }
        public string ReservationDetailPageContentUrl { get; set; }
        public string CancellationPenaltyInformationEmailTitle { get; set; }
        public string ReservationDetailContentUrl { get; set; }
        public string ReservationVendorContractUrl { get; set; }
        public float VendorLocationFee { get; set; }
        public string FullCreditInformation { get; set; }
        public bool VendorShowLogo { get; set; }
        public LanguageTypes DefaultLanguageType { get; set; }
        public string ReservationSuccessUrl { get; set; }
    }
}
