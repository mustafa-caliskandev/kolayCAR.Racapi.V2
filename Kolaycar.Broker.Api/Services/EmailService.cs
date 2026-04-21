using KolayCAR.Broker.API.Extensions;
using KolayCAR.Broker.API.Helpers.Telegram;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Services
{
    public interface IEmailService
    {
        Task<bool> PostEmail(
            string typeName,
            string fromTitle,
            string toMailAddress,
            string subject,
            string body,
            string replyTo,
            bool isCancelMail = false,
            bool disableSsl = false,
            List<PathString> attachmentPaths = null,
            List<string> attachmentFileNames = null);
    }

    public class EmailLogger
    {
        public string TypeName { get; set; }
        public string FromTitle { get; set; }
        public string ToMailAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ReplyTo { get; set; }
        public string Exception { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly IConfigurationService _configurationService;
        private readonly ITelegramBot _telegramBot;

        public EmailService(
            IConfigurationService configurationService,
            ITelegramBot telegramBot
            )
        {
            _configurationService = configurationService;
            _telegramBot = telegramBot;
        }

        public async Task<bool> PostEmail(
            string typeName,
            string fromTitle,
            string toMailAddress,
            string subject,
            string body,
            string replyTo,
            bool isCancelMail = false,
            bool disableSsl = false,
            List<PathString> attachmentPaths = null,
            List<string> attachmentFileNames = null)
        {
            if (string.IsNullOrEmpty(toMailAddress)) return false;

            var configurations = await _configurationService.GetConfigurations();

            var result = true;
            try
            {
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(configurations.SenderEmail);
                email.From.Add(new MailboxAddress(fromTitle, configurations.SenderEmail));

                foreach (var toEmail in toMailAddress.Split(';'))
                {
                    email.To.Add(MailboxAddress.Parse(toEmail));
                }
                email.Subject = subject;
                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };

                if (attachmentPaths?.Any() ?? false)
                {
                    foreach (var pathString in attachmentPaths)
                    {
                        if (!File.Exists(pathString)) continue;

                        var pathIndex = attachmentPaths.FindIndex(a => a == pathString);
                        var fileName = Path.GetFileName(pathString);
                        var fileExtension = Path.GetExtension(pathString);
                        if ((attachmentFileNames?.Any() ?? false) && !string.IsNullOrEmpty(attachmentFileNames.ElementAtOrDefault(pathIndex)))
                        {
                            fileName = attachmentFileNames.ElementAtOrDefault(pathIndex);
                        }

                        var file = new FileStream(pathString, FileMode.Open, FileAccess.Read);
                        byte[] fileBytes;
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            fileBytes = ms.ToArray();
                        }
                        builder.Attachments.Add($"{fileName}.{fileExtension}", fileBytes, ContentType.Parse("application/octet-stream"));
                    }
                }

                email.Body = builder.ToMessageBody();
                email.ReplyTo.Add(!string.IsNullOrEmpty(replyTo) ? MailboxAddress.Parse(replyTo) : MailboxAddress.Parse(configurations.SenderEmail));

                using var smtp = new MailKit.Net.Smtp.SmtpClient();

                if (disableSsl)
                    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await smtp.ConnectAsync(
                    configurations.SMTPServer,
                    configurations.SMTPPort,
                    configurations.SMTPPort == 465
                        ? SecureSocketOptions.SslOnConnect
                        : SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(configurations.SenderEmail, configurations.SenderPassword);

                await smtp.SendAsync(email);

                await smtp.DisconnectAsync(true);

                await _telegramBot.SendMessage(
                    $"{typeName} E-Mail Posted! Sender: {configurations.SenderEmail}, Receiver/s: {toMailAddress}",
                    TelegramMessageType.Error);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("{@MailSendingError}", ex.ModelToJson());
                await _telegramBot.SendMessage(
                    $"{typeName} E-Mail Post Error! Sender: {configurations.SenderEmail}, Receiver/s: {toMailAddress}, Error: {ex}",
                    TelegramMessageType.Error);
                result = false;
            }
            return result;

            #region Kapatıldı
            //if (configurations.SMTPServer.Contains("gmail"))
            //{
            //    return await PostGMail(typeName, fromTitle, toMailAddress, subject, body, replyTo, configurations);
            //}

            //var mailLog = new EmailLogger()
            //{
            //    TypeName = typeName,
            //    FromTitle = fromTitle,
            //    ToMailAddress = toMailAddress,
            //    Subject = subject,
            //    //Body = body,
            //    ReplyTo = replyTo
            //};

            //try
            //{
            //    if (!string.IsNullOrEmpty(configurations.SMTPServer) &&
            //        !string.IsNullOrEmpty(configurations.SenderEmail) &&
            //        !string.IsNullOrEmpty(configurations.SenderPassword))
            //    {
            //        #region Yorumda - gkursad
            //        //var mailInfo = new MailInfo
            //        //{
            //        //    Port = configurations.SMTPPort,
            //        //    Host = configurations.SMTPServer,
            //        //    EnableSSL = configurations.SMTPSSL,
            //        //    Email = configurations.SenderEmail,
            //        //    Password = configurations.SenderPassword,
            //        //    FromMail = configurations.SenderEmail,
            //        //    FromTitle = fromTitle,
            //        //    ToMailAddress = toMailAddress,
            //        //    Subject = subject,
            //        //    Body = body,
            //        //    ReplyTo = replyTo
            //        //};
            //        //var smtpClient = new SmtpClient
            //        //{
            //        //    Port = mailInfo.Port,
            //        //    Host = mailInfo.Host,
            //        //    EnableSsl = configurations.SMTPSSL,
            //        //    UseDefaultCredentials = false,
            //        //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //        //    Credentials = new System.Net.NetworkCredential(mailInfo.Email, mailInfo.Password)
            //        //};

            //        //var mail = new MailMessage
            //        //{
            //        //    From = new MailAddress(mailInfo.FromMail, mailInfo.FromTitle)
            //        //};

            //        //var mailAddresses = mailInfo.ToMailAddress.Split(';');
            //        //foreach (var address in mailAddresses)
            //        //    mail.To.Add(address);

            //        ////başlıkta /n /r hataya sebebiyet veriyor, eklenecek
            //        ////mail.Subject = mailInfo.Subject.Replace('\r', ' ').Replace('\n', ' ');
            //        //mail.Subject = mailInfo.Subject;
            //        //mail.IsBodyHtml = true;
            //        //mail.Body = mailInfo.Body;

            //        //if (attachment != null)
            //        //{
            //        //    mail.Attachments.Add(attachment);
            //        //}

            //        //mail.ReplyToList.Add(new MailAddress(!string.IsNullOrEmpty(mailInfo.ReplyTo)
            //        //    ? mailInfo.ReplyTo
            //        //    : mailInfo.FromMail));
            //        //smtpClient.Timeout = 30000;

            //        //await AddLog("{@EmailCreated}",
            //        //    $"{{\"Type\": \"{typeName}\", \"Infos\": {JsonConvert.SerializeObject(mailLog)}}}",
            //        //    "Debug");

            //        //smtpClient.Send(mail);

            //        //await AddLog("{@EmailSentSuccessfully}",
            //        //    $"{{\"Type\": \"{typeName}\", \"Infos\": {JsonConvert.SerializeObject(mailLog)}}}",
            //        //    "Debug");
            //        //return true; 
            //        #endregion

            //        var mailInfo = new MailInfo
            //        {
            //            Port = configurations.SMTPPort,
            //            Host = configurations.SMTPServer,
            //            EnableSSL = configurations.SMTPSSL,
            //            Email = configurations.SenderEmail,
            //            Password = configurations.SenderPassword,
            //            FromMail = configurations.SenderEmail,
            //            FromTitle = fromTitle,
            //            ToMailAddress = toMailAddress,
            //            Subject = subject,
            //            Body = body,
            //            ReplyTo = replyTo
            //        };
            //        using var client = new SmtpClient(configurations.SMTPServer, configurations.SMTPPort);

            //        client.EnableSsl = true; // TLS kullanımı için true, SSL kullanımı için false
            //        client.Credentials = new NetworkCredential(mailInfo.Email, mailInfo.Password);

            //        using var message = new MailMessage()
            //        {
            //            From = new MailAddress(mailInfo.FromMail, mailInfo.FromTitle)
            //        };

            //        #region Alıcılar ayarlanıyor
            //        var mailAddresses = mailInfo.ToMailAddress.Split(';');
            //        foreach (var address in mailAddresses)
            //            message.To.Add(address);
            //        #endregion

            //        #region Cevaplayıcılar ayarlanıyor
            //        message.ReplyToList.Add(new MailAddress(!string.IsNullOrEmpty(mailInfo.ReplyTo)
            //                        ? mailInfo.ReplyTo
            //                        : mailInfo.FromMail));

            //        #endregion

            //        #region Mail ekleri ayarlanıyor
            //        if (attachment != null)
            //        {
            //            message.Attachments.Add(attachment);
            //        }
            //        #endregion

            //        message.Subject = mailInfo.Subject;
            //        message.IsBodyHtml = true;
            //        message.Body = mailInfo.Body;

            //        await AddLog("{@EmailCreated}",
            //            $"{{\"Type\": \"{typeName}\", \"Infos\": {JsonConvert.SerializeObject(mailLog)}}}",
            //            "Debug");

            //        client.Timeout = 30000;
            //        client.Send(message);

            //        await AddLog("{@EmailSentSuccessfully}",
            //            $"{{\"Type\": \"{typeName}\", \"Infos\": {JsonConvert.SerializeObject(mailLog)}}}",
            //            "Debug");
            //        return true;
            //    }
            //    else
            //    {
            //        await AddLog("{@EmailMissingData}", "Email host bilgileri eksik!", "Error");
            //        return false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    mailLog.Exception = ex.Message;
            //    await AddLog("{@EmailError}",
            //        $"{{\"Type\": \"{typeName}\", \"Error\": \"{ex.Message}\", \"Infos\": {JsonConvert.SerializeObject(mailLog)}}}",
            //        "Error");
            //    return false;
            //} 
            #endregion
        }
    }
}
