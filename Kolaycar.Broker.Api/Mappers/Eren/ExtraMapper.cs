using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Responses.Eren;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers.Eren;

public static class ExtraMapper
{
    public static Extra Map(this ErenExtra extra) =>
       extra != null ? new Extra
       {
           ExtraId = 0,
           ExtraCode = extra.ServiceId.ToStringNullSafe(),
           ApiExtraCode = extra.ServiceId.ToStringNullSafe(),
           ExtraName = extra.ServiceName.ToStringNullSafe(),
           ExtraRentalType = extra.PerDay == 0 ? ExtraRentalTypes.PerRental : ExtraRentalTypes.Daily,
       }
       : null;

    public static List<Extra> Map(this List<ErenExtra> extras)
    {
        var _extras = new List<Extra>();

        if (extras != null && extras.Count > 0)
            foreach (var extra in extras)
                _extras.Add(extra.Map());

        return _extras;
    }
}
