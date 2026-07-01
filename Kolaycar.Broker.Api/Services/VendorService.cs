using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models.Response;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CommonModels = KolayCAR.Broker.Domain.Models;
using KolayCARProvider = KolayCAR.Broker.API.Providers.KolayCAR;

namespace KolayCAR.Broker.API.Services
{
    public interface IVendorService
    {
        Task<List<CommonModels.Vendor>> GetActiveVendors(int agencyId);
        Task<IEnumerable<CommonModels.Vendor>> GetVendors(int agencyId);
        Task<IEnumerable<CommonModels.Vendor>> GetVendorsByLocationId(int agencyId, int locationId);
        Task<CommonModels.Vendor> GetVendorAsync(string apiKey, string apiPassword, string apiClientId, string secretKey, CommonModels.VendorTypes vendorType, CommonModels.Agency agency, int? subVendorId = null);
        Task<CommonModels.Vendor> GetVendorById(int vendorId);
        Task<Vendor> GetVendorByIdAll(int vendorId);
        Task<CommonModels.Vendor> GetVendorById(int vendorId, CommonModels.Agency agency, int? subVendorId = null);
        Task<IEnumerable<Vendor>> GetVendorListAsync();
        Task<List<Vendor>> GetAllActiveVendors();
        Task<bool> CheckVendorIsClosedCurrentDate(int vendorId, int locationId, DateTime pickupDate);
        Task<IEnumerable<int>> GetAgencyBasedPassiveVendors(int agencyId);
        Task<ServiceResponseBase> GetSettingsFromVendorApi(int vendorId);
        Task<List<CommonModels.VendorVendor>> GetVendorVendors(int vendorId = 0, List<CommonModels.Vendor> vendors = null);
        Task<float> GetVendorLocationFeeByVendorIdAndLocationId(int vendorId, int locationId, float totalPrice = 0);
        Task<List<Locationvendorcloseddate>> GetLocationVendorClosedDates();
        Task<List<Subvendor>> GetSubVendorList();
        Task<List<Vendor>> GetVendors();
        Task<List<CommonModels.Vendor>> GetAllVendors();
    }
    public class VendorService : IVendorService
    {
        private readonly BrokerContext _context;
        private readonly IConfigurationService _configurationService;
        private readonly ICacheService _cacheService;
        private readonly IAgencyVendorService _agencyVendorService;
        private readonly IVendorVendorService _vendorVendorService;
        private readonly IVendorRepository _vendorRepository;
        private readonly ISubVendorService _subVendorService;

        public VendorService(BrokerContext context, IConfigurationService configurationService, ICacheService cacheService, IAgencyVendorService agencyVendorService, IVendorRepository vendorRepository, IVendorVendorService vendorVendorService, ISubVendorService subVendorService)
        {
            _context = context;
            _configurationService = configurationService;
            _cacheService = cacheService;
            _agencyVendorService = agencyVendorService;
            _vendorRepository = vendorRepository;
            _vendorVendorService = vendorVendorService;
            _subVendorService = subVendorService;

        }
        public async Task<CommonModels.Vendor> GetVendorAsync(string apiKey, string apiPassword, string apiClientId, string secretKey, CommonModels.VendorTypes vendorType, CommonModels.Agency agency, int? subVendorId = null)
        {
            var vendor = await GetVendorByFilter(apiKey, apiPassword, apiClientId, secretKey, vendorType);
            if (vendor == null) return null;

            var vendorVendor = (await _vendorVendorService.GetVendorVendorList()).Where(x => x.VendorId == vendor.Vendorid).ToList();

            if (vendorVendor == null) return null;

            var vendorVendors = vendorVendor.Map();

            var agencyVendorProfitMarkup = (await _agencyVendorService.GetAgencyVendorProfitmarkupListAsync()).Where(x => x.Vendorid == vendor.Vendorid && x.Agencyid == agency.AgencyId).FirstOrDefault();

            var vendorReturn = vendor.Map(agency: agency, agencyVendorProfitMarkup: agencyVendorProfitMarkup);
            vendorReturn.VendorVendors = vendorVendors;

            return await SetProfitMarkup(vendorReturn, subVendorId);
        }

