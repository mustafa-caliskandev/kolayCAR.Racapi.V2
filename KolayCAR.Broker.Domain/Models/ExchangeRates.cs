using System;

namespace KolayCAR.Broker.Domain.Models
{
    public class ExchangeRates
    {
        public int Id { get; set; }
        public CurrencyTypes CurrencyType { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public float ExchangeRate { get; set; }
    }
}
