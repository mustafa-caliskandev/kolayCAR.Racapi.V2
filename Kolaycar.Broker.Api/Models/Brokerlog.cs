using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Brokerlog
    {
        public int Id { get; set; }
        public DateTime Logdate { get; set; }
        public string Logkey { get; set; }
        public string Content { get; set; }
        public int Logtype { get; set; }
        public string Logtypekey { get; set; }
    }
}
