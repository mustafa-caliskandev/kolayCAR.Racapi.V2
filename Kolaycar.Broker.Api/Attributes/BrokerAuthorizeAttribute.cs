using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;

namespace KolayCAR.Broker.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class BrokerAuthorizeAttribute : AuthorizeAttribute
    {
        public BrokerAuthorizeAttribute(params object[] roles)
        {
            if (roles.Any(r => r.GetType().BaseType != typeof(Enum)))
                throw new ArgumentException("roles");

            this.Roles = string.Join(",", roles.Select(r => Enum.GetName(r.GetType(), r)));
        }
    }
}
