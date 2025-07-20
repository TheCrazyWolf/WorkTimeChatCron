using Microsoft.Extensions.Configuration;
using WorkTimeChat.Models;

namespace WorkTimeChat.Vk;

public class ChatWorkTimeService(IConfiguration configuration)
{
    public bool IsWorkingTime(DateTime dateTime)
    {
        var configSection = configuration.Get<WorkTimeConfig>();
        if (configSection == null) return false;

        var allowedTimes = configSection.AllowedTimes;
        var today = dateTime.DayOfWeek;

        var item = allowedTimes.FirstOrDefault(t => string.Equals(t.Key, today.ToString(), StringComparison.InvariantCultureIgnoreCase));

        if (!Enum.TryParse<DayOfWeek>(item.Key, true, out var day)) return false;
        if (day != today) return false;

        var timeRange = item.Value;
        if (string.IsNullOrWhiteSpace(timeRange))
            return false;

        var parts = timeRange.Split('-');
        if (parts.Length != 2)
            return false;

        if (!TimeSpan.TryParse(parts[0], out var start))
            return false;

        if (!TimeSpan.TryParse(parts[1], out var end))
            return false;

        var nowTime = dateTime.TimeOfDay;

        if (start <= end)
        {
            // Простой случай (например: 08:00-17:00)
            return nowTime >= start && nowTime < end;
        }
        else
        {
            // Переход через полночь (например: 22:00-06:00)
            return nowTime >= start || nowTime < end;
        }
    }
    
}