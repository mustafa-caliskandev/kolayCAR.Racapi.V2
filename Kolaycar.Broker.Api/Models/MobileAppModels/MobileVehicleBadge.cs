namespace KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels
{
    public class MobileVehicleBadge
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public int? ConditionId { get; set; }
        public int LanguageId { get; set; }
        public string Text { get; set; }
        public string BorderColor { get; set; }
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }
        public string IconPath { get; set; }
        public bool? Active { get; set; }
    }
}
