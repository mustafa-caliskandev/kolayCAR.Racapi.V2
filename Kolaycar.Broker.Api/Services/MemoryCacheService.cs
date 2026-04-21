using kolayCAR.Broker.AWS.Models.AwsModels.Kinesis;
using UserDetailModel = kolayCAR.Broker.AWS.Models.AwsModels.Kinesis.UserDetailModel;
using CouponModel = kolayCAR.Broker.AWS.Models.AwsModels.Kinesis.CouponModel;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.Dtos;
using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels;
using KolayCAR.Broker.API.Models.MobileAppModels;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using City = KolayCAR.Broker.API.Models.City;

namespace KolayCAR.Broker.API.Services
{
    public interface IMemoryCacheService
    {
        Task<List<Currency>> GetCurrencies();
        Task<List<Icerik>> GetAllActiveContents(int tipId);
        Task<List<Icerikdil>> GetAllActiveContentLanguages(int tipid, int languageId);
        Task<List<Label>> GetLabels(int languageId);
        Task<List<MobileVehicleDetail>> GetVehicleDetails();
        Task<List<MobileVehicleFeature>> GetVehicleFutures();
        Task<List<Vehiclefuellang>> GetVehicleFuelLangs();
        Task<List<Vehicletransmissionlang>> GetVehicleTransmissionLangs();
        Task<List<Coupon>> GetVendorActiveCoupons();
        Task<List<MobileAppSetting>> GetMobileSettings();
        Task<int> GetLanguageId(string languageCode);
        Task<List<AgencyVendorDto>> GetLocationVendors(int pickupLocationId);
        Task<List<MobileVehicleListFilter>> GetFilters();
        Task<List<MobileVehicleSortingOption>> GetSortingOptions();
        Task<List<MobileVehicleBadge>> GetBadges();
        Task<List<Vendorcontactinformation>> GetLocationVendorContacts(int pickupLocationId);
        Task<IEnumerable<Icerikdil>> GetContentLanguageBySettings((string, string) settings, int languageId);
        Task<List<DeliveryTypeLanguage>> GetDeliveryTypes();
        Task<IEnumerable<SearchLocationDto>> GetAllSearchLocations(int agencyId);
        Task<City> GetCityByLocationId(int locationId, int languageId, int countryid);
        Task<Countrylang> GetCountryByLocationId(int locationId, int languageId);
        Task<Location> GetLocationByLocationId(int locationId, int languageId);
        Task<bool> SetUserDetailModel(string memoryKey, UserDetailModel userDetailModel);
        Task<bool> SetCouponDetailModel(string memoryKey, CouponModel couponModel);
        Task<UserDetailModel> GetUserDetailModel(string memoryKey);
        Task<CouponModel> GetCouponDetailModel(string memoryKey);
        Task<List<VendorLocationCondition>> GetVendorLocationCondition(int vendorId, int pickupLocationId, int languageId);
        Task<bool> CheckVendorInCampaign(int vendorId, int couponId);
        Task<List<VendorOffice>> GetVendorOffices(int pickupLocationId);
        Task<List<VendorLocationDeliveryType>> GetVendorLocationDeliveryTypes();
        Task<Kullanici> GetUserByEmail(string email);
    }
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly BrokerContext _context;

        private readonly IAgencyService _agencyService;
        private readonly IContentService _contentService;
        private readonly IMemoryCache _memoryCache;
        private readonly ICacheService _cacheService;

        public MemoryCacheService(
            BrokerContext context,

            IAgencyService agencyService,
            IContentService contentService,
            IMemoryCache memoryCache,
            ICacheService cacheService)
        {
            _context = context;

            _agencyService = agencyService;
            _contentService = contentService;
            _memoryCache = memoryCache;
            _cacheService = cacheService;
        }


