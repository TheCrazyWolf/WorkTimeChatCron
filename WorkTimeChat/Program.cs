using WorkTimeChat;
using WorkTimeChat.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<TelegramWorker>(sp =>
{
    var telegramBackground = new TelegramWorker(builder.Configuration.Get<WorkTimeConfig>()!.AccessToken, sp);
    _ = telegramBackground.StartAsync();
    return telegramBackground;
});

var host = builder.Build();
host.Run();