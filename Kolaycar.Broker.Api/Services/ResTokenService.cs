using KolayCAR.Broker.API.Helpers;
using KolayCAR.Broker.API.Models;
using KolayCAR.Broker.API.Repositories.Abstract;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IResTokenService
    {
        Task AddRestokenList(IEnumerable<Restoken> restokenList);
        Task<ReservationToken> GetReservationTokenByUniqueId(string uniqueId);
        Task<Restoken> GetRestokenByUniqueId(string uniqueId);
        Task UpdateResToken(string uniqueId, ReservationToken resToken);
    }
    public class ResTokenService : IResTokenService
    {
        private readonly IResTokenRepository _resTokenRepository;
        public ResTokenService(IResTokenRepository resTokenRepository)
        {
            _resTokenRepository = resTokenRepository;
        }
        public async Task AddRestokenList(IEnumerable<Restoken> restokenList)
        {
            await _resTokenRepository.AddRangeAsync(restokenList);
        }

        public async Task<ReservationToken> GetReservationTokenByUniqueId(string uniqueId)
        {
            try
            {
                var resToken = await _resTokenRepository.GetRestokenByUniqueId(uniqueId);

                if (resToken is null)
                    return null;

                return JsonConvert.DeserializeObject<ReservationToken>(ObjectHelper.DecompressToString(resToken.Token));
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@GetReservationTokenByUniqueId}", $"{uniqueId} - {ex.ToJson()}");
                return null;
            }
        }
        public async Task<Restoken> GetRestokenByUniqueId(string uniqueId)
        {
            return await _resTokenRepository.GetRestokenByUniqueId(uniqueId);
        }
        public async Task UpdateResToken(string uniqueId, ReservationToken reservationToken)
        {
            var token = await GetRestokenByUniqueId(uniqueId);
            token.Token = ObjectHelper.CompressString(JsonConvert.SerializeObject(reservationToken));
            await _resTokenRepository.UpdateAsync(token);
        }
    }
}