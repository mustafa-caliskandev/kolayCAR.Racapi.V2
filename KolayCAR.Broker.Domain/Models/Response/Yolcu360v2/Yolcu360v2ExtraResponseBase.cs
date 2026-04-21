using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Yolcu360v2;

public class Yolcu360v2ExtraResponseBase
{
    public class Amount
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class Fee
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class Insurance
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class Net
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class PaymentTotal
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class PreCommissionFee
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class PreDiscountTotal
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class Price
    {
        public Amount amount { get; set; }
        public string charge { get; set; }
        public string description { get; set; }
        public string type { get; set; }
    }

    public class Pricing
    {
        public Net net { get; set; }
        public Fee fee { get; set; }
        public PaymentTotal paymentTotal { get; set; }
        public PreCommissionFee preCommissionFee { get; set; }
        public PreDiscountTotal preDiscountTotal { get; set; }
        public Total total { get; set; }
        public VendorTotal vendorTotal { get; set; }
        public List<Price> prices { get; set; }
    }

    public class Root
    {
        public string code { get; set; }
        public int max { get; set; }
        public int min { get; set; }
        public string name { get; set; }
        public string searchID { get; set; }
        public string type { get; set; }
        public Pricing pricing { get; set; }
        public Insurance insurance { get; set; }
    }

    public class Total
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

    public class VendorTotal
    {
        public int amount { get; set; }
        public string currency { get; set; }
    }

}
