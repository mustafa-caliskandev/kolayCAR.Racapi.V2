namespace KolayCAR.Broker.Domain.Models
{
    public class VendorScoreDto
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string VendorLogoUrl { get; set; }
        public string VendorContentUrl { get; set; }
        public decimal Score { get; set; }
        public int CommentCount { get; set; }
        public decimal CurrentScore { get; set; }
    }
}
