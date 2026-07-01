using KolayCAR.Broker.Domain.Models;
using Newtonsoft.Json;

namespace KolayCAR.Broker.API.Helpers
{
    public static class CreditHelper
    {
        public static CreditType ResolveEffectiveCreditType(Vendor vendor, Agency agency, Vehicle vehicle)
        {
            var supplierCreditType = ResolveSupplierCreditType(vendor, vehicle);

            return ResolveEffectiveCreditType(agency, supplierCreditType);
        }

        public static void ApplyEffectiveCreditType(Vendor vendor, Agency agency, Vehicle vehicle)
        {
            if (vehicle == null)
                return;

            var supplierCreditType = ResolveSupplierCreditType(vendor, vehicle);
            var effectiveCreditType = ResolveEffectiveCreditType(agency, supplierCreditType);

            vehicle.CreditType = supplierCreditType;
            vehicle.FullCredit = effectiveCreditType == CreditType.FullCredit;
            vehicle.ReservationToken = UpdateReservationToken(vehicle.ReservationToken, effectiveCreditType, supplierCreditType);
        }

        public static string UpdateReservationToken(string reservationTokenJson, CreditType creditType, CreditType? apiCreditType = null)
        {
            if (string.IsNullOrWhiteSpace(reservationTokenJson))
                return reservationTokenJson;

            try
            {
                var reservationToken = JsonConvert.DeserializeObject<ReservationToken>(reservationTokenJson);
                if (reservationToken == null)
                    return reservationTokenJson;

                var tokenApiCreditType = apiCreditType ?? creditType;
                reservationToken.CreditType = creditType;
                reservationToken.APICreditType = tokenApiCreditType;
                reservationToken.FullCredit = creditType == CreditType.FullCredit;
                reservationToken.APIFullCredit = tokenApiCreditType == CreditType.FullCredit;
                return JsonConvert.SerializeObject(reservationToken);
            }
            catch (JsonException)
            {
                return reservationTokenJson;
            }
        }

        public static CreditType ResolveTokenCreditType(ReservationToken token)
        {
            if (token == null)
                return CreditType.Non;

            if (token.CreditType != CreditType.Non)
                return token.CreditType;

            return token.FullCredit || token.APIFullCredit == true
                ? CreditType.FullCredit
                : CreditType.Non;
        }

        public static CreditType ResolveTokenResponseCreditType(ReservationToken token)
        {
            if (token == null)
                return CreditType.Non;

            if (token.APICreditType.HasValue && token.APICreditType.Value != CreditType.Non)
                return token.APICreditType.Value;

            return ResolveTokenCreditType(token);
        }

        private static CreditType ResolveSupplierCreditType(Vendor vendor, Vehicle vehicle)
        {
            if (vehicle?.CreditType != CreditType.Non)
                return vehicle.CreditType;

            if (vehicle?.FullCredit == true)
                return CreditType.FullCredit;

            // Limited Credit bilgisini dinamik dönmeyen sağlayıcılarda paneldeki vendor ayarı belirler.
            if (vendor?.CreditType == CreditType.LimitedCredit && vendor.VendorType != VendorTypes.Yolcu360v2)
                return CreditType.LimitedCredit;

            return CreditType.Non;
        }

        private static CreditType ResolveEffectiveCreditType(Agency agency, CreditType supplierCreditType)
        {
            return supplierCreditType != CreditType.Non && AgencyAllowsCreditType(agency, supplierCreditType)
                ? supplierCreditType
                : CreditType.Non;
        }

        public static bool AgencyAllowsCreditType(Agency agency, CreditType creditType)
        {
            if (creditType == CreditType.Non)
                return true;

            if (agency == null)
                return false;

            if (agency.CreditType == creditType)
                return true;

            return agency.CreditType == CreditType.FullCredit && creditType == CreditType.LimitedCredit;
        }
    }
}
