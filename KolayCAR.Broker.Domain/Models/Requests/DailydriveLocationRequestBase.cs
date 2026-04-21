using System.Xml.Serialization;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class DailydriveRequestBase
    {
        #region Location Request

        [XmlRoot(ElementName = "LocationsRequest")]
        public class LocationsRequest
        {

            [XmlElement(ElementName = "cityName")]
            public object CityName { get; set; }
        }

        [XmlRoot(ElementName = "Body")]
        public class Body
        {

            [XmlElement(ElementName = "LocationsRequest")]
            public LocationsRequest LocationsRequest { get; set; } = new LocationsRequest();
        }

        [XmlRoot(ElementName = "Envelope")]
        public class DailydriveLocationRequest
        {

            [XmlElement(ElementName = "Header")]
            public object Header { get; set; }

            [XmlElement(ElementName = "Body")]
            public Body Body { get; set; } = new Body();

            [XmlAttribute(AttributeName = "soapenv")]
            public string Soapenv { get; set; }

            [XmlAttribute(AttributeName = "dto")]
            public string Dto { get; set; }
        }

        #endregion

    }
}
