using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Helpers.FiloNova
{
    public class VehicleQueryHelper
    {
        public static bool CheckLocationIsAvailable(
            List<FiloNovaResponseBase.WorkingHour> workingHour,
            string apiPickupLocationCode,
            string apiReturnLocationCode,
            DateTime pickupDateTime,
            DateTime returnDateTime)
        {
            var apiPickupLocation = workingHour.FirstOrDefault(x => x.branchId == apiPickupLocationCode && x.dayCode == (int)pickupDateTime.DayOfWeek);
            var apiReturnLocation = workingHour.FirstOrDefault(x => x.branchId == apiReturnLocationCode && x.dayCode == (int)returnDateTime.DayOfWeek);

            if (apiPickupLocation == null || apiReturnLocation == null)
                return false;

            DateTime apiPickupStartTime = TimeSpan.FromMinutes(apiPickupLocation.beginingTime).ToString("hh':'mm").ToDateTimeNullSafe();
            DateTime apiPickupEndTime = TimeSpan.FromMinutes(apiPickupLocation.endTime).ToString("hh':'mm").ToDateTimeNullSafe();
            DateTime apiReturnStartTime = TimeSpan.FromMinutes(apiReturnLocation.beginingTime).ToString("hh':'mm").ToDateTimeNullSafe();
            DateTime apiReturnEndTime = TimeSpan.FromMinutes(apiReturnLocation.endTime).ToString("hh':'mm").ToDateTimeNullSafe();

            return (apiPickupStartTime.TimeOfDay <= pickupDateTime.TimeOfDay && apiPickupEndTime.TimeOfDay >= pickupDateTime.TimeOfDay) && (apiReturnStartTime.TimeOfDay <= returnDateTime.TimeOfDay && apiReturnEndTime.TimeOfDay >= returnDateTime.TimeOfDay);
        }
    }
}
