using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Mappers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{

    public interface IProfitMarkupService
    {
        public Task<IEnumerable<API.Models.ProfitMarkup>> GetProfitMarkupList();
        public Task<List<Domain.Models.ProfitMarkup>> GetProfitMarkupFilter(int vendorID, long agencyId, int locationId, DateTime pickupDate, DateTime returnDate, int rentalDuration);
    }

    public class ProfitMarkupService : IProfitMarkupService
    {
        private readonly BrokerContext _context;
        private readonly ICacheService _cacheService;
        private readonly string ProfitMarkupsCacheKey = "ProfitMarkupsCacheKey";

        public ProfitMarkupService(BrokerContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        private IQueryable<API.Models.ProfitMarkup> GetProfitMarkupQuery()
        {
            return _context.ProfitMarkups
                .Include(p => p.ProfitMarkupAgencies)
                .Include(p => p.ProfitMarkupLocations)
                .Include(p => p.ProfitMarkupVendors);
        }

        public async Task<IEnumerable<ProfitMarkup>> GetProfitMarkupQueryAsync()
        {
            var profitMarkups = await _context.Set<ProfitMarkup>().AsNoTracking().ToListAsync();

            var profitMarkupAgencies = await _context.Set<ProfitMarkupAgency>()
                .AsNoTracking()
                .Select(v => new { v.ProfitMarkupId, v.AgencyId })
                .ToListAsync();

            var profitMarkupLocations = await _context.Set<ProfitMarkupLocation>()
                .AsNoTracking()
                .Select(a => new { a.ProfitMarkupId, a.LocationId })
                .ToListAsync();

            var profitMarkupVendor = await _context.Set<ProfitMarkupVendor>()
                .AsNoTracking()
                .Select(l => new { l.ProfitMarkupId, l.VendorId })
                .ToListAsync();

            var profitMarkupAgenciesLookup = profitMarkupAgencies.ToLookup(v => v.ProfitMarkupId);
            var profitMarkupLocationsLookup = profitMarkupLocations.ToLookup(a => a.ProfitMarkupId);
            var profitMarkupVendorLookup = profitMarkupVendor.ToLookup(l => l.ProfitMarkupId);

            foreach (var sr in profitMarkups)
            {
                sr.ProfitMarkupVendors = profitMarkupVendorLookup[sr.Id].Select(v => new ProfitMarkupVendor { VendorId = v.VendorId }).ToList();
                sr.ProfitMarkupAgencies = profitMarkupAgenciesLookup[sr.Id].Select(a => new ProfitMarkupAgency { AgencyId = a.AgencyId }).ToList();
                sr.ProfitMarkupLocations = profitMarkupLocationsLookup[sr.Id].Select(l => new ProfitMarkupLocation { LocationId = l.LocationId }).ToList();
            }

            return profitMarkups;
        }

        public async Task<IEnumerable<API.Models.ProfitMarkup>> GetProfitMarkupList()
        {
            if (CacheSettings.UseCache)
                return await _cacheService.GetOrCreateAsync(ProfitMarkupsCacheKey, () =>
                      GetProfitMarkupQueryAsync());

            return await GetProfitMarkupQueryAsync();
        }
        public async Task<List<Domain.Models.ProfitMarkup>> GetProfitMarkupFilter(int vendorID, long agencyId, int locationId, DateTime pickupDate, DateTime returnDate, int rentalDuration)
        {
            var profitMarkupList = await GetProfitMarkupList();
            var filter = new ProfitMarkupFilter(vendorID, agencyId, locationId, pickupDate, returnDate, rentalDuration);

            var filteredProfitMarkups = filter.ApplyFilters(profitMarkupList)
                                              .OrderBy(pm => pm.Priority ?? int.MaxValue)
                                              .ToList();

            return filteredProfitMarkups.Map();
        }
    }


    public class ProfitMarkupFilter
    {
        private readonly int _vendorID;
        private readonly long _agencyId;
        private readonly int _locationId;
        private readonly DateTime _pickupDate;
        private readonly DateTime _returnDate;
        private readonly int _rentalDuration;
        private readonly DateTime _currentDate = DateTime.Now;

        public ProfitMarkupFilter(int vendorID, long agencyId, int locationId, DateTime pickupDate, DateTime returnDate, int rentalDuration)
        {
            _vendorID = vendorID;
            _agencyId = agencyId;
            _locationId = locationId;
            _pickupDate = pickupDate;
            _returnDate = returnDate;
            _rentalDuration = rentalDuration;
        }

        public IEnumerable<API.Models.ProfitMarkup> ApplyFilters(IEnumerable<API.Models.ProfitMarkup> profitMarkups)
        {
            return profitMarkups.Where(pm =>
                FilterByVendor(pm) &&
                FilterByAgency(pm) &&
                FilterByLocation(pm) &&
                FilterByPickupDate(pm) &&
                FilterByReservationDate(pm) &&
                FilterByRentalDuration(pm));
        }

        private bool FilterByVendor(API.Models.ProfitMarkup pm)
        {
            return pm.ProfitMarkupVendors.Any(v => v.VendorId == _vendorID);
        }

        private bool FilterByAgency(API.Models.ProfitMarkup pm)
        {
            return pm.ProfitMarkupAgencies.Count == 0 || pm.ProfitMarkupAgencies.Any(a => a.AgencyId == _agencyId);
        }

        private bool FilterByLocation(API.Models.ProfitMarkup pm)
        {
            return pm.ProfitMarkupLocations.Count == 0 || pm.ProfitMarkupLocations.Any(l => l.LocationId == _locationId);
        }

        private bool FilterByPickupDate(API.Models.ProfitMarkup pm)
        {
            return (!pm.PickupStartDate.HasValue && !pm.PickupEndDate.HasValue) ||
                   (pm.PickupStartDate.Value.Date <= _pickupDate.Date && pm.PickupEndDate.Value.Date >= _returnDate.Date);
        }

        private bool FilterByReservationDate(API.Models.ProfitMarkup pm)
        {
            return (!pm.ReservationStartDate.HasValue && !pm.ReservationEndDate.HasValue) ||
                   (pm.ReservationStartDate.Value.Date <= _currentDate.Date && pm.ReservationEndDate.Value.Date >= _currentDate.Date);
        }

        private bool FilterByRentalDuration(API.Models.ProfitMarkup pm)
        {
            return (!pm.MinimumDay.HasValue && !pm.MaximumDay.HasValue) ||
                   (pm.MinimumDay <= _rentalDuration && pm.MaximumDay >= _rentalDuration);
        }
    }
}
