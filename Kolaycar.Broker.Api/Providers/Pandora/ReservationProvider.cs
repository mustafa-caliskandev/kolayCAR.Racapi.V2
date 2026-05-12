using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PandoraProvider = KolayCAR.Broker.API.Providers.Pandora;

namespace KolayCAR.Broker.API.Providers.Pandora
{
    public class ReservationProvider : IReservationProvider
    {
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        ILocationProvider locationProvider { get; set; }
        private readonly IConfigurationService _configurationService;

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            RestManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString/*connectionString: configurationService.GetConnectionString()*/);
            AuthProvider = new AuthProvider(apiBaseUrl);
            locationProvider = new PandoraProvider.LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {
            var auth = await AuthProvider.GetAccessToken(vendor);

            if (auth.access_token != null)
            {
                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = JsonConvert.SerializeObject(localReservation.APIReservationNumber),
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
                });

                Serilog.Log.Error("{PandoraPostCancelReservationsRequestParameters}", localReservation.APIReservationNumber);

                var result = await RestManager.DeleteAsyncResult<PandoraResponseBase.PandoraPostBookingSaveResponse>(
                    requestPath: $"tr/api/bookings/cancel/{localReservation.APIReservationNumber}",
                    headers: AuthProvider.CreateAuthHeader(auth.access_token),
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                    }, isReservationRequest: true);

                Serilog.Log.Error("{@PandoraPostCancelReservationsResponse}", result);

                if (result?.Data?.Number != null)
                {
                    localReservation.APIReservationCancel = true;

                    return new ServiceResponseBase
                    {
                        Success = true,
                        Data = localReservation
                    };
                }

                return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisi rezervasyon iptali başarısız!");
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = localReservation,
                Message = "Pandora servisi ile bağlantı kurulamadı!",
            };
        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            var auth = await AuthProvider.GetAccessToken(vendor);

            if (auth.access_token != null)
            {
                float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
                var postBookingSaveRequestBodyEntity = CreatePostBookingSaveRequestBodyEntity(postReservationRequest, additionalInformation, reservationToken, localReservation);
                var authHeaderWithContentType = AuthProvider.CreateAuthHeaderWithContentType(auth.access_token);

                await _configurationService.WriteLog(new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    Content = PostBookingSaveRequestBodyEntity(postReservationRequest, additionalInformation, reservationToken, localReservation),
                    LogType = BrokerLogTypes.ReservationVendorAPIRequest
                });

                Serilog.Log.Error("{@PandoraPostReservationRequestParameters}", postBookingSaveRequestBodyEntity);
                Serilog.Log.Error("{@PandoraPostReservationRequestHeaders}", authHeaderWithContentType);

                var result = await RestManager.PostAsyncRestClientResult<PandoraResponseBase.PandoraPostBookingSaveResponse>(
                    requestPath: $"tr/api/bookings/save",
                    parameterType: RestSharp.ParameterType.RequestBody,
                    headers: authHeaderWithContentType,
                    entity: postBookingSaveRequestBodyEntity,
                    brokerLogModel: new BrokerLogModel
                    {
                        LogKey = localReservation.ReservationNumber,
                        LogType = BrokerLogTypes.ReservationVendorAPIResponse
                    },
                    isReservationRequest: true);

                Serilog.Log.Error("{@PandoraPostReservationResult}", result);

                if (result?.Data?.Number != null)
                {
                    localReservation.APIReservationSuccessfully = true;
                    localReservation.APIReservationNumber = result.Data.Number;

                    var location = await locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                    Serilog.Log.Error("{@PandoraGetLocationsResponse}", location);
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
                            localReservation.VendorPhone = reservationPickupLocation.PhoneNumber; //Pandora tedarikçisi için API'dan gelen telefon numarası yazılacak.
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

                return VendorReservationResponseHelper.CreateErrorResponse(localReservation, vendor, result, "servisinden herhangi bir veri alınamadı!");
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Access denied!",
                Data = null
            };
        }

        public string PostBookingSaveRequestBodyEntity(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, Reservation localReservation) =>
            JsonConvert.SerializeObject(new PostBookingSaveRequest
            {
                ClientName = $"{postReservationRequest.CustomerName} {postReservationRequest.CustomerSurname}",
                ClientEmail = postReservationRequest.CustomerEmail,
                ClientPhone = postReservationRequest.CustomerTelephone,
                Client = new Client
                {
                    Name = postReservationRequest.CustomerName,
                    Surname = postReservationRequest.CustomerSurname,
                    VatId = postReservationRequest.CustomerPersonalNumber,
                    Street = postReservationRequest.CustomerAddress,
                    Email = postReservationRequest.CustomerEmail,
                    Phone = postReservationRequest.CustomerTelephone,
                    PassportNumber = postReservationRequest.CustomerPersonalNumber
                },
                VoucherNumber = localReservation.ReservationNumber,
                FlightNumber = $"{postReservationRequest.FlightNumberArrival} - {postReservationRequest.DepartureInfo}",
                Services = localReservation.ReservationExtras.Count > 0 ? localReservation.ReservationExtras.Select(x => new Service
                {
                    ServiceId = Convert.ToInt32(x.ExtraCode),
                    IsSelected = true,
                    Quantity = x.Piece,
                    PayOnArrival = additionalInformation.Agency.AdditionalProductAmountDeliveryPayment
                }).ToList()
                : new List<Service>(),
                Remark = $"{postReservationRequest.CustomerNote} | Araç: {localReservation.FuelTypeName}-{localReservation.TransmissionTypeName} | Uçuş: {localReservation.CustomerArrivalFlightNumber} / {localReservation.DepartureInfo}",
                BookAsCommissioner = false,
                Currency = reservationToken.BaseVendorRequestCurrencyType.ToString(),
                CarCategoryId = localReservation.VehicleId,
                PricelistId = Convert.ToInt32(reservationToken.APIReferenceCode),
                OfficeOutId = Convert.ToInt32(additionalInformation.APIPickupLocationCode),
                OfficeInId = Convert.ToInt32(additionalInformation.APIReturnLocationCode),
                DateOut = additionalInformation.PickupDateTime,
                DateIn = additionalInformation.ReturnDateTime,
                DeliveryLocation = localReservation.PickupLocationName,
                CollectionLocation = localReservation.ReturnLocationName,
                Booking_Drivers = new List<BookingDriver>{
                    new BookingDriver
                    {
                        Name = postReservationRequest.CustomerName,
                        Surname = postReservationRequest.CustomerSurname,
                        Email = postReservationRequest.CustomerEmail,
                        Phone = postReservationRequest.CustomerTelephone
                    }
                }
            });

        public Dictionary<string, object> CreatePostBookingSaveRequestBodyEntity(PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, Reservation localReservation) =>
            new Dictionary<string, object>()
            {
                { "application/json", PostBookingSaveRequestBodyEntity(postReservationRequest,additionalInformation,reservationToken,localReservation) },
            };
    }

    public class Service
    {
        public int ServiceId { get; set; }
        public bool IsSelected { get; set; }
        public int Quantity { get; set; }
        public double DiscountPercentage { get; set; }
        public double DiscountAmount { get; set; }
        public bool IsDiscountPercentage { get; set; }
        public bool PayOnArrival { get; set; }
    }

    public class Client
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string VatId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Street { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PassportNumber { get; set; }
    }

    public class BookingDriver
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public int DriverAge { get; set; }
        public string Email { get; set; }
    }

    public class PostBookingSaveRequest
    {
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhone { get; set; }
        public Client Client { get; set; }
        public string VoucherNumber { get; set; }
        public string FlightNumber { get; set; }
        public string PartnerWebCode { get; set; }
        public List<Service> Services { get; set; }
        public string Remark { get; set; }
        public bool BookAsCommissioner { get; set; }
        public string Currency { get; set; }
        public int CarCategoryId { get; set; }
        public int PricelistId { get; set; }
        public int OfficeOutId { get; set; }
        public DateTime DateOut { get; set; }
        public int OfficeInId { get; set; }
        public DateTime DateIn { get; set; }
        public string DeliveryLocation { get; set; }
        public string CollectionLocation { get; set; }
        public List<BookingDriver> Booking_Drivers { get; set; }
    }
}
