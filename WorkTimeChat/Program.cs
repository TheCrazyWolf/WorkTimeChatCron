using System.Globalization;
using Quartz;
using WorkTimeChat.Jobs;
using WorkTimeChat.Models;
using WorkTimeChat.Telegram;

var ci = new CultureInfo("ru-RU");
Thread.CurrentThread.CurrentCulture = ci;
Thread.CurrentThread.CurrentUICulture = ci;
CultureInfo.DefaultThreadCurrentCulture = ci;
CultureInfo.DefaultThreadCurrentUICulture = ci;
CultureInfo.CurrentCulture = ci;
CultureInfo.CurrentUICulture = ci;

var builder = Host.CreateApplicationBuilder(args);

var config = builder.Configuration.Get<WorkTimeConfig>()!;

builder.Services.AddQuartz(q =>
{
    foreach (var item in config.JobTurnOnParams)
    {
        var jobId = $"{nameof(JobStartWorkTimeChat)}-{Guid.NewGuid().ToString().Split("-").First()}";
        var jobKeyStartChat = new JobKey(jobId);
        q.AddJob<JobStartWorkTimeChat>(opts => opts.WithIdentity(jobKeyStartChat));

        q.AddTrigger(opts => opts
            .ForJob(jobKeyStartChat)
            .WithIdentity(jobId)
            .WithCronSchedule(item));
    }
    
    foreach (var item in config.JobTurnOffParams)
    {
        var jobId = $"{nameof(JobEndOfWorkTimeChat)}-{Guid.NewGuid().ToString().Split("-").First()}";
        var jobKeyEndChat = new JobKey(jobId);
        q.AddJob<JobEndOfWorkTimeChat>(opts => opts.WithIdentity(jobKeyEndChat));

        q.AddTrigger(opts => opts
            .ForJob(jobKeyEndChat)
            .WithIdentity(jobId)
            .WithCronSchedule(item));
    }
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddSingleton<TelegramWorker>(sp =>
{
    var telegramBackground = new TelegramWorker(config.AccessToken, sp); 
    return telegramBackground;
});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var bot = scope.ServiceProvider.GetRequiredService<TelegramWorker>();
    await bot.StartAsync();
}
host.Run();