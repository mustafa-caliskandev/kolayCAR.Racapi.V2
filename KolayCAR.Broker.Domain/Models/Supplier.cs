namespace KolayCAR.Broker.Domain.Models
{
    public class Supplier
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public VendorTypes VendorType { get; set; }
        public string ApiKey { get; set; }
        public string ApiPassword { get; set; }
        public bool Active { get; set; }
        public bool ReservationListActive { get; set; }
    }
}
