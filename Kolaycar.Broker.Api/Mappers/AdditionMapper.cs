using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Mappers
{
    public static class AdditionMapper
    {
        public static Addition Map(this Additions status) =>
           status != null ? new Addition
           {
               Id = status.Id,
               UserId = status.Userid,
               AdditionTypeId = status.Additiontypeid,
               CreateDate = status.Createdate ?? DateTime.Now,
               LastStatusChangeDate = status.Laststatuschangedate,
               ReservationNumber = status.Reservationnumber,
               AdditionName = status.Additionname,
               Description = status.Description,
               VendorAmount = status.Vendoramount.ToFloatNullAvailable(),
               VendorCurrencyId = status.Vendorcurrencyid,
               AgencyAmount = status.Agencyamount.ToFloatNullAvailable(),
               AgencyCurrencyId = status.Agencycurrencyid,
               AdditionStatusType = (ReservationAdditionStatusTypes)status.Additionstatustype,
               LastStatusChangeUserId = status.Laststatuschangeuserid
           }
           : null;

        public static List<Addition> Map(this List<Additions> stuations)
        {
            var _stuations = new List<Addition>();

            if (stuations != null && stuations.Count != 0)
                foreach (var status in stuations)
                    _stuations.Add(status.Map());

            return _stuations;
        }
    }
}
