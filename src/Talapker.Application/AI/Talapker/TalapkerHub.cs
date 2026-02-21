using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Talapker.Application.AI.Talapker;

public class TalapkerHub : Hub
{
    private readonly ITalapkerAgent _talapkerAgent;
    private readonly ILogger<TalapkerHub> _logger;

    public TalapkerHub(
        ITalapkerAgent talapkerAgent,
        ILogger<TalapkerHub> logger)
    {
        _talapkerAgent = talapkerAgent;
        _logger = logger;
    }

    public async Task AskStreaming(TalapkerChatRequest request)
    {
        try
        {
            _logger.LogInformation("Starting streaming for user {UserId}, institution {InstitutionId}: {Message}",
                request.UserId, request.InstitutionId, request.Message);

            var connectionId = Context.ConnectionId;

            await _talapkerAgent.AskStreamingAsync(
                request,
                async chunk => await SendChunk(connectionId, chunk),
                Context.ConnectionAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Streaming error");
            await SendChunk(Context.ConnectionId, new StreamingChunk
            {
                Error = "Произошла ошибка при обработке запроса"
            });
        }
    }

    // Вспомогательный метод для отправки чанков
    private async Task SendChunk(string connectionId, StreamingChunk chunk)
    {
        await Clients.Client(connectionId).SendAsync("ReceiveChunk", chunk);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}