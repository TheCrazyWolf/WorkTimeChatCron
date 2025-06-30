using Microsoft.Extensions.Configuration;
using Quartz;
using WorkTimeChat.Models;
using WorkTimeChat.Telegram;

namespace WorkTimeChat.Jobs;

public class JobStartWorkTimeChat(TelegramWorker telegramWorker, IConfiguration configuration) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var botInstance = telegramWorker.BotBaseInstance!;
        var config = configuration.Get<WorkTimeConfig>();
        
    }
}