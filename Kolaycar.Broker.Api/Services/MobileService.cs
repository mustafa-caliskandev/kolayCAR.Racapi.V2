using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels;
using KolayCAR.Broker.API.Models.MobileAppModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IMobileService
    {
        Task<List<MobileVehicleListFilter>> GetMobileVehicleListFilters(int languageId);
        Task<MobileAppSetting> GetMobileAppSettingByParameterName(string parameter);
        Task<List<MobileAppSetting>> GetMobileAppSettings();
        Task<List<MobileVehicleSortingOption>> GetMobileVehicleSortingOptions(int languageId);
        Task<List<MobileVehicleBadge>> GetMobileVehicleBadges(int languageId);
        Task<List<MobileVehicleListFastFilter>> GetMobileVehicleListFastFilters(int languageId);
        Task<List<MobileVehicleDetail>> GetMobileVehicleDetails();
        Task<List<MobileVehicleFeature>> GetMobileVehicleFeatures();
        Task<List<ReservationFindeksDetail>> GetReservationFindeksDetails(string token, string tckn);
        Task<Tuple<bool, string>> AddFindeksMobileUrl(MobileFindeksUrl entity);
    }
    public class MobileService : IMobileService
    {
        public readonly BrokerContext _context;
        public MobileService(
            BrokerContext context
            )
        {
            _context = context;
        }

        public async Task<Tuple<bool, string>> AddFindeksMobileUrl(MobileFindeksUrl entity)
        {
            try
            {
                await _context.MobileFindeksUrl.AddAsync(entity);
                _context.SaveChanges();
                return new Tuple<bool, string>(true, "");
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        public async Task<MobileAppSetting> GetMobileAppSettingByParameterName(string parameter)
        {
            return _context.MobileAppSettings.FirstOrDefault(p => p.Parameter == parameter);
        }

        public async Task<List<MobileAppSetting>> GetMobileAppSettings()
        {
            return await _context.MobileAppSettings.ToListAsync();
        }

        public async Task<List<MobileVehicleBadge>> GetMobileVehicleBadges(int languageId)
        {
            return await _context.MobileVehicleBadges
                     .Where(vb => vb.Active == true && vb.LanguageId == languageId)
                     .ToListAsync();
        }

        public async Task<List<MobileVehicleDetail>> GetMobileVehicleDetails()
        {
            return await _context.MobileVehicleDetails.Where(d => d.Active ?? false).ToListAsync();
        }

        public async Task<List<MobileVehicleFeature>> GetMobileVehicleFeatures()
        {
            return await _context.MobileVehicleFeatures.Where(f => f.Active ?? false).ToListAsync();
        }

        public async Task<List<MobileVehicleListFastFilter>> GetMobileVehicleListFastFilters(int languageId)
        {
            return await _context.MobileVehicleListFastFilters
                    .Where(vf => vf.Active ?? false && vf.LanguageId == languageId)
                    .OrderBy(vf => vf.Order)
                    .ToListAsync();
        }

        public async Task<List<MobileVehicleListFilter>> GetMobileVehicleListFilters(int languageId)
        {
            return await _context.MobileVehicleListFilters
                    .Where(vf => vf.Active == true && vf.LanguageId == languageId)
                    .OrderBy(vf => vf.Order)
                    .ToListAsync();
        }

        public async Task<List<MobileVehicleSortingOption>> GetMobileVehicleSortingOptions(int languageId)
        {
            return await _context.MobileVehicleSortingOptions
                    .Where(so => so.Active == true && so.LanguageId == languageId)
                    .OrderBy(so => so.Order)
                    .ToListAsync();
        }

        public async Task<List<ReservationFindeksDetail>> GetReservationFindeksDetails(string token, string tckn)
        {
            return await _context.ReservationFindeksDetail
                                .Where(d => d.ReservationToken == token && d.IdentityNumber == tckn)
                                .ToListAsync();
        }
    }
}
