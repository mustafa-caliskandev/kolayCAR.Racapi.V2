using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KolayCAR.Broker.API.Models
{
    public partial class Location
    {
        [Key]
        public int Id { get; set; }
        public bool? Active { get; set; }
        public int Langid { get; set; }
        public int Countryid { get; set; }
        public int Cityid { get; set; }
        public string Locationname { get; set; }
        public string Iata { get; set; }
        public bool? Airport { get; set; }
        public bool? Ispickup { get; set; }
        public string Mailaddress { get; set; }
        public string Address { get; set; }
        public string Phonenumber { get; set; }
        public string Coordinatelatitude { get; set; }
        public string Coordinatelongitude { get; set; }
        public bool? Ispopular { get; set; }
        public string Imagepath { get; set; }

        public virtual ICollection<ProfitMarkupLocation> ProfitMarkupLocations { get; set; }
    }
}
