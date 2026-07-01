using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.TurmobilResponseBase;
using TurmobilProvider = KolayCAR.Broker.API.Providers.Turmobil;

namespace KolayCAR.Broker.API.Providers.Turmobil
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        private readonly IConfigurationService _configurationService;
        ILocationProvider locationProvider { get; set; }


        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            AuthProvider = new AuthProvider(apiBaseUrl);
            _configurationService = configurationService;
            locationProvider = new TurmobilProvider.LocationProvider(apiBaseUrl);
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var postCancelReservationRequestBody = PostCancelReservationRequestBody(localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postCancelReservationRequestBody),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@TurmobilPostCancelReservationsRequestParameters}", postCancelReservationRequestBody);

            // var token = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);

            var result = await RestManager.PostAsyncResult<TurmobilRequestBase.PostCancelReservationRequest, CancelReservationResponse>(
                requestPath: "rest/dailyrezervation/cancelRezervation",
                //headers: AuthProvider.CreateAuthHeaderWithContentType(token.Token),
                entity: postCancelReservationRequestBody,
                headers: new Dictionary<string, object>
                {
                        {"username" , vendor.ApiKey},
                        {"password" , vendor.ApiPassword}
                },
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            if (result?.Data != null && result.Data.responseCode == "00" && result.Data.responseMsg == "Approved")
            {
                localReservation.APIReservationCancel = true;

                return new ServiceResponseBase
                {
                    Success = true,
                    Data = localReservation
                };
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "Rezervasyon Servisine Ulaşılamadı.");
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Domain.Models.Extra> apiExtras)
        {
            float apiPaidAmount = string.IsNullOrEmpty(postReservationRequest.CouponCode) ? CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest) : postReservationRequest.PaidAmountAfterUsingCouponCode.ToFloatNullSafe();
            string extraList = localReservation.ReservationExtras.Count > 0 ? FormatExtraList(localReservation) : string.Empty;
            var postReservationRequestBody = PostReservationRequestParameters(postReservationRequest, reservationToken, additionalInformation, localReservation, apiPaidAmount, vendor, extraList);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestBody),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@TurmobilPostReservationRequestParameters}", postReservationRequestBody);

            //var token = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);

            var result = await RestManager.PostAsyncResult<TurmobilRequestBase.PostReservationRequest, TurmobilResponseBase.PostReservationResponse>(
                requestPath: "rest/dailyrezervation/createRezervation",
                //headers: AuthProvider.CreateAuthHeaderWithContentType(token.Token),
                entity: postReservationRequestBody,
                headers: new Dictionary<string, object>
                {
                        {"username" , vendor.ApiKey},
                        {"password" , vendor.ApiPassword}
                },
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@TurmobilPostReservationResult}", result);

            if (result?.Data != null && !string.IsNullOrEmpty(result.Data.uuid))
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Data.uuid;

                //var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                //Serilog.Log.Error("{@TurmobilGetLocationsResponse}", location);
                //var reservationLocation = new List<Domain.Models.Location>();
                //if (location.Success)
                //{
                //    reservationLocation = location.Data as List<Domain.Models.Location>;

                //    var reservationPickupLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIPickupLocationCode).FirstOrDefault();
                //    var reservationReturnLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIReturnLocationCode).FirstOrDefault();

                //    if (reservationPickupLocation != null)
                //    {
                //        localReservation.APIVendorPickupAddress = reservationPickupLocation.Address;
                //        localReservation.APIVendorPickupPhone = reservationPickupLocation.PhoneNumber;
                //    }

                //    if (reservationReturnLocation != null)
                //    {
                //        localReservation.APIVendorReturnAddress = reservationReturnLocation.Address;
                //        localReservation.APIVendorReturnPhone = reservationReturnLocation.PhoneNumber;
                //    }
                //}
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = localReservation
                };
            }

            return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "Rezervasyon Servisine Ulaşılamadı.");
        }

        private TurmobilRequestBase.PostReservationRequest PostReservationRequestParameters(PostReservationRequest postReservationRequest, ReservationToken reservationToken, ResponseReservationStepsAdditionalInformation additionalInformation, Reservation localReservation, float PaidAmount, Vendor vendor, string extraList)
        {
            return new TurmobilRequestBase.PostReservationRequest
            {
                customerName = postReservationRequest.CustomerName + " " + postReservationRequest.CustomerSurname,
                taxNo = postReservationRequest.CustomerPersonalNumber,
                deliveryDate = postReservationRequest.PickupDate.Replace('.', '/') + " " + postReservationRequest.PickupTime,
                returnDate = postReservationRequest.ReturnDate.Replace('.', '/') + " " + postReservationRequest.ReturnTime,
                vehicleTypeId = reservationToken.VehicleCode,
                deliveryLocationId = additionalInformation.APIPickupLocationCode.ToString(),
                returnLocationId = additionalInformation.APIReturnLocationCode.ToString(),
                damageInsuranceId = string.Empty,
                //addServicesId = localReservation.ReservationExtras.Count > 0 ? localReservation.ReservationExtras.Select(x => x.ExtraCode)  (x => new TurmobilRequestBase.ReservationAdditionalProduct
                //{
                //    id = x.ExtraCode
                //}).ToList() : new List<TurmobilRequestBase.ReservationAdditionalProduct>(),
                additionalServices = extraList,
                customerPhone = postReservationRequest.CustomerTelephone,
                isPaid = PaidAmount > 0,
                //paidAmount = PaidAmount,
                //paidAmount = reservationToken.DailyPrice * additionalInformation.RentalDuration,
                paidAmount = reservationToken.APIDailyPrice * reservationToken.RentalDuration,
                fullCredit = CreditHelper.ResolveTokenCreditType(reservationToken) == CreditType.FullCredit,
                curr = reservationToken.BaseVendorRequestCurrencyType.ToString()
            };
        }

        private string FormatExtraList(Reservation localReservation)
        {
            string extraList = "[";
            for (int i = 0; i < localReservation.ReservationExtras.Count; i++)
            {
                if (i != localReservation.ReservationExtras.Count - 1)
                    extraList += localReservation.ReservationExtras[i].ExtraCode + ",";
                else
                    extraList += localReservation.ReservationExtras[i].ExtraCode;
            }
            extraList += "]";
            return extraList;
        }

        private TurmobilRequestBase.PostCancelReservationRequest PostCancelReservationRequestBody(Reservation localReservation)
        {
            return new TurmobilRequestBase.PostCancelReservationRequest
            {
                uuid = localReservation.APIReservationNumber
            };
        }
    }
}