        //Sadece api sorgulama bilgilerine ulaşmak içindir!
        public async Task<CommonModels.Vendor> GetVendorById(int vendorId)
        {
            var vendor = await GetVendorListAsync();
            var vendorDb = vendor.FirstOrDefault(x => x.Vendorid == vendorId);
            return new CommonModels.Vendor
            {
                VendorType = (CommonModels.VendorTypes)vendorDb.Vendortype,
                APIBaseUrl = vendorDb.Apibaseurl,
                ApiKey = vendorDb.Apikey,
                ApiPassword = vendorDb.Apipassword,
                ApiClientId = vendorDb.Apiclientid,
                SecretKey = vendorDb.Secretkey,
                FreeCancellationHour = vendorDb.Freecancellationhour ?? 0,
                PRIORITYFEE = vendorDb.PRIORITYFEE.ToBoolNullSafe(),
                UseBrokerConfigurations = vendorDb.Usebrokerconfigurations.ToBoolNullSafe(),
                ExtraMappingActive = vendorDb.Extramappingactive.ToBoolNullSafe(),
                ProfitMarkupDailyPrice = (float)(vendorDb.Profitmarkup ?? 0),
                CountryId = vendorDb.CountryId,
                FlightNumberRequired = vendorDb.FlightNumberRequired,
                ExtraDescriptionFromVendor = vendorDb.ExtraDescriptionFromVendor ?? false,
                SendAvailabilityRequest = vendorDb.SendAvailabilityRequest ?? false,
                SendDefaultMailAddress = vendorDb.SendDefaultMailAddress ?? false,
                DeliveryTypeFromVendor = vendorDb.DeliveryTypeFromVendor ?? false,
                Address = vendorDb.Address
            };
        }

        public async Task<CommonModels.Vendor> GetVendorById(int vendorId, CommonModels.Agency agency, int? subVendorId = null)
        {
            var vendorList = await GetVendorListAsync();
            var agencyVendorProfitMarkupList = await _agencyVendorService.GetAgencyVendorProfitmarkupListAsync();
            var vendorVendorList = await _vendorVendorService.GetVendorVendorList();

            //var vendorDb = await _context.Vendor.FirstOrDefaultAsync(x => x.Vendorid == vendorId);
            var vendorDb = vendorList.FirstOrDefault(x => x.Vendorid == vendorId);
            //var agencyVendorProfitMarkup = await _context.Agencyvendorprofitmarkup.Where(x => x.Vendorid == vendorDb.Vendorid && x.Agencyid == agency.AgencyId).FirstOrDefaultAsync();
            var agencyVendorProfitMarkup = agencyVendorProfitMarkupList.Where(x => x.Vendorid == vendorDb.Vendorid && x.Agencyid == agency.AgencyId).FirstOrDefault();

            var vendor = vendorDb.Map(encrypt: false, agency: agency, agencyVendorProfitMarkup: agencyVendorProfitMarkup);
            //var vendorVendors = await _context.VendorVendors.Where(x => x.VENDORID == vendorDb.Vendorid).ToListAsync();
            var vendorVendors = vendorVendorList.Where(x => x.VendorId == vendorDb.Vendorid).ToList();
            vendor.VendorVendors = vendorVendors.Map();
            return await SetProfitMarkup(vendor, subVendorId);
        }

        private async Task<CommonModels.Vendor> SetProfitMarkup(CommonModels.Vendor vendor, int? subVendorId)
        {
            if (vendor != null && subVendorId != null)
            {
                //var subVendorDb = await _context.Subvendor.Where(x => x.Vendorid == vendor.VendorId && x.Subvendorid == subVendorId).FirstOrDefaultAsync();
                var subVendorDb = (await _subVendorService.GetAllAsync()).Where(x => x.Vendorid == vendor.VendorId && x.Subvendorid == subVendorId).FirstOrDefault();
                var subVendor = subVendorDb.Map();
                vendor = VendorHelper.SetVendorProfitMarkup(vendor, subVendor);
            }

            return vendor;
        }

        public async Task<List<CommonModels.Vendor>> GetActiveVendors(int agencyId)
        {
            var sqlParameters = new SqlParameter[] {
                new SqlParameter
                {
                    ParameterName = "@OPERATIONID",
                    SqlDbType = SqlDbType.Int,
                    Value = (int)CommonModels.AgencyOperationTypes.GetVendors
                }
            };

            var vendors = await _context.Vendor.FromSqlRaw("EXECUTE SP_AGENCY_OPERATIONS " +
                "@OPERATIONID = @OPERATIONID", sqlParameters).ToListAsync();

            var agencyBasedPassiveVendors = await GetAgencyBasedPassiveVendors(agencyId);
            var mappedVendors = vendors.Map().Where(x => x.Active == true && !agencyBasedPassiveVendors.Contains(x.VendorId)).ToList();
            if (mappedVendors != null && mappedVendors.Count > 0)
            {
                var configurations = await _configurationService.GetConfigurations();
                mappedVendors.ForEach(x => x.Logo = $"{configurations.PortalOwnerDomain}{x.Logo}");
            }

            return mappedVendors;
        }

