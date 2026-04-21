using KolayCAR.Broker.Domain.Models.Response;

namespace KolayCAR.Broker.API.Mappers.KolayCAR
{
    public static class BankMapper
    {
        public static BankInfo Map(this BANK bank) =>
            bank != null ? new BankInfo
            {
                BankId = bank.BANKID,
                BankName = bank.BANKNAME,
                BankVendorId = bank.BANKVENDORID,
                BankDefinition = bank.BANKDEFINITION,
                InstallmentActive = bank.INSTALLMENTACTIVE,
                ThreeDPaymentActive = bank.THREEDSTATUS != 0,
                ThreeDPaymentRequired = bank.THREEDSTATUS == 2,
                AmexActive = bank.BANKAMEXCARDACTIVE,
                Account = bank.BANKACCOUNTNO,
                IBAN = bank.IBAN
            }
            : null;
    }
}
