using System;

namespace KolayCAR.Broker.API.Models
{
    public partial class Surveystatus
    {
        public int Id { get; set; }
        public int Surveyid { get; set; }
        public int Surveystatustypeid { get; set; }
        public DateTime Statusdate { get; set; }
    }
}
