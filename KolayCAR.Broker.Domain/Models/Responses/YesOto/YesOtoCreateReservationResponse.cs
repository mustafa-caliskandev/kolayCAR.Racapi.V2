namespace KolayCAR.Broker.Domain.Models.Responses.YesOto
{
    public class YesOtoCreateReservationResponse
    {
        public YesOtoReservationData data { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public string reservationId { get; set; }
        public string approvalNumber { get; set; }
        public string reservationCode { get; set; }
        public string status { get; set; }
    }

    public class YesOtoReservationData
    {
        public string reservationId { get; set; }
        public string approvalNumber { get; set; }
        public string reservationCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
    }
}
