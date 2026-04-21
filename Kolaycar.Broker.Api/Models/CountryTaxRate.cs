namespace KolayCAR.Broker.API.Models
{
    public class CountryTaxRate
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public decimal TaxRate { get; set; }
    }
}
