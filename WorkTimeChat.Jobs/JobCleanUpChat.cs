using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using VkNet.Model;
using WorkTimeChat.Models;
using WorkTimeChat.Vk;

namespace WorkTimeChat.Jobs;

public class JobCleanUpChat(VkBotWorker vkBot, IConfiguration configuration, ChatWorkTimeService chatWorkTimeService, ILogger<JobCleanUpChat> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var bot = vkBot.VkApi!;
        var config = configuration.Get<WorkTimeConfig>()!;
        
        foreach (var chatId in config.ChatIds)
        {
            var isWorkTime = chatWorkTimeService.IsWorkingTime(DateTime.Now);

            if (isWorkTime) return;

            var history = await bot.Messages.GetHistoryAsync(new MessagesGetHistoryParams() { PeerId = chatId });

            var msgIdsToBeDeleted = history.Messages
                .Where(x => !chatWorkTimeService.IsWorkingTime(x.Date!.Value.AddHours(4)))
                .Where(x=> !config.AllowedUsersIdAlways.Contains((long)x.FromId!))
                .Select(x => Convert.ToUInt64(x.Id)).ToList();

            if (!msgIdsToBeDeleted.Any()) continue;

            try
            {
                var result = await bot.Messages.DeleteAsync(messageIds: msgIdsToBeDeleted, null, (ulong)chatId, true);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }
    }
}