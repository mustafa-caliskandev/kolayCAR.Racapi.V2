using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.Circular2ResponseBase;

namespace KolayCAR.Broker.API.Providers.Circular2
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly HttpManager _httpManager;
        private readonly RestManager _restManager;
        private readonly AuthProvider _authProvider;
        private readonly IConfigurationService _configurationService;
        private readonly ILocationProvider _locationProvider;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _httpManager = new HttpManager(apiBaseUrl);
            _restManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _authProvider = new AuthProvider(apiBaseUrl);
            _configurationService = configurationService;
            _locationProvider = new LocationProvider(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Domain.Models.Reservation localReservation)
        {
            var postCancelReservationRequestBody = GetCancelParametersForUrlEncoded(postCancelReservationRequest, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postCancelReservationRequestBody),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@CircularPostCancelReservationRequest}", postCancelReservationRequestBody);

            var auth = await _authProvider.GetTokenAsync(vendor.ApiKey);

            var result = await _restManager.PostAsyncWithUrlEncoded<List<KeyValuePair<string, string>>, ReservationCancelBase>(
                //requestPath: $"reservation/save?token={auth.token}&size=500&referralagent={vendor.ApiPassword}",
                requestPath: $"reservation/cancel?token={auth.token}",
                headers: _authProvider.CreateHeaderWithContentType(),
                entity: postCancelReservationRequestBody,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }, isReservationRequest: true);

            Serilog.Log.Error("{@CircularPostCancelReservationResponse}", result);

            if (result != null && result.result != null && result.result.status.ToLower() == "canceled")
            {
                localReservation.APIReservationCancel = true;

                return new ServiceResponseBase
                {
                    Success = true,
                    Data = localReservation
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Circular Servisine Ulaşılamadı",
                Data = localReservation
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Domain.Models.Reservation localReservation, List<Extra> apiExtras)
        {
            var auth = await _authProvider.GetTokenAsync(vendor.ApiKey);


            if (auth != null && !string.IsNullOrEmpty(auth.token))
            {
                var driverSaveEntity = GetDriverEntity(postReservationRequest);

                var driverSave = await _httpManager.PostAsyncWithUrlEncoded<List<KeyValuePair<string, string>>, DriverResponse>(
                   requestPath: $"driver/save?token={auth.token}",
                   headers: _authProvider.CreateHeaderWithContentType(),
                   entity: driverSaveEntity,
                   brokerLogModel: new BrokerLogModel()
                   {
                       LogKey = localReservation.ReservationNumber,
                       LogType = BrokerLogTypes.ReservationVendorAPIResponse
                   },
                   isReservationRequest: true
                 );

                var driverId = driverSave?.data?.id.ToStringNullSafe() ?? "";

                var postReservationRequestBody = GetParametersForUrlEncoded(postReservationRequest, localReservation, additionalInformation, vendor, reservationToken, driverId);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(postReservationRequestBody),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                Serilog.Log.Error("{@CircularPostReservationRequestParameters}", postReservationRequestBody);

                float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);

                var result = await _restManager.PostAsyncWithUrlEncoded<List<KeyValuePair<string, string>>, ReservationResponse>(
                  //requestPath: $"reservation/save?token={auth.token}&size=500&referralagent={vendor.ApiPassword}",
                  requestPath: $"reservation/save?token={auth.token}",
                  headers: _authProvider.CreateHeaderWithContentType(),
                  entity: postReservationRequestBody,
                  brokerLogModel: new BrokerLogModel()
                  {
                      LogKey = localReservation.ReservationNumber,
                      LogType = BrokerLogTypes.ReservationVendorAPIResponse
                  },
                  isReservationRequest: true
                  );

                //var result = await _httpManager.PostAsync<Circular2PostReservationRequestBody, ReservationResponse>(
                //    requestPath: $"reservation/save?token={auth.token}",
                //    headers: _authProvider.CreateHeaderWithContentType(),
                //    entity: postReservationRequestBody,
                //    brokerLogModel: new BrokerLogModel()
                //    {
                //        LogKey = localReservation.ReservationNumber,
                //        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                //    }
                //    );

                Serilog.Log.Error("{@CircularPostReservationResult}", result);

                if (result != null && result.success && result.data != null)
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.data.id.ToString();

                    var location = await _locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);

                    Serilog.Log.Error("{@CircularGetLocationsResponse}", location);

                    var reservationLocation = new List<Domain.Models.Location>();

                    if (location.Success)
                    {
                        reservationLocation = location.Data as List<Domain.Models.Location>;

                        var reservationPickupLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIPickupLocationCode).FirstOrDefault();
                        var reservationReturnLocation = reservationLocation.Where(x => x.LocationCode == additionalInformation.APIReturnLocationCode).FirstOrDefault();

                        if (reservationPickupLocation != null)
                        {
                            localReservation.APIVendorPickupAddress = reservationPickupLocation.Address;
                            localReservation.APIVendorPickupPhone = reservationPickupLocation.PhoneNumber;
                        }

                        if (reservationReturnLocation != null)
                        {
                            localReservation.APIVendorReturnAddress = reservationReturnLocation.Address;
                            localReservation.APIVendorReturnPhone = reservationReturnLocation.PhoneNumber;
                        }
                    }

                    return new ServiceResponseBase
                    {
                        Success = true,
                        Data = localReservation
                    };
                }
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Circular servisine erişim engellendi.",
                Data = localReservation
            };

        }

        private List<KeyValuePair<string, string>> GetDriverEntity(PostReservationRequest postReservationRequest)
        {
            var parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("id", string.Empty));
            parameters.Add(new KeyValuePair<string, string>("fullname", $"{postReservationRequest.CustomerName} {postReservationRequest.CustomerSurname}"));
            parameters.Add(new KeyValuePair<string, string>("phone", postReservationRequest.CustomerTelephone));
            parameters.Add(new KeyValuePair<string, string>("bdate", DateTime.Now.ToString("dd.MM.yyyy")));
            parameters.Add(new KeyValuePair<string, string>("email", postReservationRequest.CustomerEmail));

            return parameters;
        }

        private Circular2PostReservationRequestBody GetEntity(PostReservationRequest postReservationRequest, Domain.Models.Reservation localReservation, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, ReservationToken reservationToken, List<Extra> apiExtras)
        {
            var entity = new Circular2PostReservationRequestBody();
            entity.id = string.Empty;
            entity.currencycode = postReservationRequest.CurrencyCode.ToUpper();
            entity.reserveddate = DateTime.Now.ToString("dd.MM.yyyy");
            entity.startdate = postReservationRequest.PickupDate;
            entity.starttime = postReservationRequest.PickupTime;
            entity.expectedenddate = postReservationRequest.ReturnDate;
            entity.expectedendtime = postReservationRequest.ReturnTime;
            entity.cargroup = reservationToken.APIReferenceCode2;
            entity.driver = postReservationRequest.CustomerName + " " + postReservationRequest.CustomerSurname;
            entity.paytype = postReservationRequest.PaymentType == PaymentTypes.AdvancePayment || postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? "GCC" : "G/A";
            entity.pricing = "daily";
            entity.specialprice = localReservation.TotalPrice.ToFloatNullSafe().ToString();
            entity.dailyprice = localReservation.DailyPrice.ToString();
            entity.outlocation = additionalInformation.APIPickupLocationCode;
            entity.returnlocation = additionalInformation.APIReturnLocationCode;
            entity.referralagent = vendor.ApiPassword.ToIntNullSafe().ToString();
            entity.referralno = localReservation.ReservationNumber;
            entity.prepayment = localReservation.PaidAmount.ToString();
            entity.flightarrivalnumber = postReservationRequest.FlightNumberArrival;
            entity.dropprice = localReservation.OneWayFee.ToString();
            //  parameters.Add(new KeyValuePair<string, string>("pricing", isSpecialPrice ? "special" : "daily"));
            // parameters.Add(new KeyValuePair<string, string>("specialprice", isSpecialPrice ? localReservation.TotalPrice.ToFloatNullSafe().ToString() : localReservation.APITotalAmount.ToFloatNullSafe().ToString()));
            //   parameters.Add(new KeyValuePair<string, string>("dailyprice", isSpecialPrice ? localReservation.DailyPrice.ToString() : localReservation.APIDailyPrice.ToString()));
            //parameters.Add(new KeyValuePair<string, string>("outlocation", additionalInformation.APIPickupLocationCode));
            //parameters.Add(new KeyValuePair<string, string>("returnlocation", additionalInformation.APIReturnLocationCode));
            //parameters.Add(new KeyValuePair<string, string>("referralagent", vendor.ApiClientId.ToIntNullSafe().ToString()));
            //parameters.Add(new KeyValuePair<string, string>("referralno", localReservation.ReservationNumber));
            // parameters.Add(new KeyValuePair<string, string>("prepayment", isSpecialPrice ? localReservation.PaidAmount.ToString() : "0"));
            //parameters.Add(new KeyValuePair<string, string>("flightarrivalnumber", postReservationRequest.FlightNumberArrival));
            //parameters.Add(new KeyValuePair<string, string>("dropprice", localReservation.OneWayFee.ToString()));
            //parameters.Add(new KeyValuePair<string, string>("extra", extras));

            return entity;
        }
        private List<KeyValuePair<string, string>> GetParametersForUrlEncoded(PostReservationRequest postReservationRequest, Domain.Models.Reservation localReservation, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, ReservationToken reservationToken, string driverId)
        {
            bool isSpecialPrice = false;
            var extras = localReservation.ReservationExtras.Count > 0 ? JsonConvert.SerializeObject(localReservation.ReservationExtras.Select(x => new CircularRequestBase.CircularRequestExtra()
            {
                id = x.ExtraCode.ToIntNullSafe(),
                //value = x.APIPrice,
                value = x.ApiPrice,
                include = 1
            }).ToList()) : string.Empty;

            var parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("id", string.Empty));
            parameters.Add(new KeyValuePair<string, string>("currencycode", reservationToken.BaseVendorRequestCurrencyType.ToString()));
            parameters.Add(new KeyValuePair<string, string>("reservatedate", DateTime.Now.ToString("dd.MM.yyyy")));
            parameters.Add(new KeyValuePair<string, string>("startdate", postReservationRequest.PickupDate));
            parameters.Add(new KeyValuePair<string, string>("starttime", postReservationRequest.PickupTime));
            parameters.Add(new KeyValuePair<string, string>("expectedenddate", postReservationRequest.ReturnDate));
            parameters.Add(new KeyValuePair<string, string>("expectedendtime", postReservationRequest.ReturnTime));
            parameters.Add(new KeyValuePair<string, string>("cargroup", reservationToken.APIReferenceCode2));
            parameters.Add(new KeyValuePair<string, string>("driver", !string.IsNullOrEmpty(driverId) ? driverId : postReservationRequest.CustomerName + " " + postReservationRequest.CustomerSurname));
            parameters.Add(new KeyValuePair<string, string>("paytype", postReservationRequest.PaymentType == PaymentTypes.AdvancePayment || postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? "GCC" : "G/A"));
            parameters.Add(new KeyValuePair<string, string>("pricing", isSpecialPrice ? "special" : "daily"));
            parameters.Add(new KeyValuePair<string, string>("specialprice", isSpecialPrice ? localReservation.TotalPrice.ToFloatNullSafe().ToString() : localReservation.APITotalAmount.ToFloatNullSafe().ToString()));
            parameters.Add(new KeyValuePair<string, string>("dailyprice", isSpecialPrice ? localReservation.DailyPrice.ToString() : localReservation.APIDailyPrice.ToString()));
            parameters.Add(new KeyValuePair<string, string>("pricing", "daily"));
            parameters.Add(new KeyValuePair<string, string>("specialprice", localReservation.APITotalAmount.ToFloatNullSafe().ToString()));
            parameters.Add(new KeyValuePair<string, string>("dailyprice", localReservation.APIDailyPrice.ToString()));
            parameters.Add(new KeyValuePair<string, string>("outlocation", additionalInformation.APIPickupLocationCode));
            parameters.Add(new KeyValuePair<string, string>("returnlocation", additionalInformation.APIReturnLocationCode));
            parameters.Add(new KeyValuePair<string, string>("referralagent", vendor.ApiPassword.ToIntNullSafe().ToString()));
            parameters.Add(new KeyValuePair<string, string>("referralno", localReservation.ReservationNumber));
            parameters.Add(new KeyValuePair<string, string>("prepayment", "0"));
            parameters.Add(new KeyValuePair<string, string>("prepayment", isSpecialPrice ? localReservation.PaidAmount.ToString() : "0"));
            parameters.Add(new KeyValuePair<string, string>("flightarrivalnumber", postReservationRequest.FlightNumberArrival));
            parameters.Add(new KeyValuePair<string, string>("dropprice", localReservation.OneWayFee.ToString()));
            parameters.Add(new KeyValuePair<string, string>("extra", extras));

            return parameters;
        }
        private List<KeyValuePair<string, string>> GetCancelParametersForUrlEncoded(PostCancelReservationRequest postCancelReservationRequest, Domain.Models.Reservation localReservation)
        {
            var parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("referralno", postCancelReservationRequest.ReservationNumber));
            parameters.Add(new KeyValuePair<string, string>("info", postCancelReservationRequest.CancelNote));
            parameters.Add(new KeyValuePair<string, string>("id", localReservation.APIReservationNumber));

            return parameters;
        }
    }
}
