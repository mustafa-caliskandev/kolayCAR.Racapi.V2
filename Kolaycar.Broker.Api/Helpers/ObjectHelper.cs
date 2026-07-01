using DocumentFormat.OpenXml.Office2010.ExcelAc;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Infrastructure.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace KolayCAR.Broker.API.Helpers
{
    public class ObjectHelper
    {
        public static DateTime CombineDateAndTime(string date, string time) => new DateTime(
                        Convert.ToInt32(date.Split('.')[2]),
                        Convert.ToInt32(date.Split('.')[1]),
                        Convert.ToInt32(date.Split('.')[0]),
                        Convert.ToInt32(time.Split(':')[0]),
                        Convert.ToInt32(time.Split(':')[1]),
                        Convert.ToInt32("000"));

        //public static DateTime CombineDateAndTime(string date, string time)
        //{
        //    try
        //    {
        //        var dateTime = new DateTime(
        //                Convert.ToInt32(date.Split('.')[2]),
        //                Convert.ToInt32(date.Split('.')[1]),
        //                Convert.ToInt32(date.Split('.')[0]),
        //                Convert.ToInt32(time.Split(':')[0]),
        //                Convert.ToInt32(time.Split(':')[1]),
        //                Convert.ToInt32("000"));

        //        return dateTime;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}



        //public string ObjectToXML(object data)
        //{
        //    using (var stringwriter = new System.IO.StringWriter())
        //    {
        //        var serializer = new XmlSerializer(this.GetType());
        //        serializer.Serialize(stringwriter, this);
        //        return stringwriter.ToString();
        //    }
        //}
        public static List<FormattedExtra> ParseFormattedExtra(string requestExtraList)
        {
            var extraList = new List<FormattedExtra>();
            if (string.IsNullOrEmpty(requestExtraList))
                return extraList;

            var extraArr = requestExtraList.Split('|');
       
            foreach (var extra in extraArr) 
            {
                var parts = extra.Split('~');
                extraList.Add(new FormattedExtra
                {
                    Id = parts[0].ToIntNullSafe(),
                    Piece = parts[1].ToIntNullSafe(),
                    Price = parts[2].Replace(",", ".").ToFloatNullSafe(),
                    Name = parts.Length > 3 ? parts[3] : "",
                    ApiCode = parts.Length > 4 ? parts[4] : "",
                    RentalType = parts.Length > 5 ? parts[5].ToIntNullSafe() == 0 ? ExtraRentalTypes.PerRental : ExtraRentalTypes.Daily : ExtraRentalTypes.Daily,
                    ApiPrice = parts.Length > 6 ? parts[6].ToFloatNullSafe() : 0,
                    Description = parts.Length > 7 ? parts[7].ToString() : ""
                });
            }
            return extraList;
        }

        public static string ObjectToXML(object data, string rootString = "")
        {
            var json = JsonConvert.SerializeObject(data);
            XNode node = JsonConvert.DeserializeXNode(json, rootString);

            return node.ToString();
        }

        public static GetVehiclesRequest GetVehiclesRequestEntity(GetExtrasRequest getExtrasRequest, Domain.Models.Vendor vendor) 
        {
            return new GetVehiclesRequest
            {
                VendorType = getExtrasRequest.VendorType,
                ApiKey = vendor.ApiKey,
                ApiPassword = vendor.ApiPassword,
                LanguageCode = getExtrasRequest.LanguageCode,
                CurrencyCode = getExtrasRequest.CurrencyCode,
                PickupLocationId = getExtrasRequest.PickupLocationId,
                ReturnLocationId = getExtrasRequest.ReturnLocationId,
                PickupDate = getExtrasRequest.PickupDate,
                ReturnDate = getExtrasRequest.ReturnDate,
                PickupTime = getExtrasRequest.PickupTime,
                ReturnTime = getExtrasRequest.ReturnTime,
                UserToken = getExtrasRequest.UserToken,
                CouponCode = getExtrasRequest.CouponCode
            };
        }
        public static PostReservationRequest PostReservationRequestObjectEdit(PostReservationRequest postReservationRequest, int agencyId = 0, string agencyCode = "")
        {
            var postReservationRequestEdited = new PostReservationRequest
            {
                AgencyId = agencyId,
                AgencyCode = agencyCode.TrimNullSafe(),
                MemberId = postReservationRequest.MemberId,
                ExtraList = postReservationRequest.ExtraList.TrimNullSafe(),
                ReservationToken = postReservationRequest.ReservationToken.TrimNullSafe(),
                CustomerInstutionTypeNumber = postReservationRequest.CustomerInstutionTypeNumber ?? 0,
                CustomerName = postReservationRequest.CustomerName.TrimNullSafe(),
                CustomerSurname = postReservationRequest.CustomerSurname.TrimNullSafe(),
                CustomerTelephone = postReservationRequest.CustomerTelephone.TrimNullSafe(),
                CustomerEmail = postReservationRequest.CustomerEmail.TrimNullSafe().ToLower(),
                CustomerPersonalNumber = postReservationRequest.CustomerPersonalNumber.TrimNullSafe(),
                CustomerNote = postReservationRequest.CustomerNote.TrimNullSafe(),
                CustomerExplanation = postReservationRequest.CustomerExplanation.TrimNullSafe(),
                CompanyTitle = postReservationRequest.CompanyTitle.TrimNullSafe(),
                CustomerAddress = postReservationRequest.CustomerAddress.TrimNullSafe(),
                CompanyTaxOffice = postReservationRequest.CompanyTaxOffice.TrimNullSafe(),
                CompanyTaxNumber = postReservationRequest.CompanyTaxNumber.TrimNullSafe(),
                FlightNumberArrival = postReservationRequest.FlightNumberArrival.TrimNullSafe(),
                FlightNumberDeparture = postReservationRequest.FlightNumberDeparture.TrimNullSafe(),
                CustomerIPAddress = postReservationRequest.CustomerIPAddress.TrimNullSafe(),
                PaidAmount = postReservationRequest.PaidAmount.ToFloatNullSafe(),
                UpdateReservationNumber = postReservationRequest.UpdateReservationNumber.TrimNullSafe(),
                CreditCardPaymentTypeActive = postReservationRequest.CreditCardPaymentTypeActive ?? false,
                AdvancePaymentTypeActive = postReservationRequest.AdvancePaymentTypeActive ?? false,
                BankId = postReservationRequest.BankId ?? 0,
                BankVendorId = postReservationRequest.BankVendorId ?? 0,
                CreditCardHolder = postReservationRequest.CreditCardHolder.TrimNullSafe(),
                CreditCardNumber = postReservationRequest.CreditCardNumber.TrimNullSafe(),
                ExpiredYear = postReservationRequest.ExpiredYear,
                ExpiredMonth = postReservationRequest.ExpiredMonth,
                SecurityCode = postReservationRequest.SecurityCode.TrimNullSafe(),
                InstallmentCount = postReservationRequest.InstallmentCount ?? 0,
                ThreeDPaymentActive = postReservationRequest.ThreeDPaymentActive ?? false,
                ThreeDStatus = postReservationRequest.ThreeDStatus.TrimNullSafe(),
                ThreeDAuth = postReservationRequest.TrimNullSafe(),
                ThreeDLevel = postReservationRequest.ThreeDLevel.TrimNullSafe(),
                ThreeDTxnId = postReservationRequest.ThreeDTxnId.TrimNullSafe(),
                ThreeDMd = postReservationRequest.ThreeDMd.TrimNullSafe(),
                ThreeDPnOrInfo = postReservationRequest.ThreeDPnOrInfo.TrimNullSafe(),
                SpecialDailyPrice = postReservationRequest.SpecialDailyPrice == 0 ? -1 : postReservationRequest.SpecialDailyPrice,
                SpecialOneWayFee = postReservationRequest.SpecialOneWayFee == 0 ? -1 : postReservationRequest.SpecialOneWayFee,
                IsCommissionFreePrice = postReservationRequest.IsCommissionFreePrice ?? false,
                ExtraAmount = postReservationRequest.ExtraAmount.ToFloatNullSafe(),
                SendReservationMail = postReservationRequest.SendReservationMail,
                DepartureInfo = postReservationRequest.DepartureInfo.TrimNullSafe(),
                CustomerBirthDay = postReservationRequest.CustomerBirthDay,
                VehicleImageURL = postReservationRequest.VehicleImageURL.TrimNullSafe(),
                AgencyReservationReference = postReservationRequest.AgencyReservationReference.TrimNullSafe(),
                PaymentType = postReservationRequest.PaymentType,
                ExtraPricePayToDelivery = postReservationRequest.ExtraPricePayToDelivery,
                OneWayFeePayToDelivery = postReservationRequest.OneWayFeePayToDelivery,
                CouponCode = postReservationRequest.CouponCode,
                IsSpecialWebSiteAgency = postReservationRequest.IsSpecialWebSiteAgency,
                SkyscannerRedirectID = postReservationRequest.SkyscannerRedirectID.TrimNullSafe(),
                CommercialAllowance = postReservationRequest.CommercialAllowance,
                ReservationSourceId = postReservationRequest.ReservationSourceId,
                AdvancedPaymentWithoutPayment = postReservationRequest.AdvancedPaymentWithoutPayment,
                PaidAmountAfterUsingCouponCode = postReservationRequest.PaidAmountAfterUsingCouponCode.ToFloatNullSafe(),
                HighAmountDiscountActive = postReservationRequest.HighAmountDiscountActive,
                FullCredit = postReservationRequest.FullCredit,
                CreditType = postReservationRequest.CreditType,
                CountryCode = postReservationRequest.CountryCode,
                LanguageCode = postReservationRequest.LanguageCode,
                PaymentCode = postReservationRequest.PaymentCode
            };
            return postReservationRequestEdited;
        }
        public static string DecompressToString(byte[] compressedData)
        {
            using (var input = new MemoryStream(compressedData))
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return Encoding.UTF8.GetString(output.ToArray());
            }
        }
        public static byte[] CompressString(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
                {
                    gzip.Write(bytes, 0, bytes.Length);
                }
                return output.ToArray();
            }
        }
    }
}