        public async Task<IEnumerable<SearchLocationDto>> GetAllSearchLocations(int agencyId)
        {
            var result = new List<SearchLocationDto>();
            var cacheExists = _memoryCache.TryGetValue("AllSearchLocations", out result);

            if (!cacheExists)
            {
                result = await _context.SearchLocationDtos.FromSqlRaw($"EXEC GETALLSEARCHLOCATIONS @agencyId = {agencyId}").ToListAsync();
                _memoryCache.Set("AllSearchLocations", result);
            }

            return result;
        }

        public async Task<List<VendorOffice>> GetVendorOffices(int pickupLocationId)
        {
            var result = new List<VendorOffice>();
            var cacheExists = _memoryCache.TryGetValue($"{pickupLocationId}-AllVendorOffices", out result);

            if (!cacheExists)
            {
                result = await _context.VendorOffices.Where(v => v.LocationId == pickupLocationId).ToListAsync();
                _memoryCache.Set($"{pickupLocationId}-AllVendorOffices", result);
            }

            return result;
        }

        public async Task<IEnumerable<Icerikdil>> GetContentLanguageBySettings((string, string) settings, int languageId)
        {
            var (typeName, groupName) = settings;

            var type = await _context.Iceriktip.FirstOrDefaultAsync(i => i.Tipadi == typeName);

            if (type != null)
            {
                //var allContents = await _contentCacheService.GetListFromCache(); // TODO : AKTÝFLERE GEÇÝLDÝ
                var contentLanguagesCache = await GetAllActiveContentLanguages(type.Tipid, languageId);
                var allContents = contentLanguagesCache?.Select(c => c.Icerik);

                IEnumerable<int> contents;
                if (groupName != "") // Group Name verilmemiþse
                {
                    var groups = await _context.Icerikgrup.ToListAsync();
                    var group = groups.FirstOrDefault(cg => cg.Grupadi == groupName);

                    contents = allContents?.Where(c => c.Grupid == (group?.Grupid ?? -1)).Select(c => c.Icerikid);
                }
                else
                {
                    contents = allContents?.Select(c => c.Icerikid);
                }

                var contentLanguages = contentLanguagesCache
                        .Where(c => contents.Contains(c.Icerikid) && (c.Aktifmi ?? false))
                        .ToList();
                return contentLanguages.Any() ? contentLanguages : null;
            }
            return null;
        }

        public async Task<List<Currency>> GetCurrencies()
        {
            var currencyList = new List<Currency>();
            var cacheExists = _memoryCache.TryGetValue("Currencies", out currencyList);
            if (!cacheExists)
            {
                currencyList = await _context.Currency.ToListAsync();
                _memoryCache.Set("Currencies", currencyList);
            }

            return currencyList;
        }

        public async Task<List<Icerik>> GetAllActiveContents(int tipId)
        {
            var contentList = new List<Icerik>();
            var cacheExists = _memoryCache.TryGetValue($"ActiveContents-{tipId}", out contentList);
            if (!cacheExists)
            {
                var contents = await _contentService.GetAllActiveContents(tipId);
                contentList = contents.ToList();
                _memoryCache.Set($"ActiveContents-{tipId}", contentList);
            }

            return contentList;
        }

        public async Task<List<Icerikdil>> GetAllActiveContentLanguages(int tipid, int languageId)
        {
            var contentLanguageList = new List<Icerikdil>();
            var cacheExists = _memoryCache.TryGetValue($"ActiveContentLanguages-{tipid}-{languageId}", out contentLanguageList);
            if (!cacheExists)
            {
                var contentLanguages = await _contentService.GetAllContentLanguagesIncludeContent(tipid, languageId);
                contentLanguageList = contentLanguages.ToList();
                _memoryCache.Set($"ActiveContentLanguages-{tipid}-{languageId}", contentLanguageList);
            }

            return contentLanguageList;
        }

        public async Task<List<Label>> GetLabels(int languageId)
        {
            var labelList = new List<Label>();
            var cacheExists = _memoryCache.TryGetValue($"Labels-{languageId}", out labelList);
            if (!cacheExists)
            {
                labelList = await _context.Label.Where(l => l.Dilid == languageId).ToListAsync();
                _memoryCache.Set($"Labels-{languageId}", labelList);
            }

            return labelList;
        }

