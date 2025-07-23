using System.Globalization;
using Quartz;
using WorkTimeChat;
using WorkTimeChat.Jobs;
using WorkTimeChat.Models;
using WorkTimeChat.Vk;

var ci = new CultureInfo("ru-RU");
Thread.CurrentThread.CurrentCulture = ci;
Thread.CurrentThread.CurrentUICulture = ci;
CultureInfo.DefaultThreadCurrentCulture = ci;
CultureInfo.DefaultThreadCurrentUICulture = ci;
CultureInfo.CurrentCulture = ci;
CultureInfo.CurrentUICulture = ci;


System.Diagnostics.Debug.WriteLine(typeof(WorkTimeConfig).Assembly.Location);
var builder = Host.CreateApplicationBuilder(args);
IServiceCollection services = builder.Services;

var config = builder.Configuration.Get<WorkTimeConfig>()!;

builder.Logging.AddConsole();
builder.Services.AddQuartz(q =>
{
    var jobCleanUpId = $"{nameof(JobCleanUpChat)}-{Guid.NewGuid().ToString().Split("-").First()}";
    var jobCleanUp = new JobKey(jobCleanUpId);
    q.AddJob<JobCleanUpChat>(opts => opts.WithIdentity(jobCleanUp));

    q.AddTrigger(opts => opts
        .ForJob(jobCleanUp)
        .WithIdentity(jobCleanUpId)
        .WithCronSchedule("0/30 * * * * ?"));
    
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

builder.Services.AddSingleton<VkBotWorker>(sp =>
{
    var telegramBackground = new VkBotWorker(config.AccessToken, services); 
    return telegramBackground;
});

builder.Services.AddSingleton<ChatWorkTimeService>();
builder.Services.AddSingleton<LongPollHandler>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var iconfog = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var tester = new CronScheduleTester(iconfog);
    _ = tester.Run();

    var bot = scope.ServiceProvider.GetRequiredService<VkBotWorker>();
    await bot.StartAsync();
    var handler = scope.ServiceProvider.GetRequiredService<LongPollHandler>();
    _ = handler.Setup();
  
}


host.Run();