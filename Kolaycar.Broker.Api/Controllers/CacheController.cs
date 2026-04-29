using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private const string secretKey = "aks8249nsje4290lk";
        public CacheController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet]
        [Route("clear-cache/{cacheKey}")]
        [Authorize]
        public async Task<IActionResult> ClearCache([FromRoute] string cacheKey)
        {
            try
            {
                if (!CacheSettings.IsValidCacheKey(cacheKey))
                {
                    return Ok(new
                    {
                        Status = false,
                        Message = "Gecersiz cache key!"
                    });
                }

                if (CacheSettings.IsAllKey(cacheKey))
                    await _cacheService.ClearAllCacheAsync();
                else
                    await _cacheService.RemoveByPrefixAsync(cacheKey.Trim());

                return Ok(new
                {
                    Status = true,
                    Message = "Cache temizlendi!"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Status = false,
                    Message = "Cache temizlenirken bir hata oluştu!"
                });
            }
        }

        [HttpGet]
        [Route("clear-all-cache")]
        [Authorize]
        public async Task<IActionResult> ClearAllCache()
        {
            try
            {
                await _cacheService.ClearAllCacheAsync();
                return Ok(new
                {
                    Status = true,
                    Message = "Tüm cache verileri temizlendi!"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Status = false,
                    Message = "Cache temizlenirken bir hata oluştu!"
                });
            }
        }

        [HttpGet]
        [Route("getall-cache-data/{secretkey}")]
        [Authorize]
        public IActionResult GetAllCacheData([FromRoute] string secretkey)
        {
            if (secretKey != secretkey)
                return null;

            try
            {
                return Ok(new { data = _cacheService.GetAllCacheAsJsonAsync(), processId = Process.GetCurrentProcess().Id });

            }
            catch (System.Exception ex)
            {
                return Ok(new
                {
                    Status = false,
                    Message = "Cache temizlenirken bir hata oluştu!"
                });
            }
        }
    }
}
