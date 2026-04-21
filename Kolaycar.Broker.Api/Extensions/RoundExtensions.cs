using System;
using System.Globalization;

namespace KolayCAR.Broker.API.Extensions
{
    public static class RoundExtensions
    {
        public static float Round(this float value, int round = 2, float defaultValue = 0)
        {
            if (!decimal.TryParse(value.ToString(), out var decimalValue))
            {
                return defaultValue;
            }
            var result = Math.Round(decimalValue, round);
            return (float)result;
        }

        public static decimal Round(this decimal value, int round = 2)
        {
            var result = Math.Round(value, round);
            return result;
        }
    }
}
