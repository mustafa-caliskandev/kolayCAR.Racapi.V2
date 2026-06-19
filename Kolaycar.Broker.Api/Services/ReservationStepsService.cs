using KolayCAR.Broker.API.Extensions;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.ResponseDtos;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiLocation = KolayCAR.Broker.API.Models.Location;
using ApiLocationVendor = KolayCAR.Broker.API.Models.Locationvendor;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Services
{
    public interface IReservationStepsService
    {
        Task<ResponseReservationStepsAdditionalInformation> GetAdditionalInformation(
            CommonModels.Vendor vendor,
            CommonModels.Agency agency,
            string languageCode,
            string currencyCode,
            CurrencyTypes apiCurrencyType,
            int pickupLocationId,
            int returnLocationId,
            string pickupDate,
            string returnDate,
            string pickupTime,
            string returnTime,
            int vehicleId = 0,
            int apiVendorId = 0,
            int rentalDuration = 0,
            string apiReferenceCode = "",
            string couponCode = "",
            ReservationToken reservationToken = null,
            string apiLocationCode = "");
        Task<float> CurrencyExchange(CommonModels.Vendor vendor, float price, CurrencyTypes sourceCurrencyTypes, CurrencyTypes targetCurrencyTypes);
        // Task<ReservationToken> GetReservationToken(string tokenGuid);
        Task<HttpResult<object>> CheckReservationVehicleIsAvailable(List<Vehicle> vehicles, ReservationToken reservationToken, LanguageTypes languageType, CommonModels.Vendor vendor, string requestToken = "");
        Task<List<ReservationSource>> GetReservationSources();
        Task<long> CreateNewResIdIfExist(long resId = 0);
    }

    public class ReservationStepsService : IReservationStepsService
    {
        private readonly BrokerContext _context;
        private readonly IConfigurationService _configurationService;
        private readonly ICacheService _cacheService;
        private readonly IResTokenService _resTokenService;

        public ReservationStepsService(
            BrokerContext context,
            IConfigurationService configurationService,
            ICacheService cacheService,
            IResTokenService resTokenService)
        {
            _context = context;
            _configurationService = configurationService;
            _cacheService = cacheService;
            _resTokenService = resTokenService;
        }

        public async Task<ResponseReservationStepsAdditionalInformation> GetAdditionalInformation(
            CommonModels.Vendor vendor,
            CommonModels.Agency agency,
            string languageCode,
            string currencyCode,
            CurrencyTypes apiCurrencyType,
            int pickupLocationId,
            int returnLocationId,
            string pickupDate,
            string returnDate,
            string pickupTime,
            string returnTime,
            int vehicleId = 0,
            int apiVendorId = 0,
            int rentalDuration = 0,
            string apiReferenceCode = "",
            string couponCode = "",
            ReservationToken reservationToken = null,
            string apiLocationCode = "")
        {
            //TODO: Gönderilen LanguageCode ve CurrencyCode değerleri validasyonu sağlanacak
            var languageId = Convert.ToInt32((LanguageTypes)Enum.Parse(typeof(LanguageTypes), languageCode.ToUpper())) + 1;
            var currencyId = Convert.ToInt32((CurrencyTypes)Enum.Parse(typeof(CurrencyTypes), currencyCode.ToUpper())) + 1;

            var couponModel = Activator.CreateInstance<CouponDetailDto>();
            if (reservationToken != null && reservationToken.AgencyId > 0)
            {
                couponModel = await GetCouponResults(new GetCouponDetailsResponseDto
                {
                    CouponCode = couponCode,
                    CustomerMailAddress = "",
                    LanguageId = languageId,
                    CurrencyId = currencyId,
                    VendorId = vendor.VendorId,
                    TotalPrice = (reservationToken.DailyPrice * reservationToken.RentalDuration).ToString(),
                    PickupDate = reservationToken.PickupDateTime.ToString(),
                    ReturnDate = reservationToken.ReturnDateTime.ToString(),
                    PickupLocationId = reservationToken.PickupLocationId,
                    ReturnLocationId = reservationToken.ReturnLocationId,
                    RentalDuration = reservationToken.RentalDuration,
                    AgencyId = (int)reservationToken.AgencyId
                });
            }

            var pickupLocation = await GetVendorPickupLocalLocation(pickupLocationId, languageId, vendor);
            var returnLocation = await GetVendorReturnLocalLocation(returnLocationId, languageId, vendor);

            if (pickupLocation != null && returnLocation != null)
            {
                DateTime PickupDateTime = ObjectHelper.CombineDateAndTime(pickupDate, pickupTime);
                DateTime ReturnDateTime = ObjectHelper.CombineDateAndTime(returnDate, returnTime);
                //rentalDuration = (int)Math.Ceiling((ReturnDateTime - PickupDateTime).TotalDays);
                rentalDuration = GetRentalDuration(PickupDateTime, ReturnDateTime, vendor);

                var pickupLocationCode = pickupLocation.Locationvendor.Locationcode;
                var returnLocationCode = returnLocation.Locationvendor.Locationcode;

                if (!string.IsNullOrEmpty(apiLocationCode))
                {
                    pickupLocationCode = apiLocationCode;
                    returnLocationCode = pickupLocationId == returnLocationId
                                         ? apiLocationCode
                                         : returnLocationCode;
                }


                return new ResponseReservationStepsAdditionalInformation
                {
                    Agency = agency,
                    Vendor = vendor,
                    APIVendorId = apiVendorId,
                    VehicleId = vehicleId,
                    LanguageCode = languageCode,
                    CurrencyCode = currencyCode,
                    APICurrencyType = apiCurrencyType,
                    PickupLocationId = pickupLocation.Location.Id,
                    APIPickupLocationId = pickupLocation.Locationvendor.Locationid,
                    APIPickupLocationCode = pickupLocationCode,
                    PickupLocationName = pickupLocation.Location.Locationname,
                    APIPickupLocationName = pickupLocation.Locationvendor.Apilocationname,
                    ReturnLocationId = returnLocation.Location.Id,
                    APIReturnLocationId = returnLocation.Locationvendor.Locationid,
                    APIReturnLocationCode = returnLocationCode,
                    ReturnLocationName = returnLocation.Location.Locationname,
                    APIReturnLocationName = returnLocation.Locationvendor.Apilocationname,
                    PickupDateTime = PickupDateTime,
                    ReturnDateTime = ReturnDateTime,
                    APIReferenceCode = apiReferenceCode,
                    RentalDuration = rentalDuration,
                    ReservationToken = reservationToken,
                    UseOnlyDefaultCurrency = vendor.UseOnlyDefaultCurrency,
                    CouponType = couponModel?.CouponDiscountType ?? null,
                    CouponDiscountAmount = couponModel?.DiscountAmount ?? null,
                    FlightCardMandatory = pickupLocation?.Locationvendor?.FlightCardMandatory ?? false,
                    LocationRateCode = pickupLocation?.Locationvendor?.RateCode ?? "",
                    //CityCode = pickupLocation?.Locationvendor?.Citycode ?? "",
                    //DistrictCode = pickupLocation?.Locationvendor?.DistrictCode ?? "",
                };
            }

            return null;
        }

        public async Task<CouponDetailDto> GetCouponResults(GetCouponDetailsResponseDto getCouponDetailsDto)
        {
            var stringQuery = $"EXEC GETCOUPONDETAIL " +
                              $"@couponCode = '{getCouponDetailsDto.CouponCode}', " +
                              $"@languageId = {getCouponDetailsDto.LanguageId}, " +
                              $"@currencyId = {getCouponDetailsDto.CurrencyId}, " +
                              $"@vendorId = {getCouponDetailsDto.VendorId}, " +
                              $"@customerMail = '{getCouponDetailsDto.CustomerMailAddress}', " +
                              $"@totalPrice = '{getCouponDetailsDto.TotalPrice.Replace(",", ".")}', " +
                              $"@pickupDate = '{getCouponDetailsDto.PickupDate}', " +
                              $"@returnDate = '{getCouponDetailsDto.ReturnDate}', " +
                              $"@pickupLocationId = {getCouponDetailsDto.PickupLocationId}, " +
                              $"@returnLocationId = {getCouponDetailsDto.ReturnLocationId}, " +
                              $"@rentalDuration= {getCouponDetailsDto.RentalDuration}, " +
                              $"@agencyId = {getCouponDetailsDto.AgencyId}, " +
                              $"@memberId = {getCouponDetailsDto.MemberId}, " +
                              $"@paymentType = {getCouponDetailsDto.PaymentType}";

            var result = await _context.CouponDetailDtos.FromSqlRaw(stringQuery).ToListAsync();
            return result.FirstOrDefault();
        }

        public int GetRentalDuration(DateTime pickupDateTime, DateTime returnDateTime, CommonModels.Vendor vendor)
        {
            if (returnDateTime.TimeOfDay > pickupDateTime.TimeOfDay)
            {
                TimeSpan differenceTime = returnDateTime - pickupDateTime;
                TimeSpan differenceMinutes = differenceTime - TimeSpan.FromDays((int)differenceTime.TotalDays);
                if (vendor.ToleranceTime != null && vendor.ToleranceTime >= (int)differenceMinutes.TotalMinutes)
                {
                    return (int)differenceTime.TotalDays;
                }
            }
            return (int)Math.Ceiling((returnDateTime - pickupDateTime).TotalDays);
        }

        // Eski hali:
        // public async Task<dynamic> GetVendorPickupLocalLocation(int pickupLocationId, int languageId, CommonModels.Vendor vendor) =>
        //     await _context.Location
        //        .Join(_context.Locationvendor,
        //        l => l.Id,
        //        lv => lv.Locallocationid,
        //        (l, lv) => new { Location = l, Locationvendor = lv })
        //        .Where(x =>
        //        x.Location.Id == pickupLocationId &&
        //        x.Location.Langid == languageId &&
        //        x.Location.Active == true &&
        //        x.Locationvendor.Vendorid == vendor.VendorId &&
        //        x.Locationvendor.Ispickup == true &&
        //        x.Locationvendor.Active == true)
        //        .FirstOrDefaultAsync();
        public async Task<dynamic> GetVendorPickupLocalLocation(int pickupLocationId, int languageId, CommonModels.Vendor vendor)
        {
            if (CacheSettings.UseCache)
            {
                return await _cacheService.GetOrCreateAsync(
                    $"{CacheSettings.LocationVendor}-PickupLocalLocation-{pickupLocationId}-{languageId}-{vendor.VendorId}",
                    () => GetVendorLocalLocation(pickupLocationId, languageId, vendor.VendorId, true));
            }

            return await GetVendorLocalLocation(pickupLocationId, languageId, vendor.VendorId, true);
        }

        // Eski hali:
        // public async Task<dynamic> GetVendorReturnLocalLocation(int returnLocationId, int languageId, CommonModels.Vendor vendor) =>
        //     await _context.Location
        //         .Join(_context.Locationvendor,
        //         l => l.Id,
        //         lv => lv.Locallocationid,
        //         (l, lv) => new { Location = l, Locationvendor = lv })
        //         .Where(x =>
        //         x.Location.Id == returnLocationId &&
        //         x.Location.Langid == languageId &&
        //         x.Location.Active == true &&
        //         x.Locationvendor.Vendorid == vendor.VendorId &&
        //         x.Locationvendor.Active == true)
        //         .FirstOrDefaultAsync();
        public async Task<dynamic> GetVendorReturnLocalLocation(int returnLocationId, int languageId, CommonModels.Vendor vendor)
        {
            if (CacheSettings.UseCache)
            {
                return await _cacheService.GetOrCreateAsync(
                    $"{CacheSettings.LocationVendor}-ReturnLocalLocation-{returnLocationId}-{languageId}-{vendor.VendorId}",
                    () => GetVendorLocalLocation(returnLocationId, languageId, vendor.VendorId, false));
            }

            return await GetVendorLocalLocation(returnLocationId, languageId, vendor.VendorId, false);
        }

        private async Task<ReservationStepLocationCacheItem> GetVendorLocalLocation(int locationId, int languageId, int vendorId, bool isPickup)
        {
            var query = _context.Location
                .Join(_context.Locationvendor,
                    l => l.Id,
                    lv => lv.Locallocationid,
                    (l, lv) => new ReservationStepLocationCacheItem
                    {
                        Location = l,
                        Locationvendor = lv
                    })
                .Where(x =>
                    x.Location.Id == locationId &&
                    x.Location.Langid == languageId &&
                    x.Location.Active == true &&
                    x.Locationvendor.Vendorid == vendorId &&
                    x.Locationvendor.Active == true);

            if (isPickup)
                query = query.Where(x => x.Locationvendor.Ispickup == true);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<float> CurrencyExchange(CommonModels.Vendor vendor, float price, CurrencyTypes sourceCurrencyTypes, CurrencyTypes targetCurrencyTypes)
        {
            var exchangeRates = await _context.Exchangerates.ToListAsync();
            var mappedExchangeRates = exchangeRates.Map();

            return CalculationHelper.CurrencyExchange(mappedExchangeRates, vendor, price, sourceCurrencyTypes, targetCurrencyTypes);
        }

        //public async Task<ReservationToken> GetReservationToken(string tokenGuid)
        //{
        //    if (Guid.TryParse(tokenGuid, out var reservationQuid))
        //    {
        //        var token = await _context.Restoken.Where(x => x.Uniqueid == reservationQuid.ToString()).FirstOrDefaultAsync();

        //        return JsonConvert.DeserializeObject<ReservationToken>(EncryptionHelper.DecryptAES256(token.Token));
        //    }

        //    return null;
        //}

        public async Task<HttpResult<object>> CheckReservationVehicleIsAvailable(List<Vehicle> vehicles, ReservationToken reservationToken, LanguageTypes languageType, CommonModels.Vendor vendor, string requestToken = "")
        {
            if (vehicles != null && vehicles.Count > 0)
            {
                Vehicle vehicle = null;

                if (!string.IsNullOrEmpty(requestToken))
                    Serilog.Log.Error("{@CheckReservationVehicleIsAvailable}", $"{requestToken} - {reservationToken.ModelToJson()} - {vehicles.ModelToJson()}");

                if (!vendor.UseBrokerConfigurations)
                {
                    if (vendor.VendorType == VendorTypes.Yolcu360)
                    {
                        vehicle = vehicles.Where(x => x.VendorId == reservationToken.APIVendorId && x.VehicleName == reservationToken.VehicleName && x.FuelType == reservationToken.FuelType && x.TransmissionType == reservationToken.TransmissionType && x.VehicleCategoryType == reservationToken.VehicleCategoryType).FirstOrDefault();
                    }
                    else
                        if (vendor.VendorType == VendorTypes.Yolcu360v2)
                        {
                            vehicle = vehicles.FirstOrDefault(e => e.VehicleCode == reservationToken.VehicleCode);
                        }
                        else
                            if (vendor.VendorType == VendorTypes.EnUygun)
                            {
                                vehicle = vehicles.Where(x => x.VendorId == reservationToken.APIVendorId && x.VehicleName == reservationToken.VehicleName && x.ApiVendorName == reservationToken.APIVendorName && x.VehicleCode == reservationToken.VehicleCode).FirstOrDefault();
                            }
                            else
                            {
                                vehicle = vehicles.Where(x => x.VehicleCode == reservationToken.VehicleCode && x.VendorId == reservationToken.VendorId).FirstOrDefault();
                            }
                }
                else
                {
                    if (reservationToken.ApiVendorType == VendorTypes.Yolcu360)
                    {
                        vehicle = vehicles.Where(x => x.VendorId == reservationToken.APIVendorId && x.VehicleName == reservationToken.VehicleName && x.FuelType == reservationToken.FuelType && x.TransmissionType == reservationToken.TransmissionType && x.VehicleCategoryType == reservationToken.VehicleCategoryType).FirstOrDefault();
                    }
                    else
                        if (vendor.VendorType == VendorTypes.Yolcu360v2)
                        {
                            vehicle = vehicles.FirstOrDefault(e => e.VehicleCode == reservationToken.VehicleCode);
                        }
                        else if (vendor.VendorType == VendorTypes.EnUygun)
                        {
                            vehicle = vehicles.Where(x => x.VendorId == reservationToken.APIVendorId && x.VehicleName == reservationToken.VehicleName && x.ApiVendorName == reservationToken.APIVendorName && x.VehicleCode == reservationToken.VehicleCode).FirstOrDefault();
                        }
                        else
                        {
                            vehicle = vehicles.Where(x => x.VehicleCode == reservationToken.VehicleCode && x.VendorId == reservationToken.APIVendorId).FirstOrDefault();
                        }
                }

                if (vehicle == null) //Kiralanacak araç müsait değilse
                    return new HttpResult<object>
                    {
                        Success = false,
                        Message = await GetResultMessage(ResultCodes.VehicleNotAvailable, languageType),
                        ResultCode = (int)ResultCodes.VehicleNotAvailable
                    };

                var newReservationToken = await _resTokenService.GetReservationTokenByUniqueId(vehicle.ReservationToken);
                Serilog.Log.Error("{@NewReservationToken}", newReservationToken);

                if (newReservationToken == null)
                    return new HttpResult<object>
                    {
                        Success = false,
                        Message = await GetResultMessage(ResultCodes.Error, languageType),
                        ResultCode = (int)ResultCodes.Error
                    };

                if (newReservationToken.APIDailyPrice > reservationToken.APIDailyPrice * 1.05f) //Kiralanacak araç fiyatı %5'ten fazla arttıysa
                {
                    Serilog.Log.Error("{@VehiclePriceChange}", $"{newReservationToken.APIDailyPrice}-{reservationToken.APIDailyPrice}-{reservationToken.APIVendorName}");
                    return new HttpResult<object>
                    {
                        Success = false,
                        Message = await GetResultMessage(ResultCodes.VehiclePriceChange, languageType),
                        ResultCode = (int)ResultCodes.VehiclePriceChange
                    };
                }

                return new HttpResult<object>
                {
                    Success = true,
                    Message = await GetResultMessage(ResultCodes.Success, languageType),
                    ResultCode = (int)ResultCodes.Success,
                    Data = vehicle,
                };
            }
            else //Kiralanacak araç müsait değilse
            {
                return new HttpResult<object>
                {
                    Success = false,
                    Message = await GetResultMessage(ResultCodes.VehicleNotAvailable, languageType),
                    ResultCode = (int)ResultCodes.VehicleNotAvailable
                };
            }
        }

        public async Task<string> GetResultMessage(ResultCodes resultCode, LanguageTypes languageType)
        {
            string message = string.Empty;

            switch (resultCode)
            {
                case ResultCodes.Success:
                    {
                        message = await _configurationService.GetLabel(149, languageType);
                        break;
                    }
                case ResultCodes.Error:
                    {
                        message = await _configurationService.GetLabel(2129, languageType);
                        break;
                    }
                case ResultCodes.VehicleNotAvailable:
                    {
                        message = await _configurationService.GetLabel(847, languageType);
                        break;
                    }
                case ResultCodes.VehiclePriceChange:
                    {
                        message = await _configurationService.GetLabel(2274, languageType);
                        break;
                    }
            }

            return message;
        }

        public async Task<List<ReservationSource>> GetReservationSources()
        {
            var sources = await _context.Ressource.Where(x => x.IsActive == true).ToListAsync();
            return sources.Map();
        }

        public async Task<long> CreateNewResIdIfExist(long resId = 0)
        {
            if (resId == 0)
                resId = ReservationHelper.GenerateReservationId();

            if (resId != -1)
            {
                var isExist = await _context.Rez.AnyAsync(x => x.Rezid == resId);
                if (!isExist) return resId;
            }
            resId = await CreateNewResIdIfExist();
            return resId;
        }

        private sealed class ReservationStepLocationCacheItem
        {
            public ApiLocation Location { get; set; }
            public ApiLocationVendor Locationvendor { get; set; }
        }
    }
}
