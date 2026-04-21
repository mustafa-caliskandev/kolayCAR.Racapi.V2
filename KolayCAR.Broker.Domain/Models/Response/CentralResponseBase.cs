namespace KolayCAR.Broker.Domain.Models.Response
{
    public class CentralResponseBase
    {
        public class CentralReservation
        {
            public string cevap { get; set; }
            public string mesaj { get; set; }
            public string ReservationId { get; set; }
        }

        public class CentralCancelReservation
        {
            public string cevap { get; set; }
            public string mesaj { get; set; }
        }

        public class CentralSummary
        {
            public string OptionSCDW { get; set; }
            public string OptionSuperSCDW { get; set; }
            public string OptionExtraDriver { get; set; }
            public string OptionYoungDriverInsurance { get; set; }
            public string OptionChildSeat { get; set; }
            public int OptionChildSeatCount { get; set; }
            public string OptionBabySeat { get; set; }
            public int OptionBabySeatCount { get; set; }
            public string OptionNavigation { get; set; }
            public string OptionWinterTire { get; set; }
            public string ExtraDriverServiceFee { get; set; }
            public int InsuranceServiceFee { get; set; }
            public int OptionalEquipmentServiceFee { get; set; }
            public string Success { get; set; }
        }

        public class CentralVehicle
        {
            public string GroupId { get; set; }
            public string SubGroupId { get; set; }
            public string SubGroupName { get; set; }
            public string SubGroupShortName { get; set; }
            public string DriverMinAge { get; set; }
            public string DriverMinLicenceYear { get; set; }
            public string YoungDriverMinAge { get; set; }
            public string YoungDriverMinLicenceYear { get; set; }
            public string MakeName { get; set; }
            public string ModelName { get; set; }
            public string Segment { get; set; }
            public string Transmission { get; set; }
            public string FuelType { get; set; }
            public string ProvisionAmountTRY { get; set; }
            public string DailyKmLimit { get; set; }
            public string TotalKmLimit { get; set; }
            public string CarClass { get; set; }
            public string LokasyonFarkMesaji { get; set; }
            public double? DropPrice { get; set; }
            public string BaggageCapacity { get; set; }
            public string PassengerCapacity { get; set; }
            public int? Days { get; set; }
            public string DiscountedDays { get; set; }
            public double? WebPaymentPrice { get; set; }
            public double? WebPaymentNonDiscountedPrice { get; set; }
            public double? DiscountedDailyPrice { get; set; }
            public string OfficePaymentPrice { get; set; }
            public object NavigationFree { get; set; }
            public object ChargerFree { get; set; }
            public object BabySeatFree { get; set; }
            public object ChildSeatFree { get; set; }
            public object SCDWFree { get; set; }
            public object SuperSCDWFree { get; set; }
            public object LCFFree { get; set; }
            public object TPEkstraFree { get; set; }
            public object YoungDriverInsuranceFree { get; set; }
            public object ExtraDriverFree { get; set; }
            public object CampaignId { get; set; }
            public string MainRulesId { get; set; }
            public string AdditionalWebCampaignID { get; set; }
            public string AdditionalWebDiscountDay { get; set; }
            public double? AdditionalWebdiscountedDailyPrice { get; set; }
            public double? AdditionalGrandTotal { get; set; }
            public double? GrandTotal { get; set; }
            public string VehiclePhoto { get; set; }
        }

        public class CentralAdditionalProduct
        {
            public string Id { get; set; }
            public string Description { get; set; }
            public string GroupName { get; set; }
            public string Price { get; set; }
            public string Status { get; set; }
        }

        public class CentralLocation
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
        }
    }
}
