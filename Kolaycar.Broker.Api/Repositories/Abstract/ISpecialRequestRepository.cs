using KolayCAR.Broker.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface ISpecialRequestRepository : IRepository<SpecialRequest>
    {
        Task<IEnumerable<SpecialRequest>> GetSpecialRequestByFilter(int vendorId, int agencyId, int locationId, DateTime reservationDate, DateTime pickupDate, int rentalDuration, float vehiclePrice, int categoryId);
        Task<IEnumerable<SpecialRequest>> GetAllSpecialRequestsWithRelationsAsync();
    }
}
