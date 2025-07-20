using Microsoft.Extensions.Configuration;
using Quartz;
using WorkTimeChat.Models;

namespace WorkTimeChat.Vk;

public class ChatWorkTimeService(IConfiguration config)
{
    public bool IsWorkingTime(DateTime dateTime)
    {
        List<CronExpression> turnOnCrons = config.Get<WorkTimeConfig>()!.JobTurnOnParams.Select(e => new CronExpression(e)).ToList();
        List<CronExpression> turnOffCrons = config.Get<WorkTimeConfig>()!.JobTurnOffParams.Select(e => new CronExpression(e)).ToList();

        // Найдём последнее включение
        var lastTurnOn = turnOnCrons
            .Select(expr => expr.GetTimeBefore(dateTime))
            .Where(d => d.HasValue)
            .Max();

        // Найдём ближайшее выключение
        var nextTurnOff = turnOffCrons
            .Select(expr => expr.GetNextValidTimeAfter(lastTurnOn ?? dateTime))
            .Where(d => d.HasValue)
            .Min();

        return lastTurnOn.HasValue && nextTurnOff.HasValue && dateTime >= lastTurnOn && dateTime < nextTurnOff;
    }
}