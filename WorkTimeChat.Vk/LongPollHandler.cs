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
                if (msg.Text.Contains("identity"))
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

                var isWorkTime = chatWorkTimeService.IsWorkingTime(DateTime.Now);
                var isAllowedUser = config.AllowedUsersIdAlways.Contains((long)msg.FromId!);

                if (isWorkTime) return;
                if (isAllowedUser) return;
                
                var history = await bot.Messages.GetHistoryAsync(new MessagesGetHistoryParams()
                    { PeerId = msg.PeerId });

                var msgIdsToBeDeleted = history.Messages
                    .Where(x => !chatWorkTimeService.IsWorkingTime(x.Date!.Value.AddHours(4)))
                    .Select(x=> Convert.ToUInt64(x.Id)).ToList();
                
                if (!msgIdsToBeDeleted.Any()) continue;
                
                try
                {
                    var result = await bot.Messages.DeleteAsync(
                        messageIds: msgIdsToBeDeleted, null, (ulong)msg.PeerId, true
                    );
                }
                catch (Exception e)
                {
                    //
                }
            }
        }
    }
}