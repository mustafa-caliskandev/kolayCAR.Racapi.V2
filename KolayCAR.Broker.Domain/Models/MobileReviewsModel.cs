using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models
{
    public class MobileReviewsModel
    {
        public int VendorCommentCount { get; set; }
        public double VendorScore { get; set; }
        public string VendorScoreLabel { get; set; }
        public string VendorName { get; set; }
        public string VendorLogo { get; set; }
        public string CommentLabel { get; set; }
        public List<SpecialScore> VendorSpecialScores { get; set; }
        public List<Comment> Comments { get; set; }
    }
    public class SpecialScore
    {
        public string Name { get; set; }
        public string ScoreLabel { get; set; }
        public double Score { get; set; }
    }
    public class Comment
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string CommentText { get; set; }
        public string CommentSource { get; set; }
        public string StatusDate { get; set; }
        public string ScoreLabel { get; set; }
        public double? Score { get; set; }
    }
}
