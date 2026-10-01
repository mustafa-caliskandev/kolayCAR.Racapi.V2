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
        private static readonly HashSet<string> InternationalDialingCodes = new HashSet<string>
        {
            "1", "7",
            "20", "27", "30", "31", "32", "33", "34", "36", "39", "40", "41", "43", "44", "45", "46", "47", "48", "49",
            "51", "52", "53", "54", "55", "56", "57", "58", "60", "61", "62", "63", "64", "65", "66",
            "81", "82", "84", "86", "90", "91", "92", "93", "94", "95", "98",
            "211", "212", "213", "216", "218", "220", "221", "222", "223", "224", "225", "226", "227", "228", "229",
            "230", "231", "232", "233", "234", "235", "236", "237", "238", "239", "240", "241", "242", "243", "244", "245", "246", "248", "249",
            "250", "251", "252", "253", "254", "255", "256", "257", "258", "260", "261", "262", "263", "264", "265", "266", "267", "268", "269",
            "290", "291", "297", "298", "299",
            "350", "351", "352", "353", "354", "355", "356", "357", "358", "359",
            "370", "371", "372", "373", "374", "375", "376", "377", "378", "379", "380", "381", "382", "383", "385", "386", "387", "389",
            "420", "421", "423",
            "500", "501", "502", "503", "504", "505", "506", "507", "508", "509",
            "590", "591", "592", "593", "594", "595", "596", "597", "598", "599",
            "670", "672", "673", "674", "675", "676", "677", "678", "679",
            "680", "681", "682", "683", "685", "686", "687", "688", "689",
            "690", "691", "692",
            "800", "808", "850", "852", "853", "855", "856", "870", "878", "880", "881", "882", "883", "886", "888",
            "960", "961", "962", "963", "964", "965", "966", "967", "968",
            "970", "971", "972", "973", "974", "975", "976", "977", "979",
            "992", "993", "994", "995", "996", "998"
        };

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
            var trimmedPhoneNumber = phoneNumber.ToStringNullSafe().TrimNullSafe();
            var isInternationalNumber = trimmedPhoneNumber.StartsWith("+") || trimmedPhoneNumber.StartsWith("00");
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

            if (isInternationalNumber)
            {
                var internationalCode = ResolveInternationalDialingCode(number);
                if (!string.IsNullOrWhiteSpace(internationalCode) && number.Length > internationalCode.Length)
                {
                    return new VonarentMobile
                    {
                        code = internationalCode,
                        number = RemoveDomesticPrefix(number.Substring(internationalCode.Length))
                    };
                }
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

        private static string ResolveInternationalDialingCode(string number)
        {
            for (var length = 3; length >= 1; length--)
            {
                if (number.Length >= length)
                {
                    var candidate = number.Substring(0, length);
                    if (InternationalDialingCodes.Contains(candidate))
                        return candidate;
                }
            }

            return string.Empty;
        }

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
            var currencyCode = reservationToken.BaseVendorRequestCurrencyType.ToString();
            return !string.IsNullOrWhiteSpace(currencyCode)
                ? currencyCode
                : reservationToken.APIReferenceCode2.ToStringNullSafe();
        }
    }
}
