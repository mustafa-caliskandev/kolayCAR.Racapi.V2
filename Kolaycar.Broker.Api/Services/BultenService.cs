using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Domain.Models.Requests;
using KolayCAR.Broker.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IBultenService : IService<Bulten>
    {
        public Task CheckContactPermission(PostReservationRequest request);
        public Task CheckContactPermission(Domain.Models.Requests.PostReservationRequestV2 request);
    }
    public class BultenService : IBultenService
    {
        private readonly IBultenRepository _repository;
        private readonly IBultenLogService _bultenLogService;
        public BultenService(IBultenRepository repository, IBultenLogService bultenLogService)
        {
            _repository = repository;
            _bultenLogService = bultenLogService;
        }
        public async Task<IEnumerable<Bulten>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task CheckContactPermission(PostReservationRequest request)
        {
            try
            {
                var bulten = await _repository.GetByEmail(request.CustomerEmail);

                if (bulten != null && bulten?.ContactPermission != request.ContactPermission)
                {
                    bulten.ContactPermission = request.ContactPermission;
                    await _repository.UpdateAsync(bulten);
                    await _bultenLogService.SaveLog(new BultenLog
                    {
                        AgencyId = (int)request.AgencyId,
                        Date = DateTime.Now,
                        ContactPermission = (bool)request.ContactPermission,
                        Email = request.CustomerEmail,
                        IP = request.CustomerIPAddress,
                        Description = (bool)request.ContactPermission ? "İletişim izni verildi!" : "İletişim izni kaldırıldı!"
                    });
                }
                if (bulten == null && (bool)request.ContactPermission)
                {
                    await _repository.AddAsync(new Bulten
                    {
                        Acenteid = (int)request.AgencyId,
                        ContactPermission = request.ContactPermission,
                        Email = request.CustomerEmail,
                        Ip = request.CustomerIPAddress,
                        Tarih = DateTime.Now
                    });
                    await _bultenLogService.SaveLog(new BultenLog
                    {
                        AgencyId = (int)request.AgencyId,
                        Date = DateTime.Now,
                        ContactPermission = (bool)request.ContactPermission,
                        Email = request.CustomerEmail,
                        IP = request.CustomerIPAddress,
                        Description = "İletişim izni verildi!"
                    });
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@CheckContactPermissionError}", ex.ToJson());
            }
           
        }
        public async Task CheckContactPermission(Domain.Models.Requests.PostReservationRequestV2 request)
        {
            try
            {
                var bulten = await _repository.GetByEmail(request.Customer.Email);

                if (bulten != null && bulten?.ContactPermission != request.ContactPermission)
                {
                    bulten.ContactPermission = request.ContactPermission;
                    await _repository.UpdateAsync(bulten);
                    await _bultenLogService.SaveLog(new BultenLog
                    {
                        AgencyId = (int)request.Agency.AgencyId,
                        Date = DateTime.Now,
                        ContactPermission = (bool)request.ContactPermission,
                        Email = request.Customer.Email,
                        IP = request.Customer.IPAddress,
                        Description = (bool)request.ContactPermission ? "İletişim izni verildi!" : "İletişim izni kaldırıldı!"
                    });
                }
                if (bulten == null && request.ContactPermission == true)
                {
                    await _repository.AddAsync(new Bulten
                    {
                        Acenteid = (int)request.Agency.AgencyId,
                        ContactPermission = request.ContactPermission,
                        Email = request.Customer.Email,
                        Ip = request.Customer.IPAddress,
                        Tarih = DateTime.Now
                    });
                    await _bultenLogService.SaveLog(new BultenLog
                    {
                        AgencyId = (int)request.Agency.AgencyId,
                        Date = DateTime.Now,
                        ContactPermission = (bool)request.ContactPermission,
                        Email = request.Customer.Email,
                        IP = request.Customer.IPAddress,
                        Description = "İletişim izni verildi!"
                    });
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@CheckContactPermissionError}", ex.Message);
            }

        }
    }
}
