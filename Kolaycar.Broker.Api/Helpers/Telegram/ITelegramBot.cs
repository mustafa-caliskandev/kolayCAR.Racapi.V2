using System.Collections.Generic;
using System.Threading.Tasks;

namespace KolayCAR.Broker.API.Helpers.Telegram
{
    public interface ITelegramBot
    {
        Task<bool> AskQuestion(List<string> questionsList, string subject, TelegramMessageType telegramMessageType);
        Task<bool> SendMessage(string message, TelegramMessageType telegramMessageType);
        Task<bool> SendSpecialMessage(string chatId, string message, string data, TelegramMessageType telegramMessageType);
    }
}
