using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Extensions
{
    public static class ReservationDetailDtoToReservationRequest
    {
        public static ReservationsRequestDto ToRequestModel(this ReservationDetail reservationDetail, PaymentTypes paymentType, PaymentSetting paymentSetting = null, bool highAmountDiscountActive = false)
        {
            var model = System.Activator.CreateInstance<ReservationsRequestDto>();
            #region Hesaplamalar yapılıyor
            var totalPrice = reservationDetail.ReservationPaymentDetails.FirstOrDefault()?.TotalPrice ?? 0;
            decimal paidAmount = reservationDetail.ReservationPaymentDetails.FirstOrDefault()?.PaidAmount ?? 0;
            var installmentCommissionAmount = reservationDetail.ReservationPaymentDetails.FirstOrDefault()?.InstallmentCommissionAmount ?? 0;
            var additionalProductPricePoa = reservationDetail.ReservationPaymentDetails.FirstOrDefault().AdditionalProductPricePoa;
            var oneWayFeePoa = reservationDetail.ReservationPaymentDetails.FirstOrDefault().OneWayFeePoa;
            var couponDiscountAmount = reservationDetail.CouponDiscountAmount;
            var oneWayFee = reservationDetail.ReservationVehicleInfos.FirstOrDefault()?.OneWayFee;
            var serviceCharge = (decimal)reservationDetail.ServiceCharge;
            var extraAmount = 0F;
            var extras = "";
            var paidAmountAfterPoa = paidAmount;
            var specialPrice = -1F;
            var specialOneWayPrice = -1F;

            var isFullCredit = reservationDetail.FullCredit;
            if (reservationDetail.ReservationVehicleInfos.FirstOrDefault()?.SpecialDailyPrice != "-1" && reservationDetail.ReservationVehicleInfos.FirstOrDefault()?.SpecialDailyPrice != null)
            {
                if (float.TryParse(reservationDetail.ReservationVehicleInfos.FirstOrDefault()?.SpecialDailyPrice, out float price))
                {
                    specialPrice = price;
                }
            }
            if (reservationDetail.ReservationVehicleInfos.FirstOrDefault()?.SpecialOneWayFee != "-1" && reservationDetail.ReservationVehicleInfos.FirstOrDefault()?.SpecialOneWayFee != null)
            {
                if (float.TryParse(reservationDetail.ReservationVehicleInfos.FirstOrDefault()?.SpecialOneWayFee, out float price))
                {
                    specialOneWayPrice = price;
                    oneWayFee = price.ToCurrencyString();
                }
            }
            if (!string.IsNullOrEmpty(reservationDetail.SelectedExtrasJson) && reservationDetail.SelectedExtrasJson.ValidateJson())
            {
                var selectedExtras = JsonConvert.DeserializeObject<IEnumerable<ReservationDetailSelectedExtra>>(reservationDetail.SelectedExtrasJson);
                extras = string.Join('|', selectedExtras.Select(e => $"{e.Id}~1~{e.Price}").ToList());
                extraAmount = selectedExtras.Sum(e => e.ExtraRentalType == 0 ? (e.Price) : (e.RentalDuration * e.Price));
            }

            if (installmentCommissionAmount > 0)
            {
                specialPrice =
                    float.TryParse(reservationDetail.ReservationVehicleInfos.FirstOrDefault()?.DailyPrice ?? "-1", out var specialPriceOut)
                        ? specialPriceOut
                        : -1F;
            }


            if (!(bool)isFullCredit)
            {
                if (!(bool)additionalProductPricePoa)
                {
                    paidAmountAfterPoa += (decimal)extraAmount;
                }
                if (!(bool)oneWayFeePoa && !(reservationDetail.ReservationVehicleInfos.FirstOrDefault()?.SpecialOneWayFee != "-1" && reservationDetail.ReservationVehicleInfos.FirstOrDefault()?.SpecialOneWayFee != null))
                {
                    paidAmountAfterPoa += oneWayFee.ToDecimal();
                }
            }

            else
            {

                paidAmountAfterPoa += (decimal)extraAmount;

                paidAmountAfterPoa += oneWayFee.ToDecimal();

            }


            //Ek ürün ve Tek Yön Acente Teslimat Ödemesi Bilgisine Göre KK tutarına ekleme yapılıyor.


            var calculatedPaidAmount = paymentType switch
            {
                PaymentTypes.PayAll => (float)paidAmount,// + model.ExtraAmount,
                PaymentTypes.AdvancePayment => (float)paidAmount,
                PaymentTypes.CommissionFree => (float)totalPrice + extraAmount,
                PaymentTypes.PayOnDelivery => 0,
                PaymentTypes.PayToAgency => (float)paidAmountAfterPoa,
                _ => (float)totalPrice + extraAmount,
            };
            var paidAmountAfterUsingCouponCode = paymentType switch
            {
                PaymentTypes.PayAll => (float)paidAmount,// + model.ExtraAmount,
                PaymentTypes.AdvancePayment => (float)paidAmount,
                PaymentTypes.CommissionFree => (float)totalPrice + extraAmount,
                PaymentTypes.PayOnDelivery => 0,
                PaymentTypes.PayToAgency => (float)paidAmountAfterPoa,
                _ => (float)totalPrice + extraAmount,
            };
            #endregion

            #region Model atamaları yapılıyor
            model.ReservationToken = reservationDetail.ReservationToken;

            #region Sürücü Bilgileri
            foreach (var driverInfo in reservationDetail.ReservationDriverInfos)
            {
                model.CustomerPersonalNumber = driverInfo.IdentityNumber;
                model.CustomerName = driverInfo.Name;
                model.CustomerSurname = driverInfo.Surname;
                model.CustomerEmail = driverInfo.Email;
                model.CustomerTelephone = $"{driverInfo.CountryPhoneCode} {driverInfo.PhoneNumber}";
                model.CustomerBirthDay = driverInfo.Birthday?.ToString("dd.MM.yyyy") ?? DateTime.Now.ToString("dd.MM.yyyy");
                model.ContactPermission = driverInfo.ContactPermission;
                model.FlightNumberArrival = driverInfo.FlightNumber;
                model.CustomerNote = reservationDetail.ReservationNote;
            }
            #endregion

            #region Ödeme bilgileri
            foreach (var paymentDetail in reservationDetail.ReservationPaymentDetails)
            {
                model.CreditCardNumber = paymentDetail.CreditCardNumber;
                model.CreditCardHolder = paymentDetail.CreditCardOwnerName;
                model.ExpiredMonth = paymentDetail.ExpireMonth;
                model.ExpiredYear = paymentDetail.ExpireYear;
                model.SecurityCode = paymentDetail.Cvc;
                model.InstallmentCount = paymentDetail.InstallmentCount;
                model.InstallmentCommissionAmount = paymentDetail.InstallmentCommissionAmount;// Vade farkı tutarı (Toplam tutar * (Taksit Komisyon Oranı / 100))
            }

            if (paymentSetting != null)
            {
                model.Bank = paymentSetting.Bank.BankName;
                model.BankAccountingCode = paymentSetting.Bank.Account;
                model.BankAccountCode = paymentSetting.Bank.Account;
                model.CreditCardBank = paymentSetting.Bank.BankDefinition;
                model.OrderNumber = paymentSetting.Message;
                model.ProvisionNumber = paymentSetting.ProvisionNumber ?? "";
            }
            #endregion

            #region Extra Bilgileri
            if (!string.IsNullOrEmpty(reservationDetail.SelectedExtrasJson) && reservationDetail.SelectedExtrasJson.ValidateJson())
            {
                var selectedExtras = JsonConvert.DeserializeObject<IEnumerable<ReservationDetailSelectedExtra>>(reservationDetail.SelectedExtrasJson);
                model.ExtraList = extras;
                model.ExtraAmount = extraAmount;
            }
            #endregion

            #region Fatura bilgileri
            foreach (var invoiceAdress in reservationDetail.ReservationInvoiceAddresses)
            {
                model.CustomerAddress = invoiceAdress.Address;

                if (reservationDetail.InvoiceToDifferentAddress != true) continue;
                model.CompanyTitle = invoiceAdress.Title;
                model.CompanyTaxOffice = invoiceAdress.TaxOffice;
                model.CompanyTaxNumber = invoiceAdress.TaxNumber;
                model.Country = invoiceAdress.Country;
                model.City = invoiceAdress.City;
                model.Disctrict = invoiceAdress.District;
            }
            #endregion

            if (!string.IsNullOrEmpty(reservationDetail.CouponCode)) // Kupon Kodu Var
            {
                model.CouponCode = reservationDetail.CouponCode;
                model.PaidAmountAfterUsingCouponCode = paidAmountAfterUsingCouponCode;
            }

            model.SkyscannerRedirectID = reservationDetail.SkyScannerRedirectId;
            model.MemberId = (reservationDetail.MemberId ?? 0) > 0 ? reservationDetail.MemberId : null;
            model.PaymentType = paymentType;
            model.SendReservationMail = true;
            model.ExtraPricePayToDelivery =
                (reservationDetail.FullCredit ?? false)
                    ? false
                    : (reservationDetail.ReservationPaymentDetails.FirstOrDefault().AdditionalProductPricePoa ?? false);
            model.OneWayFeePayToDelivery =
                (bool)reservationDetail.FullCredit
                    ? false
                    : (bool)reservationDetail.ReservationPaymentDetails.FirstOrDefault().OneWayFeePoa;
            model.HighAmountDiscountActive = highAmountDiscountActive;
            model.SpecialDailyPrice = specialPrice;
            model.SpecialOneWayFee = specialOneWayPrice;
            model.PaidAmount = calculatedPaidAmount;
            model.FullCredit = reservationDetail.FullCredit;
            #endregion

            return model;
        }
    }
}
