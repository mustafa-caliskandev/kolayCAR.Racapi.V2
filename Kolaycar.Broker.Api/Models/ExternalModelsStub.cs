using System;

namespace KolayCAR.Broker.API.Models
{
    // Stubs to satisfy compilation since the models were from an external missing project
    public class UserDetailModel
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string PaymentType { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentCard { get; set; }
        public int InstallmentCount { get; set; }
        public decimal LateCharge { get; set; }
        public bool ContactPermission { get; set; }
    }

    public class CouponModel
    {
        public string CouponCode { get; set; }
        public string CouponName { get; set; }
        public float UsedAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal CouponAmount { get; set; }
    }

    public class ErrorDto
    {
        public string Message { get; set; }
        public int ErrorCode { get; set; }
        public string ReservationToken { get; set; }
        public string PaymentCode { get; set; }
        public string SystemMessage { get; set; }
        public string ErrorStage { get; set; }
        public string ErrorType { get; set; }
    }

    public class VehicleFilterDto { }
}
