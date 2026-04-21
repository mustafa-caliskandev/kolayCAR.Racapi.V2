using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Helpers.Aytu
{
    public class CurrencyHelper
    {
        public static AytuCurrencyTypes GetLongCurrencyType(CurrencyTypes currencyType)
        {
            switch (currencyType)
            {
                default:
                case CurrencyTypes.TRY:
                    return AytuCurrencyTypes.TL;
                case CurrencyTypes.USD:
                    return AytuCurrencyTypes.USD;
                case CurrencyTypes.EUR:
                    return AytuCurrencyTypes.EURO;
            }
        }
    }

    public enum AytuCurrencyTypes
    {
        TL,
        USD,
        EURO
    }
}
