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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.Eganis
{
    public class ReservationProvider : IReservationProvider
    {
        AuthProvider authProvider;
        private readonly IConfigurationService _configurationService;
        HttpManager httpManager;
        LocationProvider locationProvider;
        RestManager restManager;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            authProvider = new AuthProvider(apiBaseUrl);
            httpManager = new HttpManager(apiBaseUrl);
            locationProvider = new LocationProvider(apiBaseUrl);
            this._configurationService = configurationService;
            restManager = new RestManager(apiBaseUrl);
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var getToken = await authProvider.getToken(vendor);

            if (getToken == null)
                return new ServiceResponseBase { Success = false, Message = "Token Başarılı Bir Şekilde Alınamadı.", };

            var request = ReservationCancelRequest(localReservation, postCancelReservationRequest);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(request),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest,
            });

            Serilog.Log.Error("{@EganisPostCancelReservationsRequestParameters}", request);
            var result = await restManager.PostAsync<EganisRequestBase.ReservationCancelRequest, EganisResponseBase.ReservationCancelResponse>
                (
                    requestPath: "/Api/CancelReservation",
                    entity: request,
                    headers: getToken,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    },
                    isReservationRequest: true
                );

            Serilog.Log.Error("{@EganisPostCancelReservationsResponse}", result);

            if (result?.isSucceed == true)
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
                Message = "Eganis rezervasyon iptal servisine ulaşılamadı (Message =" + result?.isSucceed + ")"
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var getToken = await authProvider.getToken(vendor);

            if (getToken == null)
                return new ServiceResponseBase { Success = false, Message = "Token Başarılı Bir Şekilde Alınamadı.", };

            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var request = ReservationRequest(postReservationRequest, additionalInformation, reservationNumber, reservationToken, apiPaidAmount, vendor, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(request),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@EganisPostReservationRequestParameters}", request);

            var result = await httpManager.PostAsync<EganisRequestBase.ReservationRequest.Root, EganisResponseBase.ReservationResponse.Data>
                (
                    requestPath: "/Api/SaveReservation",
                    headers: getToken,
                    entity: request,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                    isReservationRequest: true
                );

            Serilog.Log.Error("{@EganisPostReservationResult}", result.ToJson());

            if (result?.Data?.reservationId != 0 && result?.Data?.reservationCode != null)
            {
                localReservation.ReservationPostedToAPI = true;
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Data.reservationId.ToString();
                localReservation.APIVendorName = vendor.VendorName;


                var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@EganisGetLocationsResponse}", location.Data);
                var reservationLocation = new List<Domain.Models.Location>();
                if (location.Success)
                {
                    reservationLocation = location.Data as List<Domain.Models.Location>;

                    if (reservationLocation != null && reservationLocation.Count > 0)
                    {
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
                }


                return new ServiceResponseBase
                {
                    Success = true,
                    Data = localReservation,
                };
            }


            return new ServiceResponseBase(localReservation, false, vendor.VendorName + " servisinden herhangi bir veri alınamadı!");

        }


        public EganisRequestBase.ReservationRequest.Root ReservationRequest(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount, Vendor vendor, Reservation localReservation)
        {
            var defaultBirthDate = "19000101"; // vendor-safe, anlamsız ama geçerli

            var birthDate = DateTime.TryParseExact(
                    postReservationRequest.CustomerBirthDay,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate)
                ? parsedDate.ToString("yyyyMMdd")
                : defaultBirthDate;

            var request = new EganisRequestBase.ReservationRequest.Root
            {
                pickupLocationId = additionalInformation.APIPickupLocationCode.ToIntNullSafe(),
                dropOffLocationId = additionalInformation.APIReturnLocationCode.ToIntNullSafe(),
                pickupDay = additionalInformation.PickupDateTime.Day,
                pickupMonth = additionalInformation.PickupDateTime.Month,
                pickupYear = additionalInformation.PickupDateTime.Year,
                pickupHour = additionalInformation.PickupDateTime.Hour,
                pickupMin = additionalInformation.PickupDateTime.Minute,
                dropOffDay = additionalInformation.ReturnDateTime.Day,
                dropOffMonth = additionalInformation.ReturnDateTime.Month,
                dropOffYear = additionalInformation.ReturnDateTime.Year,
                dropOffHour = additionalInformation.ReturnDateTime.Hour,
                dropOffMin = additionalInformation.ReturnDateTime.Minute,
                currencyCode = reservationToken.BaseVendorRequestCurrencyType.ToString(),
                vehGroupId = reservationToken.VehicleCode.ToIntNullSafe(),
                sippCode = reservationToken.SippCode,
                passportId = postReservationRequest.CustomerPersonalNumber.Any(char.IsLetter) ? postReservationRequest.CustomerPersonalNumber : null,
                identityNr = postReservationRequest.CustomerPersonalNumber.Any(char.IsLetter) ? null : postReservationRequest.CustomerPersonalNumber,
                name = postReservationRequest.CustomerName,
                surname = postReservationRequest.CustomerSurname,
                birthDate = birthDate,
                driverLicenseDate = null,
                phoneNr = postReservationRequest.CustomerTelephone,
                eMail = postReservationRequest.CustomerEmail,
                address = postReservationRequest.CustomerAddress,
                country = postReservationRequest.Country,
                city = postReservationRequest.City,
                town = string.Empty,
                flightNr = postReservationRequest.FlightNumberArrival,
                assurancePackageCode = string.Empty,
                assurancePackageFee = 0,
                rentFee = reservationToken.APITotalPrice.ToFloatNullSafe(),
                dropFee = reservationToken.APIOneWayFee.ToFloatNullSafe(),
                provisionFee = reservationToken.DepositPrice.ToFloatNullSafe(),
                paymentAmount = paidAmount.ToFloatNullSafe(),
                refReservationId = reservationNumber,
            };

            if (postReservationRequest.PostReservationRequestV2 != null)
            {
                if (postReservationRequest.PostReservationRequestV2.Extras?.Count > 0)
                {
                    var extraList = new List<EganisRequestBase.ReservationRequest.Extra>();
                    foreach (var extra in postReservationRequest.PostReservationRequestV2.Extras)
                    {
                        extraList.Add(new EganisRequestBase.ReservationRequest.Extra
                        {
                            code = extra.ExtraCode,
                            fee = extra.ApiPrice.ToFloatNullSafe(),
                            quantity = extra.Piece.ToIntNullSafe(),

                        });
                    }
                    request.extras = extraList;
                }
            }

            if (postReservationRequest.ExtraList != "")
            {
                var extraList = new List<EganisRequestBase.ReservationRequest.Extra>();

                var extraItems = postReservationRequest.ExtraList.Split("|");

                foreach (var extraItem in extraItems)
                {
                    var item = extraItem.Trim().Split("~");
                    var extra = new EganisRequestBase.ReservationRequest.Extra
                    {
                        code = item[4],
                        fee = item[2].ToFloatNullSafe(),
                        quantity = item[3].ToIntNullSafe(),

                    };
                    extraList.Add(extra);
                }
                request.extras = extraList;
            }

            return request;
        }

        public EganisRequestBase.ReservationCancelRequest ReservationCancelRequest(Reservation localReservation, PostCancelReservationRequest postCancelReservationRequest)
        {
            return new EganisRequestBase.ReservationCancelRequest
            {
                reservationId = localReservation.APIReservationNumber.ToIntNullSafe(),
                cancelReason = postCancelReservationRequest.CancelNote,
            };
        }

    }
}
