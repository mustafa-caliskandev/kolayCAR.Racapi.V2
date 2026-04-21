using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class SpecialRequestRepository : Repository<SpecialRequest>, ISpecialRequestRepository
    {
        public SpecialRequestRepository(BrokerContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SpecialRequest>> GetSpecialRequestByFilter(int vendorId, int agencyId, int locationId, DateTime reservationDate, DateTime pickupDate, int rentalDuration, float vehiclePrice, int categoryId)
        {
            var filteredData = await _dbSet
           .Include(e => e.Vendors)
           .Include(e => e.Agencies)
           .Include(e => e.Locations)
           .Include(e => e.Categories)
           .Include(e => e.SpecialRequestTariff)
           .Where(e => e.Vendors.Any(v => v.VendorId == vendorId) &&
                      e.Agencies.Any(a => a.AgencyId == agencyId) &&
                      e.Locations.Any(l => l.LocationId == locationId) &&
                      (e.Categories == null || e.Categories.Count == 0 || e.Categories.Any(c => c.CategoryId == categoryId)) &&
                      e.ReservationStartDate < DateTime.Now &&
                      e.ReservationEndDate > DateTime.Now &&
                      (e.MinimumDay == null || e.MinimumDay <= rentalDuration) &&
                      (e.MaximumDay == null || e.MaximumDay >= rentalDuration) &&
                      (e.PickupStartDate == null || e.PickupStartDate < pickupDate) &&
                      (e.PickupEndDate == null || e.PickupEndDate > pickupDate) &&
                      (e.ReservationStartDate == null || e.ReservationStartDate < reservationDate) &&
                      (e.ReservationEndDate == null || e.ReservationEndDate > reservationDate) &&
                      (e.MaximumAmount == null || e.MaximumAmount >= vehiclePrice) &&
                      (e.MinimumAmount == null || e.MinimumAmount <= vehiclePrice) &&
                      e.Active).ToListAsync();

            return filteredData.GroupBy(e => e.AdditionalProductId)
                .Select(g => g.OrderByDescending(x => x.Priority).FirstOrDefault())
                .ToList();

            //var filteredData = await _dbSet
            //   .Where(e =>
            //             e.Vendors.Any(v => v.VendorId == vendorId) &&
            //             e.Agencies.Any(a => a.AgencyId == agencyId) &&
            //             e.Locations.Any(l => l.LocationId == locationId) &&
            //             e.Categories.Any(c => c.CategoryId == categoryId) &&
            //             e.ReservationStartDate < DateTime.Now &&
            //             e.ReservationEndDate > DateTime.Now &&
            //             (e.MinimumDay == null || e.MinimumDay <= rentalDuration) &&
            //             (e.MaximumDay == null || e.MaximumDay >= rentalDuration) &&
            //             (e.PickupStartDate == null || e.PickupStartDate < pickupDate) &&
            //             (e.PickupEndDate == null || e.PickupEndDate > pickupDate) &&
            //             (e.ReservationStartDate == null || e.ReservationStartDate < reservationDate) &&
            //             (e.ReservationEndDate == null || e.ReservationEndDate > reservationDate) &&
            //             (e.MaximumAmount == null || e.MaximumAmount >= vehiclePrice) &&
            //             (e.MinimumAmount == null || e.MinimumAmount <= vehiclePrice) &&
            //             e.Active)
            //   .ToListAsync();

            //return filteredData.GroupBy(e => e.AdditionalProductId).Select(g => g.OrderByDescending(x => x.Priority).FirstOrDefault()).ToList();
        }

        public async Task<IEnumerable<SpecialRequest>> GetAllSpecialRequestsWithRelationsAsync()
        {
            var specialRequests = await _dbSet.Include(e => e.SpecialRequestTariff).AsNoTracking().ToListAsync();

            var vendorRelations = await _context.Set<SpecialRequestVendor>()
                .AsNoTracking()
                .Select(v => new { v.SpecialRequestId, v.VendorId })
                .ToListAsync();

            var agencyRelations = await _context.Set<SpecialRequestAgency>()
                .AsNoTracking()
                .Select(a => new { a.SpecialRequestId, a.AgencyId })
                .ToListAsync();

            var locationRelations = await _context.Set<SpecialRequestLocation>()
                .AsNoTracking()
                .Select(l => new { l.SpecialRequestId, l.LocationId })
                .ToListAsync();

            var categoryRelations = await _context.Set<SpecialRequestCategory>()
                .AsNoTracking()
                .Select(c => new { c.SpecialRequestId, c.CategoryId })
                .ToListAsync();

            var vendorLookup = vendorRelations.ToLookup(v => v.SpecialRequestId);
            var agencyLookup = agencyRelations.ToLookup(a => a.SpecialRequestId);
            var locationLookup = locationRelations.ToLookup(l => l.SpecialRequestId);
            var categoryLookup = categoryRelations.ToLookup(c => c.SpecialRequestId);

            foreach (var sr in specialRequests)
            {
                sr.Vendors = vendorLookup[sr.Id].Select(v => new SpecialRequestVendor { VendorId = v.VendorId }).ToList();
                sr.Agencies = agencyLookup[sr.Id].Select(a => new SpecialRequestAgency { AgencyId = a.AgencyId }).ToList();
                sr.Locations = locationLookup[sr.Id].Select(l => new SpecialRequestLocation { LocationId = l.LocationId }).ToList();
                sr.Categories = categoryLookup[sr.Id].Select(c => new SpecialRequestCategory { CategoryId = c.CategoryId }).ToList();
            }

            return specialRequests;
        }
    }
}
