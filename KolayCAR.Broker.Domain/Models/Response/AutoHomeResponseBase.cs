using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response
{
    public class AutoHomeResponseBase
    {
        public class Location
        {
            public string code { get; set; }
            public string name { get; set; }
            public string MondayStartTime { get; set; }
            public string MondayEndTime { get; set; }
            public string TuesdayStartTime { get; set; }
            public string TuesdayEndTime { get; set; }
            public string WednesdayStartTime { get; set; }
            public string WednesdayEndTime { get; set; }
            public string ThursdayStartTime { get; set; }
            public string ThursdayEndTime { get; set; }
            public string FridayStartTime { get; set; }
            public string FridayEndTime { get; set; }
            public string SaturdayStartTime { get; set; }
            public string SaturdayEndTime { get; set; }
            public string SundayStartTime { get; set; }
            public string SundayEndTime { get; set; }
            public string PhoneNo { get; set; }
            public string Email { get; set; }
            public string City { get; set; }
            public string Address1 { get; set; }
            public string havaalani_lokasyonu { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
        }


        public class DiscountedDailyPriceCurrency
        {
            public string TRY { get; set; }
            public string USD { get; set; }
            public string EUR { get; set; }
        }

        public class DropPriceCurrency
        {
            public string TRY { get; set; }
            public string USD { get; set; }
            public string EUR { get; set; }
        }

        public class Vehicle
        {
            public int GroupId { get; set; }
            public int SubGroupId { get; set; }
            public string SubGroupName { get; set; }
            public string SubGroupShortName { get; set; }
            public int DriverMinAge { get; set; }
            public int DriverMinLicenceYear { get; set; }
            public int YoungDriverMinAge { get; set; }
            public int YoungDriverMinLicenceYear { get; set; }
            public string MakeName { get; set; }
            public string ModelName { get; set; }
            public string Segment { get; set; }
            public string Transmission { get; set; }
            public string FuelType { get; set; }
            public string ProvisionAmountTRY { get; set; }
            public string DailyKmLimit { get; set; }
            public int? TotalKmLimit { get; set; }
            public string CarClass { get; set; }
            public string LokasyonFarkMesaji { get; set; }
            public string DropPrice { get; set; }
            public string BaggageCapacity { get; set; }
            public string PassengerCapacity { get; set; }
            public int? Days { get; set; }
            public int? DiscountedDays { get; set; }
            public float WebPaymentPrice { get; set; }
            public string WebPaymentNonDiscountedPrice { get; set; }
            public string DiscountedDailyPrice { get; set; }
            public int? CampaignId { get; set; }
            public object MainRulesId { get; set; }
            public object AdditionalWebCampaignID { get; set; }
            public object AdditionalWebDiscountDay { get; set; }
            public string AdditionalWebdiscountedDailyPrice { get; set; }
            public string AdditionalGrandTotal { get; set; }
            public float GrandTotal { get; set; }
            public object IsBolBol { get; set; }
            public object OverKmPrice { get; set; }
            public object Contractual { get; set; }
            public object AnaGrupAd { get; set; }
            public object WebName { get; set; }
            public DiscountedDailyPriceCurrency DiscountedDailyPriceCurrency { get; set; }
            public DropPriceCurrency DropPriceCurrency { get; set; }
            public List<Extra> Extras { get; set; }
        }
        public class Extra
        {
            public string ProductCode { get; set; }
            public string ProductName { get; set; }
            public string Description { get; set; }
            public string Amount { get; set; }
            public string TotalAmount { get; set; }
            public string WebOnlineSelling { get; set; }
            public object IsFree { get; set; }
            public AmountCurrencyTotal AmountCurrencyTotal { get; set; }
            public AmountCurrency AmountCurrency { get; set; }
        }
        public class AmountCurrency
        {
            public string TRY { get; set; }
            public string USD { get; set; }
            public string EUR { get; set; }
        }

        public class AmountCurrencyTotal
        {
            public string TRY { get; set; }
            public string USD { get; set; }
            public string EUR { get; set; }
        }
        public class ListExtra
        {
            public string Id { get; set; }
            public string Description { get; set; }
            public string DescriptionLong { get; set; }
            public object GroupName { get; set; }
            public string Price { get; set; }
            public string Status { get; set; }
        }
        public class Reservation
        {
            public string cevap { get; set; }
            public string mesaj { get; set; }
            public string ReservationId { get; set; }
        }

        public class ReservationCancel
        {
            public string cevap { get; set; }
            public string mesaj { get; set; }
        }
    }
}
