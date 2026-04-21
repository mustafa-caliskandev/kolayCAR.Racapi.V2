using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using static KolayCAR.Broker.Domain.Models.Response.ZiraatFiloResposeBase;

namespace KolayCAR.Broker.API.Helpers.Turevrac
{
    public class CurrencyHelper
    {
        public static TurevracCurrencyTypes GetLongCurrencyType(CurrencyTypes currencyType)
        {
            switch (currencyType)
            {
                default:
                case CurrencyTypes.TRY:
                    return TurevracCurrencyTypes.TL;
                case CurrencyTypes.USD:
                    return TurevracCurrencyTypes.USD;
                case CurrencyTypes.EUR:
                    return TurevracCurrencyTypes.EURO;
                case CurrencyTypes.GBP:
                    return TurevracCurrencyTypes.GBP;
            }
        }
    }

    public enum TurevracCurrencyTypes
    {
        TL,
        EURO,
        USD,
        GBP,
        TRY
    }
}
