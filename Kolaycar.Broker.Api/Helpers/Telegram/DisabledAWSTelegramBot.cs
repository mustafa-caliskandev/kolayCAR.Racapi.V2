using kolayCAR.Broker.AWS.Logger.Telegram;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Helpers.Telegram
{
    public class DisabledAWSTelegramBot : IAWSTelegramBot
    {
        public Task<bool> AskQuestion(List<string> questionsList, string subject, kolayCAR.Broker.AWS.Logger.Telegram.TelegramMessageType telegramMessageType)
        {
            return Task.FromResult(false);
        }

        public Task<bool> SendMessage(string message, kolayCAR.Broker.AWS.Logger.Telegram.TelegramMessageType telegramMessageType)
        {
            return Task.FromResult(false);
        }

        public Task<bool> SendSpecialMessage(string chatId, string message, string data, kolayCAR.Broker.AWS.Logger.Telegram.TelegramMessageType telegramMessageType)
        {
            return Task.FromResult(false);
        }
    }
}
