using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.ResponseDtos;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;
using UsingCouponCode = KolayCAR.Broker.Domain.Models.UsingCouponCode;

namespace KolayCAR.Broker.API.Services
{
    public interface ICouponService
    {
        Task<ApplyCouponCodeResponse> ApplyCouponCode(int memberId, string couponCode, Vehicle vehicle, CommonModels.Vendor vendor, CurrencyTypes requestCurrencyType, DateTime pickupDate, DateTime returnDate, decimal dailyPrice, float? paidAmount, bool highAmountDiscountActive, GetSummaryRequest getSummaryRequest, GetSummaryResponse getSummaryResponse, ReservationToken reservationToken);
        Task<Coupon> GetActiveCouponsByVendorIdAsync(int vendorId, int locationId);
        Task<UsingCouponCode> GetUsingCouponCode(int memberId, string couponCode, float totalAmount, CommonModels.Vendor vendor, CurrencyTypes requestCurrencyType, DateTime pickupDate, DateTime returnDate, decimal dailyPrice, int rentalDuration, float? paidAmount = null, bool highAmountDiscountActive = false);
        Task<CouponCode> GetCouponCode(int memberId, string couponCode, DateTime pickupDate, DateTime returnDate, decimal dailyPrice, CurrencyTypes currencyTypes, int rentalDuration);
        Task<Coupon> GetLatestActiveCouponByVendorId(int vendorId);
        Task<List<Coupon>> GetVendorCoupons();
        Task<List<Coupon>> GetCouponsByLocationId(int locationId);
        Task<CouponCode> GetCouponCode(int couponId);
        //kazan kazan
        Task<CouponCode> GetCouponCodeByReservastionNumber(long rezervationNo);
        Task<bool> UseCouponCode(int couponId);
        Task<bool> CheckCouponIsUsable(string couponCode, int memberId, float amount, CurrencyTypes currencyType, DateTime pickupDate, DateTime returnDate, decimal dailyPrice, int rentalDuration, bool highAmountDiscountActive);
        Task<bool> CheckCouponIsShowPrice(int couponId);
        Task<CouponDetailDto> GetCouponResults(GetCouponDetailsResponseDto getCouponDetailsDto);
        Task<CouponDto> GetCouponDetails(string couponCode);
        Task<CouponDto> CreateCoupon(CouponResultDto couponResultDto);

    }

    public class CouponService : ICouponService
    {
        private readonly BrokerContext _context;
        private readonly IAgencyService _agencyService;
        private readonly IReservationStepsService _reservationStepsService;
        private int _currentAgencyId;

        public CouponService(BrokerContext context, IAgencyService agencyService, IReservationStepsService reservationStepsService)
        {
            _context = context;
            _agencyService = agencyService;
            _reservationStepsService = reservationStepsService;
            _currentAgencyId = _agencyService.GetCurrentAgencyId();
        }

        public async Task<Coupon> GetActiveCouponsByVendorIdAsync(int vendorId, int locationId)
        {
            var results = await _context.Coupon.Where(c => c.Active ?? false).ToListAsync();
            return results
                .Where(r =>
                    r.VendorId == vendorId
                    && (r.PickupLocation ?? locationId) == locationId
                    && (r.ShowInVehicleList ?? false)
                    && (r.Active ?? false))
                .OrderByDescending(r => r.CouponEndDate ?? DateTime.Now)
                .FirstOrDefault();
        }

