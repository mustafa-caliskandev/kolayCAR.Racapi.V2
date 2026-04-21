using System.Xml.Serialization;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class OtorentoRequestBase
    {

        [XmlElement("xsi", Namespace = "http://www.w3.org/2005/Atom")]
        [XmlElement("xsd", Namespace = "http://www.w3.org/2005/Atom")]
        [XmlElement("soap12", Namespace = "http://www.w3.org/2005/Atom")]
        public EnvelopeT Envelope { get; set; }

        [XmlRoot(ElementName = "GetTicket")]
        public class GetTicket
        {

            [XmlElement(ElementName = "userid")]
            public int Userid { get; set; }

            [XmlElement(ElementName = "password")]
            public int Password { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }

            [XmlText]
            public int Text { get; set; }
        }

        [XmlRoot(ElementName = "soap12:Body")]
        public class Body
        {

            [XmlElement(ElementName = "GetTicket")]
            public GetTicket GetTicket { get; set; }
        }

        [XmlRoot(ElementName = "saop12:Envelope")]
        public class EnvelopeT
        {

            [XmlElement(ElementName = "Body")]
            public Body Body { get; set; }

            [XmlAttribute(AttributeName = "xsi")]
            public string Xsi { get; set; }

            [XmlAttribute(AttributeName = "xsd")]
            public string Xsd { get; set; }

            [XmlAttribute(AttributeName = "soap12")]
            public string Soap12 { get; set; }

            [XmlText]
            public int Text { get; set; }
        }

    }
}
