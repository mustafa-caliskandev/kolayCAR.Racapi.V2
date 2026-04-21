namespace KolayCAR.Broker.Domain.Models
{
    public class ReservationPenalty
    {
        public int DifferenceHour { get; set; }
        public int PenaltyRate { get; set; }
        public float PenaltyAmount { get; set; }
        public float RefundedAmount { get; set; }
    }
}
