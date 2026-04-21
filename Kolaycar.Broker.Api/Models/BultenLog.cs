using System;

namespace KolayCAR.Broker.API.Models
{
    public class BultenLog
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public int AgencyId { get; set; }
        public bool ContactPermission { get; set; }
        public string IP { get; set; }
    }
}