        public async Task<ApplyCouponCodeResponse> ApplyCouponCode(int memberId, string couponCode, Vehicle vehicle, CommonModels.Vendor vendor, CurrencyTypes requestCurrencyType, DateTime pickupDate, DateTime returnDate, decimal dailyPrice, float? paidAmount, bool highAmountDiscountActive, GetSummaryRequest getSummaryRequest, GetSummaryResponse getSummaryResponse, ReservationToken reservationToken)
        {
            var usingCouponCode = await GetUsingCouponCode(memberId, couponCode, vehicle.TotalPricePayNow, vendor, requestCurrencyType, pickupDate, returnDate, dailyPrice, vehicle.RentalDuration, paidAmount, highAmountDiscountActive);
            var carPrice = getSummaryResponse.Vehicle.DailyPrice * getSummaryResponse.Vehicle.RentalDuration;

            var stringQuery = $"EXEC GETCOUPONDETAIL " +
                              $"@couponCode = '{getSummaryRequest.CouponCode}', " +
                              $"@languageId = {(int)reservationToken.LanguageType + 1}, " +
                              $"@currencyId = {(int)reservationToken.CurrencyType + 1}, " +
                              $"@vendorId = {vendor.VendorId}, " +
                              $"@customerMail = '', " +
                              $"@totalPrice = '{carPrice.ToString().Replace(",", ".")}', " +
                              $"@pickupDate = '{reservationToken.PickupDateTime:yyyy-MM-dd}', " +
                              $"@returnDate = '{reservationToken.ReturnDateTime:yyyy-MM-dd}', " +
                              $"@isAPI = 1";

            var couponResponse = (await _context.CouponDetailDtos.FromSqlRaw(stringQuery).ToListAsync()).FirstOrDefault();

            if (couponResponse != null)
            {
                var applyCouponCodeResponse = new UsingCouponCode
                {
                    Success = couponResponse.Result == 1,
                    Message = couponResponse.Message,
                    CouponUsageResultType = CouponUsageResultTypes.GeneralError,
                    CouponId = couponResponse.CouponId ?? 0,
                    CouponCode = couponResponse.CouponCode,
                    DiscountValue = (float)(couponResponse.DiscountAmount ?? 0),
                    DiscountAmount = (float)(couponResponse.DiscountAmount ?? 0),
                    CouponDiscountType = (CouponDiscountTypes)(couponResponse.CouponDiscountType ?? 0),
                    TotalPriceBeforeDiscount = (float)(couponResponse.TotalAmount ?? 0),
                    TotalPricePayNowBeforeDiscount = (float)(couponResponse.TotalAmount ?? 0)
                };
                applyCouponCodeResponse.TotalPriceBeforeDiscount = vehicle.TotalPrice;
                applyCouponCodeResponse.TotalPricePayNowBeforeDiscount = vehicle.TotalPricePayNow;

                vehicle.UsingCouponCode = usingCouponCode;
                vehicle.TotalPricePayNow -= applyCouponCodeResponse.DiscountAmount;
                vehicle.TotalPrice -= applyCouponCodeResponse.DiscountAmount;
            }

            return new ApplyCouponCodeResponse
            {
                Vehicle = vehicle,
                CouponUsageResultType = usingCouponCode.CouponUsageResultType
            };
        }

