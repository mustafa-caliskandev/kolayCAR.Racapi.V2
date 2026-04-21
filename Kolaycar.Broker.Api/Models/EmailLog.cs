using System;

namespace KolayCAR.Broker.API.Models
{
    public class EmailLog
    {
        public int Id { get; set; }
        public DateTime LogDate { get; set; }
        public string MailType { get; set; }
        public string SenderMail { get; set; }
        public string ReceiverMail { get; set; }
        public string MailBody { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
