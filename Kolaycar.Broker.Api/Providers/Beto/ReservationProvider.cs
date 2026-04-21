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
using static KolayCAR.Broker.Domain.Models.Response.BetoResponseBase;

namespace KolayCAR.Broker.API.Providers.Beto
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
            locationProvider = new LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }
        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var auth = await AuthProvider.GetToken(vendor.ApiKey, vendor.ApiPassword);
            if (auth != null && !string.IsNullOrEmpty(auth.access_token))
            {
                var brokerName = await _configurationService.GetConfigurationValueByFieldName<string>("POTITLE");
                float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
                var postBookingSaveRequestBodyEntity = PostReservationRequestBodyEntity(postReservationRequest, additionalInformation, vendor, reservationNumber, reservationToken, localReservation, brokerName, reservationToken.BaseVendorRequestCurrencyType);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(postBookingSaveRequestBodyEntity),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                Serilog.Log.Error("{@BetoPostReservationRequestParameters}", postBookingSaveRequestBodyEntity);

                var result = await RestManager.PostAsync<BetoRequestBase.PostReservationRequest, ReservationResponse>(
                    requestPath: $"api/rezervation/post",
                    entity: postBookingSaveRequestBodyEntity,
                    headers: AuthProvider.CreateAuthHeaderWithContentTypeJson(auth.access_token),
                    ignoreNull: true,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                isReservationRequest: true);

                Serilog.Log.Error("{@BetoPostReservationResult}", result);

                if (result != null && !string.IsNullOrEmpty(result.REZERVNO))
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.REZERVNO;

                    var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                    Serilog.Log.Error("{@BetoGetLocationsResponse}", location);
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

                Serilog.Log.Error("Beto servisinden herhangi bir veri alınamadı!");
                localReservation.APIMessage = "Beto servisinden herhangi bir veri alınamadı!";

                return new ServiceResponseBase
                {
                    Success = false,
                    Data = localReservation,
                    Message = "Beto servisinden herhangi bir veri alınamadı!"
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

                Serilog.Log.Error("{@BetoPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

                var result = await RestManager.PostFormUrlEncoded<string>(
                    requestPath: "api/Cancel/rezervation",
                    postData: postCancelReservationsRequestParameters,
                    headers: AuthProvider.CreateAuthHeaderWithContentType(auth.access_token),
                    isStringResponse: true,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    }, isReservationRequest: true);

                Serilog.Log.Error("{@BetoPostCancelReservationsResponse}", result);

                //if (result != null && !string.IsNullOrEmpty(result) && result.Contains("Ok") && result.Contains("Başarılı"))
                if (result != null && !string.IsNullOrEmpty(result) && (result.Contains("true") || result.Contains("Başarılı") || result.Contains("Ok")))
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
                    Message = "Beto servisi rezervasyon iptali başarısız!",
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
            new KeyValuePair<string,string>("REZERVNO", reservation.APIReservationNumber),
            new KeyValuePair<string,string>("TARIH", DateTime.Now.ToString("yyyy-MM-dd")),
            new KeyValuePair<string,string>("BROKER_NAME", "Obilet"),
            new KeyValuePair<string,string>("ACIKLAMA", string.IsNullOrEmpty(postCancelReservationRequest.CancelNote) ? "İptal" :       postCancelReservationRequest.CancelNote)
        };

        private BetoRequestBase.PostReservationRequest PostReservationRequestBodyEntity(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, Vendor vendor, string reservationNumber, ReservationToken reservationToken, Reservation localReservation, string brokerName, CurrencyTypes baseVendorRequestCurrencyType)
        {
            bool containsLetter = System.Text.RegularExpressions.Regex.IsMatch(postReservationRequest.CustomerPersonalNumber, "[a-zA-Z]");
            string dailyPrice, totalPrice, paidAmount;
            float couponAmount = localReservation.CouponDiscountAmount;

            if (vendor.RentalWorkingType == VendorWorkingTypes.Commission)
            {
                dailyPrice =
                    postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery
                        ? localReservation.DailyPrice.ToString()
                        : reservationToken.DailyPrice.ToString();
                totalPrice =
                    postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery
                        ? localReservation.TotalPrice.ToString()
                        : ((reservationToken.DailyPrice * reservationToken.RentalDuration) + postReservationRequest.ExtraAmount).ToString();
                paidAmount =
                    postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery
                        ? localReservation.PaidAmount.ToString()
                        : (reservationToken.DailyPrice * reservationToken.RentalDuration).ToString();
            }
            else
            {
                dailyPrice =
                    postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery
                        ? localReservation.DailyPrice.ToString()
                        : localReservation.APIDailyPrice.ToString();
                totalPrice =
                    postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery
                        ? localReservation.TotalPrice.ToString()
                        : (localReservation.APITotalAmount + postReservationRequest.ExtraAmount).ToString();
                paidAmount =
                    postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery
                        ? localReservation.PaidAmount.ToString()
                        : localReservation.APIPaidAmount.ToString();
            }

            var entity = new BetoRequestBase.PostReservationRequest
            {
                TARIH = DateTime.Now.ToString("yyyy-MM-dd"),
                BROKER_NAME = brokerName,
                REZERVNO = reservationNumber,
                ARACNO = reservationToken.VehicleCode,
                SINIFI = reservationToken.APIReferenceCode2,
                ADISOYADI = postReservationRequest.CustomerName + " " + postReservationRequest.CustomerSurname,
                ALISYERI = additionalInformation.APIPickupLocationCode,
                REZERVBASTARIH = additionalInformation.PickupDateTime.ToString("yyyy-MM-dd"),
                BASSAAT = additionalInformation.PickupDateTime.ToString("HH:mm"),
                IADEYERI = additionalInformation.APIReturnLocationCode,
                REZERVBITTARIH = additionalInformation.ReturnDateTime.ToString("yyyy-MM-dd"),
                BITSAAT = additionalInformation.ReturnDateTime.ToString("HH:mm"),
                GUN = reservationToken.RentalDuration.ToStringNullSafe(),
                PARA_BIRIMI = GetBetoCurrency(baseVendorRequestCurrencyType.ToString()),
                //FIYAT = (reservationToken.APIDailyPrice * reservationToken.RentalDuration).ToStringNullSafe(),
                //FIYAT = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? localReservation.TotalPrice.ToString() : localReservation.APITotalAmount.ToString(),
                FIYAT = totalPrice,
                //GUNLUKFIYAT = reservationToken.APIDailyPrice.ToStringNullSafe(),
                //GUNLUKFIYAT = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? localReservation.DailyPrice.ToString() : localReservation.APIDailyPrice.ToString(),
                GUNLUKFIYAT = dailyPrice.Replace(',', '.'),
                TAHSIL_EDILEN = paidAmount,
                //fiyat_kontrol = vendor.SecretKey == "1" ? true : false,
                fiyat_kontrol = "true",
                EPOSTA = postReservationRequest.CustomerEmail,
                PASSPORTNO = !string.IsNullOrEmpty(postReservationRequest.CustomerPersonalNumber) ? containsLetter ? postReservationRequest.CustomerPersonalNumber : "" : "R123456",
                TCKIMLIK = !string.IsNullOrEmpty(postReservationRequest.CustomerPersonalNumber) ? !containsLetter ? postReservationRequest.CustomerPersonalNumber : "" : "",
                DROPUCRET = reservationToken.OneWayFee > 0 ? reservationToken.OneWayFee.ToString() : "0",
                ULKE = "TÜRKİYE",
                GSM = postReservationRequest.CustomerTelephone.Replace(" ", string.Empty),
                ODEME_TURU = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? "Ofiste Öde" : "Hemen Öde",
                EkHizmetler = localReservation.ReservationExtras.Count > 0 ? localReservation.ReservationExtras.Select(x => new BetoRequestBase.EkHizmetler
                {
                    REZERVNO = reservationNumber,
                    MASRAFKODU = x.ExtraCode,
                    MASRAFADI = x.ExtraName,
                    MIKTAR = "1",
                    BIRIMTUTAR = x.ExtraRentalType == ExtraRentalTypes.PerRental ? x.Price.ToStringNullSafe() : (x.Price / localReservation.RentalDuration).ToStringNullSafe(),
                    TUTAR = x.Price.ToStringNullSafe()
                }).ToList()
                : null,
                //BROKER_HK = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? "" : vendor.ApiClientId,
                BROKER_TYPE = "1",
                tolerance_hours = "1"
            };

            return entity;
        }


        private string GetBetoCurrency(string currency)
        {
            switch (currency.ToLower())
            {
                case "try": return "TL";
                case "eur": return "EURO";
                case "usd": return "USD";
                default: return "TL";
            }
        }
    }
}
