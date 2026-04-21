using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Providers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kolaycar.Broker.Api.Providers.RentGo
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly IConfigurationService _configurationService;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _configurationService = configurationService;
        }
        public Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            return Task.FromResult(new ServiceResponseBase(localReservation, false, "RentGo iptal servisi entegre edilmedi"));
        }
        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            //var entity = GetEntity();


            //var headers = new Dictionary<string, object>
            //{
            //    { "Content-Type", "application/json" },
            //    { "Authorization", $"Bearer {vendor.ApiKey + vendor.ApiPassword + vendor.ApiClientId}" },
            //};

            //Serilog.Log.Error("{@RentGoPostReservationRequestParameters}", bookRequest);

            //var response = await _httpManager.PostAsyncWithModel<RentGoReservationRequest, RentGoReservationResponse>(
            //    "/reservation",
            //    bookRequest,
            //    null,
            //    headers
            //);

            //Serilog.Log.Error("{@RentGoPostReservationResponse}", response);

            //localReservation.ReservationPostedToAPI = true;
            //localReservation.APIVendorName = vendor.VendorName;

            //if (response != null && !string.IsNullOrEmpty(response.Pnr))
            //{
            //    localReservation.APIReservationSuccessfully = true;
            //    localReservation.APIReservationNumber = response.Pnr;

            //    return new ServiceResponseBase(localReservation, true, "Rezervasyon başarılı. PNR: " + response.Pnr);
            //}

            //localReservation.APIMessage = "RentGo rezervasyon onayı alınamadı!";
            return new ServiceResponseBase(localReservation, false, "RentGo rezervasyon hatası!");
        }

        //private RentGoReservationRequest GetEntity(PostReservationRequest postReservationRequest, ReservationToken reservationToken, Reservation reservation)
        //{
        //    List<string> extras = new List<string>();

        //    if (reservation.ReservationExtras.Count > 0)
        //    {
        //        foreach (var extra in reservation.ReservationExtras)
        //            extras.Add(extra.ApiExtraCode);
        //    }


        //    var bookRequest = new RentGoReservationRequest
        //    {
        //        ResType = 1,
        //        ListId = reservationToken.APIReferenceCode,
        //        VersionId = reservationToken.VehicleCode,
        //        CustomerInfo = new RentGoCustomerInfo
        //        {
        //            FirstName = postReservationRequest.CustomerName,
        //            LastName = postReservationRequest.CustomerSurname,
        //            Email = postReservationRequest.CustomerEmail,
        //            MobileNumber = postReservationRequest.CustomerTelephone?.Replace(" ", string.Empty),
        //            GovernmentId = postReservationRequest.CustomerPersonalNumber,
        //            BirthDate = postReservationRequest.CustomerBirthDay,
        //            CustomerType = 1,
        //            IsTurkish = true,
        //            DialCode = "+90",
        //            Gender = 1,
        //        },
        //        InvoiceInfo = new RentGoInvoiceInfo
        //        {
        //            Name = postReservationRequest.CustomerName,
        //            Surname = postReservationRequest.CustomerSurname,
        //            Title = "Bireysel",
        //            GovernmentId = postReservationRequest.CustomerPersonalNumber
        //        }
        //    };
        //}
    }
}
