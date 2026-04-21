namespace KolayCAR.Broker.API.Models
{
    public class ReservationInvoiceAddress
    {
        public int Id { get; set; }
        public int ReservationDetailId { get; set; }

        public string Address { get; set; }
        public string Title { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string ZipCode { get; set; }

        public virtual ReservationDetail ReservationDetail { get; set; }
    }
}
