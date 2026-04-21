namespace KolayCAR.Broker.Domain.Models.Renticar.Request
{
    public class SearchRequestBody
    {
        public string pickup_datetime { get; set; }
        public string return_datetime { get; set; }
        public string pickup_location { get; set; }
        public string return_location { get; set; }
    }
}
