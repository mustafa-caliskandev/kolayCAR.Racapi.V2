namespace KolayCAR.Broker.API.Models.Dtos
{
    public class BrokerApiExtraDto
    {
        public int ExtraId { get; set; }
        public string ExtraCode { get; set; }
        public string ExtraName { get; set; }
        public string ExtraDescription { get; set; }
        public int? ExtraRentalType { get; set; } = 1;
        public int? ExtraType { get; set; } = 2;
        public bool ExtraQuantityIncreasable { get; set; }
        public float Price { get; set; }
        public string Icon { get; set; }
        public string CurrencyCode { get; set; }
        public int? VendorId { get; set; }
        public string VendorName { get; set; }
        public int? ShowDayCountStart { get; set; }
        public int? ShowDayCountEnd { get; set; }
        public int? Sequence { get; set; } = 1;
    }
}
