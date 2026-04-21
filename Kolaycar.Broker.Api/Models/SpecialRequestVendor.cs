namespace KolayCAR.Broker.API.Models
{
    public class SpecialRequestVendor
    {
        public int Id { get; set; }
        public int SpecialRequestId { get; set; }
        public int VendorId { get; set; }
        public SpecialRequest SpecialRequest { get; set; }
        public Vendor Vendor { get; set; }
    }
}