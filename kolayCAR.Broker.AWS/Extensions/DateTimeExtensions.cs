using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace kolayCAR.Broker.AWS.Extensions
{
    internal static class DateTimeExtensions
    {
        #region Int
        public static int DifferanceDay(this DateTime baseDate, DateTime? outDate)
        {
            TimeSpan diff = baseDate - (outDate ?? DateTime.Now);
            return (int)Math.Abs(diff.TotalDays);
        }

        public static int DifferanceDay(this string baseDate, string outDate = "")
        {
            DateTime outDateTime = !string.IsNullOrEmpty(outDate) ? outDate.ToDateTime() : DateTime.Now;
            DateTime baseDateTime = baseDate.ToDateTime();

            TimeSpan diff = baseDateTime - outDateTime;
            return (int)Math.Abs(diff.TotalDays);
        }

        public static int DifferanceMinute(this DateTime baseDate)
        {
            TimeSpan diff = baseDate - DateTime.Now;
            return (int)Math.Abs(diff.TotalMinutes);
        }

        public static int DifferanceMinute(this string baseDate, string outDate = "")
        {
            DateTime outDateTime = !string.IsNullOrEmpty(outDate) ? outDate.ToDateTime() : DateTime.Now;
            DateTime baseDateTime = baseDate.ToDateTime();

            TimeSpan diff = baseDateTime - outDateTime;
            return (int)Math.Abs(diff.TotalMinutes);
        }
        #endregion

        #region DateTime
        public static DateTime ToDateTime(this string baseDate)
        {
            try
            {
                if (baseDate != null && baseDate.Contains('.'))
                {
                    var splitted = baseDate.Split();
                    var dateParts = splitted[0].Split('.')?.Select(Int32.Parse)?.ToList();
                    var timeParts = splitted.Count() > 1 ? splitted[1]?.Split(':')?.Select(Int32.Parse)?.ToList() : new List<int>();

                    return new DateTime(
                        year: dateParts[2],
                        month: dateParts[1],
                        day: dateParts[0],
                        hour: timeParts.Any() ? timeParts[0] : 0,
                        minute: timeParts.Any() ? timeParts[1] : 0,
                        second: 0
                    );
                }
                else if (!DateTime.TryParse(baseDate, out DateTime result))
                {
                    return DateTime.Now;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }

        public static DateTime ToDate(this string input, bool throwExceptionIfFailed = false)
        {
            DateTime result;
            var valid = DateTime.TryParse(input, out result);
            if (!valid)
                if (throwExceptionIfFailed)
                    throw new FormatException(string.Format("'{0}' cannot be converted as DateTime", input));
            return result;
        }
        #endregion


        #region String
        public static string RoundUp(this DateTime baseDate, TimeSpan timeSpan)
        {
            return new DateTime((baseDate.Ticks + timeSpan.Ticks - 1) / timeSpan.Ticks * timeSpan.Ticks, baseDate.Kind).ToString("HH:mm");
        }

        public static string TurkishDate(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy", new CultureInfo("tr-TR", false));
        }

        public static string TurkishDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy HH:mm", new CultureInfo("tr-TR", false));
        }

        public static string TurkishDateDay(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy", new CultureInfo("tr-TR", false));
        }

        public static string TurkishDateTimeDay(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy dddd HH:mm", new CultureInfo("tr-TR", false));
        }

        public static string ToDateTimeString(this DateTime dateTime, string languageCode)
        {
            return dateTime.ToString("dd MMMM yyyy dddd HH:mm", new CultureInfo(languageCode, false));
        }

        public static string TurkishDate(this DateTime? dateTime)
        {
            return (dateTime ?? DateTime.Now).ToString("dd MMMM yyyy", new CultureInfo("tr-TR", false));
        }

        public static string TurkishDateTime(this DateTime? dateTime)
        {
            return (dateTime ?? DateTime.Now).ToString("dd MMMM yyyy HH:mm", new CultureInfo("tr-TR", false));
        }

        public static string TurkishDateDay(this DateTime? dateTime)
        {
            return (dateTime ?? DateTime.Now).ToString("dd MMMM yyyy", new CultureInfo("tr-TR", false));
        }

        public static string TurkishDateTimeDay(this DateTime? dateTime)
        {
            return (dateTime ?? DateTime.Now).ToString("dd MMMM yyyy dddd HH:mm", new CultureInfo("tr-TR", false));
        }
        #endregion
    }
}
