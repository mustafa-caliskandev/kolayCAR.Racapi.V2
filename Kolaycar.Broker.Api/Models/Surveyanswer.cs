namespace KolayCAR.Broker.API.Models
{
    public partial class Surveyanswer
    {
        public int Id { get; set; }
        public int Surveyid { get; set; }
        public int Questionid { get; set; }
        public int Score { get; set; }
    }
}
