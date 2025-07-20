using WorkTimeChat.Vk;

namespace WorkTimeChat;

public class CronScheduleTester
{
    private readonly ChatWorkTimeService _checker;

    public CronScheduleTester(IConfiguration config)
    {
        _checker = new ChatWorkTimeService(config); // твой класс с методом IsWithinWorkTime
    }

    public Task Run()
    {
        var testTimes = new[]
        {
            new TimeSpan(7, 30, 0),
            new TimeSpan(10, 00, 0),
            new TimeSpan(19, 30, 0)
        };

        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            foreach (var time in testTimes)
            {
                DateTime testDate = GetNextWeekday(day).Date + time;
                bool result = _checker.IsWorkingTime(testDate);
                Console.WriteLine($"{day,-9} {testDate:HH:mm} => {(result ? "✅ WORK" : "❌ OFF")}");
            }
        }
        
        return Task.CompletedTask;
    }

    private DateTime GetNextWeekday(DayOfWeek day)
    {
        DateTime today = DateTime.Today;
        int diff = ((int)day - (int)today.DayOfWeek + 7) % 7;
        return today.AddDays(diff);
    }
}