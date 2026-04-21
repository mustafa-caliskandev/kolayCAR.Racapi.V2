using System;
using static KolayCAR.Broker.Domain.Models.Response.AvisResponseBase;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class FindeksRequestBase
    {
        public class GetIsActiveFindeksReportRequest : RequestBase
        {
            public string ReservationToken { get; set; }

        }
        public class GetPhoneIdListRequest : RequestBase
        {

        }
        public class IsVehicleSuitableForCustomer
        {
            public string Tckn { get; set; }
            public string ReservationToken { get; set; }
            public FindeksReportExist FindeksReportExist { get; set; }

        }
        public class GetFindeksReportRequest : RequestBase
        {
            public DateTime BirthDate { get; set; }
            public DateTime DriverLicenseDate { get; set; }
            public string PhoneNo { get; set; }
            public string PhoneId { get; set; }
        }
        public class CreatePinRenewRequest : RequestDeepBase
        {
            public int RequestId { get; set; }
        }
        public class ConfirmPinRequest : RequestDeepBase
        {
            public int RequestId { get; set; }
            public string BirthYear { get; set; }
            public string PinCode { get; set; }
        }
        public class RequestBase : RequestDeepBase
        {
            public string Tckn { get; set; }
        }
        public class RequestDeepBase
        {
            public int VendorId { get; set; }
            public int? LicenseNo { get; set; }
        }

        public class FindeksCheck
        {
            public string BirthDate { get; set; }
            public string DriverLicenseDate { get; set; }
            public string Tckn { get; set; }
            public string ReservationToken { get; set; }
            public int VendorId { get; set; }
            public string LanguageCode { get; set; }
        }
    }
}