        public async Task<List<MobileVehicleDetail>> GetVehicleDetails()
        {
            var vehicleDetails = new List<MobileVehicleDetail>();
            var cacheExists = _memoryCache.TryGetValue("MobileVehicleDetails", out vehicleDetails);
            if (!cacheExists)
            {
                vehicleDetails = await _context.MobileVehicleDetails.Where(d => d.Active ?? false).ToListAsync();
                _memoryCache.Set("MobileVehicleDetails", vehicleDetails);
            }
            return vehicleDetails;
        }

        public async Task<List<MobileVehicleFeature>> GetVehicleFutures()
        {
            var vehicleFeatures = new List<MobileVehicleFeature>();
            var cacheExists = _memoryCache.TryGetValue("MobileVehicleFeatures", out vehicleFeatures);
            if (!cacheExists)
            {
                vehicleFeatures = await _context.MobileVehicleFeatures.Where(f => f.Active ?? false).ToListAsync();
                _memoryCache.Set("MobileVehicleFeatures", vehicleFeatures);
            }

            return vehicleFeatures;
        }

        public async Task<List<Coupon>> GetVendorActiveCoupons()
        {
            var couponList = await _cacheService.GetOrCreateAsync($"VendorActiveCoupon", async () =>
            {
                return await _context.Coupon.Where(c => c.Active == true && c.VendorId != null).ToListAsync();
            }, TimeSpan.FromDays(7));

            return couponList;
        }

        public async Task<List<MobileAppSetting>> GetMobileSettings()
        {
            var mobileSettings = new List<MobileAppSetting>();
            var cacheExists = _memoryCache.TryGetValue("MobileAppSettings", out mobileSettings);
            if (!cacheExists)
            {
                mobileSettings = await _context.MobileAppSettings.ToListAsync();
                _memoryCache.Set("MobileAppSettings", mobileSettings);
            }

            return mobileSettings;
        }

        public async Task<int> GetLanguageId(string languageCode)
        {
            languageCode = languageCode.ToLower();
            var cacheExists = _memoryCache.TryGetValue("Languages", out List<Dil> languages);
            if (!cacheExists)
            {
                languages = await _context.Dil.ToListAsync();
                _memoryCache.Set("Languages", languages);
            }

            return languages.FirstOrDefault(l => l.Dilkod == languageCode)?.Dilid ?? -1;
        }

        public async Task<List<AgencyVendorDto>> GetLocationVendors(int pickupLocationId)
        {
            var vendors = new List<AgencyVendorDto>();
            var vendorsExists = _memoryCache.TryGetValue("AgencyVendors", out vendors);
            if (!vendorsExists)
            {
                vendors = (await _agencyService.GetAgencyVendors(_agencyService.GetCurrentAgencyId())).ToList();
                _memoryCache.Set("AgencyVendors", vendors);
            }

            return vendors.Where(v => v.LocationId == pickupLocationId).ToList();
        }

        public async Task<List<MobileVehicleListFilter>> GetFilters()
        {
            List<MobileVehicleListFilter> filter = new List<MobileVehicleListFilter>();

            var cacheExists = _memoryCache.TryGetValue("VehicleListFilters", out filter);
            if (!cacheExists)
            {
                filter = await _context.MobileVehicleListFilters
               .Where(vf => vf.Active == true)
               .OrderBy(vf => vf.Order)
               .ToListAsync();

                _memoryCache.Set("VehicleListFilters", filter);
            }

            return filter;
        }

        public async Task<List<MobileVehicleSortingOption>> GetSortingOptions()
        {
            List<MobileVehicleSortingOption> options = new List<MobileVehicleSortingOption>();

            var cacheExists = _memoryCache.TryGetValue("VehicleListSortingOptions", out options);
            if (!cacheExists)
            {
                options = await _context.MobileVehicleSortingOptions
                .Where(so => so.Active == true)
                .OrderBy(so => so.Order)
                .ToListAsync();

                _memoryCache.Set("VehicleListSortingOptions", options);
            }

            return options;
        }

