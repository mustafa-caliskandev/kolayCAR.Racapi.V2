using KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Helpers.CredyCar
{
    public class CurrencyHelper
    {
        public static CredyCarCurrencyTypes GetLongCurrencyType(CurrencyTypes currencyType)
        {
            switch (currencyType)
            {
                default:
                case CurrencyTypes.TRY:
                    return CredyCarCurrencyTypes.TL;
                case CurrencyTypes.USD:
                    return CredyCarCurrencyTypes.DOLLAR;
                case CurrencyTypes.EUR:
                    return CredyCarCurrencyTypes.EURO;
            }
        }
    }

    public enum CredyCarCurrencyTypes
    {
        TL,
        DOLLAR,
        EURO
    }
}
