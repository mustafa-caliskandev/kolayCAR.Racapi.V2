using KolayCAR.Broker.API.Attributes;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace KolayCAR.Broker.API.Controllers
{
    //[Route("[controller]")]
    //[ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ICacheService _cacheService;

        public HomeController(IMemoryCache memoryCache, ICacheService cacheService)
        {
            _memoryCache = memoryCache;
            _cacheService = cacheService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok($"kolayCAR © {DateTime.Now.Year}");
        }

        [HttpGet]
        [Route("clear-cache")]
        [BrokerAuthorize(UserRoles.Admin, UserRoles.Agency, UserRoles.User)]
        [Authorize]
        public IActionResult ClearCache()
        {
            //if (_memoryCache is MemoryCache memoryCache)
            //{
            //    memoryCache.Compact(1.0);
            //}

            //_cacheService.InvalidateCache();

            return RedirectToAction("Index");
        }
    }
}
