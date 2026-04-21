namespace KolayCAR.Broker.Domain.Models
{
    public class MailInfo
    {
        public int Port { get; set; }
        public string Host { get; set; }
        public bool EnableSSL { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FromMail { get; set; }
        public string FromTitle { get; set; }
        public string ToMailAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ReplyTo { get; set; }
    }
}
