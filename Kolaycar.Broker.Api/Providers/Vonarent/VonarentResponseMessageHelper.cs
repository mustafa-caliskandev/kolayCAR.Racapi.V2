using KolayCAR.Broker.Domain.Models.Response.Vonarent;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Providers.Vonarent
{
    public static class VonarentResponseMessageHelper
    {
        public static string ExtractMessage(VonarentResponseBase response, params string[] fallbacks)
        {
            if (response == null)
                return GetFallbackMessage(fallbacks);

            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(response.message))
                parts.Add(response.message);

            if (response.errors != null)
            {
                var errorMessages = response.errors
                    .Where(x => !string.IsNullOrWhiteSpace(x.Key) && x.Value != null)
                    .SelectMany(x => x.Value
                        .Where(y => !string.IsNullOrWhiteSpace(y))
                        .Select(y => $"{x.Key}: {y}"))
                    .ToList();

                if (errorMessages.Count > 0)
                    parts.Add(string.Join(" | ", errorMessages));
            }

            var message = string.Join(" - ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
            return !string.IsNullOrWhiteSpace(message) ? message : GetFallbackMessage(fallbacks);
        }

        private static string GetFallbackMessage(IEnumerable<string> fallbacks)
        {
            return fallbacks?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;
        }
    }
}
