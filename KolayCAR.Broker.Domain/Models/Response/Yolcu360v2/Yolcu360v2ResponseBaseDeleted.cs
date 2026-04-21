using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Yolcu360v2
{
    public class Yolcu360v2ResponseBaseDeleted
    {

        public class Yolcu360v2LocationResponse
        {
            public string description { get; set; }
            public string mainText { get; set; }
            public string placeId { get; set; }
            public string secondaryText { get; set; }
            public List<string> types { get; set; }
        }

        public class Point
        {
            public double lat { get; set; }
            public double lon { get; set; }
        }

        public class Yolcu360v2LocationDetailResponse
        {
            public string city { get; set; }
            public string countryCode { get; set; }
            public string name { get; set; }
            public string placeId { get; set; }
            public Point point { get; set; }
            public string timezone { get; set; }
        }
        public class Address
        {
            public string adm1 { get; set; }
            public string adm2 { get; set; }
            public string country { get; set; }
            public string street { get; set; }
        }

        public class Amount
        {
            public int amount { get; set; }
            public string currency { get; set; }
        }
        public class AppointmentReservation
        {
            public DateTime checkInDateTime { get; set; }
            public CheckOfficeReservation checkInOffice { get; set; }
            public DateTime checkOutDateTime { get; set; }
            public CheckOfficeReservation checkOutOffice { get; set; }
        }
        public class AppointmentSearch
        {
            public DateTime checkInDateTime { get; set; }
            public CheckInOffice checkInOffice { get; set; }
            public DateTime checkOutDateTime { get; set; }
            public CheckOutOffice checkOutOffice { get; set; }
        }

        public class Appointment
        {
            public DateTime checkInDateTime { get; set; }
            public CheckOffice checkInOffice { get; set; }
            public DateTime checkOutDateTime { get; set; }
            public CheckOffice checkOutOffice { get; set; }
        }

        public class Brand
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Class
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Commission
        {
            public int amount { get; set; }
            public string currency { get; set; }
        }

        public class Coordinates
        {
            public double lat { get; set; }
            public double lon { get; set; }
        }

        public class DeliveryType
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Deposit
        {
            public int amount { get; set; }
            public string currency { get; set; }
        }

        public class Fee
        {
            public int amount { get; set; }
            public string currency { get; set; }
        }

        public class Fuel
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Image
        {
            public string size { get; set; }
            public string url { get; set; }
        }

        public class Information
        {
            public string contactEmail { get; set; }
            public string contactPhone { get; set; }
            public string contactAddress { get; set; }
            public string mernisNo { get; set; }
        }

        public class Logo
        {
            public string url { get; set; }
        }

        public class Model
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Net
        {
            public int amount { get; set; }
            public string currency { get; set; }
        }

        public class OpeningHour
        {
            public string close { get; set; }
            public int dayOfWeek { get; set; }
            public string open { get; set; }
        }

        public class PaymentTotal
        {
            public int amount { get; set; }
            public string currency { get; set; }
        }

        public class Phone
        {
            public string cc { get; set; }
            public string number { get; set; }
        }

        public class PreCommissionFee
        {
            public int amount { get; set; }
            public string currency { get; set; }
        }

        public class PreDiscountTotal
        {
            public int amount { get; set; }
            public string currency { get; set; }
        }

        public class Price
        {
            public Amount amount { get; set; }
            public string charge { get; set; }
            public string description { get; set; }
            public string type { get; set; }
        }

        public class Pricing
        {
            public Commission commission { get; set; }
            public Net net { get; set; }
            public Fee fee { get; set; }
            public PaymentTotal paymentTotal { get; set; }
            public PreCommissionFee preCommissionFee { get; set; }
            public PreDiscountTotal preDiscountTotal { get; set; }
            public Total total { get; set; }
            public TotalDiscount totalDiscount { get; set; }
            public VendorTotal vendorTotal { get; set; }
            public List<Price> prices { get; set; }
        }

        public class RangeLimit
        {
            public int amount { get; set; }
            public string period { get; set; }
            public string unit { get; set; }
            public bool unlimited { get; set; }
        }

        public class RentalConditions
        {
            public string productInformationPDF { get; set; }
            public string termsConditionPDF { get; set; }
        }

        public class Yolcu360v2SearchVehicle
        {
            public string code { get; set; }
            public string integrationCode { get; set; }
            public string searchID { get; set; }
            public bool applicableForFullCredit { get; set; }
            public bool applicableForLimitCredit { get; set; }
            public AppointmentSearch appointment { get; set; }
            public Brand brand { get; set; }
            public object carRuleInformation { get; set; }
            public Class @class { get; set; }
            public string customClassName { get; set; }
            public Fuel fuel { get; set; }
            public List<Image> images { get; set; }
            public string imageURL { get; set; }
            public Model model { get; set; }
            public RentalConditions rentalConditions { get; set; }
            public List<Rule> rules { get; set; }
            public int seatCount { get; set; }
            public Segment segment { get; set; }
            public string sippCode { get; set; }
            public Transmission transmission { get; set; }
            public Yolcu360v2Vendor vendor { get; set; }
            public int rentalDurationInDays { get; set; }
            public Pricing pricing { get; set; }
            public bool isFindeksRequired { get; set; }
        }

        public class Yolcu360v2SearchResponse
        {
            public int count { get; set; }
            public List<Yolcu360v2SearchVehicle> results { get; set; }
        }

        public class Rule
        {
            public string name { get; set; }
            public string type { get; set; }
            public Deposit deposit { get; set; }
            public RangeLimit rangeLimit { get; set; }
            public int? minDriverAge { get; set; }
            public int? minLicenseYear { get; set; }
        }

        public class Segment
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Total
        {
            public int amount { get; set; }
            public string currency { get; set; }
        }

        public class Transmission
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Yolcu360v2Vendor
        {
            public string displayName { get; set; }
            public int id { get; set; }
            public Logo logo { get; set; }
            public Parent parent { get; set; }
            public string name { get; set; }
            public Information information { get; set; }
        }

        public class VendorTotal
        {
            public int amount { get; set; }
            public string currency { get; set; }
        }

        public class Yolcu360v2ExtrasResponse
        {
            public string code { get; set; }
            public int max { get; set; }
            public int min { get; set; }
            public string name { get; set; }
            public string searchID { get; set; }
            public string type { get; set; }
            public Pricing pricing { get; set; }
        }

        public class Billing
        {
            public string label { get; set; }
            public string type { get; set; }
            public string countryName { get; set; }
            public string countryCode { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string taxDivision { get; set; }
            public string taxIdentifier { get; set; }
            public string zipCode { get; set; }
            public string adm1 { get; set; }
            public string adm2 { get; set; }
            public string line { get; set; }
        }
        public class Campaign
        {
            public int id { get; set; }
            public string name { get; set; }
            public string activationCode { get; set; }
            public double merchantShare { get; set; }
        }

        public class Car
        {
            public string code { get; set; }
            public string integrationCode { get; set; }
            public string searchID { get; set; }
            public bool applicableForFullCredit { get; set; }
            public bool applicableForLimitedCredit { get; set; }
            public AppointmentReservation appointment { get; set; }
            public Brand brand { get; set; }
            public Model model { get; set; }
            public Class @class { get; set; }
            public string customClassName { get; set; }
            public Fuel fuel { get; set; }
            public FuelPolicy fuelPolicy { get; set; }
            public Transmission transmission { get; set; }
            public bool isFindeksRequired { get; set; }
            public int seatCount { get; set; }
            public int rentalDurationInDays { get; set; }
            public Pricing pricing { get; set; }
            public Yolcu360v2Vendor vendor { get; set; }
            public List<Rule> rules { get; set; }
            public RentalConditions rentalConditions { get; set; }
            public List<IncludedCoverage> includedCoverages { get; set; }
        }

        public class CheckOffice
        {
            public Address address { get; set; }
            public Coordinates coordinates { get; set; }
            public DeliveryType deliveryType { get; set; }
            public string email { get; set; }
            public string iataCode { get; set; }
            public int id { get; set; }
            public Location location { get; set; }
            public List<OpeningHour> openingHours { get; set; }
            public List<Phone> phones { get; set; }
            public Rating rating { get; set; }
        }
        public class CheckInOffice
        {
            public Address address { get; set; }
            public Coordinates coordinates { get; set; }
            public DeliveryType deliveryType { get; set; }
            public string email { get; set; }
            public string iataCode { get; set; }
            public int id { get; set; }
            public Location location { get; set; }
            public List<OpeningHour> openingHours { get; set; }
            public List<Phone> phones { get; set; }
            public Rating rating { get; set; }
        }
        public class CheckOutOffice
        {
            public Address address { get; set; }
            public Coordinates coordinates { get; set; }
            public DeliveryType deliveryType { get; set; }
            public string email { get; set; }
            public string iataCode { get; set; }
            public int id { get; set; }
            public Location location { get; set; }
            public List<OpeningHour> openingHours { get; set; }
            public List<Phone> phones { get; set; }
            public Rating rating { get; set; }
        }
        public class CheckOfficeReservation
        {
            public Address address { get; set; }
            public Coordinates coordinates { get; set; }
            public DeliveryType deliveryType { get; set; }
            public string email { get; set; }
            public string iataCode { get; set; }
            public int id { get; set; }
            public Location location { get; set; }
            public List<OpeningHour> openingHours { get; set; }
            public List<Phone> phones { get; set; }
            public Rating rating { get; set; }
        }
        public class ExtraProduct
        {
            public Campaign campaign { get; set; }
            public string code { get; set; }
            public int max { get; set; }
            public int min { get; set; }
            public string name { get; set; }
            public string searchID { get; set; }
            public string type { get; set; }
            public Pricing pricing { get; set; }
        }

        public class FuelPolicy
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class IncludedCoverage
        {
            public string key { get; set; }
            public string description { get; set; }
        }

        public class Location
        {
            public string placeId { get; set; }
            public string name { get; set; }
            public string city { get; set; }
            public string countryCode { get; set; }
            public Point point { get; set; }
            public string timezone { get; set; }
        }

        public class OrderedCarProduct
        {
            public int id { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public DateTime earlyCheckoutDate { get; set; }
            public bool isFullCredit { get; set; }
            public bool isLimitedCredit { get; set; }
            public string orderID { get; set; }
            public int paymentID { get; set; }
            public int quantity { get; set; }
            public int replacedByID { get; set; }
            public string status { get; set; }
            public bool vendorCancelled { get; set; }
            public string vendorReservationID { get; set; }
            public Car car { get; set; }
        }

        public class OrderedExtraProduct
        {
            public int id { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public DateTime earlyCheckoutDate { get; set; }
            public bool isFullCredit { get; set; }
            public bool isLimitedCredit { get; set; }
            public string orderID { get; set; }
            public int paymentID { get; set; }
            public int quantity { get; set; }
            public int replacedByID { get; set; }
            public string status { get; set; }
            public bool vendorCancelled { get; set; }
            public string vendorReservationID { get; set; }
            public ExtraProduct extraProduct { get; set; }
        }

        public class Parent
        {
        }

        public class Passenger
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string nationality { get; set; }
            public string phone { get; set; }
            public string identityNumber { get; set; }
            public string passportNo { get; set; }
            public string birthDate { get; set; }
        }
        public class Rating
        {
            public int participants { get; set; }
            public float point { get; set; }
        }

        public class Yolcu360v2PostReservationResponse
        {
            public string id { get; set; }
            public Billing billing { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public string language { get; set; }
            public Passenger passenger { get; set; }
            public string paymentCurrency { get; set; }
            public string paymentType { get; set; }
            public OrderedCarProduct orderedCarProduct { get; set; }
            public List<OrderedExtraProduct> orderedExtraProducts { get; set; }
        }
        public class Yolcu360v2PostPayResponse
        {
            public PaymentBilling billing { get; set; }
            public DateTime createdAt { get; set; }
            public string id { get; set; }
            public string language { get; set; }
            public PaymentPassenger passenger { get; set; }
            public string paymentCurrency { get; set; }
            public string paymentType { get; set; }
            public DateTime updatedAt { get; set; }
            public OrderedCarPaymentProduct orderedCarProduct { get; set; }
        }
        public class TotalDiscount
        {
            public int amount { get; set; }
            public string currency { get; set; }
        }
        public class Yolcu360v2CheckCancellableResponse
        {
            public bool cancellable { get; set; }
            public bool refundable { get; set; }
        }

        public class Yolcu360v2PostCancelReservationResponse
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string orderId { get; set; }
            public string status { get; set; }
        }

        public class PaymentBilling
        {
            public string adm1 { get; set; }
            public string adm2 { get; set; }
            public object adm3 { get; set; }
            public string countryCode { get; set; }
            public string countryName { get; set; }
            public string email { get; set; }
            public string firstName { get; set; }
            public int id { get; set; }
            public string label { get; set; }
            public string language { get; set; }
            public object lastName { get; set; }
            public string line { get; set; }
            public string phone { get; set; }
            public int registeredAddressID { get; set; }
            public string taxDivision { get; set; }
            public string taxIdentifier { get; set; }
            public string type { get; set; }
            public string zipCode { get; set; }
        }
        public class PaymentCar
        {
            public string code { get; set; }
            public string integrationCode { get; set; }
            public string searchID { get; set; }
            public bool applicableForFullCredit { get; set; }
            public bool applicableForLimitCredit { get; set; }
            public Appointment appointment { get; set; }
            public Brand brand { get; set; }
            public object carRuleInformation { get; set; }
            public Class @class { get; set; }
            public string customClassName { get; set; }
            public Fuel fuel { get; set; }
            public List<Image> images { get; set; }
            public Model model { get; set; }
            public List<Rule> rules { get; set; }
            public int seatCount { get; set; }
            public Segment segment { get; set; }
            public string sippCode { get; set; }
            public Transmission transmission { get; set; }
            public Yolcu360v2PayVendor vendor { get; set; }
            public int rentalDurationInDays { get; set; }
            public PaymentPricing pricing { get; set; }
            public bool isFindeksRequired { get; set; }
        }


        public class OrderedCarPaymentProduct
        {
            public int id { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public string orderID { get; set; }
            public int paymentID { get; set; }
            public int quantity { get; set; }
            public string status { get; set; }
            public bool vendorCancelled { get; set; }
            public string vendorReservationID { get; set; }
            public PaymentCar car { get; set; }
        }

        public class PaymentPassenger
        {
            public string birthDate { get; set; }
            public string email { get; set; }
            public string firstName { get; set; }
            public object identityNumber { get; set; }
            public string lastName { get; set; }
            public string nationality { get; set; }
            public string passportNo { get; set; }
            public string phone { get; set; }
        }


        public class PaymentPricing
        {
            public Commission commission { get; set; }
            public Net net { get; set; }
            public Fee fee { get; set; }
            public PaymentTotal paymentTotal { get; set; }
            public PreCommissionFee preCommissionFee { get; set; }
            public PreDiscountTotal preDiscountTotal { get; set; }
            public Total total { get; set; }
            public VendorTotal vendorTotal { get; set; }
            public List<Price> prices { get; set; }
        }

        public class Yolcu360v2PayVendor
        {
            public string displayName { get; set; }
            public int id { get; set; }
            public Logo logo { get; set; }
            public string name { get; set; }
            public Information information { get; set; }
        }

        public enum Yolcu360v2CurrencyTypes
        {
            TRY,
            USD,
            EUR
        }
    }
}
