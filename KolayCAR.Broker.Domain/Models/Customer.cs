using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.Domain.Models
{
    public class Customer
    {    
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string PersonalNumber { get; set; }
        public string Note { get; set; }
        public string Explanation { get; set; }
        public string IPAddress { get; set; }
        public string Address { get; set; }
        public string BirthDay { get; set; }
        public int? InstutionTypeNumber { get; set; }
    }
}
