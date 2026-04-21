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
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.Central2ResponseBase;
using Central2Provider = KolayCAR.Broker.API.Providers.Central2;

namespace KolayCAR.Broker.API.Providers.Central2
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager HttpManager { get; set; }
        ILocationProvider locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            HttpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            locationProvider = new Central2Provider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postReservationRequestParameters = PostReservationRequestParameters(postReservationRequest, additionalInformation, reservationNumber, reservationToken, apiPaidAmount, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@CentralPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await HttpManager.GetXmlAsync<CentralReservationResponse>(
                requestPath: "XML_Rez_Save.Aspx",
                parameters: postReservationRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@CentralPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result != null && result.Turevsistem != null && result.Turevsistem.MesajBilgi != null && result.Turevsistem.MesajBilgi.Status.ToBoolNullSafe() && !string.IsNullOrEmpty(result.Turevsistem.MesajBilgi.Key))
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Turevsistem.MesajBilgi.Key;

                var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@CentralGetLocationsResponse}", location);
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

            Serilog.Log.Error("Central servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Central servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Central servisinden herhangi bir veri alınamadı!"
            };
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var postCancelReservationsRequestParameters = PostCancelReservationsRequestParameters(vendor, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postCancelReservationsRequestParameters),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@CentralPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await HttpManager.GetXmlAsync<CentralReservationResponse>(
                requestPath: "XML_Cancel.Aspx",
                parameters: postCancelReservationsRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@CentralPostCancelReservationsResponse}", result);

            if (result != null && result.Turevsistem != null && result.Turevsistem.MesajBilgi != null && result.Turevsistem.MesajBilgi.Status.ToBoolNullSafe())
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
                Message = "Central servisi rezervasyon iptali başarısız!",
            };
        }

        private Dictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount, Reservation localReservation)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                 : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);

            return new Dictionary<string, object>()
            {
                { "Key_Hack",  additionalInformation.Vendor.SecretKey},
                { "User_Name",  additionalInformation.Vendor.ApiKey},
                { "User_Pass",  additionalInformation.Vendor.ApiPassword},
                { "Name", postReservationRequest.CustomerName},
                { "SurName", postReservationRequest.CustomerSurname},
                { "MobilePhone", postReservationRequest.CustomerTelephone},
                { "Mail_Adress", postReservationRequest.CustomerEmail},
                { "Rental_ID", postReservationRequest.CustomerPersonalNumber},
                { "Cars_Park_ID", reservationToken.APIReferenceCode2},
                { "Group_ID", reservationToken.VehicleCode},
                { "Rez_ID", reservationToken.APIReferenceCode},

                { "Pickup_ID", additionalInformation.APIPickupLocationCode},
                { "Pickup_Day",  additionalInformation.PickupDateTime.ToString("dd") },
                { "Pickup_Month",  additionalInformation.PickupDateTime.ToString("MM") },
                { "Pickup_Year",  additionalInformation.PickupDateTime.ToString("yyyy") },
                { "Pickup_Hour",  additionalInformation.PickupDateTime.ToString("HH") },
                { "Pickup_Min",  additionalInformation.PickupDateTime.ToString("mm") },
                { "Drop_Off_ID",  additionalInformation.APIReturnLocationCode},
                { "Drop_Off_Day",  additionalInformation.ReturnDateTime.ToString("dd") },
                { "Drop_Off_Month",  additionalInformation.ReturnDateTime.ToString("MM") },
                { "Drop_Off_Year",  additionalInformation.ReturnDateTime.ToString("yyyy") },
                { "Drop_Off_Hour",  additionalInformation.ReturnDateTime.ToString("HH") },
                { "Drop_Off_Min",  additionalInformation.ReturnDateTime.ToString("mm") },

                { "Adress", postReservationRequest.CustomerAddress},
                { "District", string.Empty},
                { "City", string.Empty},
                { "Country", string.Empty},
                { "Flight_Number", $"{postReservationRequest.FlightNumberArrival}" },
                //{ "Description", postReservationRequest.CustomerNote },

                //{ "Currency", additionalInformation.Vendor.UseOnlyDefaultCurrency ? GetCentralResCurrencyType(reservationToken.BaseVendorRequestCurrencyType) : postReservationRequest.CurrencyCode.ToUpper() },//Düzenleme yapılacak!!!!!
                { "Currency",  GetCentralResCurrency(reservationToken.BaseVendorRequestCurrencyType.ToString())  },

                { "Cancel", selectedExtraCodes != null ? selectedExtraCodes.Contains("Cancel") ? "ON" : "OFF" : "OFF"},
                { "Additional_Driver", selectedExtraCodes != null ? selectedExtraCodes.Contains("Additional_Driver") ? "ON" : "OFF": "OFF"},
                { "Mini_Damage_Insurance", selectedExtraCodes != null ? selectedExtraCodes.Contains("Mini_Damage_Insurance") ? "ON" : "OFF" : "OFF"},
                { "Super_Mini_Damage_Insurance", selectedExtraCodes != null ? selectedExtraCodes.Contains("Super_Mini_Damage_Insurance") ? "ON" : "OFF" : "OFF"},
                { "Max_Assurance", selectedExtraCodes != null ? selectedExtraCodes.Contains("Max_Assurance") ? "ON" : "OFF" : "OFF"},
                { "Young_Drive", selectedExtraCodes != null ? selectedExtraCodes.Contains("Young_Drive") ? "ON" : "OFF" : "OFF"},
                { "LCF", selectedExtraCodes != null ?selectedExtraCodes.Contains("LCF") ? "ON" : "OFF": "OFF"},
                { "IMM", selectedExtraCodes != null ? selectedExtraCodes.Contains("IMM") ? "ON" : "OFF" : "OFF"},
                { "PAI",  selectedExtraCodes != null ?selectedExtraCodes.Contains("PAI") ? "ON" : "OFF": "OFF"},
                { "Baby_Seat", selectedExtraCodes != null ? selectedExtraCodes.Contains("Baby_Seat") ? "ON" : "OFF": "OFF"},
                { "Navigation",  selectedExtraCodes != null ?selectedExtraCodes.Contains("Navigation") ? "ON" : "OFF": "OFF"},
                { "CDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("CDW") ? "ON" : "OFF" : "OFF"},
                { "SCDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("SCDW") ? "ON" : "OFF": "OFF"},
                { "Exemption_Insuranc", selectedExtraCodes != null ? selectedExtraCodes.Contains("Exemption_Insuranc") ? "ON" : "OFF" : "OFF" },
                { "Charger", selectedExtraCodes != null ? selectedExtraCodes.Contains("Charger") ? "ON" : "OFF" : "OFF" },
                { "XKP", selectedExtraCodes != null ? selectedExtraCodes.Contains("XKP") ? "ON" : "OFF" : "OFF" },

                { "Your_Rez_ID", reservationNumber },
                //{ "Your_Rent_Price", localReservation.DailyPrice * localReservation.RentalDuration },
                //{ "Your_Extra_Price", localReservation.ExtraPrice },
                //{ "Your_Drop_Price", localReservation.OneWayFee },
                //{ "Payment_Type", postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? "0" : postReservationRequest.PaymentType == PaymentTypes.AdvancePayment && !postReservationRequest.ExtraPricePayToDelivery && !postReservationRequest.OneWayFeePayToDelivery ? "1" : postReservationRequest.OneWayFeePayToDelivery && postReservationRequest.ExtraPricePayToDelivery ? "2" : "3" }
            };
        }

        private object GetCentralResCurrency(string currencyCode)
        {
            if (currencyCode == "TRY")
                return "TL";
            if (currencyCode == "EUR")
                return "EURO";
            if (currencyCode == "USD")
                return "USD";
            if (currencyCode == "GBP")
                return "GBP";
            return "TL";

        }

        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "Key_Hack", vendor.SecretKey},
                { "Rez_ID", reservation.APIReservationNumber},
                { "User_Name",  vendor.ApiKey},
                { "User_Pass",  vendor.ApiPassword},
            };
        private string GetCentralResCurrencyType(CurrencyTypes currency)
        {
            if (currency == CurrencyTypes.TRY)
                return "TL";
            if (currency == CurrencyTypes.EUR)
                return "EURO";
            if (currency == CurrencyTypes.USD)
                return "USD";
            if (currency == CurrencyTypes.GBP)
                return "GBP";
            return "TL";
        }
    }
}
