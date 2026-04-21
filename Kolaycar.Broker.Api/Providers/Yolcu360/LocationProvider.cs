using DocumentFormat.OpenXml.Wordprocessing;
using KolayCAR.Broker.API.Mappers.Yolcu360;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Helpers;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using static KolayCAR.Broker.Domain.Models.Response.Yolcu360LocationResponseBase;
using CommonModels = KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;

namespace KolayCAR.Broker.API.Providers.Yolcu360
{
    public class LocationProvider : ILocationProvider
    {
        const string YOLCU_LOCATIONS = "YOLCU360_LOCATIONS";
        RestManager RestManager { get; set; }
        AuthProvider AuthProvider { get; set; }
        private readonly IMemoryCache _memoryCache;
        public string ProviderName => "Yolcu360";

        public LocationProvider(string apiBaseUrl)
        {
            RestManager = new RestManager(apiBaseUrl);
            AuthProvider = new AuthProvider(apiBaseUrl, _memoryCache);
        }
        public LocationProvider(string apiBaseUrl, IMemoryCache memoryCache)
        {
            RestManager = new RestManager(apiBaseUrl);
            _memoryCache = memoryCache;
            AuthProvider = new AuthProvider(apiBaseUrl, _memoryCache);
        }
        public async Task<ServiceResponseBase> GetLocationDetail(Vendor vendor, int languageId, string locationCode)
        {
            return new ServiceResponseBase
            {
                Success = false,
                Data = null
            };
        }

        public async Task<ServiceResponseBase> GetLocations(Vendor vendor, int languageId, string locationName = "")
        {
            #region API ile lokasyon listeleme
            //#region Cache'de lokasyonlar mevcut ise servise istek atmadan döner
            ////string cacheKey = string.Format(YOLCU_LOCATIONS, vendor.VendorId);
            ////if (_memoryCache != null && _memoryCache.TryGetValue(cacheKey, out List<CommonModels.Location> list))
            ////{
            ////    return new ServiceResponseBase
            ////    {
            ////        Success = true,
            ////        Data = list
            ////    };
            ////}
            //#endregion

            var totalLenght = 15;//39;

            var tasks = new List<Task>();

            //var locationList = new List<CommonModels.Location>();

            var locationList = new ConcurrentBag<CommonModels.Location>();




            for (int i = 0; i <= totalLenght; i++)
            {
                var offset = i * 1000;
                #region
                //tasks.Add(Task.Run(() =>
                //{
                //    var locationsResponse = RestManager.GetAsync<GetLocationResponse>( requestPath: "location/all?offset=0" , headers: AuthProvider.CreateHeaderWithContentType()).Result;



                //    if (locationsResponse != null && locationsResponse.count > 0 && locationsResponse.data != null && locationsResponse.data.Count > 0)
                //    {
                //        var list = locationsResponse.data.Map();
                //        locationList.AddRange(list);
                //    }
                //}));
                #endregion

                //var locationsResponse = RestManager.GetAsync<GetLocationResponse>(requestPath: "location/?offset=" +offset.ToString(), headers: AuthProvider.CreateHeaderWithContentType()).Result;
                //if (locationsResponse != null && locationsResponse.count > 0 && locationsResponse.data != null && locationsResponse.data.Count > 0)
                //{
                //    var list = locationsResponse.data.Map();
                //    locationList.AddRange(list);
                //}

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var locationsResponse = await RestManager.GetAsync<GetLocationResponse>(
                                requestPath: $"location/all?offset={offset}",
                                headers: AuthProvider.CreateHeaderWithContentType()
                        );


                        if (locationsResponse != null && locationsResponse.count > 0 && locationsResponse.data != null)
                        {
                            var list = locationsResponse.data.Map();
                            foreach (var location in list)
                            {
                                locationList.Add(location);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Print($"Hata oluştu. Offset: {offset}, Hata: {ex.Message}");
                        Console.WriteLine($"Hata oluştu. Offset: {offset}, Hata: {ex.Message}");
                    }

                }));



            }



            await Task.WhenAll(tasks);
            tasks.Clear();
            //locationList = locationList.OrderBy(x => x?.LocationName).ToList();
            //locationList = locationList.GroupBy(x => x.LocationId).Select(x => x.First()).ToList();


            var finalList = locationList
                .OrderBy(x => x?.LocationName) // İsim sırasına göre sırala
                .GroupBy(x => x.LocationId)    // Aynı ID'leri gruplandır
                .Select(x => x.First())        // Her gruptan yalnızca ilkini al
                .ToList();

