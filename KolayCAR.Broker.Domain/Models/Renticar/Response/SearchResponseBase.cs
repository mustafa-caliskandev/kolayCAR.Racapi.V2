using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Renticar.Response
{
    public class SearchResponseBase
    {
        public string status { get; set; }
        public string message { get; set; }
        public Search search { get; set; }
        public List<Offer> offers { get; set; }
    }
    public class Car
    {
        public string carId { get; set; }
        public int seat { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public string @class { get; set; }
        public string segment { get; set; }
        public string size { get; set; }
        public string body { get; set; }
        public string transmission { get; set; }
        public string fuel { get; set; }
        public string fuelPolicy { get; set; }
    }

    public class Offer
    {
        public string offerId { get; set; }
        public string companySlug { get; set; }
        public int totalDays { get; set; }
        public PickupOffice pickupOffice { get; set; }
        public ReturnOffice returnOffice { get; set; }
        public Car car { get; set; }
        public Rules rules { get; set; }
        public Pricing pricing { get; set; }
        public DateTime createdAt { get; set; }
        public bool? isFullCreditAvailable { get; set; }
    }

    public class PickupOffice
    {
        public string name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string telephone { get; set; }
        public string deliveryType { get; set; }
    }

    public class Pricing
    {
        public TotalPrice totalPrice { get; set; }
        public DailyPrice dailyPrice { get; set; }
        public double vendorDay { get; set; }
        public DropPrice dropPrice { get; set; }
        public Provision provision { get; set; }
    }

    public class ReturnOffice
    {
        public string name { get; set; }
        public string address { get; set; }
        public string telephone { get; set; }
        public string deliveryType { get; set; }
    }

    public class Rules
    {
        public int? dailyRangeLimit { get; set; }
        public int totalRangeLimit { get; set; }
        public int driverAge { get; set; }
        public bool doubleCreditCard { get; set; }
        public int licenseYears { get; set; }
    }
}
