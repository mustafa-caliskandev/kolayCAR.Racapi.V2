using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Response.Reservaway;

public class ReservawayCustomerResponse : ReservawayResponseBase
{
    public ReservawayCustomer customer { get; set; }
    public ReservawayOrder order { get; set; }
}

public class ReservawayCustomer
{
    public int id { get; set; }
    public string gender { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string address { get; set; }
    public string city { get; set; }
    public int country_id { get; set; }
    public string postal_code { get; set; }
    public string flight_number { get; set; }
    public string reservation_token { get; set; }
    public List<string> extras { get; set; }
    public int payment_type_id { get; set; }
    public int product_type_id { get; set; }
    public int vehicle_id { get; set; }
    public string visitor_id { get; set; }
    public float amount_total { get; set; }
    public float amount_today { get; set; }
    public float extras_total { get; set; }
    public float extras_today { get; set; }
    public string order_id { get; set; }
    public string status { get; set; }
    public string payment_type { get; set; }
    public string product_type { get; set; }
    public bool is_cancellable { get; set; }
}

public class ReservawayOrder
{
    public string state { get; set; }
    public JToken data { get; set; }
}
