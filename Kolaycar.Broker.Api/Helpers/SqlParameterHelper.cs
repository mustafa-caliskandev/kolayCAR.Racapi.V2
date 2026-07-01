using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using BrokerReservationHelper = KolayCAR.Broker.API.Helpers.ReservationHelper;

namespace KolayCAR.Broker.API.Helpers
{
    public static class SqlParameterHelper
    {
        public static SqlParameter[] GetReservationsNewFilterSqlParameters(string customerName, string customerSurname, string customerPhone, string customerEmail)
        {
            return new SqlParameter[] {

                    new SqlParameter
                    {
                        ParameterName = "@customerName",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = !string.IsNullOrEmpty(customerName) ? customerName : (object)DBNull.Value
                    },
                    new SqlParameter
                    {
                        ParameterName = "@customerSurname",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = !string.IsNullOrEmpty(customerSurname) ? customerSurname : (object)DBNull.Value
                    },
                    new SqlParameter
                    {
                        ParameterName = "@customerPhone",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = !string.IsNullOrEmpty(customerPhone) ? customerPhone : (object)DBNull.Value
                    },
                     new SqlParameter
                    {
                        ParameterName = "@customerEmail",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = !string.IsNullOrEmpty(customerEmail) ? customerEmail : (object)DBNull.Value
                    }
                };
        }
        public static SqlParameter[] CancelLocalReservationSqlParameters(PostCancelReservationRequest postCancelReservationRequest, ReservationPenalty reservationCancelletionPenalty)
        {
            return new SqlParameter[] {
                        new SqlParameter
                        {
                            ParameterName = "@OPERATIONID",
                            SqlDbType = SqlDbType.Int,
                            Value = (int)AgencyOperationTypes.UpdateReservation
                        },
                        new SqlParameter
                        {
                            ParameterName = "@RESERVATIONNO",
                            SqlDbType = SqlDbType.NVarChar,
                            Value = postCancelReservationRequest.ReservationNumber
                        },
                        new SqlParameter
                        {
                            ParameterName = "@USEREMAIL",
                            SqlDbType = SqlDbType.NVarChar,
                            Value = postCancelReservationRequest.CustomerEmail
                        },
                        new SqlParameter
                        {
                            ParameterName = "@RESSTATUSNOTE",
                            SqlDbType = SqlDbType.NVarChar,
                            Value = postCancelReservationRequest.CancelNote ?? (object)DBNull.Value
                        },
                        new SqlParameter
                        {
                            ParameterName = "@BROKERCANCELRESERVATION",
                            SqlDbType = SqlDbType.Bit,
                            Value = postCancelReservationRequest.IsBrokerReservation
                        },
                        new SqlParameter
                        {
                            ParameterName = "@CANCELLATIONPENALTYRATE",
                            SqlDbType = SqlDbType.Int,
                            Value = reservationCancelletionPenalty.PenaltyRate
                        },
                        new SqlParameter
                        {
                            ParameterName = "@CANCELLATIONREFUNDEDAMOUNT",
                            SqlDbType = SqlDbType.Decimal,
                            Value = reservationCancelletionPenalty.RefundedAmount
                        },
                        new SqlParameter
                        {
                            ParameterName = "@CANCELLATIONPENALTYHOUR",
                            SqlDbType = SqlDbType.Int,
                            Value = reservationCancelletionPenalty.DifferenceHour
                        },
                        new SqlParameter
                        {
                            ParameterName = "@PENALTYAMOUNT",
                            SqlDbType = SqlDbType.Decimal,
                            Value = reservationCancelletionPenalty.PenaltyAmount
                        },
                        new SqlParameter
                        {
                            ParameterName = "@PenaltyStatus",
                            SqlDbType = SqlDbType.Int,
                            Value =  (int)postCancelReservationRequest.PenaltyStatus
                        },
                        new SqlParameter
                        {
                            ParameterName = "@CancelReasonId",
                            SqlDbType = SqlDbType.Int,
                            Value = postCancelReservationRequest.CancelReasonId
                        },
                        new SqlParameter
                        {
                            ParameterName = "@UserId",
                            SqlDbType = SqlDbType.NVarChar,
                            Value = string.IsNullOrEmpty(postCancelReservationRequest.UserId) ? (object)DBNull.Value : postCancelReservationRequest.UserId
                        }
                    };
        }

