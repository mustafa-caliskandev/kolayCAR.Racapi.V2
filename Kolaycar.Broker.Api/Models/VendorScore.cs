namespace KolayCAR.Broker.API.Models
{
    public class VendorScore
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public int? ApiVendorId { get; set; }
        public string ApiVendorName { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public decimal? Score { get; set; }
        public decimal? CurrentScore { get; set; }
    }
}
