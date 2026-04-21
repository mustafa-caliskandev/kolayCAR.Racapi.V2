using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Concrete
{

    public class Repository<T> : IRepository<T> where T : class
    {
        public readonly DbContext _context;
        public readonly DbSet<T> _dbSet;

        public Repository(BrokerContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@RepositoryGetByIdAsync}", ex.ToJson());
                return null;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();

            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@RepositoryGetAllAsync}", ex.ToJson());
                return null;
            }

        }
        public async Task<IEnumerable<T>> GetAllAsync(
    Func<IQueryable<T>, IQueryable<T>> include = null)
        {
            try
            {
                IQueryable<T> query = _dbSet;

                if (include != null)
                    query = include(query);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@RepositoryGetAllAsync}", ex.ToJson());
                return null;
            }
        }


        public async Task AddAsync(T entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@RepositoryAddAsync}", ex.ToJson());
            }
        }

        public async Task AddRangeAsync(IEnumerable<T> entity)
        {
            try
            {
                await _dbSet.AddRangeAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@RepositoryAddRangeAsync}", ex.ToJson());
            }

        }
        public async Task UpdateAsync(T entity)
        {
            try
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@RepositoryUpdateAsync}", ex.ToJson());
            }

        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity != null)
                {
                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@RepositoryDeleteAsync}", ex.ToJson());
            }

        }

        public Task SaveAsync()
        {
            throw new NotImplementedException();
        }
    }
}
