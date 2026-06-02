using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Responses.YesOto
{
    public class YesOtoVehicleData
    {
        public string id { get; set; }
        public YesOtoVehicleGroup vehicleGroup { get; set; }
        public object gearType { get; set; }
        public object fuelType { get; set; }
        public object vehicleGroupClass { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int adultCapacity { get; set; }
        public int ageGroupMin { get; set; }
        public bool isDailyMilageLimit { get; set; }
        public int dailyKilometerLimit { get; set; }
        public float pricePerKilometer { get; set; }
        public int largeSuiteCaseCapacity { get; set; }
        public int smallSuitCaseCapacity { get; set; }
        public int suiteCaseCapacityTotal { get; set; }
        public string imageRight { get; set; }
        public string imageLeft { get; set; }
        public bool hasCampaign { get; set; }
        public bool showPrice { get; set; }
        public bool showTop { get; set; }
        public int drivingLicenseAge { get; set; }
        public float pricePerDay { get; set; }
        public float discountedPricePerDay { get; set; }
        public float coEmmissionAmount { get; set; }
        public float totalPrice { get; set; }
        public float totalDiscountedPrice { get; set; }
        public float location_TotalDiscountedPrice { get; set; }
        public float oneDirectionPrice { get; set; }
        public float discountedOneDirectionPrice { get; set; }
        public float grandTotal { get; set; }
        public float discountedGrandTotal { get; set; }
        public float location_DiscountedGrandTotal { get; set; }
        public float blockedAmountForCreditCard { get; set; }
        public float insuranceExemptionFee { get; set; }
        public int numberOfCreditCard { get; set; }
        public bool firstReservationRental { get; set; }
        public int rentalDayCount { get; set; }
        public bool isAdditionalServiceFreeDayAdd { get; set; }
        public int freeRentalDay { get; set; }
        public string priceRuleId { get; set; }
    }

    public class YesOtoVehicleGroup
    {
        public string id { get; set; }
        public string text { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public object vehicleGroupClass { get; set; }
        public object gearType { get; set; }
        public object fuelType { get; set; }
        public int adultCapacity { get; set; }
        public int largeSuiteCaseCapacity { get; set; }
        public int smallSuitCaseCapacity { get; set; }
        public int suiteCaseCapacityTotal { get; set; }
    }

    public class YesOtoAdditionalService
    {
        public string id { get; set; }
        public string name { get; set; }
        public string text { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public float price { get; set; }
        public float discountedPrice { get; set; }
        public float totalPrice { get; set; }
        public float discountedTotalPrice { get; set; }
        public float grandTotal { get; set; }
        public float discountedGrandTotal { get; set; }
        public float pricePerDay { get; set; }
        public float discountedPricePerDay { get; set; }
        public float dailyPrice { get; set; }
        public float discountedDailyPrice { get; set; }
        public int count { get; set; }
        public int maxCount { get; set; }
        public bool isRequired { get; set; }
        public bool isQuantityIncreasable { get; set; }
        public bool quantityIncreasable { get; set; }
        public bool? isDaily { get; set; }
        public bool? perDay { get; set; }
        public object serviceType { get; set; }
        public object priceCalculationType { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class YesOtoVehicleListData
    {
        public List<YesOtoVehicleData> vehicles { get; set; }
        public YesOtoVehicleData selectedVehicle { get; set; }
        public List<YesOtoAdditionalService> additionalServices { get; set; }
        public int rentalDayCount { get; set; }
    }

    public class YesOtoVehicleListResponse
    {
        public YesOtoVehicleListData data { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }
}
