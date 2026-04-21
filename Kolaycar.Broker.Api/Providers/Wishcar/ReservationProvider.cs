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
using WishcarProvider = KolayCAR.Broker.API.Providers.Wishcar;

namespace KolayCAR.Broker.API.Providers.Wishcar
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager { get; set; }
        ILocationProvider locationProvider { get; set; }
        AuthProvider AuthProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            AuthProvider = new AuthProvider(apiBaseUrl);
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            locationProvider = new WishcarProvider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (auth != null && !string.IsNullOrEmpty(auth.access_token))
            {
                float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
                var postBookingSaveRequestBodyEntity = PostReservationRequestBodyEntity(postReservationRequest, additionalInformation, vendor, reservationNumber, reservationToken, localReservation);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(postBookingSaveRequestBodyEntity),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                Serilog.Log.Error("{@WishcarPostReservationRequestParameters}", postBookingSaveRequestBodyEntity);

                var result = await RestManager.PostAsync<WishcarRequestBase.PostReservationRequest, WishcarResponseBase.ReservationResponse>(
                    requestPath: $"api/rezervation/post",
                    entity: postBookingSaveRequestBodyEntity,
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                    isReservationRequest: true);

                Serilog.Log.Error("{@WishcarPostReservationResult}", result);

                if (result != null && !string.IsNullOrEmpty(result.REZERVNO))
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.REZERVNO;

                    var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                    Serilog.Log.Error("{@WishcarGetLocationsResponse}", location);
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

                Serilog.Log.Error("Wishcar servisinden herhangi bir veri alınamadı!");
                localReservation.APIMessage = "Wishcar servisinden herhangi bir veri alınamadı!";

                return new ServiceResponseBase
                {
                    Success = false,
                    Data = localReservation,
                    Message = "Wishcar servisinden herhangi bir veri alınamadı!"
                };
            }
            return new ServiceResponseBase
            {
                Success = false,
                Message = "Kimlik doğrulama işlemi başarısız!"
            };
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (auth != null && !string.IsNullOrEmpty(auth.access_token))
            {
                var postCancelReservationsRequestParameters = PostCancelReservationsRequestParameters(postCancelReservationRequest, vendor, localReservation);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(postCancelReservationsRequestParameters),
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
                });

                Serilog.Log.Error("{@WishcarPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

                var result = await RestManager.PostFormUrlEncoded<string>(
                    requestPath: "api/Cancel/rezervation",
                    postData: postCancelReservationsRequestParameters,
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token),
                    isStringResponse: true,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    },
                    isReservationRequest: true);

                Serilog.Log.Error("{@WishcarPostCancelReservationsResponse}", result);

                if (result != null && !string.IsNullOrEmpty(result) && result.Contains("Ok") && result.Contains("Başarılı"))
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
                    Data = localReservation,
                    Message = "Wishcar servisi rezervasyon iptali başarısız!",
                };
            }
            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Kimlik doğrulama işlemi başarısız!"
            };
        }

        private IEnumerable<KeyValuePair<string, string>> PostCancelReservationsRequestParameters(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation reservation) => new[] {
            new KeyValuePair<string,string>("REZERVNO", reservation.ReservationNumber),
            new KeyValuePair<string,string>("TARIH", DateTime.Now.ToString("yyyy-MM-dd")),
            new KeyValuePair<string,string>("ACIKLAMA", postCancelReservationRequest.CancelNote.ToStringNullSafe())
        };

        private WishcarRequestBase.PostReservationRequest PostReservationRequestBodyEntity(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, string reservationNumber, ReservationToken reservationToken, Reservation localReservation) =>
       new WishcarRequestBase.PostReservationRequest
       {
           TARIH = DateTime.Now.ToString("yyyy-MM-dd"),
           BROKER_NAME = additionalInformation.Agency.AgencyName.ToStringNullSafe(),
           REZERVNO = reservationNumber,
           SINIFI = reservationToken.APIReferenceCode2,
           ADISOYADI = postReservationRequest.CustomerName + " " + postReservationRequest.CustomerSurname,
           ALISYERI = additionalInformation.APIPickupLocationCode,
           REZERVBASTARIH = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd"),
           BASSAAT = additionalInformation.PickupDateTime.ToString("HH:mm"),
           IADEYERI = additionalInformation.APIReturnLocationCode,
           REZERVBITTARIH = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd"),
           BITSAAT = additionalInformation.ReturnDateTime.ToString("HH:mm"),
           GUN = reservationToken.RentalDuration.ToStringNullSafe(),
           PARA_BIRIMI = GetWishCurrency(postReservationRequest.CurrencyCode),
           FIYAT = (reservationToken.APIDailyPrice * reservationToken.RentalDuration).ToStringNullSafe(),
           GUNLUKFIYAT = reservationToken.APIDailyPrice.ToStringNullSafe(),
           EPOSTA = postReservationRequest.CustomerEmail,
           PASSPORTNO = !string.IsNullOrEmpty(postReservationRequest.CustomerPersonalNumber) ? postReservationRequest.CustomerPersonalNumber : string.Empty,
           TCKIMLIK = !string.IsNullOrEmpty(postReservationRequest.CustomerPersonalNumber) ? postReservationRequest.CustomerPersonalNumber : string.Empty,
           ULKE = string.Empty,
           GSM = postReservationRequest.CustomerTelephone,
           EkHizmetler = localReservation.ReservationExtras.Count > 0 ? localReservation.ReservationExtras.Select(x => new WishcarRequestBase.EkHizmetler
           {
               REZERVNO = reservationNumber,
               MASRAFKODU = x.ExtraCode,
               MASRAFADI = x.ExtraName,
               MIKTAR = "1",
               BIRIMTUTAR = x.Piece.ToStringNullSafe(),
               TUTAR = x.Piece.ToStringNullSafe()
           }).ToList()
           : new List<WishcarRequestBase.EkHizmetler>(),


       };

        private string GetWishCurrency(string currency)
        {
            switch (currency.ToLower())
            {
                case "try": return "TL";
                case "eur": return "EU";
                case "usd": return "USD";
                default: return "TL";
            }
        }
    }
}
