namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class GetSummaryRequest : ReservationStepsBase
    {
        public string ExtraList { get; set; }
        public float PaidAmount { get; set; }
        public bool HighAmountDiscountActive { get; set; }
    }
}
