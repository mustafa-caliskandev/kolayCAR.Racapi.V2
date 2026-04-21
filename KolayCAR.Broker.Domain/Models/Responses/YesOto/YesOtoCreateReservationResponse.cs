namespace KolayCAR.Broker.Domain.Models.Responses.YesOto
{
    public class YesOtoCreateReservationResponse
    {
        public YesOtoReservationData data { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class YesOtoReservationData
    {
        public string reservationCode { get; set; }
        public string status { get; set; }
        // Add more fields if needed based on actual response
    }
}
