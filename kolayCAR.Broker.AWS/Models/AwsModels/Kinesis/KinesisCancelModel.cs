using System;

namespace kolayCAR.Broker.AWS.Models.AwsModels.Kinesis
{
    public class KinesisCancelModel
    {
        public string DeviceType { get; set; }
        public KinesisStatus Status { get; set; }
        public string ReservationToken { get; set; }
        public string PaymentCode { get; set; }

        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }

        public string PickupLocationName { get; set; }
        public bool IsPickupAirport { get; set; }

        public string ReturnLocationName { get; set; }
        public bool IsReturnAirport { get; set; }

        public int PickupLocationId { get; set; }
        public int ReturnLocationId { get; set; }
        public int RentalDuration { get; set; }

        public string VendorName { get; set; }

        public string VehicleBrandName { get; set; }
        public string VehicleModelName { get; set; }
        public string VehicleCategoryTypeName { get; set; }
        public string FuelTypeName { get; set; }
        public string TransmissionTypeName { get; set; }
        public int PassangerQuantityType { get; set; }
        public string PassangerQuantityTypeName { get; set; }
        public int BaggageQuantityType { get; set; }
        public string BaggageQuantityTypeName { get; set; }
        public string VehicleTypeName { get; set; }
        public float? DailyPrice { get; set; }
        public float? TotalPrice { get; set; }
        public float? DepositPrice { get; set; }
        public float? TotalKMLimit { get; set; }
        public float? OneWayFee { get; set; }
        public int? VendorMinimumDriverAge { get; set; }
        public int? VendorMinimumDrivingLicenseAge { get; set; }

        public string AllExtras { get; set; }
        public string SelectedExtras { get; set; }

        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public bool ContactPermission { get; set; }

        public decimal VendorCommission { get; set; }
        public decimal ObCommission { get; set; }

        public string CouponCode { get; set; }
        public string CouponName { get; set; }
        public string CouponAmount { get; set; }
        public string CouponPaymentType { get; set; }

        public string PaymentMethod { get; set; }
        public string PaymentCard { get; set; }
        public string PaymentType { get; set; }
        public int InstallmentCount { get; set; }
        public decimal LateCharge { get; set; }

        public int CancelReasonId { get; set; }
        public string CancelReason { get; set; }
    }
}
