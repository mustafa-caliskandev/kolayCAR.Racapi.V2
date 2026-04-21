using KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos;
using KolayCAR.Broker.Domain.Models;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models.MobileAppModels
{
    public class MobileReservationSuccessModel
    {
        public ReservationSuccess ReservationSuccess { get; set; }
        public VehiclePickupInformationSuccess VehiclePickupInformation { get; set; }
        public VehicleDetailSuccess VehicleDetails { get; set; }
        public RentalConditionsSuccess RentalConditions { get; set; }
        public VendorDetailsSuccess VendorDetails { get; set; }
        public DriverInformationSuccess DriverInformation { get; set; }
        public PriceInformationSuccess PriceInformation { get; set; }
        public List<PopupDocumentsSuccess> PopupDocuments { get; set; }
        public LocationInfo LocationInfo { get; set; }
        public string RequestId { get; set; }
    }

    public class PopupDocumentsSuccess
    {
        public string Title { get; set; }
        public string Text { get; set; }
    }

    public class PriceInformationSuccess
    {
        public string CardTitle { get; set; }
        public string CurrencyCode { get; set; }
        public string TotalPriceTitle { get; set; }
        public float TotalPrice { get; set; }
        public float DailyPrice { get; set; }
        public CouponInfo CouponInfo { get; set; }
        public PaymentInfo CardPaymentInfo { get; set; }
        public PaymentInfo OfficePaymentInfo { get; set; }
        public List<Extra> Extras { get; set; }
    }

    public class CouponInfo
    {
        public string CouponCode { get; set; }
        public float CouponDiscountAmount { get; set; }
    }

    public class PaymentInfo
    {
        public float TotalAmount { get; set; }
        public List<PaymentDetail> Details { get; set; }
    }

    public class PaymentDetail
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public float Amount { get; set; }
    }

    public class DriverInformationSuccess
    {
        public string CardTitle { get; set; }
        public string DriverName { get; set; }
        public string DriverSurname { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Id { get; set; }
    }

    public class VendorDetailsSuccess
    {
        public string CardTitle { get; set; }
        public int VendorId { get; set; }
        public VendorOfficeLocation OfficeLocation { get; set; }
        public string VendorPhone { get; set; }
        public string VendorEmail { get; set; }
        public string VendorName { get; set; }
        public string VendorLogo { get; set; }
        public float VendorScore { get; set; }
        public int VendorCommentCount { get; set; }
        public string DeliveryTypeName { get; set; }
        public List<VendorDetailForSuccess> Details { get; set; }
    }

    public class VendorDetailForSuccess
    {
        public string Parameter { get; set; }
        public string Text { get; set; }
        public string IconPath { get; set; }
    }

    public class RentalConditionsSuccess
    {
        public string CardTitle { get; set; }
        public List<RentalConditionSuccess> RentalConditions { get; set; }
    }

    public class RentalConditionSuccess
    {
        public string Text { get; set; }
        public string IconPath { get; set; }
    }

    public class VehicleDetailSuccess
    {
        public string CardTitle { get; set; }
        public string VehicleName { get; set; }
        public string VehicleTypeName { get; set; }
        public string VehicleImage { get; set; }
        public string GearType { get; set; }
        public string FuelType { get; set; }
        public int RentalDuration { get; set; }
        public string VehicleModelName { get; set; }
        public string VehicleBrandName { get; set; }
        public string VehicleCategoryTypeName { get; set; }
        public List<VehicleFeature> VehicleFeatures { get; set; }
    }

    public class VehiclePickupInformationSuccess
    {
        public string CardTitle { get; set; }
        public RezInfo PickupInfo { get; set; }
        public RezInfo ReturnInfo { get; set; }
    }

    public class RezInfo
    {
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string GoogleMapsLink { get; set; }
    }

    public class ReservationSuccess
    {
        public string SuccessMessage { get; set; }
        public ReservationNumber ReservationNumber { get; set; }
        public string ReferanceNumber { get; set; }
        public string DetailsText { get; set; }
    }

    public class ReservationNumber
    {
        public string Title { get; set; }
        public string Number { get; set; }
    }
}
