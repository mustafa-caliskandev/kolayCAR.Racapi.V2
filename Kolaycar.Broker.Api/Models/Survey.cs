using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Survey
    {
        public int Id { get; set; }
        public long Reservationid { get; set; }
        public int? Adminid { get; set; }
        public string Comment { get; set; }
        public bool Showonwebsite { get; set; }
        public bool Sendemail { get; set; }
        public bool Sendsms { get; set; }
        public DateTime Senddate { get; set; }
        public int Languageid { get; set; }
        public int? Surveyposttypeid { get; set; }
        public string Email { get; set; }
    }
}
