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

namespace KolayCAR.Broker.API.Providers.Renteon
{
    public class ReservationProvider : IReservationProvider
    {
        private readonly IConfigurationService _configurationService;

        RestManager _restManager { get; set; }
        AuthProvider _authProvider { get; set; }
        ILocationProvider _locationProvider { get; set; }

        public ReservationProvider(string apiBaseUrl, IConfigurationService configurationService)
        {
            _restManager = new RestManager(apiBaseUrl, DbConnectionHelper.Instance().ConnectionString);
            _authProvider = new AuthProvider();
            _locationProvider = new LocationProvider(apiBaseUrl);
            _configurationService = configurationService;
        }

        public async Task<ServiceResponseBase> PostCancelReservation(PostCancelReservationRequest postCancelReservationRequest, Vendor vendor, Reservation localReservation)
        {

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(localReservation.APIReservationNumber),
                LogType = BrokerLogTypes.ReservationCancelVendorAPIRequest
            });

            Serilog.Log.Error("{@RenteonPostCancelReservationsRequestParameters}", localReservation.APIReservationNumber);

            var connectorId = vendor.ApiClientId.Split("-")[0];

            var result = await _restManager.DeleteAsyncJSON<IDictionary<string, dynamic>, RenteonRequestBase.RenteonReservationCancelRequestBase>(
                requestPath: $"/api/bookings/cancel/",
                headers: _authProvider.GetBasicAuth(vendor),
                entity: new Dictionary<string, dynamic>()
                {
                    { "ConnectorId", connectorId },
                    { "Number", localReservation.APIReservationNumber }
                },
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationCancelVendorAPIResponse
                },
                isReservationRequest: true
            );

            Serilog.Log.Error("{@RenteonPostCancelReservationsResponse}", result);

            if (result != null)
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
                Message = "Renteon servisi ile bağlantı kurulamadı!",
            };

        }

        public async Task<ServiceResponseBase> PostReservation(PostReservationRequest postReservationRequest, Vendor vendor, ResponseReservationStepsAdditionalInformation additionalInformation, string reservationNumber, ReservationToken reservationToken, List<ExchangeRates> exchangeRates, Reservation localReservation, List<Extra> apiExtras)
        {
            float apiPaidAmount = CalculationHelper.GetAPIPaidAmount(additionalInformation.Agency, vendor, reservationToken, localReservation, postReservationRequest);
            var postBookingSaveRequestBodyEntity = CreatePostReservationRequest(vendor, postReservationRequest, additionalInformation, reservationToken, localReservation);
            var authHeaderWithContentType = _authProvider.GetBasicAuth(vendor);

            await _configurationService.WriteLog(new BrokerLogModel
            {
                LogKey = localReservation.ReservationNumber,
                Content = JsonConvert.SerializeObject(postBookingSaveRequestBodyEntity),
                LogType = BrokerLogTypes.ReservationVendorAPIRequest
            });

            Serilog.Log.Error("{@RenteonPostReservationRequestParameters}", postBookingSaveRequestBodyEntity);
            Serilog.Log.Error("{@RenteonPostReservationRequestHeaders}", authHeaderWithContentType);

            var result = await _restManager.PostAsync<RenteonRequestBase.RenteonReservationPostRequestBase, RenteonRequestBase.RenteonReservationPostRequestBase>(
                requestPath: $"/api/bookings/save",
                headers: authHeaderWithContentType,
                entity: postBookingSaveRequestBodyEntity,
                brokerLogModel: new BrokerLogModel
                {
                    LogKey = localReservation.ReservationNumber,
                    LogType = BrokerLogTypes.ReservationVendorAPIResponse
                },
                isReservationRequest: true
            );

            Serilog.Log.Error("{@RenteonPostReservationResult}", result);

            if (result != null)
            {
                localReservation.APIReservationSuccessfully = true;
                localReservation.APIReservationNumber = result.Number;

                var location = await _locationProvider.GetLocations(vendor, (int)localReservation.LanguageType + 1);
                Serilog.Log.Error("{@RenteonGetLocationsResponse}", location);
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
                        localReservation.VendorPhone = reservationPickupLocation.PhoneNumber;
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

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Renteon servisi ile bağlantı kurulamadı!",
                Data = localReservation
            };
        }

        public RenteonRequestBase.RenteonReservationPostRequestBase CreatePostReservationRequest(Vendor vendor, PostReservationRequest postReservationRequest, ResponseReservationStepsAdditionalInformation additionalInformation, ReservationToken reservationToken, Reservation localReservation)
        {
            var connectorId = vendor.ApiClientId.Split("-")[0];
            var vendorName = vendor.ApiClientId.Split("-")[1];

            var additionalProducts = new List<RenteonRequestBase.Service>();

            if (postReservationRequest.ExtraList != "" && postReservationRequest.ExtraList != null)
            {
                if (postReservationRequest.ExtraList.Contains("|"))
                {
                    string[] extraArray = postReservationRequest.ExtraList.Split("|");
                    foreach (var item in extraArray)
                    {
                        var service = reservationToken.APIReferenceCode2.Split("|").FirstOrDefault(x => x.Split("~")[0] == item.Split("~")[0]);
                        additionalProducts.Add(new RenteonRequestBase.Service { ServiceId = service.Split("~")[1].ToIntNullSafe(), Code = service.Split("~")[0], Quantity = "1" });
                    }
                }
                else
                {
                    var service = reservationToken.APIReferenceCode2.Split("|").FirstOrDefault(x => x.Split("~")[0] == postReservationRequest.ExtraList.Split("~")[0]);
                    additionalProducts.Add(new RenteonRequestBase.Service { ServiceId = service.Split("~")[1].ToIntNullSafe(), Code = service.Split("~")[0], Quantity = "1" });
                }
            }

            var totals = new List<RenteonRequestBase.Fees>() {
                new RenteonRequestBase.Fees()
                {
                    Prepaid = postReservationRequest.PaymentType != PaymentTypes.PayOnDelivery ? true : false,
                    TotalVat = reservationToken.ValueAddedTax.ToStringNullSafe(),
                },
            };



            var request = new RenteonRequestBase.RenteonReservationPostRequestBase
            {
                TotalVat = reservationToken.ValueAddedTax.ToStringNullSafe(),
                Totals = totals,
                PickupOffice = new RenteonRequestBase.PickupOffice()
                {
                    OfficeId = reservationToken.APIPickupLocationId.ToStringNullSafe(),
                    ConnectorId = connectorId,
                },
                DropOffOffice = new RenteonRequestBase.DropOffOffice()
                {
                    OfficeId = reservationToken.APIReturnLocationId.ToStringNullSafe(),
                    ConnectorId = connectorId,
                },
                Drivers = new List<object>(),
                Client = new RenteonRequestBase.Client()
                {
                    Id = "0",
                    Name = postReservationRequest.CustomerName,
                    Surname = postReservationRequest.CustomerSurname,
                    VatId = postReservationRequest.CustomerPersonalNumber,
                    DateOfBirth = DateTime.Now,
                    Street = postReservationRequest.CustomerAddress,
                    Email = postReservationRequest.CustomerEmail,
                    Phone = postReservationRequest.CustomerTelephone,
                    PassportNumber = postReservationRequest.CustomerPersonalNumber,
                },
                Services = new List<RenteonRequestBase.Service>(),
                Remark = $"{postReservationRequest.CustomerNote} | Araç: {localReservation.FuelTypeName}-{localReservation.TransmissionTypeName} | Uçuş: {localReservation.CustomerArrivalFlightNumber} / {localReservation.DepartureInfo}",
                DaysForPayment = reservationToken.RentalDuration,
                ConnectorId = connectorId,
                Prepaid = postReservationRequest.PaymentType == PaymentTypes.PayOnDelivery ? false : true,
                Currency = reservationToken.BaseVendorRequestCurrencyType.ToString(),
                CarCategory = reservationToken.VehicleCode,
                ClientName = $"{postReservationRequest.CustomerName} {postReservationRequest.CustomerSurname}",
                ClientEmail = postReservationRequest.CustomerEmail,
                ClientPhone = postReservationRequest.CustomerTelephone,
                PickupDate = additionalInformation.PickupDateTime,
                DropOffDate = additionalInformation.ReturnDateTime,
                Total = localReservation.TotalPrice.ToString(),
                VoucherNumber = localReservation.ReservationNumber,
                FlightNumber = $"{postReservationRequest.FlightNumberArrival} - {postReservationRequest.DepartureInfo}",
                PickupOfficeId = reservationToken.APIPickupLocationId.ToStringNullSafe(),
                DropOffOfficeId = reservationToken.APIReturnLocationId.ToStringNullSafe(),
                PricelistId = Convert.ToInt32(reservationToken.APIReferenceCode),
            };

            return request;
        }
    }
}
