using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Helpers.Telegram
{
    public class DisabledTelegramBot : ITelegramBot
    {
        public Task<bool> AskQuestion(List<string> questionsList, string subject, TelegramMessageType telegramMessageType)
        {
            return Task.FromResult(false);
        }

        public Task<bool> SendMessage(string message, TelegramMessageType telegramMessageType)
        {
            return Task.FromResult(false);
        }

        public Task<bool> SendSpecialMessage(string chatId, string message, string data, TelegramMessageType telegramMessageType)
        {
            return Task.FromResult(false);
        }
    }
}
