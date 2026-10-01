using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models
{
    public class VendorAvailabilityLog
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string VendorType { get; set; }
        public bool Success { get; set; }
        public List<VendorHttpLogEntry> Entries { get; set; } = new List<VendorHttpLogEntry>();
    }

    public class VendorHttpLogEntry
    {
        public bool Success { get; set; }
        public int? HttpStatusCode { get; set; }
        public string HttpMethod { get; set; }
        public string RequestBaseUrl { get; set; }
        public string RequestPath { get; set; }
        public string RequestContent { get; set; }
        public long? ElapsedMilliseconds { get; set; }
        public string ResponseContent { get; set; }
        public string ExceptionMessage { get; set; }
    }

    public class VerboseVehiclesData
    {
        public object Vehicles { get; set; }
        public List<VendorAvailabilityLog> VendorLogs { get; set; } = new List<VendorAvailabilityLog>();
    }
}
