using System;

namespace KolayCAR.Broker.API.Models
{
    public class ExternalComment
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int VendorId { get; set; }
        public int LanguageId { get; set; }

        public decimal? Score { get; set; }

        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public string Comment { get; set; }
        public string Source { get; set; }

        public DateTime? CommentDate { get; set; }

        public bool? ShowOnWebsite { get; set; }
    }
}
