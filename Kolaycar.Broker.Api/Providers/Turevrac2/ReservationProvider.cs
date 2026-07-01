using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TurevracHelper = KolayCAR.Broker.API.Helpers.Turevrac;

namespace KolayCAR.Broker.API.Providers.Turevrac2
{
    public class ReservationProvider : IReservationProvider
    {
        private HttpManager _httpManager;
        ILocationProvider _locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;
        private readonly IConfiguration _configuration;
        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService, IConfiguration configuration)
        {
            _httpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _configurationService = configurationService;
            _locationProvider = new LocationProvider(apiBaseUrl);
            _configuration = configuration;
        }
        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            if (string.IsNullOrEmpty(localReservation.APIReservationNumber))
                return new ServiceResponseBase(localReservation, false, "Turevrac servisi rezervasyon iptali başarısız!");

            var postCancelReservationsRequestParameters = PostCancelReservationsRequestParameters(vendor, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postCancelReservationsRequestParameters),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@Turevrac2PostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await _httpManager.GetAsync2<List<Turevrac2ResponseBase.CancelReservation>>(
                requestPath: "/JsonCancel.aspx",
                parameters: postCancelReservationsRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true);

            Serilog.Log.Error("{@Turevrac2PostCancelReservationsResponse}", result);

            if (result?.Data?[0].success ?? false)
            {
                localReservation.APIReservationCancel = true;
                return new ServiceResponseBase(localReservation, true);
            }