        public async Task<UsingCouponCode> GetUsingCouponCode(int memberId, string couponCode, float totalAmount, CommonModels.Vendor vendor, CurrencyTypes requestCurrencyType, DateTime pickupDate, DateTime returnDate, decimal dailyPrice, int rentalDuration, float? paidAmount = null, bool highAmountDiscountActive = false)
        {
            CouponCode coupon = await GetCouponCode(memberId, couponCode, pickupDate, returnDate, dailyPrice, requestCurrencyType, rentalDuration);
            if (coupon != null)
            {
                float discountAmount = 0;

                if (coupon.CouponCurrencyType != requestCurrencyType && coupon.CouponDiscountType == CouponDiscountTypes.ByPrice)
                {
                    coupon.CouponDiscountValue = await _reservationStepsService.CurrencyExchange(vendor, coupon.CouponDiscountValue, coupon.CouponCurrencyType, requestCurrencyType);
                    coupon.CouponCurrencyType = requestCurrencyType;
                }

                var checkCouponCodeResult = CouponHelper.CheckCouponIsUsable(coupon, amount: totalAmount, currencyType: requestCurrencyType, pickupDate: pickupDate, paidAmount: paidAmount, highAmountDiscountActive: highAmountDiscountActive);

                if (checkCouponCodeResult != null && checkCouponCodeResult.Usable)
                {
                    if (coupon.CouponDiscountType == CouponDiscountTypes.ByPrice)
                    {
                        if (checkCouponCodeResult.CouponUsageResultType == CouponUsageResultTypes.GreaterThanPaymentAmount)
                        {
                            //discountAmount = (float)paidAmount;
                            discountAmount = (float)totalAmount;
                            coupon.CouponDiscountValue = discountAmount;
                        }
                        else
                            discountAmount = coupon.CouponDiscountValue;
                    }
                    else if (coupon.CouponDiscountType == CouponDiscountTypes.ByPercent)
                    {
                        if (checkCouponCodeResult.CouponUsageResultType == CouponUsageResultTypes.GreaterThanPaymentAmount)
                        {
                            discountAmount = (float)paidAmount;
                            coupon.CouponDiscountValue = CalculationHelper.RoundPrice(100 * discountAmount / totalAmount, (int)vendor.PriceRoundingType);
                        }
                        else
                            discountAmount = totalAmount * coupon.CouponDiscountValue / 100;
                    }

                    return new UsingCouponCode
                    {
                        Success = true,
                        Message = string.Empty,
                        CouponId = coupon.Id,
                        CouponCode = coupon.Code,
                        DiscountValue = coupon.CouponDiscountValue,
                        DiscountAmount = discountAmount,
                        CouponDiscountType = coupon.CouponDiscountType,
                        CouponUsageResultType = checkCouponCodeResult.CouponUsageResultType
                    };
                }
                else
                {
                    return new UsingCouponCode
                    {
                        Success = false,
                        CouponUsageResultType = checkCouponCodeResult.CouponUsageResultType
                    };
                }
            }

            return new UsingCouponCode
            {
                Success = false,
                CouponUsageResultType = CouponUsageResultTypes.GeneralError
            };
        }

        public async Task<CouponCode> GetCouponCode(
            int memberId,
            string couponCode,
            DateTime pickupDate,
            DateTime returnDate,
            decimal dailyPrice,
            CurrencyTypes requestCurrencyType,
            int rentalDuration)
        {
            // TODO : gkursad
            Coupon couponDb;
            try
            {
                couponDb = (await _context.Coupon.FromSqlRaw("EXEC GETCOUPON @code = {0}, @pickupDate = {1}, @returnDate = {2}, @dailyPrice = {3}, @rentalDuration = {4}",
                    couponCode,
                    pickupDate.ToString("yyyy-MM-dd"),
                    returnDate.ToString("yyyy-MM-dd"),
                    dailyPrice,
                    rentalDuration).ToListAsync()).FirstOrDefault();
            }
            catch (Exception e)
            {
                couponDb = await _context.Coupon.Where(x => x.Code == couponCode && (x.MemberId == memberId || x.MemberId == null) && (x.AgencyId == _currentAgencyId || x.AgencyId == null || x.AgencyId == 0)).FirstOrDefaultAsync();
            }

            return couponDb.Map(requestCurrencyType);
        }

        public async Task<CouponCode> GetCouponCode(int couponId)
        {
            var couponDb = await _context.Coupon.Where(x => x.Id == couponId).FirstOrDefaultAsync();
            return couponDb.Map();
        }

