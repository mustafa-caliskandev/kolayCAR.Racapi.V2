namespace KolayCAR.Broker.Domain.Models.Requests.YesOto
{
    public class YesOtoSearchVehicleRequest
    {
        public string BrandId { get; set; }
        public string SalesChannelId { get; set; }
        public string LanguageId { get; set; }
        public string Location { get; set; }
        public string DropOffLocation { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Age { get; set; }
        public string SelectedVehicleGroup { get; set; }
        public object SelectedAdditionalServices { get; set; }
        public string UserCampaignId { get; set; }
        public bool IsUserFirstReservation { get; set; }
        public decimal DiscPrice { get; set; }
        public string GiftCoupon { get; set; }
    }
}
