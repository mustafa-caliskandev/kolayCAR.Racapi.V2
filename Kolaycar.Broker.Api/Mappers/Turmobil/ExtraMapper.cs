using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using Default = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.Turmobil
{
    public static class ExtraMapper
    {
        public static Default.Extra Map(this TurmobilResponseBase.Extra extra, int vendorId) => extra != null ? new Default.Extra
        {
            VendorName = "Turmobil",
            VendorId = vendorId,
            ExtraId = extra.id,
            ExtraCode = extra.id.ToString(),
            ApiExtraCode = extra.id.ToString(),
            ExtraRentalType = ExtraRentalTypes.Daily,
            ExtraName = extra.name,
            ExtraDescription = extra.detail,
            Price = extra.amount
        } : null;

        public static List<Default.Extra> Map(this List<TurmobilResponseBase.Extra> extras, int vendorId)
        {
            var _extras = new List<Default.Extra>();
            foreach (var extra in extras)
            {
                var defaultExtra = extra.Map(vendorId);
                _extras.Add(defaultExtra);
            }

            return _extras;
        }
    }
}
