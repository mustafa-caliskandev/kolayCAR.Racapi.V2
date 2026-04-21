using System;

namespace KolayCAR.Broker.API.Models
{
    public class PaymentResult
    {
        public int Id { get; set; }
        public string ResToken { get; set; }
        public string PaymentCode { get; set; }
        public bool Result { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
    }
}
