namespace KolayCAR.Broker.Domain.Models
{
    public class LocationOfficeWorkingHour
    {
        // ISO 8601: 1 = Monday, 7 = Sunday.
        public int DayOfWeek { get; set; }
        public string OpeningTime { get; set; }
        public string ClosingTime { get; set; }
    }
}
