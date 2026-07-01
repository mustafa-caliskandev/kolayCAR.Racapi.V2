using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests.Reservaway;

public class ReservawayCreateCustomerRequest
{
    public string gender { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string date_of_birth { get; set; }
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
    public long vehicle_id { get; set; }
    public string locale { get; set; }
}
