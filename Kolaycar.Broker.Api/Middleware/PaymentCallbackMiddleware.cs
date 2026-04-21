using KolayCAR.Broker.API.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Middleware
{
    public class PaymentCallbackMiddleware
    {
        private readonly RequestDelegate _next;

        public PaymentCallbackMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.ToString().ToLower().Contains("payment/calback"))
            {
                var tokenDecrypt = context.Request.Query["token"].ToString().Decrypt();
                string decodedToken = Base64UrlEncoder.Decode(tokenDecrypt);
                string decompressedToken = TokenCompressExtension.Decompress(decodedToken);

                if (!string.IsNullOrEmpty(tokenDecrypt))
                {
                    context.Request.Headers["Authorization"] = $"{decompressedToken}";
                }
            }
            await _next.Invoke(context);
        }
    }
}
