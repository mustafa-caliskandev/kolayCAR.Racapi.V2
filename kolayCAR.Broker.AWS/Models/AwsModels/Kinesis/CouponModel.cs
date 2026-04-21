using Newtonsoft.Json;

namespace kolayCAR.Broker.AWS.Models.AwsModels.Kinesis
{
    /// <summary>
    /// 
    /// </summary>
    public class CouponModel
    {
        [JsonProperty("coupon_code")]
        public string CouponCode { get; set; }

        [JsonProperty("campaign_name")]
        public string CouponName { get; set; }

        [JsonProperty("discount_amount")]
        public decimal CouponAmount { get; set; }

        [JsonProperty("payment_type")] public string CouponPaymentType { get; set; } = "ObiletNonRefundable";
    }
}
