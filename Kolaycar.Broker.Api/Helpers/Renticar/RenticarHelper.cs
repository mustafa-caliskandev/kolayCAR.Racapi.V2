using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Renticar.Response;
using KolayCAR.Broker.Infrastructure.Extensions;

namespace KolayCAR.Broker.API.Helpers.Renticar
{
    public class RenticarHelper
    {
        public static float GetPrice(Offer apiVehicle, CurrencyTypes currency, int type)//type 1: daily price, 2: totalprice, 3: dropprice, 4: provission
        {
            if (type == 1)
                return currency == CurrencyTypes.EUR ?
                        apiVehicle.pricing.dailyPrice.EUR.ToFloatNullSafe() :
                        currency == CurrencyTypes.USD ?
                            apiVehicle.pricing.dailyPrice.USD.ToFloatNullSafe() :
                            currency == CurrencyTypes.TRY ?
                                apiVehicle.pricing.dailyPrice.TRY.ToFloatNullSafe() : apiVehicle.pricing.dailyPrice.EUR.ToFloatNullSafe();
            if (type == 2)
                return currency == CurrencyTypes.EUR ?
                        apiVehicle.pricing.totalPrice.EUR.ToFloatNullSafe() :
                        currency == CurrencyTypes.USD ?
                            apiVehicle.pricing.totalPrice.USD.ToFloatNullSafe() :
                            currency == CurrencyTypes.TRY ?
                                apiVehicle.pricing.totalPrice.TRY.ToFloatNullSafe() : apiVehicle.pricing.totalPrice.EUR.ToFloatNullSafe();
            if (type == 3)
                return currency == CurrencyTypes.EUR ?
                        apiVehicle.pricing.dropPrice.EUR.ToFloatNullSafe() :
                        currency == CurrencyTypes.USD ?
                            apiVehicle.pricing.dropPrice.USD.ToFloatNullSafe() :
                            currency == CurrencyTypes.TRY ?
                                apiVehicle.pricing.dropPrice.TRY.ToFloatNullSafe() : apiVehicle.pricing.dropPrice.EUR.ToFloatNullSafe();
            if (type == 4)
                return currency == CurrencyTypes.EUR ?
                        apiVehicle.pricing.provision.EUR.ToFloatNullSafe() :
                        currency == CurrencyTypes.USD ?
                            apiVehicle.pricing.provision.USD.ToFloatNullSafe() :
                            currency == CurrencyTypes.TRY ?
                                apiVehicle.pricing.provision.TRY.ToFloatNullSafe() : apiVehicle.pricing.provision.EUR.ToFloatNullSafe();
            return 0;
        }

        public static float GetExtraPrice(Domain.Models.Renticar.Response.Extra extras, CurrencyTypes currency, int type)//type 1: daily price, 2: totalprice, 3: dropprice, 4: provission
        {
            if (type == 1)
                return currency == CurrencyTypes.EUR ?
                        extras.dailyPrice.EUR.ToFloatNullSafe() :
                        currency == CurrencyTypes.USD ?
                            extras.dailyPrice.USD.ToFloatNullSafe() :
                            currency == CurrencyTypes.TRY ?
                                extras.dailyPrice.TRY.ToFloatNullSafe() :
                                extras.dailyPrice.EUR.ToFloatNullSafe();
            if (type == 2)
                return currency == CurrencyTypes.EUR ?
                        extras.totalPrice.EUR.ToFloatNullSafe() :
                        currency == CurrencyTypes.USD ?
                            extras.totalPrice.USD.ToFloatNullSafe() :
                            currency == CurrencyTypes.TRY ?
                                extras.totalPrice.TRY.ToFloatNullSafe() :
                                extras.totalPrice.EUR.ToFloatNullSafe();
            return 0;
        }
    }
}
