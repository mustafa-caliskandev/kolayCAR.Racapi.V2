using Newtonsoft.Json;

namespace kolayCAR.Broker.AWS.Models.AwsModels.Kinesis
{
    public class AdditionalProductModel
    {
        public int extraId { get; set; }
        public int extraRentalType { get; set; }
        public string extraName { get; set; }
        public int rentalDuration { get; set; }
        public decimal price { get; set; }
    }
}
