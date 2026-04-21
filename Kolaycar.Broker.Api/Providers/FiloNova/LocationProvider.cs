using KolayCAR.Broker.API.Mappers.FiloNova;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Providers.FiloNova
{
    public class LocationProvider : ILocationProvider
    {
        const string WORKING_HOUR_KEY = "FILONOVA_WORKING_HOUR_{0}";
        public string ProviderName => "FiloNova";
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        private readonly IMemoryCache _memoryCache;

        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider();
        }

        public LocationProvider(string apiBaseUrl, IMemoryCache memoryCache)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider();
            _memoryCache = memoryCache;
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            var requestBody = new FiloNovaRequestBase.MasterRequest
            {
                brokerCode = vendor.ApiClientId,
                langId = vendor.SecretKey.ToIntNullSafe()
            };

            var result = await RestManager.PostAsync<FiloNovaRequestBase.MasterRequest, FiloNovaResponseBase>(
                     requestPath: $"getmasterdata",
                     entity: requestBody,
                     headers: AuthProvider.CreateAuthHeaderWithContentType(vendor));

            if (result != null && result.branchs != null && result.branchs.Count > 0)
            {
                return new ServiceResponseBase
                {
                    Success = result != null,
                    Data = result.branchs.Map()
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };

        }

        public Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            throw new NotImplementedException();
        }

        public async Task<List<FiloNovaResponseBase.WorkingHour>> GetAPIWorkingHours(Vendor vendor)
        {
            string cacheKey = string.Format(WORKING_HOUR_KEY, vendor.VendorId);

            if (_memoryCache.TryGetValue(cacheKey, out List<FiloNovaResponseBase.WorkingHour> list))
                return list;

            var requestBody = new FiloNovaRequestBase.MasterRequest
            {
                brokerCode = vendor.ApiClientId,
                langId = vendor.SecretKey.ToIntNullSafe()
            };

            var result = await RestManager.PostAsync<FiloNovaRequestBase.MasterRequest, FiloNovaResponseBase>(
                     requestPath: $"getmasterdata",
                     entity: requestBody,
                     headers: AuthProvider.CreateAuthHeaderWithContentType(vendor));

            if (result != null && result.workingHours != null && result.workingHours.Count > 0)
            {
                _memoryCache.Set(cacheKey, result.workingHours, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddDays(1),
                    Priority = CacheItemPriority.Normal
                });
            }

            return result?.workingHours;
        }
    }
}
