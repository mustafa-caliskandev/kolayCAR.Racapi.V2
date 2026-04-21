namespace KolayCAR.Broker.API.Models.MobileAppModels
{
    public class GetAllVendorsResponse
    {
        public int Vendorid { get; set; }
        public int Vendortype { get; set; }
        public string Vendorname { get; set; }
        public bool? Active { get; set; }
        public decimal? Profitmarkup { get; set; }
        public int Rentalworkingtype { get; set; }        
        public string FoundationYear { get; set; }
    }
}
