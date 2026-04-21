using KolayCAR.Broker.API.Extensions;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels.Enums;
using KolayCAR.Broker.API.Models.MobileAppDtos.VehicleListDtos;
using KolayCAR.Broker.API.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vendor = KolayCAR.Broker.Domain.Models.Vendor;

namespace KolayCAR.Broker.API.Creators
{
    public class Creator
    {
        private readonly IAgencyService _agencyService;
        private readonly ICouponService _couponService;
        private readonly IParameterService _parameterService;
        private readonly IVendorService _vendorService;
        private readonly IReservationService _reservationService;
        private readonly IMemoryCache _memoryCache;

        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public Creator(
            IAgencyService agencyService,
            ICouponService couponService,
            IParameterService parameterService,
            IReservationService reservationService,
            IVendorService vendorService,
            IMemoryCache memoryCache
            )
        {
            _agencyService = agencyService;
            _couponService = couponService;
            _parameterService = parameterService;
            _reservationService = reservationService;
            _vendorService = vendorService;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Araçları ayarlarda belirtilen tipe göre sıralar
        /// </summary>
        public async Task<List<VehicleDto>> SortVehicles(List<VehicleDto> brokerApiVehicleDtos, int pickupPointId, int languageId)
        {
            var agencyId = _agencyService.GetCurrentAgencyId();
            // Tedarikçiye sırasına göre
            var orderByVendorOrder = (await _parameterService.GetParameter("SortVehiclesByVendorOrder"))?.Deger?.ToBool() ?? false;
            // Popüler  araca göre
            var orderByPopularVehicle = (await _parameterService.GetParameter("SortVehiclesByPopularity"))?.Deger?.ToBool() ?? false;
            // Ucuzdan pahalıya
            var orderByCheapest = (await _parameterService.GetParameter("SortVehiclesByCheapest"))?.Deger?.ToBool() ?? false;

            var reOrderedList = new List<VehicleDto>();

            // Tedarikçi Sırası önemli
            if (orderByVendorOrder)
            {
                #region Veriler toparlanıyor
                // While döngüsü tekrar sayısı (Sonsuz döngüyü engellemek için var)
                var count = 0;
                // Listede ilk kaç aracın tedarikçi sırasına göre seçileceğini belirler
                var vendorListCount = (await _parameterService.GetParameter("VendorOrderCount"))?.Deger.ToInt() ?? 10;
                var vendors = await GetVendors();
                // Sıralamaya göre tedarikçi Id değerleri
                var orderedVendorIds = vendors
                    .Where(v => v.VendorOrder != null)
                    .OrderBy(v => v.VendorOrder)
                    .Select(v => v.VendorId).ToList();

                var popularVehicles = await GetPopularVehicles(languageId, pickupPointId);
                // Popülerliğe göre araç Id değerleri
                var popularVehicleIds = popularVehicles
                    .OrderByDescending(pv => pv.RentalCount)
                    .Take(vendorListCount)
                    .Select(pv => pv.VehicleId)
                    .ToList();
                #endregion

                // Araç popülerliği aktif
                if (orderByPopularVehicle && popularVehicleIds.Any())
                {
                    // Araçları, tedarikçi sırasına göre, popüler araçların en ucuzu ilk sırada olacak şekilde sıralar
                    if (orderByCheapest)
                    {
                        while (reOrderedList.Count <= vendorListCount && count < vendorListCount)
                        {
                            foreach (var vendorId in orderedVendorIds)
                            {
                                var popularVehicle = brokerApiVehicleDtos
                                    .Where(v => !reOrderedList?.Contains(v) ?? false)?
                                    .Where(v => v.VendorId == vendorId)?
                                    .Where(v => popularVehicleIds?.Contains(v.VehicleId) ?? false)?
                                    .OrderBy(v => v.DailyPrice)?
                                    .FirstOrDefault();
                                if (popularVehicle != null && popularVehicle.VehicleId > 0)
                                {
                                    // Populer araclarin en ucuzu
                                    reOrderedList.Add(popularVehicle);
                                }
                                else
                                {
                                    // Tedarikcinin populer aracı yoksa en ucuz aracı
                                    var notPopularVehicle = brokerApiVehicleDtos
                                        .Where(v => !reOrderedList?.Contains(v) ?? false)?
                                        .Where(v => v.VendorId == vendorId)?
                                        .OrderBy(v => v.DailyPrice)?
                                        .FirstOrDefault();
                                    if (notPopularVehicle != null && notPopularVehicle.VehicleId > 0)
                                    {
                                        reOrderedList.Add(notPopularVehicle);
                                    }
                                }

                            }
                            count++;
                        }

                        // Kalan araçlar ucuzdan pahalıya göre ekleniyor
                        reOrderedList.AddRange(
                            brokerApiVehicleDtos
                                .Where(v => !reOrderedList.Contains(v))
                                .OrderBy(v => v.DailyPrice)
                        );
                    }
                    // Araçları, tedarikçi sırasına göre, popüler araçların en pahalısı ilk sırada olacak şekilde sıralar
                    else
                    {
                        while (reOrderedList.Count <= vendorListCount && count < vendorListCount)
                        {
                            foreach (var vendorId in orderedVendorIds)
                            {
                                var popularVehicle = brokerApiVehicleDtos
                                    .Where(v => !reOrderedList.Contains(v))
                                    .Where(v => v.VendorId == vendorId)
                                    .Where(v => popularVehicleIds.Contains(v.VehicleId))
                                    .OrderByDescending(v => v.DailyPrice)
                                    .FirstOrDefault();
                                if (popularVehicle != null && popularVehicle.VehicleId > 0)
                                {
                                    // Populer araclarin en ucuzu
                                    reOrderedList.Add(popularVehicle);
                                }
                                else
                                {
                                    // Tedarikcinin populer aracı yoksa en ucuz aracı
                                    var notPopularVehicle = brokerApiVehicleDtos
                                        .Where(v => !reOrderedList.Contains(v))
                                        .Where(v => v.VendorId == vendorId)
                                        .OrderByDescending(v => v.DailyPrice)
                                        .FirstOrDefault();
                                    if (notPopularVehicle != null && notPopularVehicle.VehicleId > 0)
                                    {
                                        reOrderedList.Add(notPopularVehicle);
                                    }
                                }
                            }
                            count++;
                        }

                        // Kalan araçlar ucuzdan pahalıya göre ekleniyor
                        reOrderedList.AddRange(
                            brokerApiVehicleDtos
                                .Where(v => !reOrderedList.Contains(v))
                                .OrderByDescending(v => v.DailyPrice)
                        );
                    }
                }
                // Araç popülerliği pasif
                else
                {
                    // Araçları, tedarikçi sırasına ve ucuzdan pahalıya göre sıralar
                    if (orderByCheapest)
                    {
                        while (reOrderedList.Count <= vendorListCount && count < vendorListCount)
                        {
                            foreach (var vendorId in orderedVendorIds)
                            {
                                var notPopularVehicle = brokerApiVehicleDtos
                                    .Where(v => !reOrderedList.Contains(v))
                                    .Where(v => v.VendorId == vendorId)
                                    .OrderBy(v => v.DailyPrice)
                                    .FirstOrDefault();
                                if (notPopularVehicle != null && notPopularVehicle.VehicleId > 0)
                                {
                                    reOrderedList.Add(notPopularVehicle);
                                }
                            }
                            count++;
                        }

                        // Kalan araçlar ucuzdan pahalıya göre ekleniyor
                        reOrderedList.AddRange(
                            brokerApiVehicleDtos
                                .Where(v => !reOrderedList.Contains(v))
                                .OrderBy(v => v.DailyPrice)
                        );
                    }
                    // Araçları, tedarikçi sırasına ve pahalıdan ucuza göre sıralar
                    else
                    {
                        while (reOrderedList.Count <= vendorListCount && count < vendorListCount)
                        {
                            foreach (var vendorId in orderedVendorIds)
                            {
                                var notPopularVehicle = brokerApiVehicleDtos
                                    .Where(v => !reOrderedList.Contains(v))
                                    .Where(v => v.VendorId == vendorId)
                                    .OrderByDescending(v => v.DailyPrice)
                                    .FirstOrDefault();
                                if (notPopularVehicle != null && notPopularVehicle.VehicleId > 0)
                                {
                                    reOrderedList.Add(notPopularVehicle);
                                }
                            }
                            count++;
                        }

                        // Kalan araçlar ucuzdan pahalıya göre ekleniyor
                        reOrderedList.AddRange(
                            brokerApiVehicleDtos
                                .Where(v => !reOrderedList.Contains(v))
                                .OrderByDescending(v => v.DailyPrice)
                        );
                    }
                }
            }
            // Tedarikçi Sırası önemsiz
            else
            {
                if (orderByPopularVehicle)
                {
                    #region Veriler toplanıyor
                    var popularVehicles = await GetPopularVehicles(languageId, pickupPointId);
                    // Popülerliğe göre araç Id değerleri
                    var popularVehicleIds = popularVehicles
                        .OrderByDescending(pv => pv.RentalCount)
                        .Select(pv => pv.VehicleId)
                        .ToList();
                    #endregion
                    // Araçları, popüler araçların en ucuzu ilk sırada olacak şekilde sıralar
                    if (orderByCheapest)
                    {
                        foreach (var vehicleId in popularVehicleIds)
                        {
                            var vehicle = brokerApiVehicleDtos
                                .Where(v => !reOrderedList.Contains(v))
                                .Where(v => v.VehicleId == vehicleId)
                                .OrderBy(v => v.DailyPrice)
                                .FirstOrDefault();
                            if (vehicle != null && vehicle.VehicleId > 0)
                            {
                                reOrderedList.Add(vehicle);
                            }
                        }

                        // Kalan araçlar ucuzdan pahalıya göre ekleniyor
                        reOrderedList.AddRange(
                            brokerApiVehicleDtos
                                .Where(v => !reOrderedList.Contains(v))
                                .OrderBy(v => v.DailyPrice)
                        );
                    }
                    // Araçları, popüler araçların en pahalısı ilk sırada olacak şekilde sıralar
                    else
                    {
                        foreach (var vehicle in popularVehicleIds.Select(vehicleId => brokerApiVehicleDtos
                                     .Where(v => !reOrderedList.Contains(v))
                                     .Where(v => v.VehicleId == vehicleId)
                                     .OrderByDescending(v => v.DailyPrice)
                                     .FirstOrDefault()))
                        {
                            if (vehicle != null && vehicle.VehicleId > 0)
                            {
                                reOrderedList.Add(vehicle);
                            }
                        }

                        // Kalan araçlar ucuzdan pahalıya göre ekleniyor
                        reOrderedList.AddRange(
                            brokerApiVehicleDtos
                                .Where(v => !reOrderedList.Contains(v))
                                .OrderByDescending(v => v.DailyPrice)
                        );
                    }
                }
                else
                {
                    // Araçları, en ucuzu ilk sırada olacak şekilde sıralar
                    if (orderByCheapest)
                    {
                        reOrderedList.AddRange(
                            brokerApiVehicleDtos
                                .OrderBy(v => v.DailyPrice)
                        );
                    }
                    // Araçları, en pahalısı ilk sırada olacak şekilde sıralar
                    else
                    {
                        reOrderedList.AddRange(
                            brokerApiVehicleDtos
                                .OrderByDescending(v => v.DailyPrice)
                        );
                    }
                }
            }

            #region Varsayılan sıralama için araç sıra numaraları atanıyor ve kampanyalı araçlar işaretleniyor
            foreach (var vehicle in reOrderedList)
            {
                vehicle.OrderNo = reOrderedList.IndexOf(vehicle) + 1;
                //if (await CouponExists(vehicle.VendorId, pickupPointId))
                //{
                //    vehicle.IsCampaignVehicle = true;
                //}
            }
            #endregion

            #region Mobile App için
            foreach (var vehicle in reOrderedList)
            {
                vehicle.SortableParameters.Add(new SortableParameter
                {
                    DataType = MobileDataTypes.Number,
                    Name = "orderNo",
                    Value = $"{vehicle.OrderNo ?? 0}"
                });
            }
            #endregion

            return reOrderedList;
        }

        private async Task<List<Vendor>> GetVendors()
        {
            var vendors = new List<Vendor>();
            var vendorsExists = _memoryCache.TryGetValue("Vendors", out vendors);
            if (!vendorsExists)
            {
                vendors = (await _vendorService.GetVendors(_agencyService.GetCurrentAgencyId())).ToList();
                _memoryCache.Set("Vendors", vendors, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromDays(1))
                    .SetAbsoluteExpiration(TimeSpan.FromDays(7))
                    .AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token)));
            }

            return vendors;
        }

        private async Task<IEnumerable<BrokerLocationVehicleDetailDto>> GetPopularVehicles(int languageId, int pickupPointId)
        {
            IEnumerable<BrokerLocationVehicleDetailDto> popularVehicles = null;
            var popularVehiclesExists =
                _memoryCache.TryGetValue("PopularVehiclesByReservations", out popularVehicles);

            if (!popularVehiclesExists)
            {
                popularVehicles = await _reservationService.GetAllPopularVehicles();
                _memoryCache.Set("PopularVehiclesByReservations", popularVehicles, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromDays(1))
                    .SetAbsoluteExpiration(TimeSpan.FromDays(7))
                    .AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token)));
            }

            return popularVehicles.Where(pv => pv.LanguageId == languageId && pv.LocationId == pickupPointId);
        }

        private async Task<bool> CouponExists(int vehicleVendorId, int vehiclePickupLocationId)
        {
            var result = await _couponService.GetActiveCouponsByVendorIdAsync(vehicleVendorId, vehiclePickupLocationId);
            return result?.Id > 0 ? true : false;
        }
    }
}
