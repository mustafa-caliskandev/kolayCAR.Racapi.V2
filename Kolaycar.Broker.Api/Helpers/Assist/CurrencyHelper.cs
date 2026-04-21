using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Helpers.Assist
{
    public class CurrencyHelper
    {
        public static AssistCurrencyTypes GetLongCurrencyType(CurrencyTypes currencyType)
        {
            switch (currencyType)
            {
                default:
                case CurrencyTypes.TRY:
                    return AssistCurrencyTypes.TL;
                case CurrencyTypes.USD:
                    return AssistCurrencyTypes.DOLLAR;
                case CurrencyTypes.EUR:
                    return AssistCurrencyTypes.EURO;
            }
        }
    }

    public enum AssistCurrencyTypes
    {
        TL,
        DOLLAR,
        EURO
    }
}
