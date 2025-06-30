using Microsoft.Extensions.Configuration;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types;
using WorkTimeChat.Models;
using WorkTimeChat.Telegram;

namespace WorkTimeChat.Jobs;

public class JobEndOfWorkTimeChat(TelegramWorker telegramWorker, IConfiguration configuration) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var bot = telegramWorker.BotBaseInstance!.Client.TelegramClient;
        var config = configuration.Get<WorkTimeConfig>()!;

        var isAllowed = false;

        var perm = new ChatPermissions()
        {
            CanSendAudios = isAllowed,
            CanSendDocuments = isAllowed,
            CanSendPhotos = isAllowed,
            CanSendPolls = isAllowed,
            CanSendVoiceNotes = isAllowed,
            CanSendMessages = isAllowed,
            CanSendVideoNotes = isAllowed,
            CanSendVideos = isAllowed,
            CanSendOtherMessages = isAllowed,
        };
        
        await bot.SetChatPermissions(chatId: config.ChatId, permissions: perm);
        await bot.SendMessage(chatId: config.ChatId, config.ChatTurnOffMessage);
    }
}