using Microsoft.Extensions.Configuration;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types;
using WorkTimeChat.Models;
using WorkTimeChat.Vk;

namespace WorkTimeChat.Jobs;

public class JobStartWorkTimeChat(VkBotWorker vkBotWorker, IConfiguration configuration) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var bot = vkBotWorker.VkApi!.Client.TelegramClient;
        var config = configuration.Get<WorkTimeConfig>()!;

        var isAllowed = true;

        var perm = new ChatPermissions
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

        foreach (var chatId in config.ChatIds)
        {
            await bot.SetChatPermissions(chatId: chatId, permissions: perm);
            await bot.SendMessage(chatId: chatId, config.ChatTurnOnMessage);
        }
        
    }
}