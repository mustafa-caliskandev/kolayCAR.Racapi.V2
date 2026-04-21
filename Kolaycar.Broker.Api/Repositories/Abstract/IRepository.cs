using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(
    Func<IQueryable<T>, IQueryable<T>> include = null);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task SaveAsync();
    }
}
