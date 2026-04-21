using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KolayCAR.Broker.Domain.Models.Requests
{
    public class ZiraatFiloRequestBase
    {
        public class AuthRequest
        {
            public string service_name { get; set; }
            public string username { get; set; }
            public string password { get; set; }
        }
    }
}
