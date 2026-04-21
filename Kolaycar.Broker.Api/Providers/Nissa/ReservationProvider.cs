using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvecHelper = KolayCAR.Broker.API.Helpers.Avec;
using NissaProvider = KolayCAR.Broker.API.Providers.Nissa;

namespace KolayCAR.Broker.API.Providers.Nissa
{
    public class ReservationProvider : IReservationProvider
    {
        HttpManager HttpManager { get; set; }
        ILocationProvider locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            HttpManager = new HttpManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            locationProvider = new NissaProvider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postReservationRequestParameters = PostReservationRequestParameters(postReservationRequest, additionalInformation, reservationNumber, reservationToken, postReservationRequest.PaidAmount, vendor, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@NissaPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await HttpManager.GetXmlAsync<NissaResponseBase>(
                requestPath: "XML_Rez_Save.Asp",
                parameters: postReservationRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                }, isReservationRequest: true);

            Serilog.Log.Error("{@NissaPostReservationResult}", result);

            localReservation.ReservationPostedToAPI = true;
            localReservation.APIVendorName = vendor.VendorName;

            if (result != null && result.Sistemrent != null && result.Sistemrent.NissaReservation != null && !string.IsNullOrEmpty(result.Sistemrent.NissaReservation.ID))
            {
                if (result.Sistemrent.NissaReservation.Status == false)
                {
                    Serilog.Log.Error("Nissa servisi rezervasyonu reddetti!");

                    return new ServiceResponseBase
                    {
                        Success = false,
                        Data = localReservation,
                        Message = "Nissa servisi rezervasyonu reddetti!"
                    };
                }
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Sistemrent.NissaReservation.ID;

                var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@NissaGetLocationsResponse}", location.Data);
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
                    Data = localReservation
                };
            }

            Serilog.Log.Error("Nissa servisinden herhangi bir veri alınamadı!");
            localReservation.APIMessage = "Nissa servisinden herhangi bir veri alınamadı!";

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Nissa servisinden herhangi bir veri alınamadı!"
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

            Serilog.Log.Error("{@NissaPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await HttpManager.GetXmlAsync<NissaResponseBase>(
                requestPath: "XML_Rez_Cancel.Asp",
                parameters: postCancelReservationsRequestParameters,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                }, isReservationRequest: true);

            Serilog.Log.Error("{@NissaPostCancelReservationsResponse}", result);

            if (result != null && result.Sistemrent.NissaReservation.Status)
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
                Message = "Nissa servisi rezervasyon iptali başarısız!",
            };
        }

        private Dictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount, Vendor vendor, Reservation localReservation)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList) : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);

            return new Dictionary<string, object>()
            {
                { "Key_Hack",  additionalInformation.Vendor.ApiClientId},
                { "User_Name",  additionalInformation.Vendor.ApiKey},
                { "User_Pass",  additionalInformation.Vendor.ApiPassword},
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

                { "Adress", postReservationRequest.CustomerAddress},
                { "District", string.Empty},
                { "City", string.Empty},
                { "Country", string.Empty},
                { "Flight_Number", $"{postReservationRequest.FlightNumberArrival}" },
                { "Description", postReservationRequest.CustomerNote },

                { "Currency", AvecHelper.CurrencyHelper.GetLongCurrencyType(reservationToken.BaseVendorRequestCurrencyType)},
                //{ "Payment", localReservation.TotalPrice - paidAmount },

                { "Your_Rent_Price", reservationToken.DailyPrice * reservationToken.RentalDuration + reservationToken.OneWayFee + postReservationRequest.ExtraAmount + reservationToken.ServiceCharge },
                { "Your_Extra_Price",postReservationRequest.ExtraAmount },
                { "Your_Drop_Price", reservationToken.OneWayFee },
                { "Payment_Type", postReservationRequest.PaymentType == PaymentTypes.PayAll ? 3 : postReservationRequest.PaymentType == PaymentTypes.PayToAgency && !(postReservationRequest.ExtraPricePayToDelivery || postReservationRequest.OneWayFeePayToDelivery) ? 3 : postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 0 : postReservationRequest.PaymentType == PaymentTypes.AdvancePayment ? 1 : 2 },

                //{ "Payment_Type", vendor.RentalWorkingType == VendorWorkingTypes.ProfitMarkup ? postReservationRequest.PaymentType == PaymentTypes.PayAll || postReservationRequest.PaymentType == PaymentTypes.PayToAgency ? 3 : 0 : postReservationRequest.PaymentType == PaymentTypes.PayAll || postReservationRequest.PaymentType == PaymentTypes.PayToAgency ? 3 : postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? 0 : postReservationRequest.PaymentType == PaymentTypes.AdvancePayment ? 1 : 0 },

                { "CDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("CDW") ? "ON" : "" : ""},
                { "SCDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("SCDW") ? "ON" : "": ""},
                { "LCF", selectedExtraCodes != null ?selectedExtraCodes.Contains("LCF") ? "ON" : "": ""},
                { "PAI",  selectedExtraCodes != null ?selectedExtraCodes.Contains("PAI") ? "ON" : "": ""},
                { "Baby_Seat", selectedExtraCodes != null ? selectedExtraCodes.Contains("Baby_Seat") ? "ON" : "": ""},
                { "Navigation",  selectedExtraCodes != null ?selectedExtraCodes.Contains("Navigation") ? "ON" : "": ""},
                { "Additional_Driver", selectedExtraCodes != null ? selectedExtraCodes.Contains("Additional_Driver") ? "ON" : "": ""},
            };
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
