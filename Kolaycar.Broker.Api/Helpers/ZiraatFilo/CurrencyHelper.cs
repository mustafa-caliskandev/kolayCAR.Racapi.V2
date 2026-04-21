using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Helpers.ZiraatFilo
{
    public static class CurrencyHelper
    {
        public static string GetZiraatFiloCurrencyType(CurrencyTypes currencyType)
        {
            switch (currencyType)
            {
                default:
                case CurrencyTypes.TRY:
                    return ZiraatFiloCurrencyTypes.TRY.ToString();
                case CurrencyTypes.USD:
                    return ZiraatFiloCurrencyTypes.USD.ToString();
                case CurrencyTypes.EUR:
                    return ZiraatFiloCurrencyTypes.EUR.ToString();
                case CurrencyTypes.GBP:
                    return ZiraatFiloCurrencyTypes.GBP.ToString();
            }
        }

        public enum ZiraatFiloCurrencyTypes
        {
            TRY,
            USD,
            EUR,
            GBP
        }
    }
}
