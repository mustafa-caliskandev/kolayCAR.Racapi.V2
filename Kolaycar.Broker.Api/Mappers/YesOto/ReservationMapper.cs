using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Requests.YesOto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Kolaycar.Broker.Api.Mappers.YesOto
{
    public static class ReservationMapper
    {
        private static readonly string[] CustomerDateFormats =
        {
            "dd.MM.yyyy",
            "d.M.yyyy",
            "dd.MM.yyyy HH:mm:ss",
            "d.M.yyyy HH:mm:ss",
            "yyyy-MM-dd",
            "yyyy-MM-ddTHH:mm:ss",
            "MM/dd/yyyy",
            "MM/dd/yyyy h:mm:ss tt"
        };

        public static YesOtoCreateReservationRequest Map(this PostReservationRequest request, ReservationToken reservationToken, KolayCAR.Broker.Domain.Models.Vendor vendor)
        {
            return request.Map(reservationToken, vendor, null);
        }

        public static YesOtoCreateReservationRequest Map(
            this PostReservationRequest request,
            ReservationToken reservationToken,
            KolayCAR.Broker.Domain.Models.Vendor vendor,
            List<Extra> apiExtras)
        {
            if (request == null || reservationToken == null)
                return null;

            var selectedAdditionalServices = MapSelectedAdditionalServices(request, apiExtras);
            var countryCode = FirstText(request.CountryCode, "TR");

            return new YesOtoCreateReservationRequest
            {
                BrandId = vendor.ApiClientId,
                SalesChannelId = vendor.SecretKey,
                LanguageId = null,
                FirstName = request.CustomerName,
                LastName = request.CustomerSurname,
                Phone = NormalizePhone(request.CustomerTelephone),
                Email = request.CustomerEmail,
                EmailWotcharSend = false,
                BirthDate = FormatCustomerDate(request.CustomerBirthDay),
                DrivingLicenseTakingDate = null,
                TCKN = request.CustomerPersonalNumber,
                Country = request.Country,
                CountryCode = countryCode,
                City = request.City,
                CityName = request.City,
                Address = request.CustomerAddress,
                Nationality = countryCode,
                Gender = null,
                BillingInformation = CreateBillingInformation(request),
                ReservationAdditionalServiceAddress = null,
                FlightNumber = FirstText(request.FlightNumberArrival, request.FlightNumberDeparture),
                CommunicationConfirmation = request.ContactPermission ?? false,
                RentalAgreement = true,
                Id = reservationToken.APIReferenceCode,
                Post = true,
                AnadolujetCode = null,
                ApprovalNumber = FirstText(request.OrderNumber, request.AgencyReservationReference),
                Channel = null,
                GiftCoupon = null,
                LocationPay = false,
                MailPriceInfo = null,
                RedirectUrl = "/rezervasyon/onay/",
                TakeADiscounts = false,
                TransactionNumber = null,
                UsedPointBalance = null,
                UsedPointState = false,
                ClientChannel = null,
                ReservationRequestModel = new YesOtoReservationRequestModel
                {
                    BrandId = vendor.ApiClientId,
                    SalesChannelId = vendor.SecretKey,
                    LanguageId = null,
                    Location = reservationToken.APIPickupLocationCode,
                    DropOffLocation = reservationToken.APIReturnLocationCode,
                    Start = FormatApiDate(reservationToken.PickupDateTime),
                    End = FormatApiDate(reservationToken.ReturnDateTime),
                    Age = null,
                    SelectedVehicleGroup = reservationToken.VehicleCode,
                    SelectedAdditionalServices = selectedAdditionalServices,
                    UserCampaignId = null,
                    IsUserFirstReservation = false,
                    DiscPrice = 0,
                    GiftCoupon = null,
                    LocationPay = false,
                    ProcessType = "Payment",
                    PromotionToken = null,
                    RequestType = null,
                    UsedPointState = false,
                    ZubizuSaleId = null,
                    priceMatrix = null
                }
            };
        }

        private static YesOtoBillingInformation CreateBillingInformation(PostReservationRequest request)
        {
            var isCorporate = !string.IsNullOrWhiteSpace(request.CompanyTitle) ||
                              !string.IsNullOrWhiteSpace(request.CompanyTaxNumber);

            return new YesOtoBillingInformation
            {
                Address = FirstText(request.CustomerAddress, request.CompanyTitle),
                BillingType = isCorporate ? "Corporate" : "Individual",
                FullName = isCorporate
                    ? request.CompanyTitle
                    : FirstText($"{request.CustomerName} {request.CustomerSurname}".Trim(), request.CustomerName, request.CustomerSurname),
                TaxNumber = FirstText(request.CompanyTaxNumber, request.CustomerPersonalNumber),
                TaxOffice = request.CompanyTaxOffice
            };
        }

        private static List<YesOtoSelectedAdditionalService> MapSelectedAdditionalServices(PostReservationRequest request, List<Extra> apiExtras)
        {
            var selectedServices = new List<YesOtoSelectedAdditionalService>();

            if (request.PostReservationRequestV2?.Extras?.Count > 0)
            {
                selectedServices.AddRange(request.PostReservationRequestV2.Extras
                    .Select(extra => new YesOtoSelectedAdditionalService
                    {
                        id = ResolveApiExtraCode(extra, apiExtras),
                        count = extra.Piece > 0 ? extra.Piece : 1
                    }));
            }

            if (!string.IsNullOrWhiteSpace(request.ExtraList))
            {
                selectedServices.AddRange(ObjectHelper.ParseFormattedExtra(request.ExtraList)
                    .Select(extra => new YesOtoSelectedAdditionalService
                    {
                        id = ResolveApiExtraCode(extra, apiExtras),
                        count = extra.Piece > 0 ? extra.Piece : 1
                    }));
            }

            return selectedServices
                .Where(service => !string.IsNullOrWhiteSpace(service.id))
                .GroupBy(service => service.id)
                .Select(group => new YesOtoSelectedAdditionalService
                {
                    id = group.Key,
                    count = group.Sum(service => service.count > 0 ? service.count : 1)
                })
                .ToList();
        }

        private static string ResolveApiExtraCode(Extra extra, List<Extra> apiExtras)
        {
            if (extra == null)
                return null;

            if (!string.IsNullOrWhiteSpace(extra.ApiExtraCode))
                return extra.ApiExtraCode;

            var apiExtra = apiExtras?.FirstOrDefault(item =>
                item.ExtraId == extra.ExtraId ||
                string.Equals(item.ExtraCode, extra.ExtraCode, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.ApiExtraCode, extra.ExtraCode, StringComparison.OrdinalIgnoreCase));

            return FirstText(apiExtra?.ApiExtraCode, apiExtra?.ExtraCode, extra.ExtraCode);
        }

        private static string ResolveApiExtraCode(FormattedExtra extra, List<Extra> apiExtras)
        {
            if (extra == null)
                return null;

            if (!string.IsNullOrWhiteSpace(extra.ApiCode))
                return extra.ApiCode;

            var apiExtra = apiExtras?.FirstOrDefault(item =>
                item.ExtraId == extra.Id ||
                string.Equals(item.ExtraCode, extra.Id.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.ApiExtraCode, extra.Id.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase));

            return FirstText(apiExtra?.ApiExtraCode, apiExtra?.ExtraCode);
        }

        private static string NormalizePhone(string phone)
        {
            return string.IsNullOrWhiteSpace(phone)
                ? phone
                : phone.Replace(" ", string.Empty);
        }

        private static string FormatApiDate(DateTime dateTime)
        {
            return dateTime.ToString("MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
        }

        private static string FormatCustomerDate(string date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return null;

            return DateTime.TryParseExact(
                    date,
                    CustomerDateFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate)
                ? parsedDate.ToString("d.MM.yyyy 00:00:00", CultureInfo.InvariantCulture)
                : date;
        }

        private static string FirstText(params string[] values)
        {
            return values?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
        }
    }
}
