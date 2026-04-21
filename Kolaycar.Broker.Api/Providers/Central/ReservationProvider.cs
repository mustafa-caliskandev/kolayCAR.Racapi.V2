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
using CentralProvider = KolayCAR.Broker.API.Providers.Central;

namespace KolayCAR.Broker.API.Providers.Central
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager { get; set; }
        ILocationProvider locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            locationProvider = new CentralProvider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var postCancelReservationsRequestParameters = PostCancelReservationsRequestParameters(postCancelReservationRequest, vendor, localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postCancelReservationsRequestParameters),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@CentralPostCancelReservationsRequestParameters}", postCancelReservationsRequestParameters);

            var result = await RestManager.GetAsync<CentralResponseBase.CentralCancelReservation>(
            requestPath: $"operation/API/ReservationCancel.php",
            parameters: postCancelReservationsRequestParameters,
            brokerLogModel: new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
            },
            isReservationRequest: true);

            Serilog.Log.Error("{@CentralPostCancelReservationsResponse}", result);
            if (result != null && result.cevap == "success")
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

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postReservationRequestParameters = PostReservationRequestParameters(
                postReservationRequest,
                additionalInformation,
                reservationNumber,
                reservationToken,
                apiPaidAmount,
                localReservation);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postReservationRequestParameters),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@CentralPostReservationRequestParameters}", postReservationRequestParameters);

            var result = await RestManager.GetAsync<CentralResponseBase.CentralReservation>(
            requestPath: $"operation/API/Reservation.php",
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

            if (result != null && !string.IsNullOrEmpty(result.ReservationId))
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.ReservationId;

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

            Serilog.Log.Error("Central rezervasyonu başarısız!");
            localReservation.APIMessage = result.mesaj;

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Central servisinden herhangi bir veri alınamadı!"
            };
        }

        private Dictionary<string, object> PostReservationRequestParameters(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, float paidAmount, Reservation localReservation)
        {
            var selectedExtraCodes = postReservationRequest.PostReservationRequestV2 == null ? ReservationHelper.GetSelectedExtaCodes(postReservationRequest.ExtraList)
                  : ReservationHelper.GetSelectedExtaCodesV2(postReservationRequest.PostReservationRequestV2.Extras);
            var childSeatExtra = localReservation.ReservationExtras.Where(x => x.ExtraCode == "cocuk_koltugu").FirstOrDefault();
            var babySeatExtra = localReservation.ReservationExtras.Where(x => x.ExtraCode == "bebek_koltugu").FirstOrDefault();

            return new Dictionary<string, object>()
            {
                { "login", additionalInformation.Vendor.ApiKey},
                { "passwd", additionalInformation.Vendor.ApiPassword},
                { "Name", postReservationRequest.CustomerName},
                { "LastName", postReservationRequest.CustomerSurname},
                { "Id", !string.IsNullOrEmpty(postReservationRequest.CustomerPersonalNumber) && postReservationRequest.CustomerPersonalNumber.Length == 11 ? postReservationRequest.CustomerPersonalNumber : string.Empty},
                { "PassportId", !string.IsNullOrEmpty(postReservationRequest.CustomerPersonalNumber) && postReservationRequest.CustomerPersonalNumber.Length != 11 ? postReservationRequest.CustomerPersonalNumber : string.Empty},
                { "EMail", postReservationRequest.CustomerEmail},
                { "BirthDay", !string.IsNullOrEmpty(postReservationRequest.CustomerBirthDay) ? postReservationRequest.CustomerBirthDay : "01.01.1900"},

                { "MobilePhone", postReservationRequest.CustomerTelephone},
                { "BussinessPhone", string.Empty},
                { "LicenceNumber", string.Empty},
                { "LicenceDate", string.Empty},
                { "LicenceLocation", string.Empty},
                { "Address", "Address"},
                { "Address2", string.Empty},
                { "Country", "Country"},
                { "City", "City"},
                { "PostCode", string.Empty},

                { "CompanyName", postReservationRequest.CompanyTitle},
                { "CompanyTaxNumber", postReservationRequest.CompanyTaxNumber},
                { "CompanyTaxName", string.Empty},

                { "CompanyAddress", string.Empty},
                { "CompanyAddress2", string.Empty},
                { "CompanyCountry", string.Empty},
                { "CompanyCity", string.Empty},
                { "CompanyPostCode", string.Empty},

                { "ContractDepartureLocationNo", additionalInformation.APIPickupLocationCode},
                { "ContractDepartureDate", additionalInformation.PickupDateTime.ToString("yyyy-MM-ddTHH:mm")},
                { "ContractReturnLocationNo", additionalInformation.APIReturnLocationCode},
                { "ContractReturnDate", additionalInformation.ReturnDateTime.ToString("yyyy-MM-ddTHH:mm")},

                { "ContractFlightCompany", postReservationRequest.DepartureInfo},
                { "ContractFlightNumber", postReservationRequest.FlightNumberArrival},
                { "ContractFlightTime", postReservationRequest.DepartureInfo},
                { "ContractTKMileNo", string.Empty},

                { "GroupId", reservationToken.APIReferenceCode3},
                { "SubGroupId", reservationToken.APIReferenceCode},
                { "SubGroupShortName", reservationToken.VehicleCode},
                { "webpromocode", string.Empty},
                { "DailyPrice", reservationToken.APIDailyPrice},
                { "Days", reservationToken.RentalDuration},
                { "DiscountedDays", string.Empty},
                { "WebPaymentNonDiscountedPrice", string.Empty},

                { "OptionSCDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("scdw") ? "1" : "" : ""},
                { "SCDWFree", string.Empty},
                { "OptionSuperSCDW", selectedExtraCodes != null ? selectedExtraCodes.Contains("super_scdw") ? "1" : "" : ""},
                { "SuperSCDWFree", string.Empty},
                { "OptionExtraDriver", selectedExtraCodes != null ? selectedExtraCodes.Contains("ek_surucu") ? "1" : "" : ""},
                { "ExtraDriverFree", string.Empty},
                { "OptionWinterTires", selectedExtraCodes != null ? selectedExtraCodes.Contains("kis_lastigi") ? "1" : "" : ""},
                { "OptionYoungDriverInsurance", selectedExtraCodes != null ? selectedExtraCodes.Contains("genc_surucu") ? "1" : "" : ""},
                { "YoungDriverInsuranceFree", string.Empty},
                { "OptionChildSeat", selectedExtraCodes != null ? selectedExtraCodes.Contains("cocuk_koltugu") ? "1" : "" : ""},
                { "ChildSeatFree", string.Empty},
                { "OptionChildSeatCount", childSeatExtra != null ? childSeatExtra.Piece != 0 ? childSeatExtra.Piece.ToStringNullSafe() : string.Empty : string.Empty},
                { "OptionBabySeat", selectedExtraCodes != null ? selectedExtraCodes.Contains("bebek_koltugu") ? "1" : "" : ""},
                { "BabySeatFree", string.Empty},
                { "OptionBabySeatCount", babySeatExtra != null ? babySeatExtra.Piece != 0 ? babySeatExtra.Piece.ToStringNullSafe() : string.Empty : string.Empty},
                { "OptionCharger", string.Empty},
                { "OptionLCF", selectedExtraCodes != null ? selectedExtraCodes.Contains("lastik_cam_far") ? "1" : "" : ""},
                { "OptionNavigation", string.Empty},
                { "NavigationFree", string.Empty},

                { "MaximumGuvence", selectedExtraCodes != null ? selectedExtraCodes.Contains("MaximumGuvence") ? "1" : "" : ""},
                { "MaximumGuvenceFree", string.Empty},
                { "MaliMesuliyet", selectedExtraCodes != null ? selectedExtraCodes.Contains("MaliMesuliyet") ? "1" : "" : ""},
                { "MaliMesuliyetFree", string.Empty},
                { "FerdiKaza", selectedExtraCodes != null ? selectedExtraCodes.Contains("FerdiKaza") ? "1" : "" : ""},
                { "FerdiKazaFree", string.Empty},
                { "IptalGuvence", selectedExtraCodes != null ? selectedExtraCodes.Contains("IptalGuvence") ? "1" : "" : ""},
                { "IptalGuvenceFree", string.Empty},

                { "ExtraDriverId", string.Empty},
                { "ExtraDriverPassportId", string.Empty},
                { "ExtraDriverName", string.Empty},
                { "ExtraDriverLastName", string.Empty},
                { "ExtraDriverBirthday", string.Empty},
                { "ExtraDriverLicenceNumber", string.Empty},
                { "ExtraDriverLicenceLocation", string.Empty},
                { "MainRulesId", reservationToken.APIReferenceCode2},
                { "CampaignId", string.Empty},

                { "CreditCardOwner", string.Empty},
                { "CreditCardNumber", string.Empty},
                { "CreditCardPrePaymentType", string.Empty},
                { "CollectedAmount", string.Empty},

                { "VoucherNumber", reservationNumber},

                { "ContractDepartureAirport", string.Empty},
                { "ContractReturnAirport", string.Empty},

                { "sendMail", false},
                { "Description", postReservationRequest.CustomerNote},
            };
        }

        private Dictionary<string, object> PostCancelReservationsRequestParameters(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation reservation) =>
             new Dictionary<string, object>()
            {
                { "login", vendor.ApiKey},
                { "passwd", vendor.ApiPassword},
                { "ReservationId", reservation.APIReservationNumber},
                { "Description", postCancelReservationRequest.CancelNote},
            };
    }
}
