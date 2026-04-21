using Newtonsoft.Json;

namespace kolayCAR.Broker.AWS.Models.AwsModels.Kinesis
{
    public class UserDetailModel
    {
        [JsonProperty("mail")]
        public string CustomerEmail { get; set; }

        [JsonProperty("phone")]
        public string CustomerPhone { get; set; }

        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty("payment_card")]
        public string PaymentCard { get; set; }

        [JsonProperty("payment_type")]
        public string PaymentType { get; set; }

        [JsonProperty("number_installments")]
        public int InstallmentCount { get; set; }

        [JsonProperty("late_charge")]
        public decimal LateCharge { get; set; } // Taksit Komisyonu

        [JsonProperty("has_newsletter_subscription")]
        public bool ContactPermission { get; set; }
    }
}
