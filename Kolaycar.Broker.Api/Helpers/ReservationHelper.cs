using AutoMapper;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Helpers
{
    public class ReservationHelper
    {
        public static string GenerateReservationNumber(long reservationId) => string.Format("{0:x}", reservationId).ToUpper();

        public static long GenerateReservationId()
        {
            try
            {
                var now = DateTime.Now;
                var resId = Convert.ToInt64(now.ToString("yyMMddHHmmssff"));
                //var resId = Convert.ToInt64(
                //    now.Year.ToString().Substring(2, 2) +
                //    now.Month.ToString().PadLeft(2, '0') +
                //    now.Day.ToString().PadLeft(2, '0') +
                //    now.Hour.ToString().PadLeft(2, '0') +
                //    now.Minute.ToString().PadLeft(2, '0') +
                //    now.Second.ToString().PadLeft(2, '0') +
                //    now.Millisecond.ToString()[..2]);
                return resId;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@GenerateReservationIdError}");
                return -1;
            }
        }

        public static List<int> GetSelectedExtraIds(string extraList)
        {
            if (!string.IsNullOrWhiteSpace(extraList))
            {
                var selectedExtraItems = extraList.Split('|');
                var selectedExtaIds = new List<int>();

                for (int i = 0; i < selectedExtraItems.Length; i++)
                    selectedExtaIds.Add(Convert.ToInt32(selectedExtraItems[i].Split('~')[0]));

                return selectedExtaIds;
            }

            return null;
        }

        public static List<string> GetSelectedExtaCodes(string extraList)
        {
            if (!string.IsNullOrWhiteSpace(extraList))
            {
                var selectedExtraItems = extraList.Split('|');
                var selectedExtaCodes = new List<string>();

                for (int i = 0; i < selectedExtraItems.Length; i++)
                    selectedExtaCodes.Add(selectedExtraItems[i].Split('~')[0]);

                return selectedExtaCodes;
            }

            return null;
        }
        public static List<string> GetSelectedExtaCodesV2(List<Extra> extras)
        {
            if (extras == null)
                return new List<string>();

            return extras.Select(x => x.ExtraCode).ToList();
        }

        public static float GetTotalExtraAmount(List<ReservationExtra> extras, int day)
        {
            float totalAmount = 0;
            foreach (var extra in extras)
            {
                if (extra.ExtraRentalType == ExtraRentalTypes.Daily)
                    totalAmount += extra.Price * extra.Piece * day;
                else
                    totalAmount += extra.Price * extra.Piece;
            }

            return totalAmount;
        }

        public static float GetTotalExtraAmount(string sourceExtraList, string selectedExtraList)
        {
            float apiExtraAmount = 0;
            if (!string.IsNullOrWhiteSpace(sourceExtraList) && !string.IsNullOrWhiteSpace(selectedExtraList))
            {
                var sourceExtras = sourceExtraList.Split('|');
                var requestExtras = selectedExtraList.Split('|');

                for (int i = 0; i < requestExtras.Length; i++)
                {
                    var requestItem = requestExtras[i].Split('~');
                    for (int j = 0; j < sourceExtras.Length; j++)
                    {
                        var sourceItem = sourceExtras[j].Split('~');
                        if (sourceItem[0] == requestItem[0])
                            apiExtraAmount += sourceItem[1].ToFloatNullSafe();
                    }
                }
            }

            return apiExtraAmount;
        }

        public static List<ReservationExtra> GetReservationExtrasFromStringList(string extras)
        {
            var extraListStr = new List<ReservationExtra>();
            if (!string.IsNullOrEmpty(extras))
            {
                var extraList = extras.Split('|');
                for (int i = 0; i < extraList.Length; i++)
                {
                    var item = extraList[i].Split('~');
                    extraListStr.Add(new ReservationExtra
                    {
                        ExtraId = Convert.ToInt32(item[4]),
                        ExtraCode = item[0],
                        Piece = Convert.ToInt32(item[1]),
                        Price = item[2].ToFloatNullSafe(),
                        ExtraName = item[3],
                        ExtraRentalType = (ExtraRentalTypes)item[5].ToIntNullSafe()
                    });
                }
            }

            return extraListStr;
        }

        public static ReservationExtra ExtraToReservationExtra(Extra extra, int piece) =>
            new ReservationExtra
            {
                ExtraId = extra.ExtraId,
                ExtraCode = extra.ExtraCode,
                ExtraName = extra.ExtraName,
                ExtraDescription = extra.ExtraDescription,
                ExtraRentalType = extra.ExtraRentalType,
                ExtraType = extra.ExtraType,
                ExtraQuantityIncreasable = extra.ExtraQuantityIncreasable,
                Price = extra.Price,
                VendorId = extra.VendorId,
                VendorName = extra.VendorName,
                Piece = piece
            };

        public static List<ReservationExtra> ExtraToReservationExtra(List<Extra> allExtras, string extras, bool useBrokerConfigurations = true)
        {
            var extrasData = new List<ReservationExtra>();
            if (!string.IsNullOrWhiteSpace(extras))
            {
                var selectedExtraItems = extras.Split('|');
                for (int i = 0; i < selectedExtraItems.Length; i++)
                    for (int j = 0; j < allExtras.Count; j++)
                        if (!useBrokerConfigurations && allExtras[j].ExtraId.ToStringNullSafe() == selectedExtraItems[i].Split('~')[4] ||
                            useBrokerConfigurations && allExtras[j].ExtraCode == selectedExtraItems[i].Split('~')[0])
                        {
                            int piece = Convert.ToInt32(selectedExtraItems[i].Split('~')[1]);
                            var item = new ReservationExtra
                            {
                                ExtraId = allExtras[j].ExtraId,
                                ExtraCode = allExtras[j].ExtraCode,
                                ExtraName = allExtras[j].ExtraName,
                                ExtraDescription = allExtras[j].ExtraDescription,
                                ExtraRentalType = allExtras[j].ExtraRentalType,
                                ExtraQuantityIncreasable = allExtras[j].ExtraQuantityIncreasable,
                                Price = allExtras[j].Price,
                                Piece = piece
                            };

                            extrasData.Add(item);
                        }
            }

            return extrasData;
        }

        public static string ChangeExtraLocalPriceToAPIPrice(string extraList, List<ReservationExtra> reservationExtras, bool useBrokerConfigurations = true)
        {
            if (reservationExtras.Count > 0)
            {
                string newApiExtraList = string.Empty;
                for (int i = 0; i < reservationExtras.Count; i++)
                {
                    var selectedExtraItems = extraList.Split('|');
                    for (int j = 0; j < selectedExtraItems.Length; j++)
                    {
                        var item = selectedExtraItems[j].Split('~');
                        if (!useBrokerConfigurations && reservationExtras[i].ExtraCode == item[0] ||
                            reservationExtras[i].ExtraCode == item[4])
                        {
                            //item[2] = reservationExtras[i].APIPrice.ToString().Replace(",", ".");
                            item[2] = reservationExtras[i].ApiPrice.ToString().Replace(",", ".");

                            newApiExtraList += $"{string.Join("~", item)}|";
                        }
                    }
                }

                return StringHelper.LastCharacterClear(newApiExtraList, "|");
            }

            return extraList;
        }

        public static float GetTotalExtraAmount(List<Extra> apiExtras, string selectedExtras, int day)
        {
            float totalExtraAmount = 0;

            if (apiExtras.Count > 0)
            {
                for (int i = 0; i < apiExtras.Count; i++)
                {
                    var selectedExtraItems = selectedExtras.Split('|');
                    for (int j = 0; j < selectedExtraItems.Length; j++)
                    {
                        var item = selectedExtraItems[j].Split('~');
                        if (apiExtras[i].ExtraCode == item[0])
                        {
                            if (apiExtras[i].ExtraRentalType == ExtraRentalTypes.Daily)
                                totalExtraAmount += apiExtras[i].ApiPrice * Convert.ToInt32(item[1]) * day;
                            else
                                totalExtraAmount += apiExtras[i].ApiPrice * Convert.ToInt32(item[1]);
                        }
                    }
                }
            }

            return totalExtraAmount;
        }

        public static DateTime GetDateTimeToDateAndTimeStrings(string date, string time)
        {
            var dateElements = date.Split('.').Select(x => Convert.ToInt32(x)).ToList();
            if (!time.Contains(':')) time = System.Net.WebUtility.UrlDecode(time);

            var timeElements = time.Split(':').Select(x => Convert.ToInt32(x)).ToList();

            return new DateTime(dateElements[2], dateElements[1], dateElements[0], timeElements[0], timeElements[1], 0);
        }

        public static List<Extra> RemoveZeroPriceExtras(List<Extra> extras)
        {
            if (extras?.Count > 0)
            {
                var zeroPriceExtras = new List<Extra>();
                foreach (var extra in extras)
                    if (extra.Price <= 0)
                        zeroPriceExtras.Add(extra);
                extras.RemoveAll(x => zeroPriceExtras.Contains(x));
                return extras;
            }
            return null;
        }

        public static string ChangeExtraCodeAndExtraId(string extraList)
        {
            if (!string.IsNullOrEmpty(extraList))
            {
                var requestExtraList = extraList.Split('|');
                string extraListStr = string.Empty;
                foreach (var extraItem in requestExtraList)
                {
                    string extraItemStr = string.Empty;
                    var extraProps = extraItem.Split('~');

                    string tempLast = extraProps[4];
                    extraProps[4] = extraProps[0];
                    extraProps[0] = tempLast;

                    extraItemStr += string.Join('~', extraProps);
                    extraListStr += extraItemStr + "|";
                }

                return StringHelper.LastCharacterClear(extraListStr, "|");
            }

            return string.Empty;
        }

        public static void FillGetExtrasRequest(GetExtrasRequest getExtrasRequest, ReservationToken reservationToken)
        {
            getExtrasRequest.LanguageCode = !string.IsNullOrEmpty(getExtrasRequest.LanguageCode) ? getExtrasRequest.LanguageCode : reservationToken.LanguageType.ToString();
            getExtrasRequest.CurrencyCode = reservationToken.CurrencyType.ToString();
            getExtrasRequest.PickupLocationId = reservationToken.PickupLocationId;
            getExtrasRequest.ReturnLocationId = reservationToken.ReturnLocationId;
            getExtrasRequest.PickupDate = reservationToken.PickupDateTime.ToString("dd.MM.yyyy");
            getExtrasRequest.ReturnDate = reservationToken.ReturnDateTime.ToString("dd.MM.yyyy");
            getExtrasRequest.PickupTime = reservationToken.PickupDateTime.ToString("HH:mm");
            getExtrasRequest.ReturnTime = reservationToken.ReturnDateTime.ToString("HH:mm");
        }

        public static void FillGetSummaryRequest(GetSummaryRequest getSummaryRequest, ReservationToken reservationToken)
        {
            getSummaryRequest.LanguageCode = !string.IsNullOrEmpty(getSummaryRequest.LanguageCode) ? getSummaryRequest.LanguageCode : reservationToken.LanguageType.ToString();
            getSummaryRequest.CurrencyCode = reservationToken.CurrencyType.ToString();
            getSummaryRequest.PickupLocationId = reservationToken.PickupLocationId;
            getSummaryRequest.ReturnLocationId = reservationToken.ReturnLocationId;
            getSummaryRequest.PickupDate = reservationToken.PickupDateTime.ToString("dd.MM.yyyy");
            getSummaryRequest.ReturnDate = reservationToken.ReturnDateTime.ToString("dd.MM.yyyy");
            getSummaryRequest.PickupTime = reservationToken.PickupDateTime.ToString("HH:mm");
            getSummaryRequest.ReturnTime = reservationToken.ReturnDateTime.ToString("HH:mm");
        }

        public static PostReservationRequest FillPostReservationRequest(PostReservationRequest postReservationRequest, ReservationToken reservationToken, CommonModels.Agency agency)
        {
            postReservationRequest.LanguageCode = !string.IsNullOrEmpty(postReservationRequest.LanguageCode) ? postReservationRequest.LanguageCode : reservationToken.LanguageType.ToString();
            postReservationRequest.CurrencyCode = reservationToken.CurrencyType.ToString();
            postReservationRequest.PickupLocationId = reservationToken.PickupLocationId;
            postReservationRequest.ReturnLocationId = reservationToken.ReturnLocationId;
            postReservationRequest.PickupDate = reservationToken.PickupDateTime.ToString("dd.MM.yyyy");
            postReservationRequest.ReturnDate = reservationToken.ReturnDateTime.ToString("dd.MM.yyyy");
            postReservationRequest.PickupTime = reservationToken.PickupDateTime.ToString("HH:mm");
            postReservationRequest.ReturnTime = reservationToken.ReturnDateTime.ToString("HH:mm");
            postReservationRequest.VehicleName = reservationToken.VehicleName;
            var tokenCreditType = CreditHelper.ResolveTokenCreditType(reservationToken);
            postReservationRequest.CreditType = postReservationRequest.FullCredit == true
                ? CreditType.FullCredit
                : tokenCreditType == CreditType.LimitedCredit
                    ? CreditType.LimitedCredit
                    : CreditType.Non;
            postReservationRequest.FullCredit = postReservationRequest.CreditType == CreditType.FullCredit;
            postReservationRequest.ExtraPricePayToDelivery =
                postReservationRequest.PaymentType == PaymentTypes.PayToAgency ||
                postReservationRequest.PaymentType == PaymentTypes.PayAll ||
                postReservationRequest.PaymentType == PaymentTypes.AdvancePayment
                ? !postReservationRequest.ExtraPricePayToDelivery ? agency.AdditionalProductAmountDeliveryPayment : postReservationRequest.ExtraPricePayToDelivery : false;
            postReservationRequest.OneWayFeePayToDelivery =
                postReservationRequest.PaymentType == PaymentTypes.PayToAgency ||
                postReservationRequest.PaymentType == PaymentTypes.PayAll ||
                postReservationRequest.PaymentType == PaymentTypes.AdvancePayment
                ? !postReservationRequest.OneWayFeePayToDelivery ? agency.OneWayAmountDeliveryPayment : postReservationRequest.OneWayFeePayToDelivery : false;
            ApplyLimitedCreditPaymentDelivery(postReservationRequest);

            return postReservationRequest;
        }
        public static Domain.Models.Requests.PostReservationRequestV2 FillPostReservationRequest(Domain.Models.Requests.PostReservationRequestV2 postReservationRequest, ReservationToken reservationToken, CommonModels.Agency agency)
        {
            postReservationRequest.LanguageCode = !string.IsNullOrEmpty(postReservationRequest.LanguageCode) ? postReservationRequest.LanguageCode : reservationToken.LanguageType.ToString();
            postReservationRequest.CurrencyCode = reservationToken.CurrencyType.ToString();
            postReservationRequest.PickupLocationId = reservationToken.PickupLocationId;
            postReservationRequest.ReturnLocationId = reservationToken.ReturnLocationId;
            postReservationRequest.PickupDate = reservationToken.PickupDateTime.ToString("dd.MM.yyyy");
            postReservationRequest.ReturnDate = reservationToken.ReturnDateTime.ToString("dd.MM.yyyy");
            postReservationRequest.PickupTime = reservationToken.PickupDateTime.ToString("HH:mm");
            postReservationRequest.ReturnTime = reservationToken.ReturnDateTime.ToString("HH:mm");
            postReservationRequest.VehicleName = reservationToken.VehicleName;
            var tokenCreditType = CreditHelper.ResolveTokenCreditType(reservationToken);
            postReservationRequest.CreditType = postReservationRequest.FullCredit == true
                ? CreditType.FullCredit
                : tokenCreditType == CreditType.LimitedCredit
                    ? CreditType.LimitedCredit
                    : CreditType.Non;
            postReservationRequest.FullCredit = postReservationRequest.CreditType == CreditType.FullCredit;
            postReservationRequest.Payment.ExtraPricePayToDelivery =
                postReservationRequest.Payment.PaymentType == PaymentTypes.PayToAgency ||
                postReservationRequest.Payment.PaymentType == PaymentTypes.PayAll ||
                postReservationRequest.Payment.PaymentType == PaymentTypes.AdvancePayment
                ?
                //!postReservationRequest.Payment.ExtraPricePayToDelivery ? agency.AdditionalProductAmountDeliveryPayment :                
                postReservationRequest.Payment.ExtraPricePayToDelivery
                : false;
            postReservationRequest.Payment.OneWayFeePayToDelivery =
                postReservationRequest.Payment.PaymentType == PaymentTypes.PayToAgency ||
                postReservationRequest.Payment.PaymentType == PaymentTypes.PayAll ||
                postReservationRequest.Payment.PaymentType == PaymentTypes.AdvancePayment
                ?
                        //(!postReservationRequest.Payment.OneWayFeePayToDelivery )
                        //    ? agency.OneWayAmountDeliveryPayment 
                        //    : 
                        postReservationRequest.Payment.OneWayFeePayToDelivery
                : false;
            ApplyLimitedCreditPaymentDelivery(postReservationRequest);

            return postReservationRequest;
        }

        public static void ApplyLimitedCreditPaymentDelivery(PostReservationRequest postReservationRequest)
        {
            if (postReservationRequest?.CreditType != CreditType.LimitedCredit)
                return;

            postReservationRequest.ExtraPricePayToDelivery = true;
            postReservationRequest.OneWayFeePayToDelivery = false;
        }

        public static void ApplyLimitedCreditPaymentDelivery(Domain.Models.Requests.PostReservationRequestV2 postReservationRequest)
        {
            if (postReservationRequest?.CreditType != CreditType.LimitedCredit || postReservationRequest.Payment == null)
                return;

            postReservationRequest.Payment.ExtraPricePayToDelivery = true;
            postReservationRequest.Payment.OneWayFeePayToDelivery = false;
        }

        public static string CheckGetVehiclesRequestRequireProps(GetVehiclesRequest getVehiclesRequest, Parametre maxAllowedAdvanceReservationDays, Label label)
        {

            DateTime tarihDateTime;
            DateTime tarihDateTime2;
            DateTime saatDateTime;
            DateTime saatDateTime2;

            if (!DateTime.TryParseExact(getVehiclesRequest.PickupDate, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tarihDateTime) || !DateTime.TryParseExact(getVehiclesRequest.ReturnDate, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tarihDateTime2))
            {
                return "Incorrect date format. The format should be 'dd.mm.yyyy'";
            }
            if (!DateTime.TryParseExact(getVehiclesRequest.PickupTime, "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out saatDateTime) ||
                !DateTime.TryParseExact(getVehiclesRequest.ReturnTime, "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out saatDateTime2))
            {
                return "Incorrect time format. The format should be 'HH:mm'";
            }
            var pickupDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.PickupDate, getVehiclesRequest.PickupTime);
            //var returnDateTime = ObjectHelper.CombineDateAndTime(getVehiclesRequest.ReturnDate, getVehiclesRequest.ReturnTime);

            if (maxAllowedAdvanceReservationDays?.Deger?.ToIntNullSafe() > 0)
                if ((pickupDateTime - DateTime.Now).Days >= maxAllowedAdvanceReservationDays.Deger.ToIntNullSafe())
                    return label.Labeladi;
            if (pickupDateTime < DateTime.Now || pickupDateTime < DateTime.Now)
                return "Please check the date, availability queries cannot be sent with the past date!";
            if (string.IsNullOrEmpty(getVehiclesRequest.LanguageCode))
                return "LanguageCode field is required!";
            else if (string.IsNullOrEmpty(getVehiclesRequest.CurrencyCode))
                return "CurrencyCode field is required!";
            else if (getVehiclesRequest.PickupLocationId <= 0)
                return "PickupLocationId field is required!";
            else if (getVehiclesRequest.ReturnLocationId <= 0)
                return "ReturnLocationId field is required!";
            else if (string.IsNullOrEmpty(getVehiclesRequest.PickupDate))
                return "PickupDate field is required!";
            else if (string.IsNullOrEmpty(getVehiclesRequest.ReturnDate))
                return "ReturnDate field is required!";
            else if (string.IsNullOrEmpty(getVehiclesRequest.PickupTime))
                return "PickupTime field is required!";
            else if (string.IsNullOrEmpty(getVehiclesRequest.ReturnTime))
                return "ReturnTime field is required!";
            else
                return string.Empty;
        }

        public static string CheckPostReservationRequestRequireProps(PostReservationRequest postReservationRequest, UserRoles userRole)
        {
            if (string.IsNullOrEmpty(postReservationRequest.ReservationToken.TrimNullSafe()))
                return "Invalid parameters!(empty reservationToken)";
            else if (string.IsNullOrEmpty(postReservationRequest.CustomerName.TrimNullSafe()))
                return "CustomerName field is required!";
            else if (string.IsNullOrEmpty(postReservationRequest.CustomerSurname.TrimNullSafe()))
                return "CustomerSurname field is required!";
            else if (string.IsNullOrEmpty(postReservationRequest.CustomerTelephone.TrimNullSafe()))
                return "CustomerTelephone field is required!";
            else if (string.IsNullOrEmpty(postReservationRequest.CustomerEmail.TrimNullSafe()))
                return "CustomerEmail field is required!";
            else if (string.IsNullOrEmpty(postReservationRequest.AgencyReservationReference.TrimNullSafe()) && userRole == UserRoles.External)
                return "AgencyReservationReference field is required!";
            else if (!string.IsNullOrEmpty(postReservationRequest.FlightNumberArrival) && postReservationRequest.FlightNumberArrival.Length > 15)
                return "FlightNumberArrival field must be maximum 15 characters!";
            else
                return string.Empty;
        }
        public static string CheckPostReservationRequestRequireProps(Domain.Models.Requests.PostReservationRequestV2 postReservationRequest, UserRoles userRole, CommonModels.Agency agency)
        {
            if (postReservationRequest == null)
                return "Request body is required.";

            else if (postReservationRequest.Customer == null)
                return "Customer is required.";

            else if (postReservationRequest.Payment == null)
                return "Payment is required.";

            else if (postReservationRequest.Pricing == null)
                return "Pricing is required.";

            else if (agency == null)
                return "Agency information could not be resolved.";

            else if (string.IsNullOrEmpty(postReservationRequest.ReservationToken.TrimNullSafe()))
                return "ReservationToken is required.";

            else if (string.IsNullOrEmpty(postReservationRequest.Customer.Name.TrimNullSafe()))
                return "Customer.Name is required.";

            else if (string.IsNullOrEmpty(postReservationRequest.Customer.Surname.TrimNullSafe()))
                return "Customer.Surname is required.";

            else if (string.IsNullOrEmpty(postReservationRequest.Customer.PhoneNumber.TrimNullSafe()))
                return "Customer.PhoneNumber is required.";

            else if (string.IsNullOrEmpty(postReservationRequest.Customer.Email.TrimNullSafe()))
                return "Customer.Email is required.";

            else if (string.IsNullOrEmpty(postReservationRequest.AgencyReservationReference.TrimNullSafe())
                    && userRole == UserRoles.External && !agency.AgencyName.Contains("Tatilburada"))
                return "AgencyReservationReference is required for external users.";

            else if (!string.IsNullOrEmpty(postReservationRequest.FlightNumberArrival)
                    && postReservationRequest.FlightNumberArrival.Length > 15)
                return "FlightNumberArrival must be a maximum of 15 characters.";

            else if (postReservationRequest.Extras?.Count > 0)
            {
                bool hasInvalidExtra = postReservationRequest.Extras
                    .Any(x => string.IsNullOrEmpty(x.Code) || x.Piece == 0);

                if (hasInvalidExtra)
                    return "Invalid extras detected. Each extra must have a valid Code and Piece > 0.";
            }

            return string.Empty;
        }
        public static bool CheckPickUpDate(ReservationToken reservationToken)
        {
            return reservationToken.PickupDateTime < DateTime.Now;
        }

        public static bool IsBetweenDate(DateTime input, DateTime date1, DateTime date2)
        {
            return (input.Date >= date1 && input <= date2.Date.AddDays(1).AddTicks(-1));
        }

        public static bool CheckClosedVendor(List<Locationvendorcloseddate> locationVendorClosedDateList, DateTime pickupDate)
        {
            foreach (var locationVendorClosedDate in locationVendorClosedDateList)
            {
                if (IsBetweenDate(pickupDate, locationVendorClosedDate.Startdate, locationVendorClosedDate.Enddate))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsSpecialExtraPriceUse(string extras, List<Extra> apiExtras, float profitMarkup, PaymentTypes paymentType, PostReservationRequest postReservationRequest, CommonModels.Agency agency, CommonModels.Vendor vendor)
        {
            if (apiExtras != null && apiExtras.Count > 0)
            {
                if (!string.IsNullOrEmpty(extras))
                {
                    var extraList = extras.Split('|');
                    for (int i = 0; i < extraList.Length; i++)
                    {
                        string requestExtraCode = extraList[i].Split('~')[0];
                        float requestExtraPrice = extraList[i].Split('~')[2].ToFloatNullSafe();
                        float requestExtraPriceWithoutProfitMarkup = vendor.AdditionalProductWorkingType == VendorWorkingTypes.ProfitMarkup ?
                            CalculationHelper.RemoveProfitMarkup(profitMarkup, PriceRoundingTypes.DoNotRounding, requestExtraPrice) :
                            requestExtraPrice;
                        var apiExtra = apiExtras.Where(x => x.ExtraCode == requestExtraCode).FirstOrDefault();
                        if (paymentType == PaymentTypes.PayOnDelivery ||
                            (paymentType == PaymentTypes.AdvancePayment && agency.AdvancePaymentAmountByAgencyCommissionActive) ||
                            (paymentType == PaymentTypes.AdvancePayment && !agency.AdvancePaymentAmountByAgencyCommissionActive && requestExtraPriceWithoutProfitMarkup != apiExtra.Price) ||
                            (paymentType == PaymentTypes.AdvancePayment && (postReservationRequest.SpecialDailyPrice != -1 || postReservationRequest.SpecialOneWayFee != -1)))

                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static List<ReservationExtra> GetSelectedReservationExtras(List<Extra> allExtras, string selectedExtras)
        {
            var reservationExtras = new List<ReservationExtra>();
            if (allExtras?.Count > 0 && !string.IsNullOrEmpty(selectedExtras))
            {
                var selectedExtrasArr = selectedExtras.Split('|');
                var config = new MapperConfiguration(
                    cfg => cfg.CreateMap<Extra, ReservationExtra>(),
                    Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
                var mapper = new Mapper(config);

                foreach (var selectedExtra in selectedExtrasArr)
                {
                    var selectedExtraItem = selectedExtra.Split('~');

                    var selectedExtraId = selectedExtraItem[0].ToIntNullSafe();
                    var selectedExtraPiece = selectedExtraItem[1].ToIntNullSafe();
                    var selectedExtraPrice = selectedExtraItem.Length > 2 ? selectedExtra.Split('~')[2].ToFloatNullSafe() : 0;

                    var localextra = allExtras.Where(e => e.ExtraId == selectedExtraId).FirstOrDefault();
                    if (localextra != null)
                    {
                        var reservationExtra = mapper.Map<ReservationExtra>(localextra);
                        reservationExtra.Piece = selectedExtraPiece;
                        reservationExtra.Price = selectedExtraPrice != 0 ? selectedExtraPrice : reservationExtra.Price;
                        reservationExtras.Add(reservationExtra);
                    }
                }

            }

            return reservationExtras;
        }

        public static string ReservationExtraToStringList(List<ReservationExtra> reservationExtras)
        {
            string result = string.Empty;

            if (reservationExtras != null && reservationExtras.Count > 0)
            {
                foreach (var extra in reservationExtras)
                {
                    result += $"{extra.ExtraId}~{extra.Piece}~{extra.Price.ToString().Replace(",", ".")}~{extra.ExtraName}~{extra.ExtraCode}~{(int)extra.ExtraRentalType}|";
                }

                result = StringHelper.LastCharacterClear(result, "|");
            }

            return result;
        }

        public static string ReservationExtraToApiStringList(List<ReservationExtra> reservationExtras)
        {
            string result = string.Empty;

            if (reservationExtras != null && reservationExtras.Count > 0)
            {
                foreach (var extra in reservationExtras)
                {
                    result += $"{extra.ExtraCode}~{extra.Piece}~{extra.Price.ToString().Replace(",", ".")}~{extra.ExtraName}~{extra.ExtraId}~{(int)extra.ExtraRentalType}|";
                }

                result = StringHelper.LastCharacterClear(result, "|");
            }

            return result;
        }

        public static float GetAPITotalAmount(ReservationToken reservationToken, CommonModels.Vendor vendor, float apiExtraAmount)
        {
            float apiDailyPrice = reservationToken.APIDailyPrice;
            float apiAdditonalProductAmount = apiExtraAmount;
            float apiOneWayFee = reservationToken.APIOneWayFee;

            if (vendor.RentalWorkingType == VendorWorkingTypes.Commission && vendor.ProfitMarkupDailyPrice > 0)
                apiDailyPrice = CalculationHelper.ExtractCommission(vendor.ProfitMarkupDailyPrice, vendor.PriceRoundingType, apiDailyPrice);

            if (vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission && vendor.ProfitMarkupAdditionalProducts > 0)
                apiAdditonalProductAmount = CalculationHelper.ExtractCommission(vendor.ProfitMarkupAdditionalProducts, vendor.PriceRoundingType, apiAdditonalProductAmount);

            if (vendor.OneWayFeeWorkingType == VendorWorkingTypes.Commission && vendor.ProfitMarkupOneWayFee > 0)
                apiOneWayFee = CalculationHelper.ExtractCommission(vendor.ProfitMarkupOneWayFee, vendor.PriceRoundingType, apiOneWayFee);

            return CalculationHelper.CalculateTotalPrice(vendor.PriceRoundingType, reservationToken.RentalDuration, apiDailyPrice, apiAdditonalProductAmount, apiOneWayFee);
        }

        public static float GetAPIDailyPrice(float apiDailyPrice, CommonModels.Vendor vendor) =>
            vendor.RentalWorkingType == VendorWorkingTypes.Commission ?
                CalculationHelper.ExtractCommission(vendor.ProfitMarkupDailyPrice, vendor.PriceRoundingType, apiDailyPrice) :
                apiDailyPrice;

        public static float GetAPIAdditionalPrice(float apiExtraAmount, CommonModels.Vendor vendor) =>
            vendor.AdditionalProductWorkingType == VendorWorkingTypes.Commission ?
                CalculationHelper.ExtractCommission(vendor.ProfitMarkupAdditionalProducts, vendor.PriceRoundingType, apiExtraAmount) :
                apiExtraAmount;

        public static float GetAPIOneWayFee(float apiOneWayFee, CommonModels.Vendor vendor) =>
            vendor.OneWayFeeWorkingType == VendorWorkingTypes.Commission ?
                CalculationHelper.ExtractCommission(vendor.ProfitMarkupOneWayFee, vendor.PriceRoundingType, apiOneWayFee) :
                apiOneWayFee;

        public static float GetAgencyCommissionWithAgencyProfitMarkup(float price, float agencyRentalProfitMarkup, float commission)
        {
            var agencyRentalProfitMarkupFreePrice = price - (price - price * 100 / (100 + agencyRentalProfitMarkup));
            var commissionPrice = (agencyRentalProfitMarkupFreePrice * commission) / 100;
            return (commissionPrice + (price - price * 100 / (100 + agencyRentalProfitMarkup))) * 100 / price;
        }

        public static bool CheckCreditPermission(CommonModels.Agency agency, CreditType vehicleCreditType)
        {
            return CreditHelper.AgencyAllowsCreditType(agency, vehicleCreditType);
        }

    }
}