        public async Task<IEnumerable<CommonModels.Vendor>> GetVendors(int agencyId)
        {
            var sqlParameters = new SqlParameter[] {
                new SqlParameter
                {
                    ParameterName = "@OPERATIONID",
                    SqlDbType = SqlDbType.Int,
                    Value = (int)CommonModels.AgencyOperationTypes.GetVendors
                }
            };

            var vendors = await _context.Vendor.FromSqlRaw("EXECUTE SP_AGENCY_OPERATIONS " +
                "@OPERATIONID = @OPERATIONID", sqlParameters).ToListAsync();

            var agencyBasedPassiveVendors = await GetAgencyBasedPassiveVendors(agencyId);
            var dbAgency = await _context.Agency.Where(x => x.Agencyid == agencyId && x.Active == true).FirstOrDefaultAsync();
            var agency = dbAgency.Map();
            var agencyVendorProfitMarkupList = await _context.Agencyvendorprofitmarkup.Where(x => x.Agencyid == agency.AgencyId).ToListAsync();
            return vendors.Map(agency: agency, agencyVendorProfitMarkupList: agencyVendorProfitMarkupList).Where(x => x.Active == true && !agencyBasedPassiveVendors.Contains(x.VendorId));
        }

        public async Task<bool> CheckVendorIsClosedCurrentDate(int vendorId, int locationId, DateTime pickupDate)
        {
            var locationVendorClosedDate = new List<Locationvendorcloseddate>();
            if (CacheSettings.UseCache)
            {
                var locationVendorClosedDateList = await GetLocationVendorClosedDates();
                locationVendorClosedDate = locationVendorClosedDateList.Where(x => x.Vendorid == vendorId && x.Locationid == locationId).ToList();
            }
            else
            {
                locationVendorClosedDate = await _context.Locationvendorcloseddate.Where(x => x.Vendorid == vendorId && x.Locationid == locationId).ToListAsync();
            }
            return ReservationHelper.CheckClosedVendor(locationVendorClosedDate, pickupDate);
        }

        public async Task<IEnumerable<CommonModels.Vendor>> GetVendorsByLocationId(int agencyId, int locationId)
        {
            var sqlParameters = new SqlParameter[] {
                new SqlParameter
                {
                    ParameterName = "@OPERATIONID",
                    SqlDbType = SqlDbType.Int,
                    Value = (int)CommonModels.AgencyOperationTypes.GetVendorsByLocationId
                },
                new SqlParameter
                {
                    ParameterName = "@PICKUPLOCATIONID",
                    SqlDbType = SqlDbType.Int,
                    Value = locationId
                }
                };

            var vendors = await _context.Vendor.FromSqlRaw("EXECUTE SP_AGENCY_OPERATIONS " +
                "@OPERATIONID = @OPERATIONID," +
                "@PICKUPLOCATIONID = @PICKUPLOCATIONID", sqlParameters).ToListAsync();

            var agencyBasedPassiveVendors = await GetAgencyBasedPassiveVendors(agencyId);
            var dbAgency = await _context.Agency.Where(x => x.Agencyid == agencyId && x.Active == true).FirstOrDefaultAsync();
            var agency = dbAgency.Map();
            var agencyVendorProfitMarkupList = await _context.Agencyvendorprofitmarkup.Where(x => x.Agencyid == agency.AgencyId).ToListAsync();
            var vendorList = vendors.Map(agency: agency, agencyVendorProfitMarkupList: agencyVendorProfitMarkupList).Where(x => x.Active == true && !agencyBasedPassiveVendors.Contains(x.VendorId));
            return vendorList;
        }

        public async Task<IEnumerable<int>> GetAgencyBasedPassiveVendors(int agencyId) => await _context.Agencyvendor.Where(x => x.Agencyid == agencyId).Select(x => x.Vendorid).ToListAsync();

        public async Task<ServiceResponseBase> GetSettingsFromVendorApi(int vendorId)
        {
            var vendor = await GetVendorById(vendorId);
            if (vendor != null)
            {
                KolayCAR.Broker.API.Providers.ISettingsProvider settingsProvider;
                //(CommonModels.VendorTypes)vendor.Vendortype
                switch (vendor.VendorType)
                {
                    default:
                        return new ServiceResponseBase
                        {
                            Success = false,
                            Message = "vendorType parametresine uygun tedarikçi bulunamadı!",
                            ServiceCode = null,
                            ServiceMessage = null
                        };
                    case KolayCAR.Broker.Domain.Models.VendorTypes.KolayCAR:
                        settingsProvider = new KolayCARProvider.SettingsProvider(vendor);
                        break;
                }

                //Full credit ayarı için geliştirildi.
                var serviceResponse = await settingsProvider.GetSettingsFromVendorApi(vendor);
                if (serviceResponse != null && serviceResponse.Success)
                    return serviceResponse;
            }

            return new ServiceResponseBase
            {
                Success = false,
                Message = "Sonuç bulunamadı.",
                ServiceCode = null,
                ServiceMessage = null
            };
        }

