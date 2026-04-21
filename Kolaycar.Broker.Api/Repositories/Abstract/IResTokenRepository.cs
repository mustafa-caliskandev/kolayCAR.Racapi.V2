using KolayCAR.Broker.API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Repositories.Abstract
{
    public interface IResTokenRepository : IRepository<Restoken>
    {
        Task<Restoken> GetRestokenByUniqueId(string uniqueId);
    }
}
