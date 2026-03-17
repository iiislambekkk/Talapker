using System.Collections.Concurrent;
using Telegram.Bot;

namespace Talapker.TelegramBot;

public class TelegramClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConcurrentDictionary<string, ITelegramBotClient> _clients = new();

    public TelegramClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public ITelegramBotClient GetClient(string botToken)
    {
        return _clients.GetOrAdd(botToken, token =>
        {
            var httpClient = _httpClientFactory.CreateClient("telegram");
            return new TelegramBotClient(token, httpClient);
        });
    }
}