        public static SqlParameter[] PostReservationLocalSqlParameters(long reservationId, PostReservationRequest postReservationRequest, ReservationToken reservationToken, Vendor vendor, float dailyPrice, float totalAmount, float serviceCharge, string tempExtraList, float apiDailyPrice, float apiExtraAmount, float apiTotalAmount, string vendorLogoUrl, bool isOffice, PostPaymentResponse postPaymentResponse, float discountAmount, Agency agency, float packetPricePremium, Configurations configurations, float? discountValue, int? couponId, Vehicle vehicle = null)
        {
            return new SqlParameter[] {
                                new SqlParameter
                                {
                                    ParameterName = "@RESNO",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = ReservationHelper.GenerateReservationNumber(reservationId)
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RESID",
                                    SqlDbType = SqlDbType.BigInt,
                                    Value = reservationId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@AGENCYID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.AgencyId ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@MEMBERID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.MemberId ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLENAME",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.VehicleName ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLEID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = reservationToken.VehicleId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PICKUPLOCATIONID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.PickupLocationId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RETURNLOCATIONID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.ReturnLocationId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PICKUPDATE",
                                    SqlDbType = SqlDbType.DateTime,
                                    Value = new DateTime(
                                        Convert.ToInt32(postReservationRequest.PickupDate.Split('.')[2]),
                                        Convert.ToInt32(postReservationRequest.PickupDate.Split('.')[1]),
                                        Convert.ToInt32(postReservationRequest.PickupDate.Split('.')[0]),
                                        Convert.ToInt32(postReservationRequest.PickupTime.Split(':')[0]),
                                        Convert.ToInt32(postReservationRequest.PickupTime.Split(':')[1]),
                                        Convert.ToInt32("000"))
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RETURNDATE",
                                    SqlDbType = SqlDbType.DateTime,
                                    Value = new DateTime(
                                        Convert.ToInt32(postReservationRequest.ReturnDate.Split('.')[2]),
                                        Convert.ToInt32(postReservationRequest.ReturnDate.Split('.')[1]),
                                        Convert.ToInt32(postReservationRequest.ReturnDate.Split('.')[0]),
                                        Convert.ToInt32(postReservationRequest.ReturnTime.Split(':')[0]),
                                        Convert.ToInt32(postReservationRequest.ReturnTime.Split(':')[1]),
                                        Convert.ToInt32("000"))
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = vendor.VendorId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SERVICEVENDORID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = reservationToken.APIVendorId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RENTALDURATION",
                                    SqlDbType = SqlDbType.Int,
                                    Value = reservationToken.RentalDuration
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DAILYPRICE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = dailyPrice
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@EXTRAAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.ExtraAmount
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ONEWAYAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.SpecialOneWayFee != -1 ? postReservationRequest.SpecialOneWayFee : reservationToken.OneWayFee
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@TOTALAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = totalAmount + serviceCharge
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PAIDAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.PaidAmount + serviceCharge
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@LANGID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)postReservationRequest.LanguageCode.ToEnum<LanguageTypes>() + 1
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CURRENCYID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)postReservationRequest.CurrencyCode.ToEnum<CurrencyTypes>() + 1
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERINSTUTIONTYPENO",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.CustomerInstutionTypeNumber ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERTITLE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CompanyTitle ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERADDRESS",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CustomerAddress ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERNAME",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CustomerName
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERSURNAME",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CustomerSurname
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERTELEPHONE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CustomerTelephone ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMEREMAIL",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CustomerEmail
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMEREXPLANATION",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CustomerExplanation ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COMPANYTAXOFFICE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CompanyTaxOffice ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COMPANYTAXNO",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CompanyTaxNumber ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERPERSONALNUMBER",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CustomerPersonalNumber ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERNOTE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CustomerNote ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@FLIGHTNOARRIVAL",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.FlightNumberArrival ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@FLIGHTNODEPARTURE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.FlightNumberDeparture ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PDF",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@IP",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CustomerIPAddress ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@EXTRAIDLIST",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = tempExtraList ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIDAILYPRICE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = apiDailyPrice
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIEXTRAAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = apiExtraAmount
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIONEWAYFEE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = BrokerReservationHelper.GetAPIOneWayFee(reservationToken.APIOneWayFee, vendor)
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APITOTALAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = apiTotalAmount
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORPHONE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIVendorPhone ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDOREMAIL",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIVendorEmail ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORLOGO",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = vendorLogoUrl ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORNAME",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIVendorName ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SERVICECHARGE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = serviceCharge
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SERVICECHARGECURRENCYID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)vendor.ServiceChargeCurrencyType + 1
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DEPOSITPRICE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = reservationToken.DepositPrice.ToDecimalNullAvailable() ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORMINDRIVERAGE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = reservationToken.VendorMinimumDriverAge.ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORMINDRIVINGLICENSEAGE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = reservationToken.VendorMinimumDrivingLicenseAge.ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SENDRESERVATIONMAIL",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.SendReservationMail
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERBIRTHDAY",
                                    SqlDbType = SqlDbType.DateTime,
                                    Value = string.IsNullOrEmpty(postReservationRequest.CustomerBirthDay) ? (object)DBNull.Value : postReservationRequest.CustomerBirthDay
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DEPARTUREINFO",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.DepartureInfo ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLEIMAGEURL",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.VehicleImageUrl ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ISOFFICE",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = isOffice
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@FUELTYPE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = vehicle != null ? ((int)vehicle.FuelType + 1).ToString() : ((int)reservationToken.FuelType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@TRANSMISSIONTYPE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = vehicle != null ? ((int)vehicle.TransmissionType + 1).ToString() : ((int)reservationToken.TransmissionType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIREFERENCECODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIReferenceCode ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIREFERENCECODE2",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIReferenceCode2 ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIREFERENCECODE3",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIReferenceCode3 ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APICURRENCYID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)vendor.CurrencyType + 1
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@AGENCYCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.AgencyCode ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DEPOSITCREDITCARDREQUIRED",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = reservationToken.DepositCreditCardRequired
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@AGENCYRESERVATIONREFERENCE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.AgencyReservationReference ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIPHONEACTIVE",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = vendor.APIPhoneActive
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PAYMENTRESULTMESSAGE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postPaymentResponse != null ? postPaymentResponse.Message ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PAYMENTRESULTCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postPaymentResponse != null ? postPaymentResponse.Code ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BANKID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postPaymentResponse.PaymentResult.BankId != 0 ? postPaymentResponse.PaymentResult.BankId : (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PROVISIONNUMBER",
                                    SqlDbType = SqlDbType.NVarChar,
                                    //Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postReservationRequest.ProvisionNumber /* postPaymentResponse.PaymentResult.ProvisionNumber */ ?? postReservationRequest.ProvisionNumber ?? (object)DBNull.Value : (object)DBNull.Value
                                    Value = postReservationRequest != null ? postReservationRequest.ProvisionNumber ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ORDERNUMBER",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest != null ? postReservationRequest.OrderNumber ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PAYMENTAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postPaymentResponse.PaymentResult.PaymentAmount != 0 ? postPaymentResponse.PaymentResult.PaymentAmount : (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ALERTERRORCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postPaymentResponse.PaymentResult.AlertErrorCode ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@LOGRESULTNUMBER",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postPaymentResponse.PaymentResult.LogResultNumber ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@LOGERRORCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postPaymentResponse.PaymentResult.LogErrorCode ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PAYMENTTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)postReservationRequest.PaymentType
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CREDITCARDDISCOUNTPERCENT",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.PaymentType == PaymentTypes.PayAll ? (int)agency.CreditCardDiscountPercent : 0
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COUPONDISCOUNTAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = discountAmount.ToDecimalNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COUPONID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = couponId ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SPECIALDAILYPRICE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.SpecialDailyPrice.ToDecimalNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SPECIALONEWAYFEE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.SpecialOneWayFee.ToDecimalNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BASEREQUESTCURRENCYID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)reservationToken.BaseVendorRequestCurrencyType + 1
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SIPPCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = vehicle != null ? vehicle.SippCode ?? (object)DBNull.Value : reservationToken.SippCode ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLECODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.VehicleCode ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@EXTRAPRICEPAYTODELIVERY",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.ExtraPricePayToDelivery
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ONEWAYFEEPAYTODELIVERY",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.OneWayFeePayToDelivery
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ISSPECIALWEBSITEAGENCY",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.IsSpecialWebSiteAgency
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RESERVATIONTOKEN",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = JsonConvert.SerializeObject(reservationToken)
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLETYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = vehicle != null ? ((int)vehicle.VehicleType + 1).ToString() : ((int)reservationToken.VehicleType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLECATEGORYTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = vehicle != null ? ((int)vehicle.VehicleCategoryType + 1).ToString() : ((int)reservationToken.VehicleCategoryType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PASSANGERQUANTITYTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = vehicle != null ? ((int)vehicle.PassangerQuantityType + 1).ToString() :  ((int)reservationToken.PassangerQuantityType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BAGGAGEQUANTITYTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = vehicle != null ? ((int)vehicle.BaggageQuantityType + 1).ToString() : ((int)reservationToken.BaggageQuantityType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SKYSCANNERREDIRECTID",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.SkyscannerRedirectID ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COMMERCIALALLOWANCE ",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.CommercialAllowance
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SOURCEID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.ReservationSourceId ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@TOTALKMLIMIT",
                                    SqlDbType = SqlDbType.Int,
                                    Value = vehicle != null ? vehicle.TotalKMLimit ?? (object)DBNull.Value :  reservationToken.TotalKmLimit
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DEFAULTCUSTOMERMAILADDRESS",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = configurations.DefaultCustomerMailAddress ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RENTALWORKINGTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)vendor.RentalWorkingType
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ADDITIONALPRODUCTWORKINGTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)vendor.AdditionalProductWorkingType
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ONEWAYFEEWORKINGTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)vendor.OneWayFeeWorkingType
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@AGENCYRENTALPROFITMARKUP",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = !agency.FreePriceShowActive ? agency.AgencyRentalProfitMarkup.ToDecimalNullSafe() : 0
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RESERVATIONTOKENTEXT",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.ReservationToken.ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ADVANCEDPAYMENTWITHOUTPAYMENT",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.AdvancedPaymentWithoutPayment
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COUPONDISCOUNTVALUE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = discountValue ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CREDITCARDNUMBER",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CreditCardNumber.RemoveSpecialCharacters().MaskCreditCardNumber()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@INSTALLMENTCOUNT",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.InstallmentCount
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BANK",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Bank.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BANKACCOUNTCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.BankAccountCode.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BANKACCOUNTINGCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.BankAccountingCode.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COUNTRY",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Country.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CITY",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.City.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DISTRICT",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.District.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CREDITCARDBANK",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CreditCardBank.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CONTACTPERMISSION",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.ContactPermission.ToBoolNullSafe()
                                },
                                 new SqlParameter
                                {
                                    ParameterName = "@INSTALLMENTCOMMISIONAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.InstallmentCommissionAmount ?? (object)DBNull.Value
                                },
                                 CreateCreditTypeParameter(postReservationRequest.CreditType),
                                  new SqlParameter
                                {
                                     ParameterName = "@PREMIUMEXTRAAMOUNT",
                                     SqlDbType = SqlDbType.Decimal,
                                     Value = (packetPricePremium * reservationToken.RentalDuration)
                                },
                                  new SqlParameter
                                {
                                     ParameterName = "@PAYMENTCODE",
                                     SqlDbType = SqlDbType.NVarChar,
                                     Value = postReservationRequest.PaymentCode.ToStringNullSafe()
                                },
                                 new SqlParameter
                                {
                                    ParameterName = "@FLIGHTPASSREQUIRED",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = vehicle != null ? vehicle.VendorFlightPassRequired ?? (object)DBNull.Value : reservationToken.VendorFlightPassRequired
                                },
                                 new SqlParameter
                                {
                                    ParameterName = "@EXTERNALCREDITCARDINFO",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.ExternalCreditCardInfo.ToStringNullSafe()
                                },
                                  new SqlParameter
                                {
                                    ParameterName = "@INSTALLMENTFEE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.InstallmentFee.ToDecimalNullAvailable() ?? (object)DBNull.Value
                                },
                                   new SqlParameter
                                {
                                    ParameterName = "@COUPONCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.CouponCode.ToStringNullSafe() ?? (object)DBNull.Value
                                }, new SqlParameter
                                {
                                    ParameterName = "@COUPONDISCOUNTTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.CouponDiscountType.ToIntNullSafe()
                                }
                            };
        }
        public static string SqlParamList(SqlParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return string.Empty;

            return string.Join(", ", parameters.Select(p => $"{p.ParameterName} = {p.ParameterName}"));
        }
        public static SqlParameter[] PostReservationLocalSqlParameters(long reservationId, Domain.Models.Requests.PostReservationRequestV2 postReservationRequest, ReservationToken reservationToken, Vendor vendor, float dailyPrice, float totalAmount, float serviceCharge, string tempExtraList, float apiDailyPrice, float apiExtraAmount, float apiTotalAmount, string vendorLogoUrl, bool isOffice, PostPaymentResponse postPaymentResponse, float discountAmount, Agency agency, float packetPricePremium, Configurations configurations, float? discountValue, int? couponId)
        {

            return new SqlParameter[] {
                                new SqlParameter
                                {
                                    ParameterName = "@RESNO",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = ReservationHelper.GenerateReservationNumber(reservationId)
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RESID",
                                    SqlDbType = SqlDbType.BigInt,
                                    Value = reservationId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@AGENCYID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.Agency?.AgencyId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@MEMBERID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (postReservationRequest.MemberId != null && postReservationRequest.MemberId != 0) ? postReservationRequest.MemberId : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLENAME",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.VehicleName ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLEID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = reservationToken.VehicleId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PICKUPLOCATIONID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.PickupLocationId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RETURNLOCATIONID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.ReturnLocationId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PICKUPDATE",
                                    SqlDbType = SqlDbType.DateTime,
                                    Value = new DateTime(
                                        Convert.ToInt32(postReservationRequest.PickupDate.Split('.')[2]),
                                        Convert.ToInt32(postReservationRequest.PickupDate.Split('.')[1]),
                                        Convert.ToInt32(postReservationRequest.PickupDate.Split('.')[0]),
                                        Convert.ToInt32(postReservationRequest.PickupTime.Split(':')[0]),
                                        Convert.ToInt32(postReservationRequest.PickupTime.Split(':')[1]),
                                        Convert.ToInt32("000"))
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RETURNDATE",
                                    SqlDbType = SqlDbType.DateTime,
                                    Value = new DateTime(
                                        Convert.ToInt32(postReservationRequest.ReturnDate.Split('.')[2]),
                                        Convert.ToInt32(postReservationRequest.ReturnDate.Split('.')[1]),
                                        Convert.ToInt32(postReservationRequest.ReturnDate.Split('.')[0]),
                                        Convert.ToInt32(postReservationRequest.ReturnTime.Split(':')[0]),
                                        Convert.ToInt32(postReservationRequest.ReturnTime.Split(':')[1]),
                                        Convert.ToInt32("000"))
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = vendor.VendorId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SERVICEVENDORID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = reservationToken.APIVendorId
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RENTALDURATION",
                                    SqlDbType = SqlDbType.Int,
                                    Value = reservationToken.RentalDuration
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DAILYPRICE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = dailyPrice
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@EXTRAAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.Pricing?.ExtraAmount
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ONEWAYAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.Pricing?.SpecialOneWayFee != -1 ? postReservationRequest.Pricing.SpecialOneWayFee : reservationToken.OneWayFee
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@TOTALAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = totalAmount + serviceCharge
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PAIDAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.Pricing?.PaidAmount + serviceCharge
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@LANGID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)postReservationRequest.LanguageCode.ToEnum<LanguageTypes>() + 1
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CURRENCYID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)postReservationRequest.CurrencyCode.ToEnum<CurrencyTypes>() + 1
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERINSTUTIONTYPENO",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.Customer?.InstutionTypeNumber ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERTITLE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Company?.Title ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERADDRESS",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Customer?.Address ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERNAME",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Customer?.Name
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERSURNAME",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Customer?.Surname
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERTELEPHONE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Customer?.PhoneNumber ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMEREMAIL",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Customer?.Email
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMEREXPLANATION",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Customer?.Explanation ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COMPANYTAXOFFICE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Company?.TaxOffice ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COMPANYTAXNO",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Company?.TaxNumber ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERPERSONALNUMBER",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Customer?.PersonalNumber ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERNOTE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Customer?.Note ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@FLIGHTNOARRIVAL",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.FlightNumberArrival ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@FLIGHTNODEPARTURE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.FlightNumberDeparture ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PDF",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@IP",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Customer?.IPAddress ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@EXTRAIDLIST",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = tempExtraList ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIDAILYPRICE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = apiDailyPrice
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIEXTRAAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = apiExtraAmount
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIONEWAYFEE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = BrokerReservationHelper.GetAPIOneWayFee(reservationToken.APIOneWayFee, vendor)
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APITOTALAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = apiTotalAmount
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORPHONE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIVendorPhone ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDOREMAIL",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIVendorEmail ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORLOGO",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = vendorLogoUrl ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORNAME",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIVendorName ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SERVICECHARGE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = serviceCharge
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SERVICECHARGECURRENCYID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)vendor.ServiceChargeCurrencyType + 1
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DEPOSITPRICE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = reservationToken.DepositPrice.ToDecimalNullAvailable() ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORMINDRIVERAGE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = reservationToken.VendorMinimumDriverAge.ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VENDORMINDRIVINGLICENSEAGE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = reservationToken.VendorMinimumDrivingLicenseAge.ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SENDRESERVATIONMAIL",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.SendReservationMail
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CUSTOMERBIRTHDAY",
                                    SqlDbType = SqlDbType.DateTime,
                                    Value = string.IsNullOrEmpty(postReservationRequest.Customer?.BirthDay) ? (object)DBNull.Value : postReservationRequest.Customer?.BirthDay
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DEPARTUREINFO",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.DepartureInfo ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLEIMAGEURL",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.VehicleImageUrl ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ISOFFICE",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = isOffice
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@FUELTYPE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = ((int)reservationToken.FuelType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@TRANSMISSIONTYPE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value =((int)reservationToken.TransmissionType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIREFERENCECODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIReferenceCode ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIREFERENCECODE2",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIReferenceCode2 ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIREFERENCECODE3",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.APIReferenceCode3 ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APICURRENCYID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)vendor.CurrencyType + 1
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@AGENCYCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Agency?.AgencyCode ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DEPOSITCREDITCARDREQUIRED",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = reservationToken.DepositCreditCardRequired
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@AGENCYRESERVATIONREFERENCE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.AgencyReservationReference ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@APIPHONEACTIVE",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = vendor.APIPhoneActive
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PAYMENTRESULTMESSAGE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postPaymentResponse != null ? postPaymentResponse.Message ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PAYMENTRESULTCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postPaymentResponse != null ? postPaymentResponse.Code ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BANKID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postPaymentResponse.PaymentResult?.BankId != 0 ? postPaymentResponse.PaymentResult.BankId : (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PROVISIONNUMBER",
                                    SqlDbType = SqlDbType.NVarChar,
                                    //Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postReservationRequest.ProvisionNumber /* postPaymentResponse.PaymentResult.ProvisionNumber */ ?? postReservationRequest.ProvisionNumber ?? (object)DBNull.Value : (object)DBNull.Value
                                    Value = postReservationRequest != null ? postReservationRequest.Payment?.ProvisionNumber ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ORDERNUMBER",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest != null ? postReservationRequest.OrderNumber ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PAYMENTAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postPaymentResponse.PaymentResult?.PaymentAmount != 0 ? postPaymentResponse.PaymentResult?.PaymentAmount : (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ALERTERRORCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postPaymentResponse.PaymentResult?.AlertErrorCode ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@LOGRESULTNUMBER",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postPaymentResponse.PaymentResult?.LogResultNumber ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@LOGERRORCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postPaymentResponse != null && postPaymentResponse.PaymentResult != null ? postPaymentResponse.PaymentResult?.LogErrorCode ?? (object)DBNull.Value : (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PAYMENTTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)postReservationRequest.Payment?.PaymentType
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CREDITCARDDISCOUNTPERCENT",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.Payment?.PaymentType == PaymentTypes.PayAll
                                    ? (int)agency.CreditCardDiscountPercent : 0
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COUPONDISCOUNTAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = discountAmount.ToDecimalNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COUPONID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = couponId ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SPECIALDAILYPRICE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.Pricing?.SpecialDailyPrice.ToDecimalNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SPECIALONEWAYFEE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.Pricing?.SpecialOneWayFee.ToDecimalNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BASEREQUESTCURRENCYID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)reservationToken.BaseVendorRequestCurrencyType + 1
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SIPPCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.SippCode ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLECODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = reservationToken.VehicleCode ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@EXTRAPRICEPAYTODELIVERY",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.Payment?.ExtraPricePayToDelivery
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ONEWAYFEEPAYTODELIVERY",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.Payment?.OneWayFeePayToDelivery
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ISSPECIALWEBSITEAGENCY",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.IsSpecialWebSiteAgency
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RESERVATIONTOKEN",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = JsonConvert.SerializeObject(reservationToken)
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLETYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = ((int)reservationToken.VehicleType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@VEHICLECATEGORYTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = ((int)reservationToken.VehicleCategoryType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@PASSANGERQUANTITYTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = ((int)reservationToken.PassangerQuantityType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BAGGAGEQUANTITYTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = ((int)reservationToken.BaggageQuantityType + 1).ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SKYSCANNERREDIRECTID",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.SkyscannerRedirectID ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COMMERCIALALLOWANCE ",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.CommercialAllowance
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@SOURCEID",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.ReservationSourceId ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@TOTALKMLIMIT",
                                    SqlDbType = SqlDbType.Int,
                                    Value = reservationToken.TotalKmLimit
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DEFAULTCUSTOMERMAILADDRESS",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = configurations.DefaultCustomerMailAddress ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RENTALWORKINGTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)vendor.RentalWorkingType
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ADDITIONALPRODUCTWORKINGTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)vendor.AdditionalProductWorkingType
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ONEWAYFEEWORKINGTYPE",
                                    SqlDbType = SqlDbType.Int,
                                    Value = (int)vendor.OneWayFeeWorkingType
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@AGENCYRENTALPROFITMARKUP",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = !agency.FreePriceShowActive ? agency.AgencyRentalProfitMarkup.ToDecimalNullSafe() : 0
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@RESERVATIONTOKENTEXT",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.ReservationToken.ToString()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@ADVANCEDPAYMENTWITHOUTPAYMENT",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.Payment?.AdvancedPaymentWithoutPayment
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COUPONDISCOUNTVALUE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = discountValue ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CREDITCARDNUMBER",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Payment?.CreditCardNumber?.RemoveSpecialCharacters().MaskCreditCardNumber() ?? ""
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@INSTALLMENTCOUNT",
                                    SqlDbType = SqlDbType.Int,
                                    Value = postReservationRequest.Payment?.InstallmentCount ?? (object)DBNull.Value
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BANK",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Payment?.Bank.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BANKACCOUNTCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Payment.BankAccountCode.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@BANKACCOUNTINGCODE",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Payment?.BankAccountingCode.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@COUNTRY",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Country.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CITY",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.City.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@DISTRICT",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.District.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CREDITCARDBANK",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.Payment?.CreditCardBank.ToStringNullSafe()
                                },
                                new SqlParameter
                                {
                                    ParameterName = "@CONTACTPERMISSION",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = postReservationRequest.ContactPermission.ToBoolNullSafe()
                                },
                                 new SqlParameter
                                {
                                    ParameterName = "@INSTALLMENTCOMMISIONAMOUNT",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.Payment.InstallmentCommissionAmount ?? (object)DBNull.Value
                                },
                                 CreateCreditTypeParameter(postReservationRequest.CreditType),
                                  new SqlParameter
                                {
                                     ParameterName = "@PREMIUMEXTRAAMOUNT",
                                     SqlDbType = SqlDbType.Decimal,
                                     Value = packetPricePremium
                                },
                                  new SqlParameter
                                {
                                     ParameterName = "@PAYMENTCODE",
                                     SqlDbType = SqlDbType.NVarChar,
                                     Value = postReservationRequest.Payment?.PaymentCode.ToStringNullSafe()
                                },
                                 new SqlParameter
                                {
                                    ParameterName = "@FLIGHTPASSREQUIRED",
                                    SqlDbType = SqlDbType.Bit,
                                    Value = reservationToken.VendorFlightPassRequired
                                },
                                 new SqlParameter
                                {
                                    ParameterName = "@EXTERNALCREDITCARDINFO",
                                    SqlDbType = SqlDbType.NVarChar,
                                    Value = postReservationRequest.ExternalCreditCardInfo.ToStringNullSafe()
                                },
                                  new SqlParameter
                                {
                                    ParameterName = "@INSTALLMENTFEE",
                                    SqlDbType = SqlDbType.Decimal,
                                    Value = postReservationRequest.Payment?.InstallmentFee.ToDecimalNullAvailable() ?? (object)DBNull.Value
                                },
                                  new SqlParameter
                                  {
                                      ParameterName = "@APIDELIVERYTYPEID",
                                      SqlDbType = SqlDbType.Int,
                                      Value = vendor.DeliveryTypeFromVendor ? reservationToken.APIDeliveryTypeId ?? (object)DBNull.Value : (object)DBNull.Value
                                  },
                                  new SqlParameter
                                  {
                                      ParameterName = "@SUBAGENCYID",
                                      SqlDbType = SqlDbType.Int,
                                      Value = (postReservationRequest.Agency?.SubAgencyId == null
                                                     || postReservationRequest.Agency.SubAgencyId == 0)
                                                    ? (object)DBNull.Value
                                                    : postReservationRequest.Agency.SubAgencyId
                                  }
                            };
        }

