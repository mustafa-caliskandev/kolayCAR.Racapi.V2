using KolayCAR.Broker.Domain.Models;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models.MobileAppDtos.ReservationDtos
{
    public class ReservationDto
    {
        public ReservationDetails ReservationDetails { get; set; }
        public PriceDetails PriceDetails { get; set; }
        public List<RentalCondition> RentalConditions { get; set; }
        public VendorDetails VendorDetails { get; set; }
        public CustomerDetails CustomerDetails { get; set; }
    }

    public class CustomerDetails
    {
        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerMail { get; set; }
        public string CustomerIdentityNumber { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerArrivalFlightNumber { get; set; }
        public string CustomerReturnFlightNumber { get; set; }
        public string CustomerNote { get; set; }
    }

    public class VendorDetails
    {
        public int VendorId { get; set; }
        public string VendorPhone { get; set; }
        public string VendorEmail { get; set; }
        public string VendorName { get; set; }
        public string VendorLogo { get; set; }
    }

    public class PriceDetails
    {
        public float DailyPrice { get; set; }
        public float ExtraPrice { get; set; }
        public float OneWayFee { get; set; }
        public float TotalPrice { get; set; }
        public float PaidAmount { get; set; }
    }

    public class ReservationDetails
    {
        public string ReservationNumber { get; set; }
        public long ReservationId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string VehicleName { get; set; }
        public string VehicleImageUrl { get; set; }
        public string VehicleBrandName { get; set; }
        public string VehicleModelName { get; set; }
        public string PickupLocationName { get; set; }
        public string ReturnLocationName { get; set; }
        public int RentalDuration { get; set; }
        public int? TotalKMLimit { get; set; }
        public ReservationStatusTypes ReservationStatusType { get; set; }
        public VehicleCategoryTypes VehicleCategoryType { get; set; }
        public string VehicleCategoryTypeName { get; set; }
    }

}
