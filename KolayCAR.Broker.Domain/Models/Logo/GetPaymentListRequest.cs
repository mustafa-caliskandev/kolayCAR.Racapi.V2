namespace KolayCAR.Broker.Domain.Models.Logo
{
    public class GetPaymentListRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string BranchID { get; set; }
        public bool CancelledOnly { get; set; } = false;
    }
}
