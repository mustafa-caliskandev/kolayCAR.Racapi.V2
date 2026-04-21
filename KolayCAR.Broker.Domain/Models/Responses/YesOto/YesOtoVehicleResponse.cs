using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Responses.YesOto
{
    public class YesOtoVehicleData
    {
        public string id { get; set; }
        public YesOtoVehicleGroup vehicleGroup { get; set; }
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
        public string name { get; set; }
        public string image { get; set; }
        public string vehicleGroupClass { get; set; }
        public string gearType { get; set; }
        public int adultCapacity { get; set; }
        public int suiteCaseCapacityTotal { get; set; }
    }

    public class YesOtoVehicleListData
    {
        public List<YesOtoVehicleData> vehicles { get; set; }
    }

    public class YesOtoVehicleListResponse
    {
        public YesOtoVehicleListData data { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }
}
