using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;

namespace KolayCAR.Broker.API.Helpers.Central
{
    public class VehicleQueryHelper
    {
        public static bool CheckCentralLocationIsAvailable(List<CentralResponseBase.CentralLocation> centralLocations, DateTime pickupDateTime, DateTime returnDateTime)
        {
            DateTime apiStartTime = new DateTime();
            DateTime apiEndTime = new DateTime();

            for (int i = 0; i < centralLocations.Count; i++)
            {
                var centralLocation = centralLocations[i];
                var requestDateTime = i == 0 ? pickupDateTime : returnDateTime;
                switch (requestDateTime.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        {
                            apiStartTime = centralLocation.MondayStartTime.ToDateTimeNullSafe();
                            apiEndTime = centralLocation.MondayEndTime.ToDateTimeNullSafe();
                            break;
                        }
                    case DayOfWeek.Tuesday:
                        {
                            apiStartTime = centralLocation.TuesdayStartTime.ToDateTimeNullSafe();
                            apiEndTime = centralLocation.TuesdayEndTime.ToDateTimeNullSafe();
                            break;
                        }
                    case DayOfWeek.Wednesday:
                        {
                            apiStartTime = centralLocation.WednesdayStartTime.ToDateTimeNullSafe();
                            apiEndTime = centralLocation.WednesdayEndTime.ToDateTimeNullSafe();
                            break;
                        }
                    case DayOfWeek.Thursday:
                        {
                            apiStartTime = centralLocation.ThursdayStartTime.ToDateTimeNullSafe();
                            apiEndTime = centralLocation.ThursdayEndTime.ToDateTimeNullSafe();
                            break;
                        }
                    case DayOfWeek.Friday:
                        {
                            apiStartTime = centralLocation.FridayStartTime.ToDateTimeNullSafe();
                            apiEndTime = centralLocation.FridayEndTime.ToDateTimeNullSafe();
                            break;
                        }
                    case DayOfWeek.Saturday:
                        {
                            apiStartTime = centralLocation.SaturdayStartTime.ToDateTimeNullSafe();
                            apiEndTime = centralLocation.SaturdayEndTime.ToDateTimeNullSafe();
                            break;
                        }
                    case DayOfWeek.Sunday:
                        {
                            apiStartTime = centralLocation.SundayStartTime.ToDateTimeNullSafe();
                            apiEndTime = centralLocation.SundayEndTime.ToDateTimeNullSafe();
                            break;
                        }
                }

                if (!(apiStartTime.TimeOfDay <= requestDateTime.TimeOfDay && apiEndTime.TimeOfDay >= requestDateTime.TimeOfDay))
                    return false;
            }

            return true;
        }
    }
}
