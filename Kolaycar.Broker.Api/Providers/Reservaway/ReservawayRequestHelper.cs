using KolayCAR.Broker.API.Mappers.Reservaway;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace KolayCAR.Broker.API.Providers.Reservaway
{
    internal static class ReservawayRequestHelper
    {
        internal const string DeeplinkPath = "/deeplink";
        internal const string VehiclesPath = "/vehicles";
        internal const string VehiclePath = "/vehicle";
        internal const string VehicleExtrasPath = "/vehicle/extras";
        internal const string ProductTypesPath = "/vehicle/product-types";
        internal const string PaymentTypesPath = "/vehicle/payment-types";
        internal const string LocationListPath = "/location/list";
        internal const string CreateCustomerPath = "/customer";
        internal const string PartnerReservePath = "/external/partner/reserve";
        internal const string CancelPath = "/customer/cancel";
        private const string UserAgent = "KolayCAR-Broker/1.0";

        internal static Dictionary<string, object> CreateHeaders(string visitorSessionId, string searchHash = null)
        {
            var headers = new Dictionary<string, object>
            {
                ["visitor-session-id"] = visitorSessionId,
                ["User-Agent"] = UserAgent
            };

            if (!string.IsNullOrWhiteSpace(searchHash))
                headers["search-hash"] = searchHash;

            return headers;
        }

        internal static Dictionary<string, object> CreatePartnerHeaders(string visitorSessionId, string partnerKey)
        {
            var headers = CreateHeaders(visitorSessionId);

            if (!string.IsNullOrWhiteSpace(partnerKey))
                headers["X-Partner-Key"] = partnerKey;

            return headers;
        }

        internal static Dictionary<string, object> CreateDeeplinkParameters(
            ReservationStepsBase request,
            Vendor vendor,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            string visitorSessionId)
        {
            return new Dictionary<string, object>
            {
                ["send-response-in-timestamps"] = "1",
                ["visitor-session-id"] = visitorSessionId,
                ["pickup_location_id"] = ResolveApiLocationId(additionalInformation.APIPickupLocationId, additionalInformation.APIPickupLocationCode),
                ["drop_off_location_id"] = ResolveApiLocationId(additionalInformation.APIReturnLocationId, additionalInformation.APIReturnLocationCode),
                ["start_date"] = FormatDate(additionalInformation.PickupDateTime),
                ["end_date"] = FormatDate(additionalInformation.ReturnDateTime),
                ["currency"] = request.CurrencyCode.ToStringNullSafe(),
                ["country_of_residence"] = ResolveCountryOfResidence(vendor),
                ["driver_age"] = ResolveDriverAge(vendor),
                ["locale"] = ReservawayMapperHelper.GetLocale(request.LanguageCode)
            };
        }

        internal static Dictionary<string, object> CreateVehicleSearchParameters(
            ReservationStepsBase request,
            Vendor vendor,
            ResponseReservationStepsAdditionalInformation additionalInformation,
            string visitorSessionId)
        {
            var parameters = new Dictionary<string, object>
            {
                ["start_date"] = FormatDate(additionalInformation.PickupDateTime),
                ["end_date"] = FormatDate(additionalInformation.ReturnDateTime),
                ["driver_age"] = ResolveDriverAge(vendor),
                ["visitor-session-id"] = visitorSessionId,
                ["currency"] = request.CurrencyCode.ToStringNullSafe(),
                ["locale"] = ReservawayMapperHelper.GetLocale(request.LanguageCode),
                ["country-of-residence"] = ResolveCountryOfResidence(vendor)
            };

            AddLocationParameter(
                parameters,
                "pickup_location_id",
                "pickup_location_code",
                additionalInformation.APIPickupLocationId,
                additionalInformation.APIPickupLocationCode);

            AddLocationParameter(
                parameters,
                "drop_off_location_id",
                "drop_off_location_code",
                additionalInformation.APIReturnLocationId,
                additionalInformation.APIReturnLocationCode);

            return parameters;
        }

        internal static string CreateVisitorSessionId(ReservationStepsBase request, ReservationToken reservationToken = null)
        {
            if (!string.IsNullOrWhiteSpace(reservationToken?.APIReferenceCode3))
                return reservationToken.APIReferenceCode3;

            if (!string.IsNullOrWhiteSpace(request?.UserToken))
                return request.UserToken;

            if (!string.IsNullOrWhiteSpace(request?.SessionCode))
                return request.SessionCode;

            if (request is GetVehiclesRequest vehiclesRequest && !string.IsNullOrWhiteSpace(vehiclesRequest.Guid))
                return vehiclesRequest.Guid;

            return Guid.NewGuid().ToString("N");
        }

        internal static int ResolveApiLocationId(int? apiLocationId, string apiLocationCode)
            => apiLocationId > 0 ? apiLocationId.Value : apiLocationCode.ToIntNullSafe();

        internal static string FormatDate(DateTime value)
            => value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

        private static void AddLocationParameter(
            Dictionary<string, object> parameters,
            string idKey,
            string codeKey,
            int? apiLocationId,
            string apiLocationCode)
        {
            var locationId = ResolveApiLocationId(apiLocationId, apiLocationCode);
            if (locationId > 0)
            {
                parameters[idKey] = locationId;
                return;
            }

            if (!string.IsNullOrWhiteSpace(apiLocationCode))
                parameters[codeKey] = apiLocationCode.Trim();
        }

        internal static string ResolveCountryOfResidence(Vendor vendor)
        {
            var configured = vendor.ApiClientId.ToStringNullSafe().Trim();
            return configured.Length == 2 ? configured.ToUpperInvariant() : "TR";
        }

        internal static int ResolveDriverAge(Vendor vendor)
            => vendor.EarliestResTime.HasValue && vendor.EarliestResTime.Value > 18 && vendor.EarliestResTime.Value < 100
                ? vendor.EarliestResTime.Value
                : 35;
    }
}
