namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class PostCancelReservationRequest : IntegrationBase
    {
        public string ReservationNumber { get; set; }
        public string CustomerEmail { get; set; }
        public string CancelNote { get; set; }
        public string LanguageCode { get; set; }
        public bool IsBrokerReservation { get; set; }
        public _penaltyStatus PenaltyStatus { get; set; }
        public int CancelReasonId { get; set; }
        public string UserId { get; set; }
        public bool IsKpanelAdmin { get; set; }
    }

    public enum _penaltyStatus
    {
        /// <summary>
        /// Ceza Hiç YOk :)
        /// </summary>
        None = 0,
        /// <summary>
        /// Var Kes Cezayı
        /// </summary>
        Applied = 1,
        /// <summary>
        /// Ceza Verme Bu bizden :)
        /// </summary>
        NotApplied = 2

    }
}