        public async Task<List<CommonModels.VendorVendor>> GetVendorVendors(int vendorId = 0, List<CommonModels.Vendor> vendors = null)
        {
            try
            {
                List<VendorVendor> vendorList;
                if (vendorId != 0)//id ile geldiyse o tedarikçinin tedarikçilerini getirir.
                {
                    vendorList = await _context.VendorVendors.Where(x => x.VendorId == vendorId).ToListAsync();

                    return vendorList.Map();
                }
                else//id == 0 durumunda tüm tabloyu çeker
                {
                    var compVendorId = vendors.Where(x => x.VendorType == CommonModels.VendorTypes.Yolcu360).FirstOrDefault()?.VendorId.ToIntNullSafe();
                    vendors.RemoveAll(x => x.VendorType == CommonModels.VendorTypes.Yolcu360);
                    vendorList = await _context.VendorVendors.Where(x => x.Active == true && x.VendorId == compVendorId).ToListAsync();
                    var matchedList = vendorList.Select(v => v.MatchedVendorName).ToList();
                    vendorList.RemoveAll(vl => vl.MatchedVendorId == null || vl.MatchedVendorId == 0 || !vendors.Any(v => v.VendorId == vl.MatchedVendorId));

                    return vendorList.Map();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Vendor>> GetVendors() //MobilApp
        {
            return await _context.Vendor.ToListAsync();
        }

        public async Task<float> GetVendorLocationFeeByVendorIdAndLocationId(int vendorId, int locationId, float totalPrice = 0) //MobilApp
        {
            var locationFee =
                await _context.VendorLocationFees.Where(v => v.VendorId == vendorId && v.LocationId == locationId).FirstOrDefaultAsync();
            if (locationFee?.Id > 0)
            {
                if ((locationFee.FeePercentage ?? 0) > 0)
                {
                    return (float)(locationFee.FeePercentage * totalPrice) / 100;
                }

                return (float)(locationFee.FeeAmount ?? 0);
            }
            return 0;
        }

        public async Task<Vendor> GetVendorByIdAll(int vendorId) //MobilApp
        {
            return await _context.Vendor.FirstOrDefaultAsync(v => v.Vendorid == vendorId);
        }

        public async Task<List<Vendor>> GetAllActiveVendors() //MobilApp
        {
            return await _context.Vendor.Where(v => v.Active == true).ToListAsync();
        }

        public async Task<List<Locationvendorcloseddate>> GetLocationVendorClosedDates()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{CacheSettings.LocationKey} - LocationVendorClosedDateList", () => _context.Locationvendorcloseddate.ToListAsync());

            return await _context.Locationvendorcloseddate.ToListAsync();
        }

        public async Task<List<Subvendor>> GetSubVendorList()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{CacheSettings.VendorKey} - SubVendorList", () => _context.Subvendor.ToListAsync());

            return await _context.Subvendor.ToListAsync();
        }

        private async Task<Vendor> GetVendorByFilter(string apiKey, string apiPassword, string apiClientId, string secretKey, CommonModels.VendorTypes vendorType)
        {
            if (CacheSettings.UseCache)
            {
                var vendorList = await GetVendorListAsync();
                return vendorList.FirstOrDefault(x => x.Apikey == apiKey
                                && x.Apipassword == apiPassword
                                && x.Vendortype == (int)vendorType
                                && x.Active == true
                                && (string.IsNullOrEmpty(apiClientId) || x.Apiclientid == apiClientId)
                                && (string.IsNullOrEmpty(secretKey) || x.Secretkey == secretKey));
            }

            return await _vendorRepository.GetVendorAsync(apiKey, apiPassword, apiClientId, secretKey, vendorType);
        }
        public async Task<IEnumerable<Vendor>> GetVendorListAsync()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync($"{CacheSettings.VendorKey}-VendorList", _vendorRepository.GetAllAsync);

            return await _vendorRepository.GetAllAsync();
        }

        public async Task<List<CommonModels.Vendor>> GetAllVendors()
        {
            var vendors = await _context.Vendor.ToListAsync();

            return vendors.Select(x => new CommonModels.Vendor
            {
                Active = x.Active.ToBoolNullSafe(),
                VendorId = x.Vendorid,
                VendorName = x.Vendorname,
                VendorOrder = x.VendorOrder
            }).ToList();
        }
    }
}
