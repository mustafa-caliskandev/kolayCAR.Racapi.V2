namespace KolayCAR.Broker.Domain.Models
{
    class ReservationSurvey
    {
        public int Id { get; set; }
        public long Reservationid { get; set; }
    }

    public enum SurveyStatusTypes
    {
        EmailSent = 1,
        EmailClicked = 2,
        Completed = 3
    }
}
