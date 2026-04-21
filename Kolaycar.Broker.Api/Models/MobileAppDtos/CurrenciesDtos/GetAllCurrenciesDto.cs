namespace KolayCAR.Broker.API.Models.MobileAppDtos.CurrenciesDtos
{
    public class GetAllCurrenciesDto
    {
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyIsoCode { get; set; }
        public string Symbol { get; set; }
        public bool? Active { get; set; }
    }
}
