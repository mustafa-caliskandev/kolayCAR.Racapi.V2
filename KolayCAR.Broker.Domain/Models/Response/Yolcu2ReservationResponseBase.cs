using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class Yolcu3602LocationResponseBase
    {
        public class GetLocationResponse
        {
            public int count { get; set; }
            public int total { get; set; }
            public List<Data> data { get; set; }

        }
        public class Data
        {
            public int id { get; set; }
            public string name { get; set; }
            public City city { get; set; }
        }
        public class City
        {
            public string timezone { get; set; }
            public string country { get; set; }
            public int id { get; set; }
            public string name { get; set; }
        }
    }
    public class Yolcu3602CarListingAgencyResponseBase
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class DeliveryType
        {
            public int count { get; set; }
            public string value { get; set; }
        }

        public class Vendor
        {
            public int count { get; set; }
            public string display { get; set; }
            public string value { get; set; }
            public bool supportsFullCredit { get; set; }
            public string logoUrl { get; set; }
            public int id { get; set; }
            public string name { get; set; }
        }

        public class ClassType
        {
            public int count { get; set; }
            public string value { get; set; }
        }

        public class Transmission
        {
            public int count { get; set; }
            public string value { get; set; }
        }

        public class Price
        {
            public int maximumValue { get; set; }
            public int minimumValue { get; set; }
        }

        public class FullCredit
        {
            public int count { get; set; }
            public bool value { get; set; }
        }

        public class AverageRating
        {
            public double maximumValue { get; set; }
            public double minimumValue { get; set; }
        }

        public class Fuel
        {
            public int count { get; set; }
            public string value { get; set; }
        }

        public class Model
        {
            public int count { get; set; }
            public string value { get; set; }
        }

        public class DistanceLimit
        {
            public int maximumValue { get; set; }
            public int minimumValue { get; set; }
        }

        public class Provision
        {
            public int maximumValue { get; set; }
            public int minimumValue { get; set; }
        }

        public class Brand
        {
            public int count { get; set; }
            public string value { get; set; }
            public string name { get; set; }
        }

        public class TotalStatistics
        {
            public List<DeliveryType> delivery_type { get; set; }
            public List<Vendor> vendor { get; set; }
            public List<ClassType> class_type { get; set; }
            public List<Transmission> transmission { get; set; }
            public Price price { get; set; }
            public List<FullCredit> full_credit { get; set; }
            public AverageRating average_rating { get; set; }
            public List<Fuel> fuel { get; set; }
            public List<Model> model { get; set; }
            public DistanceLimit distance_limit { get; set; }
            public Provision provision { get; set; }
            public List<Brand> brand { get; set; }
        }

        public class SearchStatistics
        {
            public List<DeliveryType> delivery_type { get; set; }
            public List<Vendor> vendor { get; set; }
            public List<ClassType> class_type { get; set; }
            public List<Transmission> transmission { get; set; }
            public Price price { get; set; }
            public List<FullCredit> full_credit { get; set; }
            public AverageRating average_rating { get; set; }
            public List<Fuel> fuel { get; set; }
            public List<Model> model { get; set; }
            public DistanceLimit distance_limit { get; set; }
            public Provision provision { get; set; }
            public List<Brand> brand { get; set; }
        }

        public class RentalPeriod
        {
            public int count { get; set; }
            public string unit { get; set; }
        }

        public class Phone
        {
            public string number { get; set; }
            public string country { get; set; }
            public int? dialCode { get; set; }
        }

        public class Address
        {
            public string city { get; set; }
            public string line { get; set; }
            public string country { get; set; }
        }

        public class Monday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Tuesday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Friday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Wednesday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Thursday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Sunday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Saturday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class OpeningHours
        {
            public Monday monday { get; set; }
            public Tuesday tuesday { get; set; }
            public Friday friday { get; set; }
            public Wednesday wednesday { get; set; }
            public Thursday thursday { get; set; }
            public Sunday sunday { get; set; }
            public Saturday saturday { get; set; }
        }

        public class Office
        {
            public List<Phone> phones { get; set; }
            public string deliveryType { get; set; }
            public int id { get; set; }
            public Address address { get; set; }
            public OpeningHours openingHours { get; set; }
            public string email { get; set; }
        }

        public class Rules
        {
            public int dailyRangeLimit { get; set; }
            public int totalRangeLimit { get; set; }
            public int driverAge { get; set; }
            public bool doubleCreditCard { get; set; }
            public int licenseYears { get; set; }
        }

        public class Image
        {
            public string small { get; set; }
            public string large { get; set; }
            public string medium { get; set; }
        }

        public class Car
        {
            public string name { get; set; }
            public int bigBagCount { get; set; }
            public int smallBagCount { get; set; }
            public string transmission { get; set; }
            public Brand brand { get; set; }
            public string @class { get; set; }
            public int seats { get; set; }
            public string fuel { get; set; }
            public Image image { get; set; }
        }

        public class Pricing
        {
            public double totalPrice { get; set; }
            public double priceToCharge { get; set; }
            public double agencyRequestedCommissionType { get; set; }
            public double rentalPriceDueAtPickup { get; set; }
            public double preDiscountPrice { get; set; }
            public double finalPrice { get; set; }
            public double agencyCommission { get; set; }
            public double oneWayPrice { get; set; }
            public double discount { get; set; }
            public double agencyPrice { get; set; }
            public double promotionalDiscount { get; set; }
            public double agencyBasePrice { get; set; }
            public double provision { get; set; }
            public double listPrice { get; set; }
            public double agencyRequestedCommissionAmount { get; set; }
            public double? deliveryFee { get; set; }
        }

        public class Appointment
        {
            public string dropoffLocationSlug { get; set; }
            public DateTime dropoffDatetime { get; set; }
            public int dropoffLocationId { get; set; }
            public int pickupLocationId { get; set; }
            public DateTime pickupDatetime { get; set; }
            public string pickupLocationSlug { get; set; }
        }

        public class DropoffLocation
        {
            public List<Phone> phones { get; set; }
            public string deliveryType { get; set; }
            public int id { get; set; }
            public Address address { get; set; }
            public OpeningHours openingHours { get; set; }
            public string email { get; set; }
        }

        public class Insurance
        {
            public string includedInsuranceType { get; set; }
        }

        public class Data
        {
            public RentalPeriod rentalPeriod { get; set; }
            public int totalDays { get; set; }
            public Vendor vendor { get; set; }
            public string listingId { get; set; }
            public Office office { get; set; }
            public Rules rules { get; set; }
            public Car car { get; set; }
            public string currency { get; set; }
            public Pricing pricing { get; set; }
            public string trackingId { get; set; }
            public Appointment appointment { get; set; }
            public DropoffLocation dropoffLocation { get; set; }
            public Insurance insurance { get; set; }
        }

        public class CarListingAgencyResponse
        {
            public int count { get; set; }
            public TotalStatistics totalStatistics { get; set; }
            public string carRentalSearchId { get; set; }
            public SearchStatistics searchStatistics { get; set; }
            public int offset { get; set; }
            public int total { get; set; }
            public List<Data> data { get; set; }
            public string categoryId { get; set; }
            public string errorCode { get; set; }
            public string errorMessage { get; set; }
        }


    }
    public class Yolcu3602ReservationResponseBase
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class User
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string identityNumber { get; set; }
            public string birthDate { get; set; }
            public int purchaseCount { get; set; }
        }

        public class Phone
        {
            public string number { get; set; }
            public string country { get; set; }
        }

        public class Address
        {
            public string line { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public int zip { get; set; }
        }

        public class ContactInformation
        {
            public Phone phone { get; set; }
            public Address address { get; set; }
        }

        public class Renter
        {
            public User user { get; set; }
            public ContactInformation contactInformation { get; set; }
        }

        public class PersonDetails
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public Address address { get; set; }
        }

        public class CompanyDetails
        {
            public string taxDivision { get; set; }
            public string taxId { get; set; }
            public string name { get; set; }
            public Address address { get; set; }
        }

        public class Billing
        {
            public string billingType { get; set; }
            public CompanyDetails companyDetails { get; set; }
            public PersonDetails personDetails { get; set; }
        }

        public class Vendor
        {
            public string name { get; set; }
            public string logoUrl { get; set; }
        }

        public class Points
        {
            public string aspect { get; set; }
            public int value { get; set; }
        }

        public class Rating
        {
            public int participants { get; set; }
            public Points points { get; set; }
        }

        public class Phone2
        {
            public string number { get; set; }
            public string country { get; set; }
        }

        public class Office
        {
            public int id { get; set; }
            public Rating rating { get; set; }
            public Address address { get; set; }
            public List<Phone> phones { get; set; }
            public string email { get; set; }
            public bool meetAndGreet { get; set; }
            public string deliveryType { get; set; }
        }

        public class Appointment
        {
            public int pickupLocationId { get; set; }
            public string pickupLocationSlug { get; set; }
            public int dropoffLocationId { get; set; }
            public string dropoffLocationSlug { get; set; }
            public DateTime pickupDatetime { get; set; }
            public DateTime dropoffDatetime { get; set; }
        }

        public class Brand
        {
            public string name { get; set; }
        }

        public class Image
        {
            public string small { get; set; }
            public string medium { get; set; }
            public string large { get; set; }
        }

        public class Car
        {
            public Brand brand { get; set; }
            public string name { get; set; }
            public Image image { get; set; }
            public string fuel { get; set; }
            public string transmission { get; set; }
            public string @class { get; set; }
            public int seats { get; set; }
            public int bigBagCount { get; set; }
            public int smallBagCount { get; set; }
        }

        public class Rules
        {
            public int driverAge { get; set; }
            public int licenseYears { get; set; }
            public int driverAgeWithYoungDriver { get; set; }
            public int licenseYearsWithYoungDriver { get; set; }
            public int dailyRangeLimit { get; set; }
            public int totalRangeLimit { get; set; }
            public bool doubleCreditCard { get; set; }
        }

        public class Pricing
        {
            public int provision { get; set; }
            public int discount { get; set; }
            public int preDiscountPrice { get; set; }
            public int listPrice { get; set; }
            public int promotionalDiscount { get; set; }
            public int outOfOfficeFee { get; set; }
            public int oneWayPrice { get; set; }
            public int deliveryFee { get; set; }
            public int totalPrice { get; set; }
            public int finalPrice { get; set; }
            public int priceToCharge { get; set; }
            public int rentalPriceDueAtPickup { get; set; }
            public int agencyPrice { get; set; }
            public int agencyBasePrice { get; set; }
            public int agencyCommission { get; set; }
        }

        public class Insurance
        {
            public string includedInsuranceType { get; set; }
        }

        public class Promotion
        {
            public string promotionType { get; set; }
            public int fixedDiscount { get; set; }
            public string fixedDiscountCurrency { get; set; }
            public int percentageDiscount { get; set; }
            public string shortDescription { get; set; }
        }

        public class Campaign
        {
            public int id { get; set; }
        }

        public class Currency
        {
        }

        public class RentalPeriod
        {
            public string unit { get; set; }
            public int count { get; set; }
        }

        public class Listing
        {
            public Vendor vendor { get; set; }
            public Office office { get; set; }
            public Appointment appointment { get; set; }
            public Car car { get; set; }
            public Rules rules { get; set; }
            public Pricing pricing { get; set; }
            public Insurance insurance { get; set; }
            public int totalDays { get; set; }
            public string listingId { get; set; }
            public string trackingId { get; set; }
            public List<Promotion> promotions { get; set; }
            public Campaign campaign { get; set; }
            public Rating rating { get; set; }
            public Currency currency { get; set; }
            public RentalPeriod rentalPeriod { get; set; }
        }

        public class Product
        {
            public string productType { get; set; }
            public string label { get; set; }
            public int price { get; set; }
            public int maxAmount { get; set; }
            public string damageInsuranceType { get; set; }
            public string damageInsuranceCategory { get; set; }
            public int extraRangeAmount { get; set; }
        }

        public class OrderedProduct
        {
            public int count { get; set; }
            public Product product { get; set; }
        }

        public class Rating3
        {
            public string aspect { get; set; }
            public int value { get; set; }
        }

        public class Review
        {
            public string text { get; set; }
            public List<Rating> ratings { get; set; }
            public string author { get; set; }
            public int created { get; set; }
        }

        public class Card
        {
        }

        public class PaymentDetails
        {
            public Card card { get; set; }
            public Vendor vendor { get; set; }
        }

        public class City
        {
            public int id { get; set; }
            public string name { get; set; }
            public string timezone { get; set; }
            public string country { get; set; }
        }

        public class PickupLocation
        {
            public int id { get; set; }
            public string name { get; set; }
            public City city { get; set; }
        }

        public class DropoffLocation
        {
            public int id { get; set; }
            public string name { get; set; }
            public City city { get; set; }
        }

        public class Agency
        {
            public string name { get; set; }
            public string logoUrl { get; set; }
        }

        public class Related
        {
            public PickupLocation pickupLocation { get; set; }
            public DropoffLocation dropoffLocation { get; set; }
            public Agency agency { get; set; }
            public bool isFullCredit { get; set; }
        }

        public class CarRentalOrderResponse
        {
            public string id { get; set; }
            public float created { get; set; }
            public string vendorReservationId { get; set; }
            public Renter renter { get; set; }
            public Billing billing { get; set; }
            public Listing listing { get; set; }
            public List<OrderedProduct> orderedProducts { get; set; }
            public Review review { get; set; }
            public PaymentDetails paymentDetails { get; set; }
            public bool cancelled { get; set; }
            public bool canBeReviewed { get; set; }
            public bool isFullCredit { get; set; }
            public Related related { get; set; }
            public bool threeds { get; set; }
            public string url { get; set; }
        }


    }
    public class Yolcu2ReservationResponseBase
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Address
        {
            public string city { get; set; }
            public string line { get; set; }
            public int zip { get; set; }
            public string country { get; set; }
        }

        public class CompanyDetails
        {
            public string taxDivision { get; set; }
            public string taxId { get; set; }
            public string name { get; set; }
            public Address address { get; set; }
        }

        public class Billing
        {
            public string billingType { get; set; }
            public CompanyDetails companyDetails { get; set; }
        }

        public class Agency
        {
            public bool corporate { get; set; }
            public string name { get; set; }
        }

        public class City
        {
            public string timezone { get; set; }
            public string country { get; set; }
            public int id { get; set; }
            public string name { get; set; }
        }

        public class DropoffLocation
        {
            public City city { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public List<Phone> phones { get; set; }
            public string deliveryType { get; set; }
            public Address address { get; set; }
            public OpeningHours openingHours { get; set; }
            public string email { get; set; }
        }

        public class PickupLocation
        {
            public City city { get; set; }
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Related
        {
            public Agency agency { get; set; }
            public DropoffLocation dropoffLocation { get; set; }
            public PickupLocation pickupLocation { get; set; }
        }

        public class Installment
        {
            public int count { get; set; }
            public int surcharge { get; set; }
        }

        public class Yolcu
        {
            public string currency { get; set; }
            public int price { get; set; }
            public string type { get; set; }
            public string description { get; set; }
            public Installment installment { get; set; }
        }

        public class Card
        {
            public string currency { get; set; }
            public int price { get; set; }
            public string type { get; set; }
            public string description { get; set; }
            public Installment installment { get; set; }
        }

        public class PaymentDetails
        {
            public List<Yolcu> yolcu { get; set; }
            public bool isCreditPayment { get; set; }
            public List<Card> card { get; set; }
        }

        public class Phone
        {
            public string country { get; set; }
            public int dialCode { get; set; }
            public string number { get; set; }
        }

        public class ContactInformation
        {
            public Phone phone { get; set; }
            public Address address { get; set; }
        }

        public class User
        {
            public string lastName { get; set; }
            public string identityNumber { get; set; }
            public string firstName { get; set; }
            public string birthDate { get; set; }
            public string email { get; set; }
        }

        public class Renter
        {
            public ContactInformation contactInformation { get; set; }
            public User user { get; set; }
        }

        public class RentalPeriod
        {
            public int count { get; set; }
            public string unit { get; set; }
        }

        public class Vendor
        {
            public bool supportsFullCredit { get; set; }
            public string logoUrl { get; set; }
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Phone2
        {
            public string country { get; set; }
            public int dialCode { get; set; }
            public string number { get; set; }
        }

        public class Monday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Tuesday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Friday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Wednesday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Thursday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Sunday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class Saturday
        {
            public string close { get; set; }
            public string open { get; set; }
        }

        public class OpeningHours
        {
            public Monday monday { get; set; }
            public Tuesday tuesday { get; set; }
            public Friday friday { get; set; }
            public Wednesday wednesday { get; set; }
            public Thursday thursday { get; set; }
            public Sunday sunday { get; set; }
            public Saturday saturday { get; set; }
        }

        public class Office
        {
            public List<Phone> phones { get; set; }
            public string deliveryType { get; set; }
            public int id { get; set; }
            public Address address { get; set; }
            public OpeningHours openingHours { get; set; }
            public string email { get; set; }
        }

        public class Rules
        {
            public int dailyRangeLimit { get; set; }
            public int totalRangeLimit { get; set; }
            public int driverAge { get; set; }
            public bool doubleCreditCard { get; set; }
            public int licenseYears { get; set; }
        }

        public class Brand
        {
            public string name { get; set; }
        }

        public class Image
        {
            public string small { get; set; }
            public string large { get; set; }
            public string medium { get; set; }
        }

        public class Car
        {
            public string name { get; set; }
            public int bigBagCount { get; set; }
            public int smallBagCount { get; set; }
            public string transmission { get; set; }
            public Brand brand { get; set; }
            public string @class { get; set; }
            public int seats { get; set; }
            public string fuel { get; set; }
            public Image image { get; set; }
        }

        public class Pricing
        {
            public int totalPrice { get; set; }
            public int priceToCharge { get; set; }
            public int agencyBasePrice { get; set; }
            public int rentalPriceDueAtPickup { get; set; }
            public int finalPrice { get; set; }
            public int agencyCommission { get; set; }
            public int agencyRequestedCommissionType { get; set; }
            public int promotionalDiscount { get; set; }
            public int agencyPrice { get; set; }
            public int provision { get; set; }
            public int listPrice { get; set; }
            public int agencyRequestedCommissionAmount { get; set; }
        }

        public class Appointment
        {
            public string dropoffLocationSlug { get; set; }
            public DateTime dropoffDatetime { get; set; }
            public int dropoffLocationId { get; set; }
            public int pickupLocationId { get; set; }
            public DateTime pickupDatetime { get; set; }
            public string pickupLocationSlug { get; set; }
        }

        public class Insurance
        {
            public string includedInsuranceType { get; set; }
        }

        public class Listing
        {
            public RentalPeriod rentalPeriod { get; set; }
            public int totalDays { get; set; }
            public Vendor vendor { get; set; }
            public string listingId { get; set; }
            public Office office { get; set; }
            public Rules rules { get; set; }
            public Car car { get; set; }
            public string currency { get; set; }
            public Pricing pricing { get; set; }
            public string trackingId { get; set; }
            public Appointment appointment { get; set; }
            public DropoffLocation dropoffLocation { get; set; }
            public Insurance insurance { get; set; }
        }

        public class Root
        {
            public Billing billing { get; set; }
            public long created { get; set; }
            public string vendorReservationId { get; set; }
            public Related related { get; set; }
            public bool canBeReviewed { get; set; }
            public bool isFullCredit { get; set; }
            public PaymentDetails paymentDetails { get; set; }
            public List<object> orderedProducts { get; set; }
            public Renter renter { get; set; }
            public Listing listing { get; set; }
            public bool cancelled { get; set; }
            public string id { get; set; }
            public string errorCode { get; set; }
            public string errorMessage { get; set; }
        }


    }
    public class Yolcu3602ProductResponseBase
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Data
        {
            public string productType { get; set; }
            public string label { get; set; }
            public float price { get; set; }
            public int? maxAmount { get; set; }
            public string damageInsuranceType { get; set; }
            public string damageInsuranceCategory { get; set; }
            public int? extraRangeAmount { get; set; }
        }

        public class CarRentalExtraProduct
        {
            public List<Data> data { get; set; }
        }
    }
    public class Yolcu3602CancelReservationResponseBase
    {
        public class RentalOrderCancellationResponse
        {
            public bool cancelled { get; set; }
            public int refundAmount { get; set; }
            public string errorCode { get; set; }
            public string errorMessage { get; set; }
        }
    }
    public enum YolcuProductsTypes2
    {
        additionalDriver, gpsNavigation, childSeat, youngDriver, babySeat, extraRange, seatAdapter, snowChain, portBaggage, winterTires, wifiModem, damageInsurance, roofRack, privateDriver
    }
}
