namespace KolayCAR.Broker.API.Models
{
    public class VendorLocationDeliveryType
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int LocationId { get; set; }
        public int DeliveryTypeId { get; set; }
    }
}
