using System.Text;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace WorkTimeChat.Forms;

public class StartCommand : AutoCleanForm
{
    public override async Task Render(MessageResult message)
    {
        if(message.IsBotGroupCommand) return;

        var strBuiled = new StringBuilder();
        strBuiled.AppendLine($"<b>SchedulerWorkTim for chat</b>\n");
        strBuiled.AppendLine($"Chat bot that configures the operating time of chats.\n");
        strBuiled.AppendLine($"Current datetime on server: {DateTime.Now.ToString()}\n");
        strBuiled.AppendLine($"Development by @kulagin_alex for samgk.ru");
        await Device.Send(strBuiled.ToString(), parseMode: ParseMode.Html);
    }
}