            return CreateVendorErrorResponse(localReservation, vendor, result, "servisi rezervasyon iptali başarısız!");
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            try
            {
                float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
                var postReservationRequestParameters = PostReservationRequestParameters(postReservationRequest, additionalInformation, reservationNumber, reservationToken, postReservationRequest.PaidAmount, vendor, localReservation);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(postReservationRequestParameters),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                Serilog.Log.Error("{@Turevrac2PostReservationRequestParameters}", postReservationRequestParameters);

                var result = await _httpManager.GetAsync2<List<Turevrac2ResponseBase.Reservation>>(
                    requestPath: "/JsonRez_Save.aspx",
                    parameters: postReservationRequestParameters,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                    isReservationRequest: true);

                Serilog.Log.Error("{@Turevrac2PostReservationResult}", result);

                localReservation.ReservationPostedToAPI = true;
                localReservation.APIVendorName = vendor.VendorName;

                if (result?.Data != null)
                    if (!string.IsNullOrEmpty(result.Data[0].rez_id))
                    {
                        if (result.Data[0].success == false)
                        {
                            Serilog.Log.Error($"Turevrac2 - {vendor.VendorName} servisi rezervasyonu reddetti!");
                            return CreateVendorErrorResponse(localReservation, vendor, result, "servisi rezervasyonu reddetti!");
                        }
                        localReservation.APIReservationSuccessfully = true;
                        localReservation.APIReservationNumber = result.Data[0].rez_id;

                        var location = await _locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                        Serilog.Log.Error("{@Turevrac2GetLocationsResponse}", location.Data);
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
                        return new ServiceResponseBase(localReservation, true);
                    }

                return CreateVendorErrorResponse(localReservation, vendor, result, "servisinden herhangi bir veri alınamadı!");
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@Turevrac2PostReservationError}", $"{vendor.VendorName} - {localReservation.ReservationNumber} - {ex.ToJson()}");
                return new ServiceResponseBase(localReservation, false, vendor.VendorName + " servisinden herhangi bir veri alınamadı!");
            }
        }

        private static ServiceResponseBase CreateVendorErrorResponse<T>(Reservation localReservation, Vendor vendor, HttpResult<T> result, string fallbackMessage) where T : class
        {
            var supplierMessage = VendorReservationResponseHelper.GetSupplierMessage(result);

            if (!string.IsNullOrWhiteSpace(supplierMessage))
                localReservation.APIMessage = supplierMessage;

            return new ServiceResponseBase(
                localReservation,
                false,
                $"{vendor.VendorName} {fallbackMessage}",
                serviceMessage: supplierMessage,
                serviceCode: result != null ? ((int)result.HttpStatusCode).ToString() : string.Empty);
        }

        private IDictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount, Vendor vendor, Reservation localReservation)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null
                    ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                    : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);
            var companyName = _configuration?.GetSection("CompanyName").Value;
            bool isObilet = false;
            if (companyName != null)
            {
                isObilet = true;
            }

            var parameters = new Dictionary<string, object>()
            {
                { "Key_Hack",  vendor.ApiClientId},
                { "User_Name",  vendor.ApiKey},
                { "User_Pass", vendor.ApiPassword},
                { "Name", postReservationRequest.CustomerName},
                { "SurName", postReservationRequest.CustomerSurname},
                { "MobilePhone", postReservationRequest.CustomerTelephone},
                { "Mail_Adress", postReservationRequest.CustomerEmail},
                { "Rental_ID", postReservationRequest.CustomerPersonalNumber},
                { "Cars_Park_ID", reservationToken.APIReferenceCode2},
                { "Group_ID", reservationToken.VehicleCode},
                { "Rez_ID", reservationNumber},
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
                { "Your_Rez_ID", reservationNumber },
                { "Adress", Regex.Replace(postReservationRequest.CustomerAddress, @"\s+", " ").Replace("#","") },
                { "District", string.Empty},
                { "City", string.Empty},
                { "Country", string.Empty},
                { "Flight_Number", $"{postReservationRequest.FlightNumberArrival}" },
                { "Description", postReservationRequest.CustomerNote.Replace("#", "").Replace("?", "") },
                { "Currency", TurevracHelper.CurrencyHelper.GetLongCurrencyType(reservationToken.BaseVendorRequestCurrencyType)},
                //{ "Payment", localReservation.TotalPrice - paidAmount },
                //{ "Your_Rent_Price", reservationToken.DailyPrice * reservationToken.RentalDuration + reservationToken.OneWayFee + postReservationRequest.ExtraAmount + reservationToken.ServiceCharge },
                { "Your_Rent_Price", postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? (reservationToken.DailyPrice * reservationToken.RentalDuration + reservationToken.ServiceCharge) : reservationToken.APIDailyPrice * reservationToken.RentalDuration},
                //21.11.2024 emr eva için yoruma alındı tekrar düzenlenecek
             
                { "Your_Extra_Price",additionalInformation.Agency.AdditionalProductAmountDeliveryPayment ? localReservation.ReservationExtras.Sum(e=> e.Price) : 0 },
                { "Your_Drop_Price", reservationToken.OneWayFee },
                //01.08.2023 Ödeme tipi obilet için statik 2 olarak gönderilecek. Geliştirme sağlanıp ödeme tipi dinamikleştirilecek
                //{ "Payment_Type", postReservationRequest.PaymentType == PaymentTypes.PayAll ? 3 : postReservationRequest.PaymentType == PaymentTypes.PayToAgency && !(postReservationRequest.ExtraPricePayToDelivery || postReservationRequest.OneWayFeePayToDelivery) ? 3 : postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 0 : postReservationRequest.PaymentType == PaymentTypes.AdvancePayment ? 1 : 2 },
                    { "Payment_Type", isObilet == true ? 2 : (postReservationRequest.PaymentType == PaymentTypes.PayAll && postReservationRequest.ExtraPricePayToDelivery && postReservationRequest.OneWayFeePayToDelivery) ? 2
                     : ((postReservationRequest.PaymentType == PaymentTypes.PayAll) ? 3 : postReservationRequest.PaymentType == PaymentTypes.PayToAgency && !(postReservationRequest.ExtraPricePayToDelivery || postReservationRequest.OneWayFeePayToDelivery) ? 3 : postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 0 : postReservationRequest.PaymentType == PaymentTypes.AdvancePayment ? 1 : 2 )
                    },
                 //{ "Payment_Type",
                 //   isObilet == true ? 2 :
                 //   ((postReservationRequest.PaymentType == PaymentTypes.PayAll) ? 3 : postReservationRequest.PaymentType == PaymentTypes.PayToAgency && !(postReservationRequest.ExtraPricePayToDelivery || postReservationRequest.OneWayFeePayToDelivery) ? 3 : postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 0 : postReservationRequest.PaymentType == PaymentTypes.AdvancePayment ? 1 : 2 )},
                //{ "Payment_Type", vendor.RentalWorkingType == VendorWorkingTypes.ProfitMarkup ? postReservationRequest.PaymentType == PaymentTypes.PayAll || postReservationRequest.PaymentType == PaymentTypes.PayToAgency ? 3 : 0 : postReservationRequest.PaymentType == PaymentTypes.PayAll || postReservationRequest.PaymentType == PaymentTypes.PayToAgency ? 3 : postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 0 : postReservationRequest.PaymentType == PaymentTypes.AdvancePayment ? 1 : 0 },
                { "CDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("CDW") ? "ON" : "" : ""},
                { "SCDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("SCDW") ? "ON" : "": ""},
                { "LCF", selectedExtraCodes != null ?selectedExtraCodes.Contains("LCF") ? "ON" : "": ""},
                { "IMM", selectedExtraCodes != null ?selectedExtraCodes.Contains("IMM") ? "ON" : "": ""},
                { "Exemption", selectedExtraCodes != null ?selectedExtraCodes.Contains("Exemption") ? "ON" : "": ""},
                { "PAI",  selectedExtraCodes != null ?selectedExtraCodes.Contains("PAI") ? "ON" : "": ""},
                { "Baby_Seat", selectedExtraCodes != null ? selectedExtraCodes.Contains("Baby_Seat") ? "ON" : "": ""},
                { "Navigation",  selectedExtraCodes != null ?selectedExtraCodes.Contains("Navigation") ? "ON" : "": ""},
                { "Addition_Drive", selectedExtraCodes != null ? selectedExtraCodes.Contains("Addition_Drive") ? "ON" : "": ""},
                { "Young_Driver", selectedExtraCodes != null ? selectedExtraCodes.Contains("Young_Driver") ? "ON" : "": ""},
                { "PKH1", selectedExtraCodes != null ? selectedExtraCodes.Contains("PKH1") ? "ON" : "": ""},
                { "PKH2", selectedExtraCodes != null ? selectedExtraCodes.Contains("PKH2") ? "ON" : "": ""},
                { "Winter_Tire", selectedExtraCodes != null ? selectedExtraCodes.Contains("Winter_Tire") ? "ON" : "": ""},

                { "Full_Credit", CreditHelper.ResolveTokenCreditType(reservationToken) == CreditType.FullCredit }
            };
            if (postReservationRequest.SpecialDailyPrice != -1 && !isObilet)
            {
                //parameters.Add("Custom_Price", (postReservationRequest.SpecialDailyPrice * reservationToken.RentalDuration).ToString().Replace(".",","));
                parameters.Add("Custom_Price", (postReservationRequest.SpecialDailyPrice).ToString().Replace(".", ","));
            }
            return parameters;
        }
        private Dictionary<string, object> PostCancelReservationsRequestParameters(Vendor vendor, Reservation reservation) =>
            new Dictionary<string, object>()
           {
                { "Key_Hack", vendor.ApiClientId},
                { "Rez_ID", reservation.ReservationNumber},
                { "ID",  reservation.APIReservationNumber},
                { "User_Name",  vendor.ApiKey},
                { "User_Pass",  vendor.ApiPassword},
           };
    }
}
