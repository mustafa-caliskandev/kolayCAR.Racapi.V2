using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Yolcu360v2;

public class Yolcu360v2VehicleResponseBase
{
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

    public class Appointment
    {
        public DateTime checkInDateTime { get; set; }
        public CheckInOffice checkInOffice { get; set; }
        public DateTime checkOutDateTime { get; set; }
        public CheckOutOffice checkOutOffice { get; set; }
    }

    public class Brand
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class CheckInOffice
    {
        public Address address { get; set; }
        public Coordinates coordinates { get; set; }
        public DeliveryType deliveryType { get; set; }
        public string email { get; set; }
        public string iataCode { get; set; }
        public int id { get; set; }
        public List<OpeningHour> openingHours { get; set; }
        public List<string> phones { get; set; }
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
        public List<OpeningHour> openingHours { get; set; }
        public List<string> phones { get; set; }
        public Rating rating { get; set; }
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

    public class Rating
    {
        public int participants { get; set; }
        public double point { get; set; }
    }

    public class RentalConditions
    {
        public string termsConditionPDF { get; set; }
    }

    public class Yolcu360v2Vehicle
    {
        public string code { get; set; }
        public string integrationCode { get; set; }
        public string searchID { get; set; }
        public bool applicableForFullCredit { get; set; }
        public bool applicableForLimitCredit { get; set; }
        public Appointment appointment { get; set; }
        public Brand brand { get; set; }
        public object carRuleInformation { get; set; }
        [JsonProperty("class")]
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
        public Vendor vendor { get; set; }
        public int rentalDurationInDays { get; set; }
        public Pricing pricing { get; set; }
        public bool isFindeksRequired { get; set; }
    }

    public class Root
    {
        public int count { get; set; }
        public List<Yolcu360v2Vehicle> results { get; set; }
    }

    public class Rule
    {
        public string name { get; set; }
        public string type { get; set; }
        public Deposit deposit { get; set; }
        public RangeLimit rangeLimit { get; set; }
        public int? minDriverAge { get; set; }
        public int? minLicenseYear { get; set; }
        public int? minYoungDriverAge { get; set; }
        public int? minYoungDriverLicenseYear { get; set; }
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

    public class Vendor
    {
        public string displayName { get; set; }
        public int id { get; set; }
        public Logo logo { get; set; }
        public string name { get; set; }
        public Information information { get; set; }
    }

    public class VendorTotal
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }
}
