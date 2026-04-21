using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace KolayCAR.Broker.API.Data.Repositories
{
    public class EFRepository<T> : IEFRepository<T> where T : class
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public EFRepository(BrokerContext context)
        {
            _context = context ?? throw new ArgumentNullException("dbContext cannot be null.");
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _dbSet = context.Set<T>();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Any(predicate);
        }

        public void Delete(T entity)
        {
            if (entity.GetType().GetProperty("IsDelete") != null)
            {
                T _entity = entity;

                _entity.GetType().GetProperty("IsDelete").SetValue(_entity, true);

                this.Update(_entity);
            }
            else
            {
                var dbEntityEntry = _context.Entry(entity);

                if (dbEntityEntry.State != EntityState.Deleted)
                {
                    dbEntityEntry.State = EntityState.Deleted;
                }
                else
                {
                    _dbSet.Attach(entity);
                    _dbSet.Remove(entity);
                }
            }
        }

        public void Delete(int id)
        {
            var entity = GetById(id);
            if (entity == null) return;
            else
            {
                if (entity.GetType().GetProperty("IsDelete") != null)
                {
                    T _entity = entity;
                    _entity.GetType().GetProperty("IsDelete").SetValue(_entity, true);

                    this.Update(_entity);
                }
                else
                {
                    Delete(entity);
                }
            }
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.AsNoTracking().Where(predicate).SingleOrDefault();
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}
