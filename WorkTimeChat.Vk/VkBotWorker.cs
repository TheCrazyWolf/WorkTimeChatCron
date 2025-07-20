using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.Model;

namespace WorkTimeChat.Vk;

public class VkBotWorker(string apiKey, IServiceCollection servics) 
{
    public VkApi? VkApi { get; private set; }
    
    public async Task StartAsync()
    {
        if (string.IsNullOrEmpty(apiKey)) return;
        var api = new VkApi(servics);

        await api.AuthorizeAsync(new ApiAuthParams
        {
            AccessToken = apiKey
        });
    }
}