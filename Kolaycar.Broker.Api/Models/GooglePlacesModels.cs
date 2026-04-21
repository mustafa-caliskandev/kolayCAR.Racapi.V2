using System.Collections.Generic;

namespace KolayCAR.Broker.API.Models
{
    public class GooglePlacesModels
    {
        public class Req
        {
            public string textQuery { get; set; }
        }

        public class Root
        {
            public List<Place> places { get; set; }
        }

        public class Place
        {
            public string name { get; set; }
            public Location location { get; set; }
            public string formattedAddress { get; set; }
            public DisplayName displayName { get; set; }
        }

        public class Location
        {
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class DisplayName
        {
            public string text { get; set; }
            public string languageCode { get; set; }
        }
    }
}
