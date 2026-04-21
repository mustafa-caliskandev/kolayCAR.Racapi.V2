namespace KolayCAR.Broker.Domain.Models
{
    public class InvoiceDto
    {
        public long RecId { get; set; }
        public bool IsCancelled { get; set; } = false;
        public string ReservationNumber { get; set; }
        public string EmailAddress { get; set; }
        public int AgencyId { get; set; }
    }
}
