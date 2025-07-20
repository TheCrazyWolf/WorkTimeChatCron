using Microsoft.Extensions.Configuration;
using Quartz;
using VkNet.Model;
using WorkTimeChat.Models;
using WorkTimeChat.Vk;

namespace WorkTimeChat.Jobs;

public class JobEndOfWorkTimeChat(VkBotWorker vkBotWorker, IConfiguration configuration) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var bot = vkBotWorker.VkApi!;
        var config = configuration.Get<WorkTimeConfig>()!;
        
        foreach (var chatId in config.ChatIds)
        {
            var param = new MessagesSendParams()
            {
                PeerId = chatId,
                RandomId = new Random().Next(),
                Message = config.ChatTurnOffMessage
            };

            try
            {
                await bot.Messages.SendAsync(param);
            }
            catch (Exception e)
            {
               //
            }
        }
    }
}