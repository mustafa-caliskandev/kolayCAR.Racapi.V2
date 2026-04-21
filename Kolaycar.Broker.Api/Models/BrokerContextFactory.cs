using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace KolayCAR.Broker.API.Models
{
    public class BrokerContextFactory : IDesignTimeDbContextFactory<BrokerContext>
    {
        public BrokerContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BrokerContext>();
            // Using the connection string found in BrokerContext.cs for design-time operations
            optionsBuilder.UseSqlServer("Data Source=broker.kolaycar.com,2727;Initial Catalog=miniyoldb_broker;User ID=miniyoluser;Password=Qwqt593&");

            return new BrokerContext(optionsBuilder.Options);
        }
    }
}
