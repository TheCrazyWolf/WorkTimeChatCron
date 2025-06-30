using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotBase;
using TelegramBotBase.Builder;
using TelegramBotBase.Commands;
using TelegramBotBase.DependencyInjection;
using TelegramBotBase.Form;
using WorkTimeChat.Forms;

namespace WorkTimeChat;

public class TelegramWorker(string apiKey, IServiceProvider serviceProvider) 
{
    public User? AccountOfBot { get; private set; }
    public BotBase? BotBaseInstance { get; private set; }
    
    public async Task StartAsync()
    {
        if (string.IsNullOrEmpty(apiKey)) return;

        BotBaseInstance = BotBaseBuilder.Create()
            .WithAPIKey(apiKey)
            .DefaultMessageLoop()
            .WithServiceProvider<FormBase>(serviceProvider)
            .NoProxy()
            .CustomCommands(x =>
                {
                    x.Add("start", "Связать аккаунт с HelpDesk");
                }
            )
            .NoSerialization()
            .UseRussian()
            .UseThreadPool()
            .Build();


        BotBaseInstance.BotCommand += async (s, en) =>
        {
            switch (en.Command)
            {
                case "/start":
                    await en.Device.ActiveForm.NavigateTo(typeof(StartCommand));
                    break;
            }
        };

        AccountOfBot = await BotBaseInstance.Client.TelegramClient.GetMe();
        await BotBaseInstance.UploadBotCommands();
        await BotBaseInstance.Start();
    }

    public async Task StopAsync()
    {
        if (BotBaseInstance is not null) await BotBaseInstance.Stop();
    }
}