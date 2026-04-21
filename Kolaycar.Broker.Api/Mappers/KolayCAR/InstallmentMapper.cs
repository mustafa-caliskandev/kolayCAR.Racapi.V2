using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.KolayCAR
{
    public static class InstallmentMapper
    {
        public static InstallmentInfo Map(this INSTALLMENTS installment) =>
            installment != null ? new InstallmentInfo
            {
                InstallmentCount = installment.INSTALLMENT.ToIntNullSafe(),
                InstallmentTotalAmount = installment.INSTALLMENTTOTALAMOUNT,
                InstallmentAmount = installment.INSTALLMENTONLYPRICE,
                Comment = installment.COMMENT,
                Percent = installment.PERCENT.ToFloatNullSafe()
            }
            : null;

        public static List<InstallmentInfo> Map(this List<INSTALLMENTS> installments)
        {
            var _installments = new List<InstallmentInfo>();

            if (installments != null && installments.Count != 0)
                foreach (var installment in installments)
                    _installments.Add(installment.Map());

            return _installments;
        }
    }
}