        public async Task<List<MobileVehicleBadge>> GetBadges()
        {
            List<MobileVehicleBadge> badges = new List<MobileVehicleBadge>();

            var cacheExists = _memoryCache.TryGetValue("VehicleListBadgets", out badges);
            if (!cacheExists)
            {
                badges = await _context.MobileVehicleBadges.Where(vb => vb.Active == true).ToListAsync();
                _memoryCache.Set("VehicleListBadgets", badges);
            }

            return badges;
        }

        public async Task<List<Vendorcontactinformation>> GetLocationVendorContacts(int pickupLocationId)
        {
            List<Vendorcontactinformation> vendorcontactinformations = new List<Vendorcontactinformation>();
            var cacheExists = _memoryCache.TryGetValue($"VehicleListVendorContactInfos-{pickupLocationId}", out vendorcontactinformations);
            if (!cacheExists)
            {
                vendorcontactinformations = await _context.Vendorcontactinformation.Where(vl => vl.Locationid == pickupLocationId).ToListAsync();
                _memoryCache.Set($"VehicleListVendorContactInfos-{pickupLocationId}", vendorcontactinformations);
            }

            return vendorcontactinformations;
        }

        public async Task<List<DeliveryTypeLanguage>> GetDeliveryTypes()
        {
            List<DeliveryTypeLanguage> deliveryTypes = new List<DeliveryTypeLanguage>();
            var cacheExists = _memoryCache.TryGetValue("DeliveryTypes", out deliveryTypes);
            if (!cacheExists)
            {
                deliveryTypes = await _context.DeliveryTypeLanguages.ToListAsync();
                _memoryCache.Set("DeliveryTypes", deliveryTypes);
            }

            return deliveryTypes;
        }

        public async Task<City> GetCityByLocationId(int locationId, int languageId, int countryid)
        {
            List<City> cities = new List<City>();
            var cacheExists = _memoryCache.TryGetValue($"Cities-{countryid}", out cities);
            if (!cacheExists)
            {
                cities = await _context.City.Where(c => c.Countryid == countryid).ToListAsync();
                _memoryCache.Set($"Cities-{countryid}", cities);
            }

            var location = await GetLocationByLocationId(locationId, languageId);

            return cities.FirstOrDefault(c => c.Cityid == location?.Cityid) ?? null;
        }

        public async Task<Countrylang> GetCountryByLocationId(int locationId, int languageId)
        {
            List<Countrylang> countries = new List<Countrylang>();
            var cacheExist = _memoryCache.TryGetValue($"Countries-{languageId}", out countries);
            if (!cacheExist)
            {
                countries = await _context.Countrylang.Where(c => c.Lang == languageId).ToListAsync();
                _memoryCache.Set($"Countries-{languageId}", countries);
            }

            var location = await GetLocationByLocationId(locationId, languageId);

            return countries.FirstOrDefault(c => c.Countryid == location?.Countryid) ?? null;
        }

        public async Task<Location> GetLocationByLocationId(int locationId, int languageId)
        {
            List<Location> locations = new List<Location>();
            var cacheExist = _memoryCache.TryGetValue($"Locations-{languageId}", out locations);
            if (!cacheExist)
            {
                locations = await _context.Location.Where(l => l.Langid == languageId).ToListAsync();
                _memoryCache.Set($"Locations-{languageId}", locations);
            }

            return locations?.FirstOrDefault(l => l.Id == locationId) ?? null;
        }

        public async Task<List<Vehiclefuellang>> GetVehicleFuelLangs()
        {
            List<Vehiclefuellang> fuelLangs = new List<Vehiclefuellang>();
            var cacheExist = _memoryCache.TryGetValue("VehicleFuelLangs", out fuelLangs);
            if (!cacheExist)
            {
                fuelLangs = await _context.Vehiclefuellang.ToListAsync();
                _memoryCache.Set("VehicleFuelLangs", fuelLangs);
            }

            return fuelLangs;
        }

