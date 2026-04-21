using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Helpers.Akkor
{
    public class CurrencyHelper
    {
        public static AkkorCurrencyTypes GetLongCurrencyType(CurrencyTypes currencyType)
        {
            switch (currencyType)
            {
                default:
                case CurrencyTypes.TRY:
                    return AkkorCurrencyTypes.TL;
                case CurrencyTypes.USD:
                    return AkkorCurrencyTypes.USD;
                case CurrencyTypes.EUR:
                    return AkkorCurrencyTypes.EURO;
                case CurrencyTypes.GBP:
                    return AkkorCurrencyTypes.GBP;
            }
        }
    }
    public enum AkkorCurrencyTypes
    {
        TL,
        EURO,
        USD,
        GBP
    }
}
