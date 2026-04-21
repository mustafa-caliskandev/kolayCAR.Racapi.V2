using System;

namespace KolayCAR.Broker.API.Models.Dtos
{
    public class PopupContentDto
    {
        public int Id { get; set; }
        public int LanguageId { get; set; }
        public bool ShowEveryTime { get; set; } = true;
        public string Editor { get; set; }
        public string RedirectUrl { get; set; }
        public string BaseUrl { get; set; }
        public int MinDayCount { get; set; }
        public int MaxDayCount { get; set; }
        public DateTime? StartingDate { get; set; }
        public DateTime? EndingDate { get; set; }
    }
}
