using Quartz;
using WorkTimeChat.Jobs;
using WorkTimeChat.Models;
using WorkTimeChat.Telegram;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<TelegramWorker>(sp =>
{
    var telegramBackground = new TelegramWorker(builder.Configuration.Get<WorkTimeConfig>()!.AccessToken, sp);
    _ = telegramBackground.StartAsync();
    return telegramBackground;
});

var config = builder.Configuration.Get<WorkTimeConfig>()!;

builder.Services.AddQuartz(q =>
{
    foreach (var item in config.JobTurnOnParams)
    {
        var jobKeyStartChat = new JobKey(nameof(JobStartWorkTimeChat));
        q.AddJob<JobStartWorkTimeChat>(opts => opts.WithIdentity(jobKeyStartChat));

        q.AddTrigger(opts => opts
            .ForJob(jobKeyStartChat)
            .WithIdentity(nameof(JobStartWorkTimeChat))
            .WithCronSchedule(item));
    }
    
    foreach (var item in config.JobTurnOffParams)
    {
        var jobKeyEndChat = new JobKey(nameof(JobEndOfWorkTimeChat));
        q.AddJob<JobEndOfWorkTimeChat>(opts => opts.WithIdentity(jobKeyEndChat));

        q.AddTrigger(opts => opts
            .ForJob(jobKeyEndChat)
            .WithIdentity(nameof(JobEndOfWorkTimeChat))
            .WithCronSchedule(item));
    }
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

var host = builder.Build();
host.Run();