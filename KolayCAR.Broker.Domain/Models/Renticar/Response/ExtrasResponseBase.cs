using System.Collections.Generic;

namespace KolayCAR.Broker.Domain.Models.Renticar.Response
{
    public class ExtrasResponseBase
    {
        public string status { get; set; }
        public string message { get; set; }
        public Search search { get; set; }
        public List<Extra> extras { get; set; }
    }
    public class Extra
    {
        public string vendor { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public int extratype { get; set; }
        public int extradaily { get; set; }
        public string description { get; set; }
        public DailyPrice dailyPrice { get; set; }
        public TotalPrice totalPrice { get; set; }
        public int numberOfSales { get; set; }
    }
}
