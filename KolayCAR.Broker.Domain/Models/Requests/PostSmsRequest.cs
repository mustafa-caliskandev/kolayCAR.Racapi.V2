namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class PostSmsRequest
    {
        public string Content { get; set; }
        public string PhoneNumber { get; set; }
        public LanguageTypes LanguageType { get; set; }
    }
}
