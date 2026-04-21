namespace KolayCAR.Broker.API.Models
{
    public partial class CouponVendor
    {
        public int Id { get; set; }
        public int CouponId { get; set; }
        public int VendorId { get; set; }
        public decimal? VendorRate { get; set; }
    }
}
