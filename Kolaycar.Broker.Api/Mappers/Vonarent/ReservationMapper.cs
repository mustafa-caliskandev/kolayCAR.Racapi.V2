using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.Vonarent;
using KolayCAR.Broker.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Mappers.Vonarent
{
    public static class ReservationMapper
    {
        public static VonarentSelectVehicleRequest MapToSelectVehicleRequest(this ReservationToken reservationToken)
        {
            return new VonarentSelectVehicleRequest
            {
                Id = reservationToken.VehicleCode,
                Currency = GetReservationCurrencyCode(reservationToken)
            };
        }

        public static VonarentSelectExtrasRequest MapToSelectExtrasRequest(this PostReservationRequest postReservationRequest, Reservation localReservation, List<Extra> apiExtras)
        {
            var selectedExtras = GetSelectedExtras(postReservationRequest, localReservation, apiExtras)
                .Where(x => !string.IsNullOrWhiteSpace(x.Id) && x.Quantity > 0)
                .GroupBy(x => x.Id)
                .Select(x => new VonarentSelectedExtraItem
                {
                    Id = x.Key,
                    Quantity = x.Sum(y => y.Quantity)
                })
                .ToList();

            return new VonarentSelectExtrasRequest
            {
                Extras = selectedExtras
            };
        }

        public static VonarentSetCustomerRequest MapToSetCustomerRequest(this PostReservationRequest postReservationRequest)
        {
            var customer = postReservationRequest?.PostReservationRequestV2?.Customer;

            return new VonarentSetCustomerRequest
            {
                FirstName = customer?.Name.ToStringNullSafe().TrimNullSafe().Length > 0
                    ? customer.Name.TrimNullSafe()
                    : postReservationRequest?.CustomerName.TrimNullSafe(),
                LastName = customer?.Surname.ToStringNullSafe().TrimNullSafe().Length > 0
                    ? customer.Surname.TrimNullSafe()
                    : postReservationRequest?.CustomerSurname.TrimNullSafe(),
                Mobile = CreateMobile(
                    customer?.PhoneNumber.ToStringNullSafe().TrimNullSafe().Length > 0
                        ? customer.PhoneNumber.TrimNullSafe()
                        : postReservationRequest?.CustomerTelephone.TrimNullSafe(),
                    postReservationRequest?.PostReservationRequestV2?.CountryCode.ToStringNullSafe().TrimNullSafe().Length > 0
                        ? postReservationRequest.PostReservationRequestV2.CountryCode.TrimNullSafe()
                        : postReservationRequest?.CountryCode.TrimNullSafe()),
                Email = customer?.Email.ToStringNullSafe().TrimNullSafe().Length > 0
                    ? customer.Email.TrimNullSafe()
                    : postReservationRequest?.CustomerEmail.TrimNullSafe()
            };
        }

        private static VonarentMobile CreateMobile(string phoneNumber, string countryCode)
        {
            var code = NormalizeDigits(countryCode);
            var number = NormalizePhoneNumber(phoneNumber);

            if (code.StartsWith("00"))
                code = code.Substring(2);

            if (number.StartsWith("00"))
                number = number.Substring(2);

            if (!string.IsNullOrWhiteSpace(code))
            {
                number = RemoveCountryCode(number, code);
                return new VonarentMobile
                {
                    code = code,
                    number = RemoveDomesticPrefix(number)
                };
            }

            if (number.StartsWith("90") && number.Length > 10)
            {
                return new VonarentMobile
                {
                    code = "90",
                    number = RemoveDomesticPrefix(number.Substring(2))
                };
            }

            if (phoneNumber.ToStringNullSafe().TrimNullSafe().StartsWith("+") && number.Length > 10)
            {
                return new VonarentMobile
                {
                    code = number.Substring(0, number.Length - 10),
                    number = RemoveDomesticPrefix(number.Substring(number.Length - 10))
                };
            }

            return new VonarentMobile
            {
                code = "90",
                number = RemoveDomesticPrefix(number)
            };
        }

        private static string NormalizePhoneNumber(string value)
            => NormalizeDigits(value);

        private static string NormalizeDigits(string value)
            => new string(value.ToStringNullSafe().Where(char.IsDigit).ToArray());

        private static string RemoveCountryCode(string number, string code)
        {
            if (!string.IsNullOrWhiteSpace(code) && number.StartsWith(code) && number.Length > code.Length)
                return number.Substring(code.Length);

            return number;
        }

        private static string RemoveDomesticPrefix(string number)
        {
            if (number.StartsWith("0") && number.Length > 10)
                return number.Substring(1);

            return number;
        }

        private static List<VonarentSelectedExtraItem> GetSelectedExtras(PostReservationRequest postReservationRequest, Reservation localReservation, List<Extra> apiExtras)
        {
            if (localReservation?.ReservationExtras?.Count > 0)
            {
                return localReservation.ReservationExtras
                    .Select(x => new VonarentSelectedExtraItem
                    {
                        Id = ResolveVendorExtraId(x.ExtraCode, x.ApiExtraCode, x.ExtraId, apiExtras),
                        Quantity = x.Piece > 0 ? x.Piece : 1
                    })
                    .ToList();
            }

            if (postReservationRequest?.PostReservationRequestV2?.Extras?.Count > 0)
            {
                return postReservationRequest.PostReservationRequestV2.Extras
                    .Select(x => new VonarentSelectedExtraItem
                    {
                        Id = ResolveVendorExtraId(x.ExtraCode, x.ApiExtraCode, x.ExtraId, apiExtras),
                        Quantity = x.Piece > 0 ? x.Piece : 1
                    })
                    .ToList();
            }

            var stringExtras = ReservationHelper.GetReservationExtrasFromStringList(postReservationRequest?.ExtraList);
            if (stringExtras?.Count > 0)
            {
                return stringExtras
                    .Select(x => new VonarentSelectedExtraItem
                    {
                        Id = ResolveVendorExtraId(x.ExtraCode, x.ApiExtraCode, x.ExtraId, apiExtras),
                        Quantity = x.Piece > 0 ? x.Piece : 1
                    })
                    .ToList();
            }

            return new List<VonarentSelectedExtraItem>();
        }

        private static string ResolveVendorExtraId(string extraCode, string apiExtraCode, int extraId, List<Extra> apiExtras)
        {
            if (!string.IsNullOrWhiteSpace(apiExtraCode))
                return apiExtraCode;

            if (!string.IsNullOrWhiteSpace(extraCode))
                return extraCode;

            var apiExtra = apiExtras?.FirstOrDefault(x => x.ExtraId == extraId);
            return apiExtra?.ApiExtraCode.ToStringNullSafe() ?? apiExtra?.ExtraCode.ToStringNullSafe() ?? string.Empty;
        }

        private static string GetReservationCurrencyCode(ReservationToken reservationToken)
        {
            var currencyCode = reservationToken.CurrencyType.ToString();
            return !string.IsNullOrWhiteSpace(currencyCode)
                ? currencyCode
                : reservationToken.APIReferenceCode2.ToStringNullSafe();
        }
    }
}
