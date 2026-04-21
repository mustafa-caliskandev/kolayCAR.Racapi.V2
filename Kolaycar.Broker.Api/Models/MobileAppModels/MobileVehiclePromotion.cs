namespace KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels
{
    public class MobileVehiclePromotion
    {
        public int Id { get; set; }
        public int? Order { get; set; }
        public string Code { get; set; }
        public string Text { get; set; }
        public bool? Active { get; set; }
    }
}