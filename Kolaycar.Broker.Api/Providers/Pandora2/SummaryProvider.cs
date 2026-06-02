using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Pandora2
{
    public class SummaryProvider : ISummaryProvider
    {
        private IExtraProvider ExtraProvider { get; set; }
        private RestManager RestManager { get; set; }
        private AuthProvider AuthProvider { get; set; }

        public SummaryProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(AuthProvider.NormalizeBaseUrl(apiBaseUrl));
            AuthProvider = new AuthProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> GetSummary(GetSummaryRequest getSummaryRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, List<ExchangeRates> exchangeRates, List<Vehicle> localVehicles, List<SubVendor> subVendors)
        {
            var reservationToken = additionalInformation.ReservationToken;
            ExtraProvider = new ExtraProvider();

            var getExtrasRequest = new GetExtrasRequest
            {
                VendorType = getSummaryRequest.VendorType,
                ApiKey = vendor.ApiKey,
                ApiPassword = vendor.ApiPassword,
                LanguageCode = getSummaryRequest.LanguageCode,
                CurrencyCode = getSummaryRequest.CurrencyCode,
                PickupLocationId = getSummaryRequest.PickupLocationId,
                ReturnLocationId = getSummaryRequest.ReturnLocationId,
                PickupDate = getSummaryRequest.PickupDate,
                ReturnDate = getSummaryRequest.ReturnDate,
                PickupTime = getSummaryRequest.PickupTime,
                ReturnTime = getSummaryRequest.ReturnTime,
                UserToken = getSummaryRequest.UserToken,
                CouponCode = getSummaryRequest.CouponCode
            };

            var getExtrasResponse = await ExtraProvider.GetExtras(getExtrasRequest, vendor, additionalInformation, exchangeRates, localVehicles, subVendors);
            var extrasResponse = getExtrasResponse.Data as GetExtrasResponse;

            if (extrasResponse?.Vehicle == null)
                return new ServiceResponseBase(null, false, "Pandora2 summary icin arac bilgisi alinamadi.");

            var selectedExtras = ReservationHelper.ExtraToReservationExtra(extrasResponse.Extras ?? new List<Extra>(), getSummaryRequest.ExtraList);
            var additions = CreateAdditions(selectedExtras, extrasResponse.Extras);
            var auth = await AuthProvider.GetAccessToken(vendor);

            if (auth != null && !string.IsNullOrWhiteSpace(auth.access_token))
            {
                await RestManager.PostAsyncRestClient<Pandora2ResponseBase.PriceResponse>(
                    requestPath: "bookings/calculate",
                    parameterType: ParameterType.RequestBody,
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token, getSummaryRequest.LanguageCode),
                    entity: AuthProvider.CreateJsonBody(CreateCalculateRequest(reservationToken, additionalInformation, additions)));
            }

            return new ServiceResponseBase
            {
                Success = extrasResponse.Vehicle != null,
                Data = new GetSummaryResponse
                {
                    Vehicle = extrasResponse.Vehicle,
                    Extras = selectedExtras
                }
            };
        }

        internal static List<Pandora2AdditionRequest> CreateAdditions(List<ReservationExtra> selectedExtras, List<Extra> apiExtras)
        {
            var additions = new List<Pandora2AdditionRequest>();

            if (selectedExtras == null || apiExtras == null)
                return additions;

            foreach (var selectedExtra in selectedExtras)
            {
                var apiExtra = apiExtras.FirstOrDefault(x => x.ExtraId == selectedExtra.ExtraId || x.ExtraCode == selectedExtra.ExtraCode);

                if (apiExtra?.ApiExtraCode == null)
                    continue;

                additions.Add(new Pandora2AdditionRequest
                {
                    Id = selectedExtra.ExtraCode.Contains("<>") ? selectedExtra.ExtraCode.Split("<>")[0].ToStringNullSafe() : "",
                    Count = selectedExtra.Piece,
                    GroupId = selectedExtra.ExtraCode.Contains("<>") ? selectedExtra.ExtraCode.Split("<>")[1].ToIntNullSafe() : 0,
                });
            }

            return additions;
        }

        internal static Pandora2CalculateRequest CreateCalculateRequest(ReservationToken reservationToken, ResponseReservationStepsAdditionalInformation additionalInformation, List<Pandora2AdditionRequest> additions) =>
            new Pandora2CalculateRequest
            {
                VehicleId = reservationToken.VehicleCode,
                OfficeOutId = additionalInformation.APIPickupLocationCode,
                OfficeInId = additionalInformation.APIReturnLocationCode,
                DateOut = VehicleProvider.FormatApiDate(additionalInformation.PickupDateTime),
                DateIn = VehicleProvider.FormatApiDate(additionalInformation.ReturnDateTime),
                Additions = additions
            };
    }

    public class Pandora2AdditionRequest
    {
        public string Id { get; set; }
        public int Count { get; set; }
        public int GroupId { get; set; }
    }

    public class Pandora2CalculateRequest
    {
        public string VehicleId { get; set; }
        public string OfficeOutId { get; set; }
        public string OfficeInId { get; set; }
        public string DateOut { get; set; }
        public string DateIn { get; set; }
        public List<Pandora2AdditionRequest> Additions { get; set; }
    }
}
