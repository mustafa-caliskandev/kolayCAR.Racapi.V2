namespace KolayCAR.Broker.API.Models
{
    public class SpecialRequestLocation
    {
        public int Id { get; set; }
        public int SpecialRequestId { get; set; }
        public int LocationId { get; set; }
        public SpecialRequest SpecialRequest { get; set; }
        //public Location Location { get; set; }
    }
}
