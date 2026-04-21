using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services;

public interface IVendorLocationDeliveryTypeService
{
    Task AddVendorLocationDeliveryType();
}
public class VendorLocationDeliveryTypeService : IVendorLocationDeliveryTypeService
{
    private readonly IVendorLocationDeliveryTypeRepository _repository;
    private readonly ILocationVendorService _locationVendorService;
    public VendorLocationDeliveryTypeService(IVendorLocationDeliveryTypeRepository repository, ILocationVendorService locationVendorService)
    {
        _repository = repository;
        _locationVendorService = locationVendorService;
    }

    public async Task AddVendorLocationDeliveryType()
    {
        var praticarLV = await _locationVendorService.GetLocationVendorByVendorId(253);
        var garentadel = _repository.GetAllAsync().Result.Where(e => e.VendorId == 21).ToList();
        var deltype = new List<VendorLocationDeliveryType>();

        foreach (var item in praticarLV)
        {
            var gdl = garentadel.Where(e => e.LocationId == item.Locallocationid).ToList();
            foreach (var item2 in gdl)
            {
                deltype.Add(new VendorLocationDeliveryType
                {
                    LocationId = item2.LocationId,
                    VendorId = (int)item.Vendorid,
                    DeliveryTypeId = item2.DeliveryTypeId
                });
            }
        }

        await _repository.AddRangeAsync(deltype);
    }
}
