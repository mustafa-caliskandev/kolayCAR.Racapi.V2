namespace KolayCAR.Broker.API.Models
{
    public class SpecialRequestAgency
    {
        public int Id { get; set; }
        public int SpecialRequestId { get; set; }
        public int AgencyId { get; set; }
        public SpecialRequest SpecialRequest { get; set; }
        public Agency Agency { get; set; }
    }
}
