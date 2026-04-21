namespace KolayCAR.Broker.API.Models
{
    public class MobileAppSetting
    {
        public int Id { get; set; }
        public string Parameter { get; set; }
        public string Value { get; set; }
        public string IconPath { get; set; }
        public int? Order { get; set; }
        public int? LanguageId { get; set; }
        public int? Type { get; set; }
    }
}
