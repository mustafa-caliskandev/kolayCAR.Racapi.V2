using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Helpers.Avec
{
    public class CurrencyHelper
    {
        public static AvecCurrencyTypes GetLongCurrencyType(CurrencyTypes currencyType)
        {
            switch (currencyType)
            {
                default:
                case CurrencyTypes.TRY:
                    return AvecCurrencyTypes.TL;
                case CurrencyTypes.USD:
                    return AvecCurrencyTypes.DOLLAR;
                case CurrencyTypes.EUR:
                    return AvecCurrencyTypes.EURO;
            }
        }
    }

    public enum AvecCurrencyTypes
    {
        TL,
        DOLLAR,
        EURO
    }
}
