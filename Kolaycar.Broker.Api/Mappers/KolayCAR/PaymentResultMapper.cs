using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;

namespace KolayCAR.Broker.API.Mappers.KolayCAR
{
    public static class PaymentResultMapper
    {
        public static PaymentResult Map(this PAYMENT payment) =>
            payment != null ? new PaymentResult
            {
                BankId = payment.BANKID.ToIntNullSafe(),
                ProvisionNumber = payment.PROVISIONNO,
                PaymentAmount = payment.PAYMENTAMOUNT.ToFloatNullSafe(),
                CurrencyCode = payment.CURRENCYISOCODE,
                AlertErrorCode = payment.ALERTERRORCODE,
                LogResultNumber = payment.LOGRESULTNO,
                LogErrorCode = payment.LOGERRORCODE,
                OrderNumber = payment.ORDERNO
            }
            : null;
    }
}
