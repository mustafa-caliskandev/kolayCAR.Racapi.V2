using System.Collections.Generic;
using Kolaycar.Broker.Api.Helpers.YesOto;
using System.Linq;

namespace Kolaycar.Broker.Api.Mappers.YesOto
{
    public static class ReservationMapper
    {
        public static KolayCAR.Broker.Domain.Models.Requests.YesOto.YesOtoCreateReservationRequest Map(
            this KolayCAR.Broker.Domain.Models.Requests.PostReservationRequest request,
            KolayCAR.Broker.Domain.Models.ReservationToken reservationToken)
        {
            if (request == null) return null;

            return new KolayCAR.Broker.Domain.Models.Requests.YesOto.YesOtoCreateReservationRequest
            {
                BrandId = YesOtoConstants.BrandId,
                SalesChannelId = "", 
                LanguageId = "1", // Default language or get from request
                FirstName = request.CustomerName,
                LastName = request.CustomerSurname,
                Phone = request.CustomerTelephone,
                Email = request.CustomerEmail,
                BirthDate = request.CustomerBirthDay, // Assuming correct format or needs parsing
                TCKN = request.CustomerPersonalNumber,
                Address = request.CustomerAddress,
                BillingInformation = new KolayCAR.Broker.Domain.Models.Requests.YesOto.YesOtoBillingInformation
                {
                    Address = request.CustomerAddress,
                    BillingType = "Individual",
                    FullName = $"{request.CustomerName} {request.CustomerSurname}",
                    TaxNumber = request.CustomerPersonalNumber
                },
                ReservationRequestModel = new KolayCAR.Broker.Domain.Models.Requests.YesOto.YesOtoReservationRequestModel
                {
                    BrandId = YesOtoConstants.BrandId,
                    Location = reservationToken.APIPickupLocationCode,
                    DropOffLocation = reservationToken.APIReturnLocationCode,
                    Start = reservationToken.PickupDateTime.ToString("MM/dd/yyyy HH:mm:ss"),
                    End = reservationToken.ReturnDateTime.ToString("MM/dd/yyyy HH:mm:ss"),
                    SelectedVehicleGroup = reservationToken.VehicleCode,
                    ProcessType = "Payment",
                    SelectedAdditionalServices = new List<KolayCAR.Broker.Domain.Models.Requests.YesOto.YesOtoSelectedAdditionalService>()
                }
            };
        }
    }
}
