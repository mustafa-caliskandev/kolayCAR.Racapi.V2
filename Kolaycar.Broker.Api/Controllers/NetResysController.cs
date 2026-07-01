using KolayCAR.Broker.API.Attributes;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.NetResys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetResysNew;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
//using static NetResys.ServiceSoapClient;
using static NetResysNew.ServiceSoapClient;

namespace KolayCAR.Broker.API.Controllers
{
    [BrokerAuthorize(UserRoles.Admin, UserRoles.Agency, UserRoles.User, UserRoles.MobileAPP, UserRoles.External)]
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class NetResysController : ControllerBase
    {
        [HttpPost]
        public async Task<HttpResult<NetResysXmlResponseBody>> Post(
            string id,
            string saleType,
            string agencyNumber,
            string paymentType,
            string paymentSurcharge,
            string reservationDate,
            string title,
            string name,
            string surname,
            string birthdate,
            string gsm,
            string title2,
            string name2,
            string surname2,
            string birthdate2,
            string gsm2,
            string pickupLocationName,
            string pickupDate,
            string returnLocationName,
            string returnDate,
            string totalDays,
            string flightNumber,
            string flightTime,
            string airport,
            string returnFlightNumber,
            string returnFlightTime,
            string returnAirport,
            string vehicle,
            string rcGroup,
            string rcCode,
            string rentACarProviderId,
            string options,
            string fuelType,
            string dailyPrice,
            string sistemUcreti,
            string dropBedeli,
            string deposit,
            string extras,
            string purchasePrice,
            string depositPaymentType,
            string totalPrice,
            string note,
            string broker,
            string charge,
            string ExternalCreditCardInfo)
        {
            NetResysXmlResponse postNetResysResult = new NetResysXmlResponse(new NetResysXmlResponseBody { });

            ZAHLUNG zahlung = new ZAHLUNG();

            if (!string.IsNullOrEmpty(ExternalCreditCardInfo))
            {
                zahlung.ZAHLUNGSART = "Kreditkarte";
                zahlung.KARTENNUMMER = ExternalCreditCardInfo.Split('|')[0];
                zahlung.BETRAG = ExternalCreditCardInfo.Split('|')[1];
                zahlung.KARTENINHABER = ExternalCreditCardInfo.Split('|')[2];
                zahlung.TRANSAKTIONID = ExternalCreditCardInfo.Split('|')[3];
            }

            var postNetResys = new PostNetResys
            {
                ID = id,
                SaleType = saleType,
                AgencyNumber = agencyNumber,
                PaymentType = paymentType,
                PaymentSurcharge = paymentSurcharge,
                ReservationDate = reservationDate,
                Title = title,
                Name = name,
                Surname = surname,
                Birthdate = birthdate,
                Gsm = gsm,
                Title2 = title2,
                Name2 = name2,
                Surname2 = surname2,
                Birthdate2 = birthdate2,
                Gsm2 = gsm2,
                PickupLocationName = pickupLocationName,
                PickupDate = pickupDate,
                ReturnLocationName = returnLocationName,
                ReturnDate = returnDate,
                TotalDays = totalDays,
                FlightNumber = flightNumber,
                FlightTime = flightTime,
                Airport = airport,
                ReturnFlightNumber = returnFlightNumber,
                ReturnFlightTime = returnFlightTime,
                ReturnAirport = returnAirport,
                Vehicle = vehicle,
                RcGroup = rcGroup,
                RcCode = rcCode,
                RentACarProviderId = rentACarProviderId,
                Options = options,
                FuelType = fuelType,
                DailyPrice = dailyPrice,
                SistemUcreti = sistemUcreti,
                DropBedeli = dropBedeli,
                Deposit = deposit,
                Extras = extras,
                PurchasePrice = purchasePrice,
                DepositPaymentType = depositPaymentType,
                TotalPrice = totalPrice,
                Note = note,
                Broker = broker,
                Charge = charge,
                CancellingCharge = "",
                ZAHLUNG = zahlung
            };
            try
            {
                Serilog.Log.Error("{@postNetResys}", postNetResys);

                var postNetResysData = new XElement("RentalCarReservation",
                    new XAttribute(nameof(postNetResys.ID), postNetResys.ID),
                    new XAttribute(nameof(postNetResys.SaleType), postNetResys.SaleType),
                    new XElement(nameof(postNetResys.AgencyNumber), postNetResys.AgencyNumber),
                    new XElement(nameof(postNetResys.PaymentType), postNetResys.PaymentType),
                    new XElement(nameof(postNetResys.PaymentSurcharge), postNetResys.PaymentSurcharge),
                    new XElement(nameof(postNetResys.ReservationDate), postNetResys.ReservationDate),
                    new XElement(nameof(postNetResys.Title), postNetResys.Title),
                    new XElement(nameof(postNetResys.Name), postNetResys.Name),
                    new XElement(nameof(postNetResys.Surname), postNetResys.Surname),
                    new XElement(nameof(postNetResys.Birthdate), postNetResys.Birthdate),
                    new XElement(nameof(postNetResys.Gsm), postNetResys.Gsm),
                    new XElement(nameof(postNetResys.Title2), postNetResys.Title2),
                    new XElement(nameof(postNetResys.Name2), postNetResys.Name2),
                    new XElement(nameof(postNetResys.Surname2), postNetResys.Surname2),
                    new XElement(nameof(postNetResys.Birthdate2), postNetResys.Birthdate2),
                    new XElement(nameof(postNetResys.Gsm2), postNetResys.Gsm2),
                    new XElement(nameof(postNetResys.PickupLocationName), postNetResys.PickupLocationName),
                    new XElement(nameof(postNetResys.PickupDate), postNetResys.PickupDate),
                    new XElement(nameof(postNetResys.ReturnLocationName), postNetResys.ReturnLocationName),
                    new XElement(nameof(postNetResys.ReturnDate), postNetResys.ReturnDate),
                    new XElement(nameof(postNetResys.TotalDays), postNetResys.TotalDays),
                    new XElement(nameof(postNetResys.FlightNumber), postNetResys.FlightNumber),
                    new XElement(nameof(postNetResys.FlightTime), postNetResys.FlightTime),
                    new XElement(nameof(postNetResys.Airport), postNetResys.Airport),
                    new XElement(nameof(postNetResys.ReturnFlightNumber), postNetResys.ReturnFlightNumber),
                    new XElement(nameof(postNetResys.ReturnFlightTime), postNetResys.ReturnFlightTime),
                    new XElement(nameof(postNetResys.ReturnAirport), postNetResys.ReturnAirport),
                    new XElement(nameof(postNetResys.Vehicle), postNetResys.Vehicle),
                    new XElement(nameof(postNetResys.RcGroup), postNetResys.RcGroup),
                    new XElement(nameof(postNetResys.RcCode), postNetResys.RcCode),
                    new XElement(nameof(postNetResys.RentACarProviderId), postNetResys.RentACarProviderId),
                    new XElement(nameof(postNetResys.Options), postNetResys.Options),
                    new XElement(nameof(postNetResys.FuelType), postNetResys.FuelType),
                    new XElement(nameof(postNetResys.DailyPrice), postNetResys.DailyPrice),
                    new XElement(nameof(postNetResys.SistemUcreti), postNetResys.SistemUcreti),
                    new XElement(nameof(postNetResys.DropBedeli), postNetResys.DropBedeli),
                    new XElement(nameof(postNetResys.Deposit), postNetResys.Deposit),
                    new XElement(nameof(postNetResys.Extras), postNetResys.Extras),
                    new XElement(nameof(postNetResys.PurchasePrice), postNetResys.PurchasePrice),
                    new XElement(nameof(postNetResys.DepositPaymentType), postNetResys.DepositPaymentType),
                    new XElement(nameof(postNetResys.TotalPrice), postNetResys.TotalPrice),
                    new XElement(nameof(postNetResys.Note), postNetResys.Note),
                    new XElement(nameof(postNetResys.Broker), postNetResys.Broker),
                    new XElement(nameof(postNetResys.Charge), postNetResys.Charge),
                    ExternalCreditCardInfo != ""
                            ? SerializeObjectToXml(nameof(postNetResys.ZAHLUNG), postNetResys.ZAHLUNG)
                            : null
                    );

                //TODO: parKey parametresi configuration' dan çekilecek!
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var netResysService = new ServiceSoapClient(EndpointConfiguration.ServiceSoap);

                postNetResysResult = await netResysService.NetResysXmlAsync(postNetResysData.ToString(), "airgh1212.-,,9787sas1l");

                Serilog.Log.Error("{@postNetResysResponse}", postNetResysResult.Body.NetResysXmlResult);


                return HttpResult<NetResysXmlResponseBody>.Result(
                    data: postNetResysResult.Body,
                    httpResultType: HttpStatusCode.OK,
                    success: postNetResysResult.Body.NetResysXmlResult.ToString() == "OK",
                    message: postNetResysResult.Body.NetResysXmlResult.ToString());
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@postNetResysError}", ex.Message);

                return HttpResult<NetResysXmlResponseBody>.Result(
                                   data: null,
                                   httpResultType: HttpStatusCode.InternalServerError,
                                   success: false,
                                   message: ex.Message);
            }
        }


