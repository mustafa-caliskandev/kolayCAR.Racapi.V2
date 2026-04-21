namespace KolayCAR.Broker.API.Models
{
    public class LocationVendorInstallment
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int? VendorId { get; set; }
        public int MaxInstallmentCount { get; set; }
    }
}
