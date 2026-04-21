using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.GarentaResponseBase;

namespace KolayCAR.Broker.API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpPost]
        [Route("CouponDetails")]
        public async Task<IActionResult> GetCouponDetails(CouponCheckDto couponCheck)
        {
            if (string.IsNullOrEmpty(couponCheck.CouponCode))
            {
                return BadRequest(new { message = "Coupon code cannot be empty." });
            }

            var serviceResponse =
                await _couponService.GetCouponDetails(couponCheck.CouponCode);

            return serviceResponse.CouponId > 0
                    ? Ok(new
                    {
                        data = serviceResponse,
                        success = true,
                        resultCode = ResultCodes.Success
                    })
                    : BadRequest(new
                    {
                        success = false,
                        resultCode = ResultCodes.Error,
                        message = "Invalid coupon code."
                    });
        }

        [HttpPost]
        [Route("CreateCoupon")]
        public async Task<IActionResult> CreateCoupon(CouponResultDto couponCode)
        {
            var serviceResponse =
                await _couponService.CreateCoupon(couponCode);

            return serviceResponse.CouponId > 0
                ? Ok(new
                {
                    data = serviceResponse,
                    success = true,
                    resultCode = ResultCodes.Success
                })
                : BadRequest(new
                {
                    success = false,
                    resultCode = ResultCodes.Error,
                    message = "An error occured!"
                });
        }
    }
}
