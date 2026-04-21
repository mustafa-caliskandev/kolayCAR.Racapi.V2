using System.ComponentModel.DataAnnotations;

namespace KolayCAR.Broker.API.Models
{
    public class VendorLocationFee
    {
        [Key]
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int LocationId { get; set; }
        public int? FeePercentage { get; set; }
        public decimal? FeeAmount { get; set; }

    }
}
