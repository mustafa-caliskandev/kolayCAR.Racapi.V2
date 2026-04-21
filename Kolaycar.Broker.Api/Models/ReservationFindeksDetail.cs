using System;

namespace KolayCAR.Broker.API.Models
{
    public class ReservationFindeksDetail
    {
        public int Id { get; set; }
        public string IdentityNumber { get; set; }
        public string ReservationToken { get; set; }
        public int VendorId { get; set; }
        public DateTime ReportDate { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime DriverLicenseDate { get; set; }
        public string RequestId { get; set; } = null;
        public FindeksSteps StepName { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string PhoneNo { get; set; } = null;
        public string PhoneId { get; set; } = null;
        public bool isRequiredYoungDriverPacked { get; set; }
        public bool isSuitableForCustomer { get; set; }
    }

    public enum FindeksSteps
    {
        IsActiveFindeksReportExists,
        getPhoneList,
        reportRequest,
        confirmPin,
        pinRenew,
        isVehicleSuitableForCustomer
    }
}
