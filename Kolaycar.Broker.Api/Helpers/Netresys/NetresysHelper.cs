using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace KolayCAR.Broker.API.Helpers.Netresys
{
    public class NetresysHelper
    {
        public NetresysRequest GetEntity(Reservation reservation, Domain.Models.Agency agency)
        {
            float exCommissionPrice = 0;

            if (reservation.ReservationExtras?.Any(x => x.ExtraType == AdditionalProductTypes.Premium) == true)
            {
                var extras = reservation.ReservationExtras.Where(x => x.ExtraType == AdditionalProductTypes.Premium).ToList();
                foreach (var item in extras)
                {
                    //exCommissionPrice = ((exCommissionPrice + ((item.AgencyAmount * 100) / (100 - (float)agency.AgencyCommissionAmount))) * reservation.RentalDuration) * ((float)agency.AgencyCommissionAmount / 100);
                    var extraTotalPrice = item.ExtraRentalType == ExtraRentalTypes.PerRental ? item.Price : item.Price * reservation.RentalDuration;
                    var commission = extraTotalPrice * agency.AgencyCommissionAmount / 100f;
                    exCommissionPrice = exCommissionPrice + commission;
                }
            }
            ;

            var agencyCommission = GetAgencyCommissionWithAgencyProfitMarkup(reservation.TotalPrice, reservation.AgencyRentalProfitMarkup, reservation.AgencyCommission);
            var netResysReservationModel = new NetresysRequest
            {
                AgencyNumber = reservation.AgencyCode,
                Airport = "",
                Birthdate = reservation.CustomerBirthday.HasValue ? reservation.CustomerBirthday.Value.ToString("yyyy-MM-dd") : "",
                Birthdate2 = "",
                Broker = reservation.AgencyCode,
                //DailyPrice = ((reservation.Data.APITotalAmount - exPrice - reservation.Data.OneWayFee) / reservation.Data.RentalDuration).ToString("N2").Replace(".", ","),
                DailyPrice = (reservation.APIDailyPrice).ToString("N2").Replace(".", ","),
                Deposit = reservation.DepositPrice.ToString().Replace(".", ","),
                DepositPaymentType = "CASH_OR_CREDIT_CARD",
                DropBedeli = reservation.OneWayFee.ToString().Replace(".", ","),
                // Extras = exPrice.ToString().Replace(".", ","),
                Extras = reservation.APIExtraAmount.ToString().Replace(".", ","),
                FlightNumber = reservation.CustomerArrivalFlightNumber + " - " + reservation.DepartureInfo,
                FlightTime = "",
                FuelType = reservation.FuelTypeName,
                Gsm = reservation.CustomerPhone,
                Gsm2 = "",
                ID = reservation.ReservationNumber,
                Name = reservation.CustomerName,
                Name2 = "",
                Surname = reservation.CustomerSurname,
                Surname2 = "",
                Note = reservation.CustomerNote,
                Options = reservation.ReservationExtras != null ? string.Join(" | ", reservation.ReservationExtras.Select(x => $"{x.ExtraName}({x.Piece})").ToArray()) : "",
                PaymentSurcharge = "0,0",
                PaymentType = "SEPA",
                PickupDate = reservation.PickupDate.ToString("dd.MM.yyyy HH:mm"),
                PickupLocationName = reservation.PickupLocationName,
                PurchasePrice = reservation.APITotalAmount.ToString().Replace(".", ","),
                RcCode = "",
                RcGroup = "",
                RentACarProviderId = "NET TOUR",
                ReservationDate = reservation.ReservationDate.ToString("yyyy-MM-dd HH:mm"),
                ReturnAirport = "",
                ReturnDate = reservation.ReturnDate.ToString("dd.MM.yyyy HH:mm"),
                ReturnFlightNumber = reservation.CustomerReturnFlightNumber,
                ReturnFlightTime = "",
                ReturnLocationName = reservation.ReturnLocationName,
                SaleType = "New",
                SistemUcreti = reservation.ServiceCharge.ToString(),
                Title = "",
                Title2 = "",
                TotalDays = reservation.RentalDuration.ToString(),
                TotalPrice = reservation.TotalPrice.ToString().Replace(".", ","),
                Vehicle = reservation.VehicleName + " - " + reservation.TransmissionTypeName,
                Charge = (reservation.TotalPrice - (reservation.DailyPrice * reservation.RentalDuration * agencyCommission / 100) - exCommissionPrice - reservation.ServiceCharge - reservation.APITotalAmount).ToString(),
                ExternalCreditCardInfo = reservation.ExternalCreditCardInfo,
            };
            return netResysReservationModel;
        }

        public NetresysRequest GetCancelEntity(Reservation reservation)
        {
            var exPrice = reservation.ReservationExtras.Any(e => e.ExtraCode == "PRMPKT-1") ? GetExtrasPrice(reservation.ReservationExtras, reservation.AgencyCommission, reservation.RentalDuration) : reservation.ExtraPrice;

            var agencyCommission = GetAgencyCommissionWithAgencyProfitMarkup(reservation.TotalPrice, reservation.AgencyRentalProfitMarkup, reservation.AgencyCommission);
            var netResysCancelModel = new NetresysRequest
            {
                ID = reservation.ReservationNumber.ToString(),
                SaleType = "CANCEL",
                CancellingCharge = reservation.CancellationPenaltyAmount.ToString(),
                PaymentType = "SEPA",
                PaymentSurcharge = "0,0",
                ReservationDate = reservation.ReservationDate.ToString("yyyy-MM-dd HH:mm"),
                Title = "",
                Name = reservation.CustomerName,
                Surname = reservation.CustomerSurname,
                Birthdate = reservation.CustomerBirthday.HasValue ? reservation.CustomerBirthday.Value.ToString("yyyy-MM-dd") : "",
                Gsm = reservation.CustomerPhone,
                Title2 = "",
                Name2 = "",
                Surname2 = "",
                Birthdate2 = "",
                Gsm2 = "",
                PickupLocationName = reservation.PickupLocationName,
                AgencyNumber = reservation.AgencyCode,
                Airport = "",
                Broker = reservation.AgencyCode,
                DailyPrice = ((reservation.APITotalAmount - exPrice - reservation.OneWayFee) / reservation.RentalDuration).ToString("N2").Replace(".", ","),
                Deposit = reservation.DepositPrice.ToString().Replace(".", ","),
                DepositPaymentType = "CASH_OR_CREDIT_CARD",
                DropBedeli = reservation.OneWayFee.ToString().Replace(".", ","),
                Extras = exPrice.ToString().Replace(".", ","),
                FlightNumber = reservation.CustomerArrivalFlightNumber + " - " + reservation.DepartureInfo,
                FlightTime = "",
                FuelType = reservation.FuelTypeName,
                Note = reservation.CustomerNote,
                Options = string.Join(" | ", reservation.ReservationExtras.Select(x => x.ExtraName + "(" + x.Piece + ")").ToArray()),
                PickupDate = reservation.PickupDate.ToString("dd.MM.yyyy HH:mm"),
                PurchasePrice = reservation.APITotalAmount.ToString("N2").Replace(".", ","),
                RcCode = "",
                RcGroup = "",
                RentACarProviderId = "NET TOUR",
                ReturnAirport = "",
                ReturnDate = reservation.ReturnDate.ToString("dd.MM.yyyy HH:mm"),
                ReturnFlightNumber = reservation.CustomerReturnFlightNumber,
                ReturnFlightTime = "",
                ReturnLocationName = reservation.ReturnLocationName,
                SistemUcreti = reservation.ServiceCharge.ToString(),
                TotalDays = reservation.RentalDuration.ToString(),
                TotalPrice = reservation.TotalPrice.ToString("N2").Replace(".", ","),
                Vehicle = reservation.VehicleName + " - " + reservation.TransmissionTypeName,
                Charge = (reservation.TotalPrice - (reservation.DailyPrice * reservation.RentalDuration * agencyCommission / 100) - reservation.ServiceCharge - reservation.APITotalAmount).ToString("N2")
            };

            return netResysCancelModel;
        }
        public IDictionary<string, object> CreateNetresysRequestModel(NetresysRequest request)
            => new Dictionary<string, object>()
            {
                {"id",request.ID ?? "" },
                {"saleType",request.SaleType ?? "" },
                {"agencyNumber",request.AgencyNumber ?? ""},
                {"paymentType",request.PaymentType ?? "" },
                {"paymentSurcharge",request.PaymentSurcharge ?? ""},
                {"reservationDate",request.ReservationDate ?? "" },
                {"title",request.Title ?? "" },
                {"name",request.Name ?? ""},
                {"surname",request.Surname ?? "" },
                {"birthdate",request.Birthdate ?? ""},
                {"gsm",request.Gsm ?? ""},
                {"title2",request.Title2 ?? ""},
                {"name2",request.Name2 ?? "" },
                {"surname2",request.Surname2 },
                {"birthdate2",request.Birthdate2 },
                {"gsm2",request.Gsm2 },
                {"pickupLocationName",request.PickupLocationName },
                {"pickupDate",request.PickupDate },
                {"returnLocationName",request.ReturnLocationName },
                {"returnDate",request.ReturnDate },
                {"totalDays",request.TotalDays },
                {"flightNumber",request.FlightNumber },
                {"flightTime",request.FlightTime },
                {"airport",request.Airport },
                {"returnFlightNumber",request.ReturnFlightNumber },
                {"returnFlightTime",request.ReturnFlightTime },
                {"returnAirport",request.ReturnAirport },
                {"vehicle",request.Vehicle },
                {"rcGroup",request.RcGroup },
                {"rcCode",request.RcCode },
                {"rentACarProviderId",request.RentACarProviderId },
                {"options",request.Options },
                {"fuelType",request.FuelType },
                {"dailyPrice",request.DailyPrice },
                {"sistemUcreti",request.SistemUcreti },
                {"dropBedeli",request.DropBedeli },
                {"deposit",request.Deposit },
                {"extras",request.Extras },
                {"purchasePrice",request.PurchasePrice },
                {"depositPaymentType",request.DepositPaymentType },
                {"totalPrice",request.TotalPrice },
                {"note",request.Note },
                {"broker",request.Broker },
                {"charge",request.Charge },
                {"ExternalCreditCardInfo", request.ExternalCreditCardInfo }
            };

        public IDictionary<string, object> CreateNetresysCancelRequestModel(NetresysRequest request)
            => new Dictionary<string, object>()
            {
                {"id",request.ID },
                {"saleType",request.SaleType },
                {"agencyNumber", request.AgencyNumber },
                {"cancellingCharge", request.CancellingCharge == "" || request.CancellingCharge == "0" ? "0" : request.CancellingCharge },
                {"paymentType",request.PaymentType },
                {"paymentSurcharge",request.PaymentSurcharge },
                {"reservationDate",request.ReservationDate },
                {"title",request.Title },
                {"name",request.Name },
                {"surname",request.Surname },
                {"birthdate",request.Birthdate },
                {"gsm",request.Gsm },
                {"title2",request.Title2 },
                {"name2",request.Name2 },
                {"surname2",request.Surname2 },
                {"birthdate2",request.Birthdate2 },
                {"gsm2",request.Gsm2 },
                {"pickupLocationName",request.PickupLocationName },
                {"pickupDate",request.PickupDate },
                {"returnLocationName",request.ReturnLocationName },
                {"returnDate",request.ReturnDate },
                {"totalDays",request.TotalDays },
                {"flightNumber",request.FlightNumber },
                {"flightTime",request.FlightTime },
                {"airport",request.Airport },
                {"returnFlightNumber",request.ReturnFlightNumber },
                {"returnFlightTime",request.ReturnFlightTime },
                {"returnAirport",request.ReturnAirport },
                {"vehicle",request.Vehicle },
                {"rcGroup",request.RcGroup },
                {"rcCode",request.RcCode },
                {"rentACarProviderId",request.RentACarProviderId },
                {"options",request.Options },
                {"fuelType",request.FuelType },
                {"dailyPrice",request.DailyPrice },
                {"sistemUcreti",request.SistemUcreti },
                {"dropBedeli",request.DropBedeli },
                {"deposit",request.Deposit },
                {"extras",request.Extras },
                {"purchasePrice",request.PurchasePrice },
                {"depositPaymentType",request.DepositPaymentType },
                {"totalPrice",request.TotalPrice },
                {"note",request.Note },
                {"charge",request.Charge },
                {"broker",request.Broker },
            };
        public float GetAgencyCommissionWithAgencyProfitMarkup(float price, float agencyRentalProfitMarkup, float commission)
        {
            var agencyRentalProfitMarkupFreePrice = price - (price - price * 100 / (100 + agencyRentalProfitMarkup));
            var commissionPrice = (agencyRentalProfitMarkupFreePrice * commission) / 100;
            return (commissionPrice + (price - price * 100 / (100 + agencyRentalProfitMarkup))) * 100 / price;
        }

        public float GetExtrasPrice(List<ReservationExtra> extras, float agencyCommision, int rentalDay)
        {
            float exPrice = 0;
            float mandatoryPackagePrice = 0;
            foreach (var extra in extras)
            {
                exPrice += extra.ExtraRentalType == 0 ? extra.Price : extra.Price * rentalDay;
            }
            //if(extras.Any(e=>e.ExtraCode == "PRMPKT-1"))
            //{
            //    var mandatoryPackage = extras.FirstOrDefault(e => e.ExtraCode == "PRMPKT-1");
            //    exPrice = mandatoryPackage.Price* rentalDay;
            //    mandatoryPackagePrice = (agencyCommision > 0
            //            ? mandatoryPackage.Price - mandatoryPackage.Price * agencyCommision / 100
            //            : mandatoryPackage.Price);
            //    mandatoryPackagePrice = mandatoryPackagePrice * rentalDay;
            //}

            return exPrice;
        }
    }

}