        public async Task<List<Vehicletransmissionlang>> GetVehicleTransmissionLangs()
        {
            List<Vehicletransmissionlang> transmissionLangs = new List<Vehicletransmissionlang>();
            var cacheExist = _memoryCache.TryGetValue("VehicleTransmissionLangs", out transmissionLangs);
            if (!cacheExist)
            {
                transmissionLangs = await _context.Vehicletransmissionlang.ToListAsync();
                _memoryCache.Set("VehicleTransmissionLangs", transmissionLangs);
            }

            return transmissionLangs;
        }

        public async Task<bool> SetUserDetailModel(string memoryKey, UserDetailModel userDetailModel)
        {
            try
            {
                var jsonModel = JsonConvert.SerializeObject(userDetailModel);
                _memoryCache.Set(memoryKey, jsonModel);
                return true;
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@SetUserDetailModelToCache}", userDetailModel.ToJson());
                return false;
            }
        }

        public async Task<bool> SetCouponDetailModel(string memoryKey, CouponModel couponModel)
        {
            try
            {
                var jsonModel = JsonConvert.SerializeObject(couponModel);
                _memoryCache.Set(memoryKey, jsonModel);
                return true;
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error("{@SetCouponDetailModelToCache}", couponModel.ToJson());
                return false;
            }
        }

        public async Task<UserDetailModel> GetUserDetailModel(string memoryKey)
        {
            var cacheExist = _memoryCache.TryGetValue(memoryKey, out string jsonModel);
            if (cacheExist)
            {
                return JsonConvert.DeserializeObject<UserDetailModel>(jsonModel);
            }
            return Activator.CreateInstance<UserDetailModel>();
        }

        public async Task<CouponModel> GetCouponDetailModel(string memoryKey)
        {
            var cacheExist = _memoryCache.TryGetValue(memoryKey, out string jsonModel);
            if (cacheExist)
            {
                return JsonConvert.DeserializeObject<CouponModel>(jsonModel);
            }
            return Activator.CreateInstance<CouponModel>();
        }

        public async Task<List<VendorLocationCondition>> GetVendorLocationCondition(int vendorId, int pickupLocationId, int languageId)
        {
            List<VendorLocationCondition> conditions = new List<VendorLocationCondition>();
            var cacheExist = _memoryCache.TryGetValue($"VendorLocationConditions-{pickupLocationId}-{languageId}", out conditions);
            if (!cacheExist)
            {
                conditions = await _context.VendorLocationConditions.Where(c => c.LocationId == pickupLocationId && c.LanguageId == languageId).ToListAsync();
                _memoryCache.Set($"VendorLocationConditions-{pickupLocationId}-{languageId}", conditions);
            }

            return conditions?.Where(l => l.VendorId == vendorId)?.ToList() ?? new List<VendorLocationCondition>();
        }

        public async Task<bool> CheckVendorInCampaign(int vendorId, int couponId)
        {
            var couponVendors = new List<CouponVendor>();
            var cacheExists = _memoryCache.TryGetValue("CouponVendorData", out couponVendors);
            if (!cacheExists)
            {
                couponVendors = await _context.CouponVendor.ToListAsync();
                _memoryCache.Set("CouponVendorData", couponVendors);
            }
            return !couponVendors.Any(cv => cv.VendorId == vendorId && cv.CouponId == couponId);
        }

        public async Task<List<VendorLocationDeliveryType>> GetVendorLocationDeliveryTypes()
        {
            var result = new List<VendorLocationDeliveryType>();
            var cacheExists = _memoryCache.TryGetValue("VendorLocationDeliveryTypes", out result);
            if (!cacheExists)
            {
                result = await _context.VendorLocationDeliveryTypes.ToListAsync();
                _memoryCache.Set("VendorLocationDeliveryTypes", result);
            }
            return result;
        }

        public async Task<Kullanici> GetUserByEmail(string email)
        {
            var result = await _context.Kullanici.FirstOrDefaultAsync(u => u.Eposta == email);

            return result;
        }
    }
}

