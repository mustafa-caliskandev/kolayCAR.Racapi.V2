using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.Reservaway;
using KolayCAR.Broker.Domain.Models.Response.Reservaway;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Reservaway
{
    public static class ReservationMapper
    {
        public static ReservawayCreateCustomerRequest MapToCreateCustomerRequest(
            this PostReservationRequest postReservationRequest,
            ReservationToken resToken,
            ReservawayVehicleDetail vehicleDetail,
            ReservawayPlanReference planReference,
            string reservationToken,
            List<Extra> apiExtras)
        {
            var customer = postReservationRequest?.PostReservationRequestV2?.Customer;

            return new ReservawayCreateCustomerRequest
            {
                gender = "mr",
                first_name = GetFirstNonEmpty(customer?.Name, postReservationRequest?.CustomerName),
                last_name = GetFirstNonEmpty(customer?.Surname, postReservationRequest?.CustomerSurname),
                date_of_birth = FormatDate(GetFirstNonEmpty(customer?.BirthDay, postReservationRequest?.CustomerBirthDay)),
                email = GetFirstNonEmpty(customer?.Email, postReservationRequest?.CustomerEmail),
                phone = NormalizePhone(GetFirstNonEmpty(customer?.PhoneNumber, postReservationRequest?.CustomerTelephone)),
                address = GetFirstNonEmpty(customer?.Address, postReservationRequest?.CustomerAddress),
                city = GetFirstNonEmpty(postReservationRequest?.PostReservationRequestV2?.City, postReservationRequest?.City),
                country_id = vehicleDetail?.country_of_residence?.id ?? vehicleDetail?.country_of_residence_id ?? 0,
                postal_code = string.Empty,
                flight_number = GetFirstNonEmpty(postReservationRequest?.PostReservationRequestV2?.FlightNumberArrival, postReservationRequest?.FlightNumberArrival),
                reservation_token = reservationToken,
                extras = GetSelectedExtraCodes(postReservationRequest, apiExtras),
                payment_type_id = planReference?.PaymentTypeId ?? 0,
                product_type_id = planReference?.ProductTypeId ?? 0,
                vehicle_id = resToken.VehicleCode.ToLongNullSafe(),
                locale = ReservawayMapperHelper.GetLocale(postReservationRequest?.LanguageCode)
            };
        }

        private static List<string> GetSelectedExtraCodes(PostReservationRequest postReservationRequest, List<Extra> apiExtras)
        {
            var selectedExtras = new List<ReservationExtra>();

            if (postReservationRequest?.PostReservationRequestV2?.Extras?.Count > 0)
            {
                selectedExtras.AddRange(postReservationRequest.PostReservationRequestV2.Extras.Select(x => new ReservationExtra
                {
                    ExtraId = x.ExtraId,
                    ExtraCode = x.ExtraCode,
                    ApiExtraCode = x.ApiExtraCode,
                    Piece = x.Piece > 0 ? x.Piece : 1
                }));
            }
            else
            {
                selectedExtras.AddRange(ReservationHelper.GetReservationExtrasFromStringList(postReservationRequest?.ExtraList));
            }

            return selectedExtras
                .Select(x => ResolveVendorExtraCode(x, apiExtras))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        private static string ResolveVendorExtraCode(ReservationExtra selectedExtra, List<Extra> apiExtras)
        {
            if (!string.IsNullOrWhiteSpace(selectedExtra.ApiExtraCode))
                return selectedExtra.ApiExtraCode;

            if (!string.IsNullOrWhiteSpace(selectedExtra.ExtraCode))
                return selectedExtra.ExtraCode;

            var apiExtra = apiExtras?.FirstOrDefault(x => x.ExtraId == selectedExtra.ExtraId);
            return apiExtra?.ApiExtraCode.ToStringNullSafe() ?? apiExtra?.ExtraCode.ToStringNullSafe() ?? string.Empty;
        }

        private static string GetFirstNonEmpty(params string[] values)
            => values?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim() ?? string.Empty;

        private static string NormalizePhone(string phone)
        {
            var normalized = phone.ToStringNullSafe().Trim();
            if (normalized.Length <= 15)
                return normalized;

            var digits = new string(normalized.Where(char.IsDigit).ToArray());
            if (digits.Length > 15)
                digits = digits.Substring(digits.Length - 15);

            return normalized.StartsWith("+") ? $"+{digits.TrimStart('+')}" : digits;
        }

        private static string FormatDate(string date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return string.Empty;

            return DateTime.TryParse(date, out var parsedDate)
                ? parsedDate.ToString("yyyy-MM-dd")
                : date;
        }
    }
}
