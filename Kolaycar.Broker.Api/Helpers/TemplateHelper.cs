using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Helpers
{
    public class TemplateHelper
    {
        private string _template;

        public TemplateHelper(string template)
        {
            _template = template;
        }

        public string GetParsedReservationTemplate(
            ReservationMailTemplate reservationMailTemplate, bool mask = false)
        {
            var tempTemplate = _template;

            var langUrl = (reservationMailTemplate.DefaultLanguageType - 1).ToString() == reservationMailTemplate.Reservation.LanguageType.ToString() ? "" : "/" + reservationMailTemplate.Reservation.LanguageType.ToString().ToLower();

            var reservationDetailPageUrl =
                reservationMailTemplate.ReservationDetailUrlTemplate
                    .Replace("{pageUrl}", reservationMailTemplate.ReservationDetailContentUrl)
                    .Replace("{reservation}", reservationMailTemplate.Reservation.ReservationNumber)
                    .Replace("{email}", reservationMailTemplate.Reservation.CustomerMail);

            var reservationTemplateFields = new List<TemplateField>
                {
                    new TemplateField
                    {
                        Field = "{frm_agency_message}",
                        Value = reservationMailTemplate.AgencyMessage
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_reztarih}",
                        Value = reservationMailTemplate.Reservation.ReservationDate.ToString("dd.MM.yyyy HH:mm")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_firmaad}",
                        Value = reservationMailTemplate.Reservation.VendorName
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_rezno}",
                        //Value = reservationDetailPageUrl
                        Value = $"<a href=\"{reservationDetailPageUrl}\" style=\"color:blue;\">{reservationMailTemplate.Reservation.ReservationNumber}</a>"
                   
                        /*$"<a href=\"{reservationMailTemplate.ReservationDetailUrlTemplate.Replace("{"+nameof(reservationMailTemplate.Reservation.LanguageType)+"}", langUrl).Replace("{SuccessUrl}", reservationMailTemplate.ReservationDetailContentUrl ).Replace("{"+nameof(reservationMailTemplate.Reservation.ReservationNumber)+"}",reservationMailTemplate.Reservation.ReservationNumber).Replace("{"+nameof(reservationMailTemplate.Reservation.CustomerMail)+"}",reservationMailTemplate.Reservation.CustomerMail).Replace("{"+nameof(reservationMailTemplate.ReservationDetailPageContentUrl)+"}", reservationMailTemplate.ReservationDetailPageContentUrl)}\" style=\"color:blue;\">{reservationMailTemplate.Reservation.ReservationNumber}</a>"*/
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_rezno2}",
                        Value = $"<a href=\"{reservationMailTemplate.ReservationDetailUrlTemplate.Replace("{" + nameof(reservationMailTemplate.ReservationDetailContentUrl) + "}", reservationMailTemplate.ReservationDetailContentUrl.ToString().ToLower()).Replace("{" + nameof(reservationMailTemplate.Reservation.ReservationNumber) + "}", reservationMailTemplate.Reservation.ReservationNumber).Replace("{" + nameof(reservationMailTemplate.Reservation.CustomerMail) + "}", reservationMailTemplate.Reservation.CustomerMail)}\" style=\"color:#fff; font-weight: bold;\">{reservationMailTemplate.Reservation.ReservationNumber}</a>"
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_rezno3}",
                        Value = reservationMailTemplate.Reservation.ReservationNumber
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_rezervasyondurum}",
                        Value = string.Join(", ", reservationMailTemplate.Reservation.ReservationStatusHistories.Select(x => $"{x.ReservationStatusName} ({x.Date.ToString("dd.MM.yyyy HH:mm")})"))
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_mesaj}",
                        Value = reservationMailTemplate.ReservationStatusMessage
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_alistarih}",
                        Value =  reservationMailTemplate.Reservation.PickupDate.ToString("dd-MM-yyyy HH:mm")
        },
                    new TemplateField
                    {
                        Field = "{frm_mail_birakistarih}",
                        Value = reservationMailTemplate.Reservation.ReturnDate.ToString("dd-MM-yyyy HH:mm")
        },
                    new TemplateField
                    {
                        Field = "{frm_mail_kiralamasure}",
                        Value = reservationMailTemplate.Reservation.RentalDuration.ToString()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_kira_suresi}",
                        Value = reservationMailTemplate.Reservation.RentalDuration.ToString()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_alisyer}",
                        Value = reservationMailTemplate.Reservation.PickupLocationName
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_birakisyer}",
                        Value = reservationMailTemplate.Reservation.ReturnLocationName
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_aracsinif}",
                        Value = reservationMailTemplate.Reservation.VehicleName
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_ekstraad}",
                        Value = string.Join(", ", reservationMailTemplate.Reservation.ReservationExtras.Select(x => $"{x.ExtraName}({x.Piece})").ToArray())
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_additions}",
                        Value = reservationMailTemplate.Reservation.Additions != null && reservationMailTemplate.Reservation.Additions.Count > 0 ? ($"<table width=\"100%\" style=\"border-bottom: 1px solid #9a9a9a;margin-bottom: 20px;\" cellspacing=\"3\" cellpadding=\"3\"><tbody><tr><td align=\"left\" style=\"font-size: 0.8rem;\"><b>{reservationMailTemplate.AdditionsTitleLabel}</b></td><td align=\"right\" style=\"font-size: 0.8rem;\"></td></tr>" + string.Join(" ", reservationMailTemplate.Reservation.Additions.Select(x => $"<tr><td align=\"left\" style=\"font-size: 0.8rem;\">{x.AdditionName} ({x.Description})</td><td align=\"right\" style=\"font-size: 0.8rem;\">{x.AgencyAmount.ToFloatNullSafe().ToString("N2")} {reservationMailTemplate.Reservation.CurrencyCode}</td></tr>").ToArray()) + "</tbody></table>") : string.Empty
                    },
                     new TemplateField
                    {
                        Field = "{frm_mail_additions2}",
                        Value = reservationMailTemplate.Reservation.Additions != null && reservationMailTemplate.Reservation.Additions.Count > 0 ? ($"<table class=\"{{price_show}}\" cellspacing=\"5\" cellpadding=\"5\" border=\"0\" style=\"margin-bottom: 15px;text-size-adjust: 100%; font-family: arial, helvetica, sans-serif; font-size: 1rem; border-collapse: collapse !important;\"><tbody><tr><td style=\"color: #666666; font-size: 0.9rem; text-size-adjust: 100%; font-family: arial, helvetica, sans-serif;\">{reservationMailTemplate.AdditionsTitleLabel}</td><td style=\"color: #666666; font-size: 0.9rem; text-size-adjust: 100%; font-family: arial, helvetica, sans-serif;\"></td><td align=\"right\" style=\"text-size-adjust: 100%; font-family: arial, helvetica, sans-serif; font-size: 1rem;\"></td></tr>" + string.Join(" ", reservationMailTemplate.Reservation.Additions.Select(x => $"<tr><td style=\"color: #666666; font-size: 0.9rem; text-size-adjust: 100%; font-family: arial, helvetica, sans-serif;\">{x.AdditionName} {(!string.IsNullOrEmpty(x.Description) ? " (" + x.Description + ") " : string.Empty)}</td><td style=\"color: #666666; font-size: 0.9rem; text-size-adjust: 100%; font-family: arial, helvetica, sans-serif;padding-left: 10px;\">:</td><td align=\"right\" style=\"text-size-adjust: 100%; font-family: arial, helvetica, sans-serif; font-size: 1rem;width: 100px;\"><b>{x.AgencyAmount.ToFloatNullSafe().ToString("N2")} {reservationMailTemplate.Reservation.CurrencyCode}</b></td></tr>").ToArray()) + "</tbody></table>") : string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_ekstraad2}",
                        Value = "<table width=\"100%\" cellspacing=\"3\" cellpadding=\"3\"><tbody>" + string.Join(" ", reservationMailTemplate.Reservation.ReservationExtras.Select(x => $"<tr><td align=\"left\" style=\"font-size: 0.8rem;\">{x.ExtraName} ({x.Piece} adet)</td><td align=\"right\" style=\"font-size: 0.8rem;\">{Math.Round(x.Price * x.Piece * (x.ExtraRentalType == ExtraRentalTypes.Daily ? reservationMailTemplate.Reservation.RentalDuration : 1), 2).ToString("N2")} {reservationMailTemplate.Reservation.CurrencyCode}</td></tr>").ToArray()) + "</tbody></table>"
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_dahilhizmetler}",
                        Value = "" //TODO: Dahil hizmetler rezervasyona yazılmıyor. Geliştirme yapılacak.
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_kiralamatutar}",
                        Value = $"{reservationMailTemplate.Reservation.DailyPrice.ToString("N2")} x {reservationMailTemplate.Reservation.RentalDuration} = {Math.Round(reservationMailTemplate.Reservation.DailyPrice * reservationMailTemplate.Reservation.RentalDuration, 2).ToString("N2")}"
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_ekstratutar}",
                        Value = reservationMailTemplate.Reservation.ExtraPrice.ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_tekyonucret}",
                        Value = reservationMailTemplate.Reservation.OneWayFee.ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vadefarki}",
                        Value = "0"//TODO: vade farkı alanı girilecek
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_odemetip}",
                        Value = ""//reservationMailTemplate.Reservation.PaymentType.ToString() // TODO: Ödeme tipleri için text dönen switch case metodu yazılacak
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_toplamtutar}",
                        Value = reservationMailTemplate.ReservationMailSendToType != ReservationMailSendToTypes.ToAgency
                            ?  (reservationMailTemplate.Reservation.TotalPrice - (reservationMailTemplate.Reservation.DailyPrice * reservationMailTemplate.Reservation.RentalDuration * reservationMailTemplate.Reservation.CreditCardDiscountPercent / 100)).ToString("N2")
                            : CalculationHelper.RoundPrice((reservationMailTemplate.Reservation.DailyPrice * reservationMailTemplate.Reservation.RentalDuration * (100 - reservationMailTemplate.Reservation.AgencyCommission) / 100) + reservationMailTemplate.Reservation.ServiceCharge + reservationMailTemplate.Reservation.ExtraPrice + reservationMailTemplate.Reservation.OneWayFee - reservationMailTemplate.Reservation.CouponDiscountAmount, 2).ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_toplamtutar2}",
                        Value = (reservationMailTemplate.Reservation.TotalPrice - (reservationMailTemplate.Reservation.DailyPrice * reservationMailTemplate.Reservation.RentalDuration * reservationMailTemplate.Reservation.CreditCardDiscountPercent / 100)).ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_ad}",
                        Value = reservationMailTemplate.Reservation.CustomerName
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_soyad}",
                        Value = reservationMailTemplate.Reservation.CustomerSurname
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_telefon}",
                        Value = reservationMailTemplate.Reservation.CustomerPhone.Replace(" ",String.Empty).ToMaskPhoneNumber()
                        //Value = mask == true ? reservationMailTemplate.Reservation.CustomerPhone.Replace(" ",String.Empty).ToMaskPhoneNumber()
                        //            : reservationMailTemplate.Reservation.CustomerPhone.Replace(" ",String.Empty)
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_email}",
                        Value = mask == true ? reservationMailTemplate.Reservation.CustomerMail.ToMaskEmail()
                                : reservationMailTemplate.Reservation.CustomerMail
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_tcno}",
                        Value = mask == true ? reservationMailTemplate.Reservation.CustomerIdentityNumber.ToMaskPersonalNumber()
                                    : reservationMailTemplate.Reservation.CustomerIdentityNumber
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_sirketunvan}",
                        Value = reservationMailTemplate.Reservation.CustomerTitle
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vergidaire}",
                        Value = reservationMailTemplate.Reservation.CustomerTaxOffice
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vergino}",
                        Value = reservationMailTemplate.Reservation.CustomerTaxNumber
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_adres}",
                        Value = reservationMailTemplate.Reservation.CustomerAddress
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_ucusno}",
                        Value = $"{reservationMailTemplate.Reservation.CustomerArrivalFlightNumber} / {reservationMailTemplate.Reservation.CustomerReturnFlightNumber}"
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_aciklama}",
                        Value = reservationMailTemplate.Reservation.CustomerNote
                    },
                    new TemplateField
                    {
                        Field = "{agencyID}",
                        Value = (reservationMailTemplate.Reservation.AgencyId ?? 0).ToString()
                    },
                    new TemplateField
                    {
                        Field = "{image-show}",
                        Value = reservationMailTemplate.Agency != null ?
                            !string.IsNullOrEmpty(reservationMailTemplate.Agency.Logo) ?
                                string.Empty :
                                "0" :
                            "0"
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_agency_logo}",
                        Value = reservationMailTemplate.Agency != null ?
                            $"{reservationMailTemplate.PortalOwnerDomain}{reservationMailTemplate.Agency.Logo}" :
                            string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_agency_name}",
                        Value = reservationMailTemplate.Agency != null ?
                            reservationMailTemplate.Agency.AgencyName :
                            string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{cancel-note}",
                        Value = string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{cancel-button}",
                        Value = string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{cancel-note-input}",
                        Value = string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_odenentutar}",
                        Value = reservationMailTemplate.Reservation.PaidAmount.ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_odenecektutar}",
                        Value = (reservationMailTemplate.Reservation.TotalPrice - (reservationMailTemplate.Reservation.DailyPrice * reservationMailTemplate.Reservation.RentalDuration * reservationMailTemplate.Reservation.CreditCardDiscountPercent / 100) + reservationMailTemplate.VendorLocationFee - 
                        // TotalPrice kupon düşmüş hali, coupondiscountvalue kuponun yüzdesi yanlış oluşuyor yoruma alındı 01.08.2025
                        //reservationMailTemplate.Reservation.CouponDiscountValue - 
                        reservationMailTemplate.Reservation.PaidAmount).ToString("N2")
                        // gkursad
                        //Value = reservationMailTemplate.Reservation.PaymentType switch
                        //{
                        //    PaymentTypes.PayAll => (reservationMailTemplate.VendorLocationFee + reservationMailTemplate.Reservation.OneWayFee + reservationMailTemplate.Reservation.ExtraPrice).ToString("N2"),
                        //    PaymentTypes.AdvancePayment => (reservationMailTemplate.Reservation.TotalPrice + reservationMailTemplate.VendorLocationFee - reservationMailTemplate.Reservation.CouponDiscountValue - reservationMailTemplate.Reservation.PaidAmount).ToString("N2"),
                        //    PaymentTypes.PayOnDelivery => (reservationMailTemplate.Reservation.TotalPrice + reservationMailTemplate.Reservation.ServiceCharge).ToString("N2"),
                        //    _ => (reservationMailTemplate.Reservation.TotalPrice + reservationMailTemplate.VendorLocationFee - reservationMailTemplate.Reservation.CouponDiscountValue - reservationMailTemplate.Reservation.PaidAmount).ToString("N2")
                        //}

                    },

                    new TemplateField
                    {
                        Field = "{frm_mail_currencycode}",
                        Value = reservationMailTemplate.Reservation.CurrencyCode
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vendor_currencycode}",
                        Value = reservationMailTemplate.Vendor.CurrencyType.ToString()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_sabithizmetbedeli}",
                        Value = reservationMailTemplate.Reservation.ServiceCharge.ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_po_title}",
                        Value = reservationMailTemplate.PortalOwnerTitle
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_po_address}",
                        Value = reservationMailTemplate.PortalOwnerAddress
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_po_phone}",
                        Value = reservationMailTemplate.PortalOwnerPhone
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_po_phone_withoutspace}",
                        Value = reservationMailTemplate.PortalOwnerPhone.TrimNullSafe().Replace(" ","")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_po_fax}",
                        Value = reservationMailTemplate.PortalOwnerFax
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_po_email}",
                        Value = reservationMailTemplate.PortalOwnerEmail
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_po_logo}",
                        Value =  $"{reservationMailTemplate.PortalOwnerDomain}{reservationMailTemplate.PortalOwnerLogo}"
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vendor_pickupaddress}",
                        Value = reservationMailTemplate.Reservation.APIVendorPickupAddress
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vendor_pickupphone}",
                        Value = reservationMailTemplate.Reservation.APIVendorPickupPhone
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vendor_returnaddress}",
                        Value = reservationMailTemplate.Reservation.APIVendorReturnAddress
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vendor_returnphone}",
                        Value = reservationMailTemplate.Reservation.APIVendorReturnPhone
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vendor_pickupvendorname}",
                        Value = reservationMailTemplate.Reservation.VendorName
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vendor_returnvendorname}",
                        Value = reservationMailTemplate.Reservation.VendorName
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_depozito}",
                        Value = reservationMailTemplate.Reservation.DepositPrice.ToFloatNullSafe().ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_yas}",
                        Value = reservationMailTemplate.Reservation.VendorMinimumDriverAge.ToString()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_ehliyetyasi}",
                        Value = reservationMailTemplate.Reservation.VendorMinimumDrivingLicenseAge.ToString()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_referansno}",
                        Value = reservationMailTemplate.Reservation.APIReservationNumber
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_tedarikci_mesaj}",
                        Value = reservationMailTemplate.Reservation.ReservationStatusType != ReservationStatusTypes.Cancelled ?
                            reservationMailTemplate.Reservation.IsOffice ?
                                reservationMailTemplate.IsOfficeTrueLabel :
                                reservationMailTemplate.IsOfficeFalseLabel?.Replace("{{APIVENDORNAME}}", reservationMailTemplate.Reservation.VendorName) :
                            string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_po_emergency_phone}",
                        Value = reservationMailTemplate.EmergencyPhone
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_aracsinif_resim}",
                        Value = reservationMailTemplate.PortalOwnerDomain + reservationMailTemplate.Reservation.VehicleImageUrl
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_po_document_title}",
                        Value = reservationMailTemplate.Reservation.ReservationStatusType == ReservationStatusTypes.Cancelled ?
                            $"<span style=\"color: red;\">{reservationMailTemplate.DocumentTitle}</span>" :
                            reservationMailTemplate.DocumentTitle
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_arac_yakit}",
                        Value = reservationMailTemplate.FuelName
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_arac_vites}",
                        Value = reservationMailTemplate.TransmissionName
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vendor_email}",
                        Value = reservationMailTemplate.Reservation.VendorEmail
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_toplamtutar_cizili}",
                        Value = reservationMailTemplate.ReservationMailSendToType == ReservationMailSendToTypes.ToAgency ?
                            $"{reservationMailTemplate.Reservation.TotalPrice.ToString("N2")} {reservationMailTemplate.Reservation.CurrencyCode}" :
                            string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vendor_logo}",
                        Value = reservationMailTemplate.Reservation.VendorLogo
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vendor_phone}",
                        Value = string.IsNullOrEmpty(reservationMailTemplate.Reservation.VendorPhone)  ? reservationMailTemplate.Reservation.APIVendorPickupPhone
                        :reservationMailTemplate.Reservation.VendorPhone
                    },
                    new TemplateField
                    {
                        Field = "{frm_agency_hide}",
                        Value = reservationMailTemplate.ReservationMailSendToType == ReservationMailSendToTypes.ToAgency ?
                            "hide" :
                            string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_agency_show}",
                        Value = reservationMailTemplate.ReservationMailSendToType == ReservationMailSendToTypes.ToAgency ?
                            string.Empty :
                            "hide"
                    },
                    new TemplateField
                    {
                        Field = "{frm_agency_commission}",
                        Value = reservationMailTemplate.Reservation.AgencyCommission.ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_flight}",
                        Value = $"{reservationMailTemplate.Reservation.CustomerArrivalFlightNumber} - {reservationMailTemplate.Reservation.DepartureInfo}"
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_deposit_message}",
                        Value = reservationMailTemplate.Reservation.DepositCreditCardRequired ? reservationMailTemplate.DepositMessage : string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_agency_code}",
                        Value = reservationMailTemplate.Agency != null ?
                                !string.IsNullOrEmpty(reservationMailTemplate.Agency.AgencyCode) ?
                                    reservationMailTemplate.Agency.AgencyCode :
                                    string.Empty :
                                string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_deposit_show}",
                        Value = reservationMailTemplate.Reservation.DepositPrice != null ? string.Empty : "hide"
                    },
                    new TemplateField
                    {
                        Field = "{frm_apivendor_phone_show}",
                        Value =  reservationMailTemplate.Reservation.APIPhoneActive ?
                            string.Empty :
                            "hide"
                    },
                    new TemplateField
                    {
                        Field = "{frm_coupon_show}",
                        Value =  reservationMailTemplate.Reservation.CouponId != null ?
                            string.Empty :
                            "hide"
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_coupon_code}",
                        Value =  !string.IsNullOrEmpty(reservationMailTemplate.Reservation.CouponCode) ? reservationMailTemplate.Reservation.CouponDiscountType == CouponDiscountTypes.ByPrice ? reservationMailTemplate.Reservation.CouponCode : $"{reservationMailTemplate.Reservation.CouponCode} / %{reservationMailTemplate.Reservation.CouponDiscountValue}" : string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_coupon_discount_amount}",
                        Value =  reservationMailTemplate.Reservation.CouponDiscountAmount.ToStringNullSafe()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_update_date_show}",
                        Value =  reservationMailTemplate.Reservation.UpdateCount > 0 ?
                            string.Empty :
                            "hide"
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_last_update_date}",
                        Value =  reservationMailTemplate.Reservation.LastUpdateDate != null ?
                            reservationMailTemplate.Reservation.LastUpdateDate.ToDateTimeNullSafe().ToString("dd.MM.yyyy HH:mm") :
                            string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_company_title}",
                        Value =  reservationMailTemplate.Reservation.CustomerTitle ?? string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_company_tax_office}",
                        Value =  reservationMailTemplate.Reservation.CustomerTaxOffice ?? string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_company_tax_number}",
                        Value =  reservationMailTemplate.Reservation.CustomerTaxNumber ?? string.Empty
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_company_area_show}",
                        Value =  !string.IsNullOrEmpty(reservationMailTemplate.Reservation.CustomerTitle) ? string.Empty : "hide"
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vehicle_category}",
                        Value =  reservationMailTemplate.Reservation.VehicleCategoryTypeName.ToStringNullSafe()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vehicle_type}",
                        Value =  reservationMailTemplate.Reservation.VehicleTypeName.ToStringNullSafe()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vehicle_passanger}",
                        Value =  reservationMailTemplate.Reservation.PassangerQuantityTypeName.ToStringNullSafe()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_vehicle_baggage}",
                        Value =  reservationMailTemplate.Reservation.BaggageQuantityTypeName.ToStringNullSafe()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_daily_price}",
                        Value =  reservationMailTemplate.Reservation.DailyPrice.ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_rental_amount}",
                        Value =  (reservationMailTemplate.Reservation.DailyPrice * reservationMailTemplate.Reservation.RentalDuration).ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_pickupdate}",
                        Value =  reservationMailTemplate.Reservation.PickupDate.ToString("dd.MM.yyyy")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_returndate}",
                        Value =  reservationMailTemplate.Reservation.ReturnDate.ToString("dd.MM.yyyy")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_pickuptime}",
                        Value =  reservationMailTemplate.Reservation.PickupDate.ToString("HH:mm")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_returntime}",
                        Value =  reservationMailTemplate.Reservation.ReturnDate.ToString("HH:mm")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_pickup_location_address}",
                        Value =  reservationMailTemplate.PickupLocation.Address.ToStringNullSafe()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_return_location_address}",
                        Value =  reservationMailTemplate.ReturnLocation.Address.ToStringNullSafe()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_pickup_location_phone}",
                        Value =  reservationMailTemplate.PickupLocation.PhoneNumber.ToStringNullSafe()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_return_location_phone}",
                        Value =  reservationMailTemplate.ReturnLocation.PhoneNumber.ToStringNullSafe()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_refundamount}",
                        Value =  reservationMailTemplate.Reservation.CancellationRefundedAmount.ToFloatNullSafe().ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_penaltyamount}",
                        Value =  reservationMailTemplate.Reservation.CancellationPenaltyAmount.ToFloatNullSafe().ToString("N2")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_penaltystatus}",
                        Value =  reservationMailTemplate.Reservation.PenaltyStatus == _penaltyStatus.NotApplied ? " (Rez. ücretsiz olarak iptal edilmistir)" : " "
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_penaltyhour}",
                        Value =  reservationMailTemplate.Reservation.CancellationPenaltyHour.ToString()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_penaltyrate}",
                        Value =  reservationMailTemplate.Reservation.CancellationPenaltyRate.ToString()
                    },
                    new TemplateField
                    {
                        Field = "{datetime_now}",
                        Value =  DateTime.Now.ToString("dd.MM.yyyy HH:mm")
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_kmlimit}",
                        Value = reservationMailTemplate.Reservation.TotalKMLimit == 0 || reservationMailTemplate.Reservation.TotalKMLimit == null || reservationMailTemplate.Reservation.TotalKMLimit > 99998 || reservationMailTemplate.Vendor.VendorType == VendorTypes.Yolcu360  || reservationMailTemplate.Reservation.IsFullCredit ? "" : reservationMailTemplate.Reservation.TotalKMLimit.ToStringNullSafe()
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_show_coupon}",
                        Value = reservationMailTemplate.Reservation.CouponDiscountAmount > 0 ? "show" : "hide"
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_friendly_reservation_number}",
                        Value = reservationMailTemplate.Reservation.FriendlyReservationNumber.ToStringNullSafe()
                    },

                    new TemplateField
                    {
                        Field = "{frm_mail_fullcredit}",
                        Value = reservationMailTemplate.Reservation.IsFullCredit.ToBoolNullSafe() ? reservationMailTemplate.FullCreditInformation : ""
                    },
                    new TemplateField
                    {
                        Field = "{frm_mail_voucher}",
                        Value = $"{reservationMailTemplate.ReservationDetailUrlTemplate.Replace("{"+nameof(reservationMailTemplate.Reservation.LanguageType)+"}", reservationMailTemplate.Reservation.LanguageType.ToString().ToLower()).Replace("{"+nameof(reservationMailTemplate.Reservation.ReservationNumber)+"}",reservationMailTemplate.Reservation.ReservationNumber).Replace("{"+nameof(reservationMailTemplate.Reservation.CustomerMail)+"}",reservationMailTemplate.Reservation.CustomerMail).Replace("{"+nameof(reservationMailTemplate.ReservationDetailPageContentUrl)+"}", reservationMailTemplate.ReservationDetailPageContentUrl)}"
                    },
                     new TemplateField
                     {
                          Field = "{frm_vendor_logo_show}",
                          Value = reservationMailTemplate.VendorShowLogo.ToBoolNullSafe() ? "block" : "none"
                      },
                     new TemplateField
                     {
                         Field = "{frm_mail_po_domain}",
                         Value = reservationMailTemplate.PortalOwnerDomain
                     },
                     new TemplateField
                     {
                         Field = "{frm_mail_vendor_contract}",
                         Value = reservationMailTemplate.ReservationVendorContractUrl
                     },
                      new TemplateField
                     {
                         Field = "{frm_mail_vendor_pickupoffice_workinghours}",
                         Value = $"{reservationMailTemplate.Reservation.PickupOfficeWorkingHours} "
                     },
                      new TemplateField
                     {
                         Field = "{frm_mail_vendor_returnoffice_workinghours}",
                         Value = $"{reservationMailTemplate.Reservation.ReturnOfficeWorkingHours}"
                     }
                };
            if (reservationMailTemplate.CouponCode != null)
            {
                var list = new List<TemplateField>()
                {
                    new TemplateField
                {
                    Field = "{KuponKodu}",
                    Value = reservationMailTemplate.CouponCode.Code.ToStringNullSafe(),
                },
                new TemplateField
                {
                    Field = "{KuponTutar}",
                    Value = reservationMailTemplate.CouponCode.CouponDiscountValue.ToStringNullSafe()
                }
                };
                reservationTemplateFields.AddRange(list);

            }
            foreach (var templateField in reservationTemplateFields)
            {
                try
                {
                    tempTemplate = tempTemplate.Replace(templateField.Field, templateField.Value.ToStringNullSafe());

                }
                catch (Exception ex)
                {
                    Serilog.Log.Fatal("{@mailTemplateError}", ex.Message);
                }
            }

            return tempTemplate;
        }
    }

    public class TemplateField
    {
        public string Field { get; set; }
        public string Value { get; set; }
    }
}
