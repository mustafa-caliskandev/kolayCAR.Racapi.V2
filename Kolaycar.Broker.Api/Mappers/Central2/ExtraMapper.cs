using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.Central2ResponseBase;

namespace KolayCAR.Broker.API.Mappers.Central2
{
    public static class ExtraMapper
    {
        public static List<Extra> Map(this List<MesajBilgi> extraList)
        {
            var _extras = new List<Extra>();

            foreach (var extra in extraList)
            {
                _extras.Add(extra.Map());
            }

            return _extras;
        }
        private static Extra Map(this MesajBilgi apiExtra)
        {
            return new Extra
            {
                ExtraId = apiExtra.Id.ToIntNullSafe(),
                ExtraCode = apiExtra.Kod,
                ApiExtraCode = apiExtra.Kod,
                ExtraName = apiExtra.Subject,
                ExtraRentalType = ExtraRentalTypes.Daily,
                ExtraDescription = apiExtra.Desc
            };
        }
    }
}
