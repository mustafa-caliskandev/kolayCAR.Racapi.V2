using KolayCAR.Broker.API.Data.Repositories;
using System;

namespace KolayCAR.Broker.API.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IEFRepository<T> GetRepository<T>() where T : class;
        int SaveChanges();
    }
}
