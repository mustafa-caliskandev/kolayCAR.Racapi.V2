using KolayCAR.Broker.Domain.Models.Response;
using CommonModels = KolayCAR.Broker.Domain.Models;

namespace KolayCAR.Broker.API.Mappers.KolayCAR
{
    public static class AgencyMapper
    {
        public static CommonModels.Agency Map(this AGENCY agency) =>
            agency != null ? new CommonModels.Agency
            {
                AgencyId = agency.PORTALAGENCYID,
                Active = agency.ACTIVE,
                AgencyName = agency.AGENCYNAME,
                PhoneNumber = agency.TELEPHONE,
                Email = agency.EMAIL,
                Address = agency.ADDRESS,
                MobilePhoneNumber = agency.GSM,
                AuthorizedPersonName = agency.AUTHORIZED,
                AgencyApiKey = agency.APIKEY,
                AgencyApiPassword = agency.PASSWORD,
                AgencyCommissionAmount = agency.AGENCYCOMMISSION,
                AgencyCommissionType = (CommonModels.AgencyCommissionTypes)agency.AGENCYCOMMISSIONTYPE
            }
            : null;
    }
}
