using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Apilog
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public string Level { get; set; }
        public DateTime? TimeStamp { get; set; }
        public string Exception { get; set; }
        public string LogEvent { get; set; }
        public string ActionName { get; set; }
        public string RequestPath { get; set; }
        public string RequestId { get; set; }
        public string Environment { get; set; }
        public string ClientIp { get; set; }
    }
}
