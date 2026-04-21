using KolayCAR.Broker.API.Factories.Abstract;
using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using KolayCAR.Broker.Infrastructure.Managers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static KolayCAR.Broker.API.Models.GooglePlacesModels;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Services
{
    public interface ILocationService
    {
        Task<List<CommonModels.Location>> GetLocations(int languageId);
        Task<List<Models.Location>> GetLocationsByLanguageId(int languageId);
        Task<ServiceResponseBase> GetLocationsByVendorId(int languageId, int vendorId, string locationName = "");
        Task<CommonModels.Location> GetLocation(int locationId, int languageId);
        Task<CommonModels.LocationVendor> GetLocationVendorByVendorId(int locationId, int vendorId);
        Task<ServiceResponseBase> GetLocationDetailsFromVendorApi(int languageId, int vendorId, string locationCode);
        Task<List<Countrylang>> GetCountries(int langId);
        Task<List<City>> GetCities(string countryCode);
        Task<bool> ExistCountry(string countryCode);
        Task<IEnumerable<SearchLocationDto>> GetAllSearchLocations(int agencyId);
        Task<List<Locationvendor>> GetActiveLocationVendorList();
        Task<Locationvendor> GetLocationVendor(int pickupLocationId, int vendorId);
        Task<List<Agencylocation>> GetAgencyLocationList();
        Task<List<int>> GetAgencyLocationsId(long agencyId);
        Task<List<API.Models.City>> GetCityList();
        Task<API.Models.City> GetCityById(int cityId);
        Task<API.Models.Location> GetPickupLocation(int pickupLocationId, List<int> agencyLocationIds);
        Task<API.Models.Location> GetLocationById(int locationId);
        Task<CountryTaxRate> GetTaxRateByCountryId(int countryId);
        Task<Countrylang> GetCountryByLocationId(int locaitonId, int languageId);
        Task MapYolcu360Locations(int vendorId);
        Task SetLocationCoordinate();
    }


    public class LocationService : ILocationService
    {
        private readonly BrokerContext _context;
        private readonly IAgencyService _agencyService;
        private int _currentAgencyId;
        private readonly IMemoryCache _memoryCache;
        private readonly ICacheService _cacheService;
        private readonly ILocationRepository _locationRepository;
        private readonly ILocationProviderFactory _locationProviderFactory;
        private readonly string CityListCacheKey = "CityListCacheKey";

        public LocationService(BrokerContext context, IAgencyService agencyService, IMemoryCache memoryCache, ICacheService cacheService, ILocationRepository locationRepository, ILocationProviderFactory locationProviderFactory)
        {
            _context = context;
            _agencyService = agencyService;
            _currentAgencyId = _agencyService.GetCurrentAgencyId();
            _memoryCache = memoryCache;
            _cacheService = cacheService;
            _locationRepository = locationRepository;
            _locationProviderFactory = locationProviderFactory;
        }

        public async Task<IEnumerable<SearchLocationDto>> GetAllSearchLocations(int agencyId)
        {
            return await _context.SearchLocationDtos.FromSqlRaw($"EXEC GETALLSEARCHLOCATIONS @agencyId = {agencyId}").ToListAsync();
        }

        public async Task<List<CommonModels.Location>> GetLocations(int languageId)
        {
            var sqlParameters = new SqlParameter[] {
                new SqlParameter
                {
                    ParameterName = "@OPERATIONID",
                    SqlDbType = SqlDbType.Int,
                    Value = CommonModels.AgencyOperationTypes.GetLocations.ToString("D")
                },
                new SqlParameter
                {
                    ParameterName = "@LANGUAGEID",
                    SqlDbType = SqlDbType.Int,
                    Value = languageId
                },
                new SqlParameter
                {
                    ParameterName = "@AGENCYID",
                    SqlDbType = SqlDbType.Int,
                    Value = _currentAgencyId
                }
            };

            var locations = await _context.Location.FromSqlRaw("EXECUTE SP_AGENCY_OPERATIONS @OPERATIONID = @OPERATIONID, @LANGUAGEID = @LANGUAGEID, @AGENCYID=@AGENCYID", sqlParameters).ToListAsync();
            var mappedLocations = locations.Map();
            await CompleteCountryInformationToLocationList(mappedLocations, languageId);
            if (mappedLocations.Count == 0)
                Serilog.Log.Fatal("Location list could not be reached!");
            return mappedLocations;
        }

        private async Task CompleteCountryInformationToLocationList(List<CommonModels.Location> locations, int languageId)
        {
            if (locations != null && locations.Count > 0)
                foreach (var location in locations)
                    await CompleteCountryInformation(location, languageId);
        }

        private async Task CompleteCountryInformation(CommonModels.Location location, int languageId)
        {
            var countryWithLanguages = await _context.Countrylang.Where(x => x.Countryid == location.CountryId && x.Lang == languageId).FirstOrDefaultAsync();
            var countryWithCodes = await _context.Country.Where(x => x.Countryid == location.CountryId).FirstOrDefaultAsync();

            var city = await _context.City.Where(x => x.Cityid == location.CityId).FirstOrDefaultAsync();

            if (city != null)
            {
                location.CityName = city.Cityname;
            }
            if (countryWithLanguages != null && countryWithCodes != null)
            {
                location.CountryName = countryWithLanguages.Countryname;
                location.CountryCode = countryWithCodes.Countrycode2;
            }

        }

        public async Task<ServiceResponseBase> GetLocationsByVendorId(int languageId, int vendorId, string locationName = "")
        {
            var vendor = await _context.Vendor.Where(x => x.Vendorid == vendorId).FirstOrDefaultAsync();
            if (vendor == null) return null;
            var mappedVendor = vendor.Map(encrypt: false);
            try
            {
                var locationProvider = _locationProviderFactory.CreateLocationProvider(mappedVendor, vendor, _memoryCache, _cacheService);

                if (locationProvider == null)
                    return new(null, false, "Location provier not found!");

                return await locationProvider.GetLocations(mappedVendor, languageId, locationName);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@LocationProviderError}", ex.ToJson());
                return new ServiceResponseBase(null, false, ex.Message);
            }
        }

        public async Task<CommonModels.Location> GetLocation(int locationId, int languageId)
        {
            var location = await _context.Location.Where(x => x.Id == locationId).FirstOrDefaultAsync();
            var mappedLocation = location.Map();
            await CompleteCountryInformation(mappedLocation, languageId);

            return mappedLocation;
        }

        public async Task<LocationVendor> GetLocationVendorByVendorId(int locationId, int vendorId)
        {
            var locationVendorList = await _context.Locationvendor.Where(x => x.Vendorid == vendorId && x.Locallocationid == locationId).FirstOrDefaultAsync();
            return locationVendorList != null ? locationVendorList.Map() : new CommonModels.LocationVendor();
        }

        public async Task<ServiceResponseBase> GetLocationDetailsFromVendorApi(int languageId, int vendorId, string locationCode)
        {
            var vendor = await _context.Vendor.Where(x => x.Vendorid == vendorId).FirstOrDefaultAsync();
            var mappedVendor = vendor.Map(encrypt: false);
            var locationProvider = _locationProviderFactory.CreateLocationProvider(mappedVendor, vendor, _memoryCache, _cacheService);

            var result = new ServiceResponseBase();
            try
            {
                result = await locationProvider.GetLocationDetail(mappedVendor, languageId, locationCode);
            }
            catch (System.Exception ex)
            {
                if (!result.Success)
                {
                    result = await locationProvider.GetLocations(mappedVendor, languageId);
                    var locations = result.Data as List<CommonModels.Location>;

                    if (locations != null)
                    {
                        result.Data = locations.FirstOrDefault(x => x.LocationCode == locationCode);
                    }
                }
            }
            return result;
        }

        public async Task<List<Countrylang>> GetCountries(int langId)
        {
            //var countyLang = await _context.Countrylang.Where(c => c.Lang == langId).ToListAsync();
            //var country = await _context.Country.ToListAsync();
            var query = from countrylang in _context.Countrylang
                        join country in _context.Country on countrylang.Countryid equals country.Countryid
                        where countrylang.Lang == langId
                        select new Countrylang
                        {
                            Id = countrylang.Id,
                            Countryid = countrylang.Countryid,
                            //CountryCode = country.Countrycode2, 
                            Countryname = countrylang.Countryname,
                            Lang = countrylang.Lang
                        };
            return await query.ToListAsync(); ;
        }

        public async Task<List<City>> GetCities(string countryCode)
        {
            int countryId = await _context.Country.Where(c => c.Countrycode2 == countryCode).Select(c => c.Countryid).FirstOrDefaultAsync();
            return await _context.City.Where(c => c.Countryid == countryId).ToListAsync();
        }

        public async Task<bool> ExistCountry(string countryCode)
        {
            return await _context.Country.AnyAsync(c => c.Countrycode2 == countryCode);
        }

        public async Task<List<Models.Location>> GetLocationsByLanguageId(int languageId)
        {
            return await _context.Location.Where(l => l.Langid == languageId).ToListAsync();
        }

        public async Task<List<Locationvendor>> GetActiveLocationVendorList()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync("LocationVendorList", () => _context.Locationvendor.Where(e => e.Active == true).ToListAsync());

            return await _context.Locationvendor.ToListAsync();
        }

        public async Task<List<Agencylocation>> GetAgencyLocationList()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync("AgencyLocationList", () => _context.Agencylocation.ToListAsync());

            return await _context.Agencylocation.ToListAsync();
        }

        public async Task<IEnumerable<Models.Location>> GetActiveLocationList()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{CacheSettings.LocationKey}-LocationList", _locationRepository.GetActiveLocationList);

            return await _context.Location.ToListAsync();
        }

        public async Task<List<City>> GetCityList()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync<List<City>>(CityListCacheKey, () => _context.City.ToListAsync());

            return await _context.City.ToListAsync();
        }

        public async Task<Locationvendor> GetLocationVendor(int pickupLocationId, int vendorId)
        {
            if (CacheSettings.UseCache)
            {
                return await _cacheService.GetOrCreateAsync($"LocationSupplier-{pickupLocationId}-{vendorId}", () => _context.Locationvendor.Where(x => x.Locallocationid == pickupLocationId &&
                                x.Active == true &&
                                x.Vendorid == vendorId)
                                .FirstOrDefaultAsync()
                );
            }
            return await _context.Locationvendor.Where(x => x.Locallocationid == pickupLocationId &&
                           x.Active == true &&
                           x.Vendorid == vendorId)
                           .FirstOrDefaultAsync();
        }

        public async Task<List<int>> GetAgencyLocationsId(long agencyId)
        {
            if (CacheSettings.UseCache)
            {
                var agencyLocationList = await GetAgencyLocationList();
                return agencyLocationList.Where(x => x.Agencyid == agencyId).Select(x => x.Locationid).ToList();
            }
            return await _context.Agencylocation.Where(x => x.Agencyid == agencyId).Select(x => x.Locationid).ToListAsync();
        }

        public async Task<Models.Location> GetPickupLocation(int pickupLocationId, List<int> agencyLocationIds)
        {
            if (CacheSettings.UseCache)
            {
                var locationList = await GetActiveLocationList();

                return locationList.Where(x => x.Id == pickupLocationId && x.Active == true && !agencyLocationIds.Contains(x.Id)).FirstOrDefault();
            }

            return await _locationRepository.GetPickupLocation(pickupLocationId, agencyLocationIds);
        }

        public async Task<Models.Location> GetLocationById(int locationId)
        {
            if (CacheSettings.UseCache)
            {
                var locationList = await GetActiveLocationList();
                return locationList.Where(e => e.Id == locationId).FirstOrDefault();
            }
            return await _locationRepository.GetLocationByIdAsync(locationId);
        }

        public async Task<City> GetCityById(int cityId)
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"Location-City-{cityId}", () => _context.City.Where(x => x.Cityid == cityId).FirstOrDefaultAsync());

            return await _context.City.Where(x => x.Cityid == cityId).FirstOrDefaultAsync();
        }

        public async Task<CountryTaxRate> GetTaxRateByCountryId(int countryId)
        {
            return await _context.CountryTaxRates.FirstOrDefaultAsync(e => e.CountryId == countryId);
        }

        public async Task<Countrylang> GetCountryByLocationId(int locaitonId, int languageId)
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"Location-Country-{locaitonId}-{languageId}", () => _context.Countrylang.Where(e => e.Id == locaitonId && e.Lang == languageId).FirstOrDefaultAsync());

            return await _context.Countrylang.Where(e => e.Id == locaitonId && e.Lang == languageId).FirstOrDefaultAsync();
        }

        public async Task MapYolcu360Locations(int vendorId)
        {
            //  var vendor = await _context.Vendor.Where(x => x.Vendorid == vendorId).FirstOrDefaultAsync();
            //var locations = await _context.Location.ToListAsync();
            // var provider = new Providers.Yolcu360v2.LocationProvider(vendor.Apibaseurl, _cacheService, locationVendorService: _locationVendorService);
            //  await provider.Yolcu360v2LocationsMap(vendor.Map(), locations.Where(e => e.Langid == 1).ToList());
        }

        public async Task SetLocationCoordinate()
        {
            var httpManager = new HttpManager("https://places.googleapis.com");

            // Sadece bir dilden (örn Langid=1) benzersiz lokasyonları alıyoruz (ID kolonuna göre)
            var uniqueLocations = await _context.Location
                .Where(l => l.Langid == 1 && (string.IsNullOrEmpty(l.Coordinatelatitude) || string.IsNullOrEmpty(l.Coordinatelongitude)))
                .ToListAsync();

            var semaphore = new SemaphoreSlim(10); // Aynı anda 10 kısıtlı istek
            var tasks = uniqueLocations.Select(async loc =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var result = await httpManager.PostAsyncWithModel<Req, Root>("/v1/places:searchText",
                        new Req { textQuery = loc.Locationname },
                        headers: new Dictionary<string, object>
                        {
                            { "X-Goog-Api-Key", "AIzaSyAS3cpUGZfFmCzzCp7HodNwT6RR8mLzFGI" },
                            { "X-Goog-FieldMask", "places.location" }
                        });

                    if (result?.places != null && result.places.Any())
                    {
                        var place = result.places.First();
                        string lat = place.location.latitude.ToString();
                        string lon = place.location.longitude.ToString();

                        // Bu ID'ye sahip tüm dillerdeki kayıtları update ediyoruz
                        var allLangRows = await _context.Location.Where(l => l.Id == loc.Id).ToListAsync();
                        foreach (var row in allLangRows)
                        {
                            row.Coordinatelatitude = lat.Length > 10 ? lat.Substring(0, 10) : lat;
                            row.Coordinatelongitude = lon.Length > 10 ? lon.Substring(0, 10) : lon;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error($"SetLocationCoordinate Error for LocId {loc.Id}: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
            await _context.SaveChangesAsync();
        }
    }
}
