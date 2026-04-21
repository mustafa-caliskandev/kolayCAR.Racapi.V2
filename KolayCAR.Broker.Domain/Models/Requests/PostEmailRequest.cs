namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class PostEmailRequest
    {
        public string FromTitle { get; set; }
        public string ToMailAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ReplyTo { get; set; }
        public string TypeName { get; set; }
        public bool IsCancelMail { get; set; }
    }
}
