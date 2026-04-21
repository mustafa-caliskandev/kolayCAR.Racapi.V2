using KolayCAR.Broker.API.Helpers.Telegram;
using KolayCAR.Broker.API.Services;
using KolayCAR.Broker.Domain.Models;
using KolayCAR.Broker.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Net;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IEmailService emailService,
            ITelegramBot telegramBot)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
                await telegramBot.SendMessage($"Error! URL: {context.Request.Path} Error Text: {ex}", TelegramMessageType.Error);
            }
            //await HandleExceptionAsync(context);
        }

        //Handle server errors
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            if (context.Response.HasStarted)
                return;

            if (ex is ObjectDisposedException)
            {
                Serilog.Log.Error(ex.ToJson(), "Client disconnected during request processing (ObjectDisposedException).");
                return;
            }

            var features = context.Features.Get<IHttpRequestFeature>();
            var message = ((HttpStatusCode)context.Response.StatusCode).Message();
            Serilog.Log
                    .ForContext("QueryString", $"{features.RawTarget}")
                    .ForContext("HttpStatusCode", $"{context.Response.StatusCode} {(HttpStatusCode)context.Response.StatusCode}")
                    .Fatal(ex, "ServerError");

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(HttpResult<string>.Result(null, HttpStatusCode.InternalServerError, false, $"{message} ({ex.Message})", resultCode: ResultCodes.Error).ToString());
        }

        //Handle client errors
        private async Task HandleExceptionAsync(HttpContext context)
        {
            if (!context.Response.HasStarted || context.Response.StatusCode.ToString().StartsWith("4"))
            {
                var features = context.Features.Get<IHttpRequestFeature>();
                var message = ((HttpStatusCode)context.Response.StatusCode).Message();

                Serilog.Log
                    .ForContext("QueryString", $"{features.RawTarget}")
                    .ForContext("HttpStatusCode", $"{context.Response.StatusCode} {(HttpStatusCode)context.Response.StatusCode}")
                    .Error("{ClientError}", message);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(HttpResult<string>.Result(null, (HttpStatusCode)context.Response.StatusCode, false, message, resultCode: ResultCodes.Error).ToString());
            }
        }

        private async Task SendErrorMail(IEmailService emailService, Exception exception)
        {
            try
            {
                var htmlBody = "<!DOCTYPE html><html lang='en'><head><title>Bootstrap Example</title><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'><link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css'><script src='https://ajax.googleapis.com/ajax/libs/jquery/3.4.0/jquery.min.js'></script><script src='https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js'></script><script src='https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js'></script></head><body><div class='jumbotron text-center'><h1>ERROR</h1></div><div class='container'><div class='row'><div class='col-sm-4'><h3>ERROR</h3><p>{{errorMessage}}</p></div></div></div></body></html>";

                var exceptionString = exception.Message + "<br/><br/>" + exception.ToString();

                await emailService.PostEmail("System Error", "kolayCAR Broker API v1", "kursad@indisyazilim.com",
                    "Broker API Errors", htmlBody.Replace("{{errorMessage}}", exceptionString), null);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@ErrorMailException}", ex.Message);
            }
        }
    }
}
