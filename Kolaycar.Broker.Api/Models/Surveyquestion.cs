namespace KolayCAR.Broker.API.Models
{
    public partial class Surveyquestion
    {
        public int SurveyQuestionId { get; set; }
        public int Id { get; set; }
        public int Languageid { get; set; }
        public string Question { get; set; }
        public string QuestionTitle { get; set; }
    }
}
