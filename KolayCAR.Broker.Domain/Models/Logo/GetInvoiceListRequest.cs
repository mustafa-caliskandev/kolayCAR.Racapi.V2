namespace KolayCAR.Broker.Domain.Models.Logo
{
    public class GetInvoiceListRequest
    {
        public string BranchID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool ShowAdditionalProductInvoices { get; set; }
        public bool ShowDropInvoices { get; set; }
        public string Version { get; set; } = "1.0";
        public bool OnlyCanceledReservations { get; set; } = false;
    }
}
