using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests.YesOto
{
    public class YesOtoCreateReservationRequest
    {
        public string BrandId { get; set; }
        public string SalesChannelId { get; set; }
        public string LanguageId { get; set; }
        public YesOtoReservationRequestModel ReservationRequestModel { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool EmailWotcharSend { get; set; }
        public string BirthDate { get; set; }
        public string DrivingLicenseTakingDate { get; set; }
        public string TCKN { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string CityName { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string Gender { get; set; }
        public YesOtoBillingInformation BillingInformation { get; set; }
        public object ReservationAdditionalServiceAddress { get; set; }
        public string FlightNumber { get; set; }
        public bool CommunicationConfirmation { get; set; }
        public bool RentalAgreement { get; set; }
        public string Id { get; set; }
        public bool Post { get; set; }
        public string AnadolujetCode { get; set; }
        public string ApprovalNumber { get; set; }
        public string Channel { get; set; }
        public string GiftCoupon { get; set; }
        public bool LocationPay { get; set; }
        public object MailPriceInfo { get; set; }
        public string RedirectUrl { get; set; }
        public bool TakeADiscounts { get; set; }
        public string TransactionNumber { get; set; }
        public object UsedPointBalance { get; set; }
        public bool UsedPointState { get; set; }
        public string ClientChannel { get; set; }
    }

    public class YesOtoReservationRequestModel
    {
        public string BrandId { get; set; }
        public string SalesChannelId { get; set; }
        public string LanguageId { get; set; }
        public string Location { get; set; }
        public string DropOffLocation { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Age { get; set; }
        public string SelectedVehicleGroup { get; set; }
        public List<YesOtoSelectedAdditionalService> SelectedAdditionalServices { get; set; }
        public string UserCampaignId { get; set; }
        public bool IsUserFirstReservation { get; set; }
        public float DiscPrice { get; set; }
        public string GiftCoupon { get; set; }
        public bool LocationPay { get; set; }
        public string ProcessType { get; set; }
        public string PromotionToken { get; set; }
        public string RequestType { get; set; }
        public bool UsedPointState { get; set; }
        public string ZubizuSaleId { get; set; }
        public object priceMatrix { get; set; }
    }

    public class YesOtoSelectedAdditionalService
    {
        public string id { get; set; }
        public int count { get; set; }
    }

    public class YesOtoBillingInformation
    {
        public string Address { get; set; }
        public string BillingType { get; set; }
        public string FullName { get; set; }
        public string TaxNumber { get; set; }
        public string TaxOffice { get; set; }
    }

    public class YesOtoCancelReservationRequest
    {
        public string ApprovalNumber { get; set; }
        public string Email { get; set; }
        public string LanguageId { get; set; }
        public string BrandId { get; set; }
        public string ReservationCancellationReason { get; set; }
        public decimal RefundAmount { get; set; }
    }

    public class YesOtoReservationCancellationInformationRequest
    {
        public string ApprovalNumber { get; set; }
        public string Email { get; set; }
        public string LanguageId { get; set; }
        public string BrandId { get; set; }
    }

    public class YesOtoSetReservationStatusRequest
    {
        public string ApprovalNumber { get; set; }
    }
}
