using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class ErboycarRequestBase
    {
        public class Reservation
        {
            public string _token { get; set; }
            public List<Extra> extra { get; set; }
            public Customer customer { get; set; }
            public string customer_type { get; set; }
            public int payment_type { get; set; }
            public float partial_amount { get; set; }
        }

        public class Extra
        {
            public string _id { get; set; }
            public int qty { get; set; }
        }

        public class Customer
        {
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string phone_mobile { get; set; }
            public string email { get; set; }
        }

        public class ReservationCancel
        {
            public string reservation_number { get; set; }
        }
    }
}