        public async Task<bool> UseCouponCode(int couponId)
        {
            var coupon = await GetCouponCode(couponId);

            if (coupon != null)
            {
                var checkCouponCode = CouponHelper.CheckCouponIsUsable(coupon);
                if (checkCouponCode != null && checkCouponCode.Usable)
                {
                    var mappedCoupon = coupon.Map();

                    mappedCoupon.UsageCount++;

                    _context.Attach(mappedCoupon);
                    _context.Entry(mappedCoupon).State = EntityState.Unchanged;

                    _context.Entry(mappedCoupon).Property(nameof(mappedCoupon.UsageCount)).IsModified = true;

                    _context.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        public async Task<bool> CheckCouponIsUsable(string couponCode, int memberId, float amount, CurrencyTypes currencyType, DateTime pickupDate, DateTime returnDate, decimal dailyPrice, int rentalDuration, bool HighAmountDiscountActive = false)
        {
            var coupon = await GetCouponCode(memberId, couponCode, pickupDate, returnDate, dailyPrice, currencyType, rentalDuration);
            if (coupon == null) return false;
            var checkCouponCode = CouponHelper.CheckCouponIsUsable(coupon, amount: amount, currencyType: currencyType, pickupDate: pickupDate, highAmountDiscountActive: HighAmountDiscountActive);

            return checkCouponCode is { Usable: true };

        }
        //kazan kazan
        public async Task<CouponCode> GetCouponCodeByReservastionNumber(long rezervationNo)
        {
            var coupon = await _context.Coupon.Where(x => x.ConnectedReservationId == rezervationNo).FirstOrDefaultAsync();
            if (coupon != null)
            {
                return coupon.Map();
            }
            return null;
        }

        public async Task<bool> CheckCouponIsShowPrice(int couponId)
        {
            var coupon = await _context.Coupon.Where(c => c.Id == couponId).FirstOrDefaultAsync();
            return coupon.ShowPrice ?? true;
        }

        public async Task<Coupon> GetLatestActiveCouponByVendorId(int vendorId)
        {
            var coupon = await _context.Coupon.Where(x => (x.Active ?? true) && (x.VendorId == vendorId)).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            return coupon;
        }

        public async Task<List<Coupon>> GetVendorCoupons()
        {
            return await _context.Coupon.Where(c => c.VendorId != null && c.Active == true).ToListAsync();
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

        public async Task<CouponDto> GetCouponDetails(string couponCode)
        {
            var result = await _context.Coupon.FirstOrDefaultAsync(c => c.Code == couponCode);
            return result is { Id: > 0 }
                ? new CouponDto()
                {
                    CouponId = result.Id,
                    CouponDiscountType = result.DiscountType,
                    CouponCode = result.Code,
                    CouponName = result.Name,
                    CouponDescription = result.Description,
                    CouponDiscountAmount = result.DiscountType == 1 ? result.DiscountValue : null,
                    CouponDiscountPercent = result.DiscountType == 0 ? (int)result.DiscountValue : null
                }
                : Activator.CreateInstance<CouponDto>();
        }

        public async Task<CouponDto> CreateCoupon(CouponResultDto couponResultDto)
        {
            var couponControl = await this.GetCouponDetails(couponResultDto.CouponCode);
            if (couponControl is { CouponId: > 0 })
            {
                return couponControl;
            }
            else if (couponResultDto.CouponDiscountAmount == null && couponResultDto.CouponDiscountPercent == null)
            {
                return null;
            }
            else switch (couponResultDto.CouponDiscountType)
                {
                    case 0 when couponResultDto.CouponDiscountPercent == null:
                    case 1 when couponResultDto.CouponDiscountAmount == null:
                        return null;
                }

            var coupon = new Coupon()
            {
                Active = true,
                AdminId = 1,
                DiscountType = couponResultDto.CouponDiscountType ?? 0,
                Code = couponResultDto.CouponCode,
                Name = couponResultDto.CouponName,
                Description = couponResultDto.CouponDescription,
                DiscountValue = couponResultDto.CouponDiscountType == 1 ? (couponResultDto.CouponDiscountAmount ?? 0) : (decimal)(couponResultDto.CouponDiscountPercent ?? 0),
                MultipleUsage = couponResultDto.MultipleUsage,
                CurrencyId = couponResultDto.CurrencyId,
                StartDate = couponResultDto.StartDate,
                EndDate = couponResultDto.EndDate,
            };
            var result = await _context.Coupon.AddAsync(coupon);
            await _context.SaveChangesAsync();

            return await this.GetCouponDetails(couponResultDto.CouponCode);
        }

        public async Task<List<Coupon>> GetCouponsByLocationId(int locationId)
        {
            return await _context.Coupon.Where(c => c.PickupLocation == locationId).ToListAsync();
        }
    }
}
