namespace KolayCAR.Broker.API.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string ConnectionString { get; set; }
        public string LogTable { get; set; }
        public string LogFilePath { get; set; }
        public int TokenExpireDay { get; set; }
        public string ApiBaseUrl { get; set; }
        public bool UseCache { get; set; }
        public string BrokerName { get; set; }
    }
}
