using System;

namespace KolayCAR.Broker.API.Models
{
    public class FindeksLog
    {
        public int Id { get; set; }
        public DateTime? LogDate { get; set; }
        public string ReservationToken { get; set; }
        public string Tckn { get; set; }
        public string StepName { get; set; }
        public string Response { get; set; }
        public string Request { get; set; }
        public bool? IsSuitable { get; set; }
    }
}
