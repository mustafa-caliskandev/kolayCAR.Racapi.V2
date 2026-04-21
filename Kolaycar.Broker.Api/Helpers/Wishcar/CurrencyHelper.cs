using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Helpers.Wishcar
{
    public class CurrencyHelper
    {
        public static WishcarCurrencyTypes GetLongCurrencyType(CurrencyTypes currencyType)
        {
            switch (currencyType)
            {
                default:
                case CurrencyTypes.TRY:
                    return WishcarCurrencyTypes.TL;
                case CurrencyTypes.USD:
                    return WishcarCurrencyTypes.USD;
                case CurrencyTypes.EUR:
                    return WishcarCurrencyTypes.EURO;
            }
        }
    }

    public enum WishcarCurrencyTypes
    {
        TL,
        USD,
        EURO
    }
}
