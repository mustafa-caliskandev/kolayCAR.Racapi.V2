using KolayCAR.Broker.API.Repositories.Abstract;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services;

public interface IMemberService
{
    public Task<int> GetFirstMemberIdByAgencyId(int agencyId);
}

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<int> GetFirstMemberIdByAgencyId(int agencyId)
    {
        var uye = await _memberRepository.GetFirstMemberByAgencyId(agencyId);
        return uye != null ? uye.Uyeid : 0;
    }
}
