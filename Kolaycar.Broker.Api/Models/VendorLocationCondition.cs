namespace KolayCAR.Broker.API.Models
{
    public class VendorLocationCondition
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int LocationId { get; set; }
        public string Conditions { get; set; }
        public int LanguageId { get; set; }
    }
}
