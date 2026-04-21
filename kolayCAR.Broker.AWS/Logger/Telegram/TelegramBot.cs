using kolayCAR.Broker.AWS.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace kolayCAR.Broker.AWS.Logger.Telegram
{
    public class AWSTelegramBot : IAWSTelegramBot
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly TelegramBotClient _bot;
        private ReplyKeyboardMarkup _replyKeyboard;

        private readonly string _botId;

        public AWSTelegramBot(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _botId = _configuration.GetSectionValueString("Telegram", "BotId").Decrypt();

            _bot = new TelegramBotClient(_botId);
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> AskQuestion(List<string> questionsList, string subject, TelegramMessageType telegramMessageType)
        {
            var result = true;
            try
            {
                var chatId = GetChatId(telegramMessageType);
                await _bot.SendChatAction(chatId, ChatAction.Typing);
                await Task.Delay(500);

                _replyKeyboard = new[]
                {
                    questionsList.ToArray()
                };

                _replyKeyboard.ResizeKeyboard = true;
                _replyKeyboard.OneTimeKeyboard = true;

                await _bot.SendMessage(
                    chatId,
                    subject,
                    replyMarkup: _replyKeyboard);
            }
            catch (Exception ex)
            {
                //TODO : loglama yap
                result = false;
            }

            return result;
        }

        public async Task<bool> SendMessage(string message, TelegramMessageType telegramMessageType)
        {
            var result = true;
            try
            {
                if (!IsLocalRequest(telegramMessageType))
                {
                    var chatId = GetChatId(telegramMessageType);

                    await _bot.SendChatAction(chatId, ChatAction.Typing);
                    await Task.Delay(500);

                    var htmlMessage = message;

                    await _bot.SendMessage(
                        parseMode: ParseMode.Html,
                        chatId: chatId,
                        text: htmlMessage
                    );
                }
            }
            catch (Exception ex)
            {
                //TODO:loglama yapılacak
                result = false;
            }
            return result;
        }

        public async Task<bool> SendSpecialMessage(string chatId, string message, string data, TelegramMessageType telegramMessageType)
        {
            var result = true;
            try
            {
                if (!IsLocalRequest(telegramMessageType))
                {
                    await _bot.SendChatAction(chatId, ChatAction.Typing);
                    await Task.Delay(500);

                    await _bot.SendDocument(
                        chatId: chatId,
                        caption: message,
                        document: InputFile.FromStream(GenerateStreamText(data))
                    );
                }
            }
            catch (Exception ex)
            {
                //TODO:loglama yapılacak
                result = false;
            }
            return result;
        }

        private bool IsLocalRequest(TelegramMessageType telegramMessageType)
        {
            if (telegramMessageType != TelegramMessageType.Sms)
            {
                if (_httpContextAccessor.HttpContext.Connection.RemoteIpAddress == null
                    && _httpContextAccessor.HttpContext.Connection.LocalIpAddress == null)
                {
                    return true;
                }

                if (_httpContextAccessor.HttpContext.Connection.RemoteIpAddress != null
                    && _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.Equals(_httpContextAccessor.HttpContext.Connection.LocalIpAddress))
                {
                    return true;
                }

                if (_httpContextAccessor.HttpContext.Connection.RemoteIpAddress != null
                    && IPAddress.IsLoopback(_httpContextAccessor.HttpContext.Connection.RemoteIpAddress))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetChatId(TelegramMessageType telegramMessageType)
        {
            return telegramMessageType switch
            {
                TelegramMessageType.Error => _configuration.GetSectionValueString("Telegram", "ChatId").Decrypt(),
                TelegramMessageType.Info => _configuration.GetSectionValueString("Telegram", "ChatId").Decrypt(),
                TelegramMessageType.Sms => _configuration.GetSectionValueString("Telegram", "AdminChatId").Decrypt(),
                _ => _configuration.GetSectionValueString("Telegram", "ErrorChatId").Decrypt()
            };
        }

        public Stream GenerateStreamText(string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
