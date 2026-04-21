using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.API.Services.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IBultenLogService : IService<BultenLog> 
    {
        public Task SaveLog(BultenLog bultenLog);
    }
    public class BultenLogService : IBultenLogService
    {
        private readonly IBultenLogRepository _repository;
        public BultenLogService(IBultenLogRepository repository)
        {
            _repository = repository;
        }
        public Task<IEnumerable<BultenLog>> GetAllAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task SaveLog(BultenLog bultenLog)
        {
            await _repository.AddAsync(bultenLog);
        }
    }
}
