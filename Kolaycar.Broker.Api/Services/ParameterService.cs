using KolayCAR.Broker.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IParameterService
    {
        Task<Parametre> GetParameter(string parameterName);
        Task<IEnumerable<Parametre>> GetParameters();
        string GetParameterValue(string parameterName);
    }
    public class ParameterService : IParameterService
    {
        private readonly BrokerContext _context;
        public ParameterService(BrokerContext context)
        {
            _context = context;
        }
        public async Task<Parametre> GetParameter(string variableName)
        {
            var parameters = await GetParameters();
            return parameters.FirstOrDefault(p => p.Degisken == variableName);
        }

        public async Task<IEnumerable<Parametre>> GetParameters()
        {
            return _context.Parametre;
        }

        public string GetParameterValue(string parameterName)
        {
            return _context.Parametre.FirstOrDefault(p => p.Degisken == parameterName)?.Deger;
        }
    }
}
