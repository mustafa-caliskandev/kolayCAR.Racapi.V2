namespace KolayCAR.Broker.API.Models.MobileAppDtos.RequestDtos
{
    public class GetSearchedLocationsRequest
    {
        public string SearchText { get; set; }
        public string LanguageCode { get; set; } = "tr";
        public bool IsPickup { get; set; } = false;
    }
}
