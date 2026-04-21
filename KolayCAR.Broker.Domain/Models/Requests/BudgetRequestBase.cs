using System;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.AvisResponseBase;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class BudgetRequestBase
    {
        public class AvisAuthRequest
        {
            public string username { get; set; }
            public string grant_type { get; set; }
            public string password { get; set; }
        }
        public class AvisAvailableVehicleRequest : BaseRequestClass
        {
            public string StartOfficeMnemoic { get; set; }
            public string StartDateTime { get; set; }
            public string EndOfficeMnemoic { get; set; }
            public string EndDateTime { get; set; }
            public string DiscountNo { get; set; }
            public string RateCode { get; set; }
            public int CorporateCustomerNo { get; set; }
            public int DriverCustomerNo { get; set; }
            public int AdditinoalHour { get; set; }
            public bool IsMobile { get; set; }
            public bool IsMotorcycle { get; set; }
            public string ClientIp { get; set; }
            public object VehicleClassCode { get; set; }
            public int LogUserId { get; set; }
        }

        public class AvisLocationRequest : BaseRequestClass
        {

        }
        public class AvisVehicleListRequest : BaseRequestClass
        {
        }
        public class BaseRequestClass
        {
            public string Brand { get; set; }
            public string CountryCode { get; set; }
        }
        public class IsActiveFindeksReportExists : FindeksBaseRequest
        {

        }
        public class FindeksPhoneIdListRequest : FindeksBaseRequest
        {

        }
        public class FindeksBaseRequest
        {
            public string Tckn { get; set; }
            public int LicenseNo { get; set; }
        }
        public class FindeksPinConfirmRequest
        {
            public int RequestId { get; set; }
            public string BirthYear { get; set; }
            public string PinCode { get; set; }
            public int LicenseNo { get; set; }

        }
        public class FindeksPinRenewRequest
        {
            public int RequestId { get; set; }
            public int LicenseNo { get; set; }
        }
        public class FindeksReportRequest
        {
            public string Tckn { get; set; }
            public DateTime BirthDate { get; set; }
            public DateTime DriverLicenseDate { get; set; }
            public string PhoneNo { get; set; }
            public string PhoneId { get; set; }
            public int LicenseNo { get; set; }
        }


        public class AdditionalProduct
        {
            public int ProductNo { get; set; }
            public int ProductCount { get; set; }
            public double ProductAmount { get; set; }
        }

        public class Address
        {
            public string address_line_1 { get; set; }
            public string address_line_2 { get; set; }
            public object address_line_3 { get; set; }
            public string city { get; set; }
            public string state_name { get; set; }
            public string postal_code { get; set; }
            public string country_code { get; set; }
        }

        public class ArrivalFlight
        {
            public string airline_code { get; set; }
            public string airline_number { get; set; }
        }

        public class Contact
        {
            public string title { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string telephone { get; set; }
            public string email { get; set; }
            public int age { get; set; }
            public string date_of_birth { get; set; }
        }

        public class Driver
        {
            public string license_number { get; set; }
            public object state_code { get; set; }
            public object country_code { get; set; }
        }
        public class Local
        {
            public string TransmissionType { get; set; }
            public float TotalPrice { get; set; }
            public string Remark { get; set; }
            public string DriverTaxNumber { get; set; }
            public string RateCodeId { get; set; }
            public int CompanyCustomerNo { get; set; }
            public float PaymentAmount { get; set; }
            public float DiscountAmount { get; set; }

        }
        public class Local2
        {
            public int DriverCity { get; set; }
            public int DriverDistrict { get; set; }
            public string TransmissionType { get; set; }
            public string VehicleCategory { get; set; }
            public string VehClassSize { get; set; }
            public float TotalPrice { get; set; }
            public double PAIAmount { get; set; }
            public double SLIAmount { get; set; }
            public double MIAmount { get; set; }
            public double LIAmount { get; set; }
            public double LCFAAmount { get; set; }
            public double SMIAmount { get; set; }
            public double OneWayAmount { get; set; }
            public string NameOnCreditCard { get; set; }
            public string CreditCardNo { get; set; }
            public string CreditCardExpirationDate { get; set; }
            public double CreditCardPayment { get; set; }
            public string Remark { get; set; }
            public string DriverTaxNumber { get; set; }
            public string DriverPassportNo { get; set; }
            public int AdditinoalHour { get; set; }
            public string RateCodeId { get; set; }
            public int DriverCustomerNo { get; set; }
            public int CompanyCustomerNo { get; set; }
            public bool IsFastDelivery { get; set; }
            public double PaymentAmount { get; set; }
            public int PaymentAmountType { get; set; }
            public List<AdditionalProduct> additionalProducts { get; set; }
            public string CompanyName { get; set; }
            public object CompanyTitle { get; set; }
            public int CompanyTaxOfficeCity { get; set; }
            public int CompanyTaxOfficeDistrict { get; set; }
            public int CompanyInvoiceCity { get; set; }
            public int CompanyInvoiceDistrict { get; set; }
            public object CompanyInvoiceAddress { get; set; }
            public object CompanyMobilePhone { get; set; }
            public object CompanyWorkPhone { get; set; }
            public object CompanyEmail { get; set; }
            public double DeliveryFee { get; set; }
            public double CollectionFee { get; set; }
            public string DeliveryAddress { get; set; }
            public int DeliveryDistrict { get; set; }
            public int DeliveryCity { get; set; }
            public string CollectionAddress { get; set; }
            public int CollectionDistrict { get; set; }
            public int CollectionCity { get; set; }
            public string ReservationNumber { get; set; }
            public double DiscountAmount { get; set; }
            public double CancelFee { get; set; }
            public double FullDayService { get; set; }
            public bool IsMobileApp { get; set; }
        }

        public class Loyalty
        {
        }

        public class Passenger
        {
            public Contact contact { get; set; }
            public Address address { get; set; }
            public Driver driver { get; set; }
        }


        public class Rate
        {
            public string rate_code { get; set; }
            public string country_code { get; set; }
            public Discount discount { get; set; }
            public Loyalty loyalty { get; set; }
            public object membership { get; set; }
            public object coupon { get; set; }
        }

        public class RateTotals
        {
            public Rate rate { get; set; }
        }

        public class Reservation
        {
            public string country_code { get; set; }
            public string discount_code { get; set; }
            public string pickup_date { get; set; }
            public string pickup_location { get; set; }
            public string dropoff_date { get; set; }
            public string dropoff_location { get; set; }
            public string vehicle_class_code { get; set; }
            public bool email_notification { get; set; }
        }

        public class BudgetPostReservationRequest
        {
            public Tempest tempest { get; set; }
            public Local local { get; set; }
            public string CountryCode { get; set; }
            public string Brand { get; set; }
            public int LogUserId { get; set; }
        }
        public class AvisCancelReservationRequest
        {
            public string CountryCode { get; set; }
            public string Brand { get; set; }
            public string ReservationNo { get; set; }
            public string LastName { get; set; }
            public double ReturnAmount { get; set; }
            public string TransactionId { get; set; }
            public bool IsRefund { get; set; }
            public int LogUserId { get; set; }
            public double ExternalRefund { get; set; }
        }
        public class Tempest
        {
            public Product product { get; set; }
            public Transaction transaction { get; set; }
            public Reservation reservation { get; set; }
            public RateTotals rate_totals { get; set; }
            public Passenger passenger { get; set; }
            public object insurance { get; set; }
            public object extras { get; set; }
            public ArrivalFlight arrival_flight { get; set; }
        }

    }
}
