namespace KolayCAR.Broker.API.Models.MobileAppDtos.RequestDtos
{
    public class GetContractsRequestDto
    {
        public string LanguageCode { get; set; }
        public string ContractType { get; set; }
        public string ReservationToken { get; set; }
    }
}
