using System;

namespace KolayCAR.Broker.API.Models.Dtos
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int LanguageId { get; set; }
        public string ProfilePicture { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Comment { get; set; }
        public string CommentSource { get; set; }
        public DateTime StatusDate { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string VendorLogo { get; set; }
        public int Score { get; set; }
        public string QuestionTitle { get; set; }
        public string Question { get; set; }
    }
}
