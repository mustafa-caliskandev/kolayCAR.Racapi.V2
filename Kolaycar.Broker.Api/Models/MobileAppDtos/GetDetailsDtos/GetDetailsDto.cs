namespace KolayCAR.Broker.API.Models.MobileAppDtos.GetDetailsDtos
{
    public class GetDetailsDto
    {
        public string ReservationToken { get; set; }
        public string LanguageCode { get; set; }
        public bool IsTokenUsed { get; set; } = false;
        public TokenSearchContext SearchContext { get; set; }
    }
}
