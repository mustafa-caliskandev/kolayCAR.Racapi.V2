namespace KolayCAR.Broker.Domain.Models
{
    public class VendorVendor
    {
        public VendorVendor()
        {
            
        }
        public VendorVendor(int VendorId, bool Active, string VendorName)
        {
            this.VendorId = VendorId;
            this.Active = Active;
            this.VendorName = VendorName;
        }
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public bool? Active { get; set; }
        public bool HgsPackage { get; set; }
        public int? MatchedVendorId { get; set; }
        public string MatchedVendorName { get; set; }
        public bool? UnlimitedKM { get; set; }
        public bool? FlightCardMandatory { get; set; }
        public bool? PassportNumberRequired { get; set; }
        public CreditType CreditType { get; set; }
    }
}
