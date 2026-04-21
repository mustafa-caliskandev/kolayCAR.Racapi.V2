using KolayCAR.Broker.API.Data.Repositories;
using KolayCAR.Broker.API.Models;
using System;

namespace KolayCAR.Broker.API.Data.UnitOfWork
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private readonly BrokerContext _context;

        public EFUnitOfWork(BrokerContext context)
        {
            _context = context ?? throw new ArgumentNullException("dbContext cannot be null.");

            //_context.Configuration.LazyLoadingEnabled = false;
            //_context.Configuration.ValidateOnSaveEnabled = false;
            //_context.Configuration.ProxyCreationEnabled = false;
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEFRepository<T> GetRepository<T>() where T : class
        {
            return new EFRepository<T>(_context);
        }

        public int SaveChanges()
        {
            try
            {
                return _context.SaveChanges();
            }
            catch
            {
                //DbEntityValidationException 
                throw;
            }
        }
    }
}
