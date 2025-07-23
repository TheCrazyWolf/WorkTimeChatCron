using Microsoft.Extensions.Configuration;
using VkNet.Enums.StringEnums;
using VkNet.Model;
using WorkTimeChat.Models;

namespace WorkTimeChat.Vk;

public class LongPollHandler(VkBotWorker vkBot, IConfiguration configuration, ChatWorkTimeService chatWorkTimeService)
{
    public async Task Setup()
    {
        var bot = vkBot.VkApi!;

        while (true) // Бесконечный цикл, получение обновлений
        {
            try
            {
                var config = configuration.Get<WorkTimeConfig>();
                var s = bot.Groups.GetLongPollServer(config!.GroupId);
                var poll = bot.Groups.GetBotsLongPollHistory(
                    new BotsLongPollHistoryParams()
                    {
                        Server = s.Server, Ts = s.Ts, Key = s.Key, Wait = 25
                    });
                if (poll?.Updates == null) continue;
                
                foreach (var msg in from a in poll.Updates
                         where a.Type.Value == GroupUpdateType.MessageNew
                         select ((MessageNew)a.Instance).Message!)
                {
                    var isAllowedUser = config.AllowedUsersIdAlways.Contains((long)msg.FromId!);
                    
                    if (msg.Text.Contains("identity") && isAllowedUser)
                    {
                        var parm = new MessagesSendParams()
                        {
                            PeerId = msg.PeerId,
                            Message = $"Индентификатор этого чата: {msg.PeerId}",
                            RandomId = new Random().Next(),
                        };
                        await bot.Messages.SendAsync(parm);
                        return;
                    }

                    if (!config.ChatIds.Contains((long)msg.PeerId!))
                    {
                        continue;
                    }
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}