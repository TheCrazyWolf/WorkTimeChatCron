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

var host = builder.Build();
host.Run();