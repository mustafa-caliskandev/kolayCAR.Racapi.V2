using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers
{
    public static class ExchangeRatesMapper
    {
        public static ExchangeRates Map(this Exchangerates exchangeRate) =>
            exchangeRate != null ? new ExchangeRates
            {
                Id = exchangeRate.Recid,
                CurrencyType = (CurrencyTypes)(exchangeRate.Currencyid - 1),
                LastUpdateDate = exchangeRate.Date,
                ExchangeRate = exchangeRate.Exchangerate.ToFloatNullSafe()
            }
            : null;

        public static List<ExchangeRates> Map(this IEnumerable<Exchangerates> exchangeRates)
        {
            var _exchangeRates = new List<ExchangeRates>();

            if (exchangeRates != null && exchangeRates.Count() > 0)
                foreach (var exchangeRate in exchangeRates)
                    _exchangeRates.Add(exchangeRate.Map());

            return _exchangeRates;
        }
    }
}