        public static SqlParameter NVarchar(string name, object value)
            => new SqlParameter(name, SqlDbType.NVarChar) { Value = value ?? (object)DBNull.Value };

        public static SqlParameter Int(string name, object value)
            => new SqlParameter(name, SqlDbType.Int) { Value = value ?? (object)DBNull.Value };

        public static SqlParameter Decimal(string name, object value)
            => new SqlParameter(name, SqlDbType.Decimal) { Value = value ?? (object)DBNull.Value };

        public static SqlParameter Bit(string name, object value)
            => new SqlParameter(name, SqlDbType.Bit) { Value = value ?? (object)DBNull.Value };

        public static SqlParameter DateTime(string name, object value)
            => new SqlParameter(name, SqlDbType.DateTime) { Value = value ?? (object)DBNull.Value };

        public static SqlParameter BigInt(string name, object value)
            => new SqlParameter(name, SqlDbType.BigInt) { Value = value ?? (object)DBNull.Value };

        public static SqlParameter[] GetReservationsSqlParameters(GetReservationsRequest getReservationsRequest, UserRoles _userRole, int _currentAgencyId)
        {
            return new SqlParameter[] {
                    new SqlParameter
                    {
                        ParameterName = "@AGENCYID",
                        SqlDbType = SqlDbType.Int,
                        Value = _userRole != UserRoles.Admin && _userRole != UserRoles.MobileAPP ? _currentAgencyId : (getReservationsRequest.AgencyId != null ? getReservationsRequest.AgencyId : (object)DBNull.Value)
                    },
                    new SqlParameter
                    {
                        ParameterName = "@AGENCYNAMEFORSEARCH",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = !string.IsNullOrEmpty(getReservationsRequest.AgencyName) ? getReservationsRequest.AgencyName : (object)DBNull.Value
                    },
                    new SqlParameter
                    {
                        ParameterName = "@MEMBERID",
                        SqlDbType = SqlDbType.Int,
                        Value = _userRole == UserRoles.External ? DBNull.Value : (getReservationsRequest.MemberId != null ? getReservationsRequest.MemberId : (object)DBNull.Value)
                    },
                    new SqlParameter
                    {
                        ParameterName = "@RESERVATIONSTARTDATE",
                        SqlDbType = SqlDbType.DateTime,
                        Value = getReservationsRequest.PickupDateStart != null ?getReservationsRequest.PickupDateStart.ToDateTimeNullSafe() : (object)DBNull.Value
                    },
                    new SqlParameter
                    {
                        ParameterName = "@RESERVATIONENDDATE",
                        SqlDbType = SqlDbType.DateTime,
                        Value = getReservationsRequest.PickupDateEnd != null ?getReservationsRequest.PickupDateEnd.ToDateTimeNullSafe() : (object)DBNull.Value
                    },
                    new SqlParameter
                    {
                        ParameterName = "@RESERVATIONRETURNSTARTDATE",
                        SqlDbType = SqlDbType.DateTime,
                        Value = getReservationsRequest.ReturnDateStart != null ?getReservationsRequest.ReturnDateStart.ToDateTimeNullSafe() : (object)DBNull.Value
                    },
                    new SqlParameter
                    {
                        ParameterName = "@RESERVATIONRETURNENDDATE",
                        SqlDbType = SqlDbType.DateTime,
                        Value = getReservationsRequest.ReturnDateEnd != null ?getReservationsRequest.ReturnDateEnd.ToDateTimeNullSafe() : (object)DBNull.Value
                    },
                    new SqlParameter
                    {
                        ParameterName = "@RESERVATIONDATEFIRST",
                        SqlDbType = SqlDbType.DateTime,
                        Value = getReservationsRequest.ReservationDateStart != null ? getReservationsRequest.ReservationDateStart.ToDateTimeNullSafe() : (object)DBNull.Value
                    },
                    new SqlParameter
                    {
                        ParameterName = "@RESERVATIONDATESECOND",
                        SqlDbType = SqlDbType.DateTime,
                        Value = getReservationsRequest.ReservationDateEnd != null ? getReservationsRequest.ReservationDateEnd.ToDateTimeNullSafe() : (object)DBNull.Value
                    },
                    new SqlParameter
                    {
                        ParameterName = "@RESERVATIONNO",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = getReservationsRequest.ReservationNumber != null ? getReservationsRequest.ReservationNumber : (object)DBNull.Value
                    },
                    new SqlParameter
                    {
                        ParameterName = "@APIRESERVATIONNO",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = getReservationsRequest.APIReservationNumber != null ? getReservationsRequest.APIReservationNumber : (object)DBNull.Value
                    },
                    new SqlParameter
                    {
                        ParameterName = "@RESERVATIONINFO",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = -1,
                        Value = getReservationsRequest.ReservationInfo != null ? getReservationsRequest.ReservationInfo : (object)DBNull.Value
                    },
                };
        }
        public static SqlParameter[] GetReservationSqlParameters(GetReservationsRequest getReservationsRequest, UserRoles _userRole, int _currentAgencyId, bool isBrokerReservation)
        {
            return new SqlParameter[] {
                    new SqlParameter
                    {
                        ParameterName = "@OPERATIONID",
                        SqlDbType = SqlDbType.Int,
                        Value = (int)AgencyOperationTypes.GetReservationDetail
                    },
                    new SqlParameter
                    {
                        ParameterName = "@AGENCYID",
                        SqlDbType = SqlDbType.Int,
                        Value =  _userRole != UserRoles.Admin && _userRole != UserRoles.MobileAPP ? _currentAgencyId : (getReservationsRequest.AgencyId != null ? getReservationsRequest.AgencyId : (object)DBNull.Value)
                    },
                    new SqlParameter
                    {
                        ParameterName = "@RESERVATIONNO",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = getReservationsRequest.ReservationNumber
                    },
                    new SqlParameter
                    {
                        ParameterName = "@USEREMAIL",
                        SqlDbType = SqlDbType.NVarChar,
                        Value = getReservationsRequest.CustomerEmail
                    },
                    new SqlParameter
                    {
                        ParameterName = "@BROKERCANCELRESERVATION",
                        SqlDbType = SqlDbType.Bit,
                        Value = isBrokerReservation
                    },
                };
        }

        private static SqlParameter CreateCreditTypeParameter(CreditType creditType)
        {
            return new SqlParameter
            {
                ParameterName = global::KolayCAR.Broker.API.Models.BrokerContext.UseLegacyCreditSchema ? "@ISFULLCREDIT" : "@CREDITTYPE",
                SqlDbType = global::KolayCAR.Broker.API.Models.BrokerContext.UseLegacyCreditSchema ? SqlDbType.Bit : SqlDbType.SmallInt,
                Value = global::KolayCAR.Broker.API.Models.BrokerContext.UseLegacyCreditSchema
                    ? creditType == CreditType.FullCredit
                    : (short)creditType
            };
        }
    }
}
