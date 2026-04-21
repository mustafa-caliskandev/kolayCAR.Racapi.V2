using KolayCAR.Broker.Domain.Models;
using System.Collections.Generic;
using static KolayCAR.Broker.Domain.Models.Response.Avec3ResponseBase;

namespace KolayCAR.Broker.API.Mappers.Avec3
{
    public static class ExtraMapper
    {

        public static Extra Map(this AvailableExtraAvec extra) =>
         extra != null ? new Extra
         {
             ExtraId = 1,
             ExtraCode = extra.id,
             ApiExtraCode = extra.id,
             ExtraName = extra.name,
             Price = extra.daily_price,
             ExtraDescription = extra.description,
             ExtraRentalType = ExtraRentalTypes.Daily,
             ExtraType = extra.type == "insurance" ? AdditionalProductTypes.Insurance : AdditionalProductTypes.Extra
         }
         : null;


        public static List<Extra> Map(this List<AvailableExtraAvec> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
        public static Extra Map(this ExtraResponseAvec extra) =>
           extra != null ? new Extra
           {
               ExtraId = 1,
               ExtraCode = extra.id + "~" + extra.code,
               ApiExtraCode = extra.id + "~" + extra.code,
               ExtraName = extra.name,
               ExtraDescription = extra.description,
               ExtraRentalType = ExtraRentalTypes.Daily
           }
           : null;


        public static List<Extra> Map(this List<ExtraResponseAvec> extras)
        {
            var _extras = new List<Extra>();

            if (extras != null && extras.Count > 0)
                foreach (var extra in extras)
                    _extras.Add(extra.Map());

            return _extras;
        }
    }
}
