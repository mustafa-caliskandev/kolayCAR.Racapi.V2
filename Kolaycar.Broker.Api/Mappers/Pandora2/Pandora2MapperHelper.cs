using KolayCAR.Broker.Infrastructure.Extensions;
using System.Globalization;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Pandora2
{
    internal static class Pandora2MapperHelper
    {
        public static int ToStableId(string value)
        {
            var digits = new string(value.ToStringNullSafe().Where(char.IsDigit).ToArray());
            return digits.ToIntNullSafe();
        }

        public static float ToMoney(string value)
        {
            var text = value.ToStringNullSafe()
                .Replace("EUR", string.Empty)
                .Replace("TRY", string.Empty)
                .Replace("USD", string.Empty)
                .Trim();

            if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantValue))
                return invariantValue;

            if (float.TryParse(text, NumberStyles.Any, new CultureInfo("tr-TR"), out var trValue))
                return trValue;

            return text.ToFloatNullSafe();
        }
    }
}
