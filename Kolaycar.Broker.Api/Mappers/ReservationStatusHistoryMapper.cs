using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers
{
    public static class ReservationStatusHistoryMapper
    {
        public static ReservationStatusHistory Map(this Resstatushistory status) =>
            status != null ? new ReservationStatusHistory
            {
                Id = status.Id,
                ReservationStatusType = (ReservationStatusTypes)(status.Resstatusid * -1 - 1),
                ReservationStatusName = status.Resstatusname,
                ReservationStatusNote = status.Resstatusnote,
                Date = status.Inserteddate ?? DateTime.Now
            }
            : null;

        public static List<ReservationStatusHistory> Map(this List<Resstatushistory> stuations, List<Resstatuslang> langs)
        {
            var _stuations = new List<ReservationStatusHistory>();

            if (stuations != null && stuations.Count != 0)
                foreach (var status in stuations)
                {
                    var lang = langs.Where(x => x.Resstatustypeid == status.Resstatusid).FirstOrDefault();
                    status.Resstatusname = lang.Statusname;
                    _stuations.Add(status.Map());
                }

            return _stuations;
        }
    }
}
