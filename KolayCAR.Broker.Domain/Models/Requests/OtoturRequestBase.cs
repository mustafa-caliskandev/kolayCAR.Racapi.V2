namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class OtoturRequestBase
    {
        public class OtoturAuthRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class OtoturSearchRequestBody
        {
            public string language { get; set; }
            public string currency { get; set; }
            public string pickupDate { get; set; }
            public int pickupLocationId { get; set; }
            public string pickupTime { get; set; }
            public string returnDate { get; set; }
            public int returnLocationId { get; set; }
            public string returnTime { get; set; }
        }
    }
}
