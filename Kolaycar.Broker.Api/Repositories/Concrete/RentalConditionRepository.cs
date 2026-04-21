using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Response;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace KolayCAR.Broker.API.Repositories.Concrete
{
    public class RentalConditionRepository : IRentalConditionRepository
    {
        BrokerContext _context;
        public RentalConditionRepository(BrokerContext context) 
        {
            _context = context;
        }
        public async Task<ServiceResponseBase> GetRentalConditions(int vendorId, LanguageTypes languageType)
        {
            var rentalConditions = await (from ap in _context.Additionalproduct
                                          join apv in _context.Additionalproductvendor on ap.Productid equals apv.Productid
                                          join v in _context.Vendor on apv.Vendorid equals v.Vendorid
                                          where ap.Active == true &&
                                          ap.Producttype == (int)AdditionalProductTypes.InternalService &&
                                          ap.Langid == (int)languageType + 1 &&
                                          apv.Vendorid == vendorId &&
                                          apv.Active == true
                                          select new RentalCondition
                                          {
                                              ConditionId = ap.Productid,
                                              ConditionCode = ap.Productcode,
                                              ConditionName = ap.Productname,
                                              ConditionSequence = ap.Sequence ?? 1,
                                              IconPath = ap.Iconpath,
                                          }).OrderBy(x => x.ConditionSequence).ToListAsync();

            return new ServiceResponseBase
            {
                Success = true,
                Data = rentalConditions
            };
        }
    }
}
