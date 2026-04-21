using System.Net;

namespace KolayCAR.Broker.Infrastructure.Extensions
{
    public static class HttpStatusCodeExtensions
    {
        public static string Message(this HttpStatusCode httpStatusCode)
        {
            switch (httpStatusCode)
            {
                default:
                case HttpStatusCode.InternalServerError:
                    return "An unexpected error has occurred. Please contact the system authority!";
                case HttpStatusCode.BadRequest:
                    return "Request parameters are missing or incorrect!";
                case HttpStatusCode.Unauthorized:
                    return "Authentication failed!";
                case HttpStatusCode.Forbidden:
                    return "You are not authorized for this transaction!";
                case HttpStatusCode.NotFound:
                    return "The requested resource was not found!";
            }
        }
    }
}