            //if (_memoryCache != null && locationList.Count > 0)
            //{
            //    _memoryCache.Set(cacheKey, locationList, new MemoryCacheEntryOptions
            //    {
            //        AbsoluteExpiration = DateTime.Now.AddDays(3),
            //        Priority = CacheItemPriority.Normal
            //    });
            //}

            return new ServiceResponseBase
            {
                Success = finalList.Count > 0,
                Data = finalList

            };
            #endregion

            #region Excel File
            //var result = GetLocationList();
            //if(result !=null)
            //    return new ServiceResponseBase
            //    {
            //        Success = true,
            //        Data = result
            //    };


            //return new ServiceResponseBase
            //{
            //    Success = false,
            //    Data = "Lokasyonlar yüklenemedi."
            //};
            #endregion
        }

        private List<CommonModels.Location> GetLocationList()
        {
            return  ReadExcelFile("Docs\\Yolcu360\\LocationList.xlsx");
            
        }

        private static List<CommonModels.Location> ReadExcelFile(string filePath)
        {
            var locationList = new List<CommonModels.Location>();

            var excelResult = ExcelHelper.ReadExcel(filePath);

            if (excelResult?.Rows?.Count > 0)
            {
                for (int i = 0; i < excelResult.Rows.Count; i++)
                {
                    locationList.Add(new CommonModels.Location
                    {
                        LocationId = excelResult.Rows[i]["id"].ToIntNullSafe(),
                        LocationCode = excelResult.Rows[i]["id"].ToStringNullSafe(),
                        LocationName = excelResult.Rows[i]["name"].ToStringNullSafe() + " " + excelResult.Rows[i]["name"].ToStringNullSafe(),
                        IsPickup = true,
                        Address = excelResult.Rows[i]["name"].ToStringNullSafe() + " " + excelResult.Rows[i]["name"].ToStringNullSafe(),
                        PhoneNumber = "",
                        MailAddress = ""



                    });
                }
            }

            return locationList;

        }

        public async Task<ServiceResponseBase> GetLocationsOld(Vendor vendor, int languageId)
        {
            #region Cache'de lokasyonlar mevcut ise servise istek atmadan döner
            string cacheKey = string.Format(YOLCU_LOCATIONS, vendor.VendorId);
            if (_memoryCache != null && _memoryCache.TryGetValue(cacheKey, out List<CommonModels.Location> list))
            {
                return new ServiceResponseBase
                {
                    Success = true,
                    Data = list
                };
            }
            #endregion

            #region Yolcu360 lokasyon servisine istek ve cache
            var tasks = new List<Task>();
            var locationList = new List<CommonModels.Location>();

            //var auth = AuthProvider.Login(vendor.ApiKey, vendor.ApiPassword).Result;

            var result = await RestManager.GetAsync<GetLocationResponse>(
                requestPath: "location",
                headers: AuthProvider.CreateHeaderWithContentType()
                );

            if (result != null && result.count > 0 && result.total > 0)
            {
                var totalLenght = 0;
                totalLenght = result.total / result.count;
                for (int i = 0; i <= totalLenght; i++)
                {
                    var offset = i * 100;
                    tasks.Add(Task.Run(() =>
                    {
                        var locationsResponse = RestManager.GetAsync<GetLocationResponse>(
                            requestPath: "location?offset=" + offset.ToString(),
                            headers: AuthProvider.CreateHeaderWithContentType()
                            ).Result;

                        if (locationsResponse != null && locationsResponse.data != null)
                        {
                            var items = locationsResponse.data.Where(x => x.city.country == "TR").ToList().Map();

                            if (items.Count > 0)
                                locationList.AddRange(items);
                        }
                    }));
                }
                await Task.WhenAll(tasks);
                locationList = locationList.OrderBy(x => x.LocationName).ToList();

                if (_memoryCache != null && locationList.Count > 0)
                {
                    _memoryCache.Set(cacheKey, locationList, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTime.Now.AddDays(2),
                        Priority = CacheItemPriority.Normal
                    });
                }
                #endregion

                return new ServiceResponseBase
                {
                    Success = true,
                    Data = locationList
                };
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Yolcu360 servisine ulaşılamadı."
            };
        }

        private Dictionary<string, object> GetLocationsParameters(int offset)
        {
            return new Dictionary<string, object>
            {
                { "city", null },
                { "offset", offset * 100 },
                { "limit", null }
            };
        }
        private string GetLocationRequestParametersForOffset(int i)
        {
            return "offsett=" + (i * 100).ToString();
        }
    }
}