        [HttpPost("cancel")]
        public async Task<HttpResult<NetResysXmlResponseBody>> Cancel(
                    string id,
                    string saleType,
                    string agencyNumber,
                    string cancellingCharge,
                    string paymentType,
                    string paymentSurcharge,
                    string reservationDate,
                    string title,
                    string name,
                    string surname,
                    string birthdate,
                    string gsm,
                    string title2,
                    string name2,
                    string surname2,
                    string birthdate2,
                    string gsm2,
                    string pickupLocationName,
                    string pickupDate,
                    string returnLocationName,
                    string returnDate,
                    string totalDays,
                    string flightNumber,
                    string flightTime,
                    string airport,
                    string returnFlightNumber,
                    string returnFlightTime,
                    string returnAirport,
                    string vehicle,
                    string rcGroup,
                    string rcCode,
                    string rentACarProviderId,
                    string options,
                    string fuelType,
                    string dailyPrice,
                    string sistemUcreti,
                    string dropBedeli,
                    string deposit,
                    string extras,
                    string purchasePrice,
                    string depositPaymentType,
                    string totalPrice,
                    string note,
                    string broker,
                    string charge)
        {
            NetResysXmlResponse postNetResysResult = new NetResysXmlResponse(new NetResysXmlResponseBody { });
            var postNetResys = new PostNetResys
            {
                ID = id,
                SaleType = saleType,
                AgencyNumber = agencyNumber,
                CancellingCharge = string.IsNullOrWhiteSpace(cancellingCharge) ? "0" : ToRefundPrice(cancellingCharge),
                PaymentType = paymentType,
                PaymentSurcharge = paymentSurcharge,
                ReservationDate = reservationDate,
                Title = title,
                Name = name,
                Surname = surname,
                Birthdate = birthdate,
                Gsm = gsm,
                Title2 = title2,
                Name2 = name2,
                Surname2 = surname2,
                Birthdate2 = birthdate2,
                Gsm2 = gsm2,
                PickupLocationName = pickupLocationName,
                PickupDate = pickupDate,
                ReturnLocationName = returnLocationName,
                ReturnDate = returnDate,
                TotalDays = totalDays,
                FlightNumber = flightNumber,
                FlightTime = flightTime,
                Airport = airport,
                ReturnFlightNumber = returnFlightNumber,
                ReturnFlightTime = returnFlightTime,
                ReturnAirport = returnAirport,
                Vehicle = vehicle,
                RcGroup = rcGroup,
                RcCode = rcCode,
                RentACarProviderId = rentACarProviderId,
                Options = options,
                FuelType = fuelType,
                DailyPrice = ToRefundPrice(dailyPrice),
                SistemUcreti = ToRefundPrice(sistemUcreti),
                DropBedeli = ToRefundPrice(dropBedeli),
                Deposit = ToRefundPrice(deposit),
                Extras = ToRefundPrice(extras),
                PurchasePrice = ToRefundPrice(purchasePrice),
                DepositPaymentType = depositPaymentType,
                TotalPrice = ToRefundPrice(totalPrice),
                Note = note,
                Broker = broker,
                Charge = ToRefundPrice(charge)
            };
            try
            {
                Serilog.Log.Error("{@postCancelNetResys}", postNetResys);

                XElement postNetResysData = new XElement("RentalCarReservation",
                    new XAttribute(nameof(postNetResys.ID), postNetResys.ID),
                    new XAttribute(nameof(postNetResys.SaleType), postNetResys.SaleType),
                    new XElement(nameof(postNetResys.AgencyNumber), postNetResys.AgencyNumber),
                    new XElement(nameof(postNetResys.PaymentType), postNetResys.PaymentType),
                    new XElement(nameof(postNetResys.PaymentSurcharge), postNetResys.PaymentSurcharge),
                    new XElement(nameof(postNetResys.ReservationDate), postNetResys.ReservationDate),
                    new XElement(nameof(postNetResys.Title), postNetResys.Title),
                    new XElement(nameof(postNetResys.Name), postNetResys.Name),
                    new XElement(nameof(postNetResys.Surname), postNetResys.Surname),
                    new XElement(nameof(postNetResys.Birthdate), postNetResys.Birthdate),
                    new XElement(nameof(postNetResys.Gsm), postNetResys.Gsm),
                    new XElement(nameof(postNetResys.Title2), postNetResys.Title2),
                    new XElement(nameof(postNetResys.Name2), postNetResys.Name2),
                    new XElement(nameof(postNetResys.Surname2), postNetResys.Surname2),
                    new XElement(nameof(postNetResys.Birthdate2), postNetResys.Birthdate2),
                    new XElement(nameof(postNetResys.Gsm2), postNetResys.Gsm2),
                    new XElement(nameof(postNetResys.PickupLocationName), postNetResys.PickupLocationName),
                    new XElement(nameof(postNetResys.PickupDate), postNetResys.PickupDate),
                    new XElement(nameof(postNetResys.ReturnLocationName), postNetResys.ReturnLocationName),
                    new XElement(nameof(postNetResys.ReturnDate), postNetResys.ReturnDate),
                    new XElement(nameof(postNetResys.TotalDays), postNetResys.TotalDays),
                    new XElement(nameof(postNetResys.FlightNumber), postNetResys.FlightNumber),
                    new XElement(nameof(postNetResys.FlightTime), postNetResys.FlightTime),
                    new XElement(nameof(postNetResys.Airport), postNetResys.Airport),
                    new XElement(nameof(postNetResys.ReturnFlightNumber), postNetResys.ReturnFlightNumber),
                    new XElement(nameof(postNetResys.ReturnFlightTime), postNetResys.ReturnFlightTime),
                    new XElement(nameof(postNetResys.ReturnAirport), postNetResys.ReturnAirport),
                    new XElement(nameof(postNetResys.Vehicle), postNetResys.Vehicle),
                    new XElement(nameof(postNetResys.RcGroup), postNetResys.RcGroup),
                    new XElement(nameof(postNetResys.RcCode), postNetResys.RcCode),
                    new XElement(nameof(postNetResys.RentACarProviderId), postNetResys.RentACarProviderId),
                    new XElement(nameof(postNetResys.Options), postNetResys.Options),
                    new XElement(nameof(postNetResys.FuelType), postNetResys.FuelType),
                    new XElement(nameof(postNetResys.DailyPrice), postNetResys.DailyPrice),
                    new XElement(nameof(postNetResys.SistemUcreti), postNetResys.SistemUcreti),
                    new XElement(nameof(postNetResys.DropBedeli), postNetResys.DropBedeli),
                    new XElement(nameof(postNetResys.Deposit), postNetResys.Deposit),
                    new XElement(nameof(postNetResys.Extras), postNetResys.Extras),
                    new XElement(nameof(postNetResys.PurchasePrice), postNetResys.PurchasePrice),
                    new XElement(nameof(postNetResys.DepositPaymentType), postNetResys.DepositPaymentType),
                    new XElement(nameof(postNetResys.TotalPrice), postNetResys.TotalPrice),
                    new XElement(nameof(postNetResys.Note), postNetResys.Note),
                    new XElement(nameof(postNetResys.Broker), postNetResys.Broker),
                    new XElement(nameof(postNetResys.Charge), postNetResys.Charge),
                    new XElement(nameof(postNetResys.CancellingCharge), postNetResys.CancellingCharge));

                //TODO: parKey parametresi configuration' dan çekilecek!
                //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var netResysService = new ServiceSoapClient(EndpointConfiguration.ServiceSoap);

                postNetResysResult = await netResysService.NetResysXmlAsync(postNetResysData.ToString(), "airgh1212.-,,9787sas1l");
                Serilog.Log.Error("{@postCancelNetResysResponse}", postNetResysResult.Body.NetResysXmlResult);

                return HttpResult<NetResysXmlResponseBody>.Result(
                    data: postNetResysResult.Body,
                    httpResultType: HttpStatusCode.OK,
                    success: postNetResysResult.Body.NetResysXmlResult.ToString() == "OK",
                    message: postNetResysResult.Body.NetResysXmlResult.ToString());
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@postCancelNetResysError}", ex.Message);
                return HttpResult<NetResysXmlResponseBody>.Result(
                    data: null,
                    httpResultType: HttpStatusCode.OK,
                    success: false,
                    message: ex.Message
                    );
            }
        }
        public static XElement SerializeObjectToXml(string elementName, object obj)
        {
            if (obj == null)
                return null;

            var element = new XElement(elementName);
            var properties = obj.GetType().GetProperties();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj, null);
                if (value != null)
                {
                    element.Add(new XElement(prop.Name, value));
                }
            }

            return element;
        }

        private static string ToRefundPrice(string price)
        {
            if (string.IsNullOrWhiteSpace(price))
                return price;

            var trimmedPrice = price.Trim();

            if (trimmedPrice.StartsWith("-") || IsZeroPrice(trimmedPrice))
                return trimmedPrice;

            return $"-{trimmedPrice}";
        }

        private static bool IsZeroPrice(string price)
        {
            var normalizedPrice = price.Replace(".", ",");

            return decimal.TryParse(
                normalizedPrice,
                NumberStyles.Number,
                CultureInfo.GetCultureInfo("tr-TR"),
                out var parsedPrice) && parsedPrice == 0;
        }

    }
}
