namespace KolayCAR.Broker.Domain.Models
{
    public class BrokerLogModel
    {
        public BrokerLogModel(string LogKey, string Content, BrokerLogTypes LogType)
        {
            this.LogKey = LogKey;
            this.Content = Content;
            this.LogType = LogType;
        }
        public BrokerLogModel()
        {
            
        }
        public string LogKey { get; set; }
        public string Content { get; set; }
        public BrokerLogTypes LogType { get; set; }

    }
}
