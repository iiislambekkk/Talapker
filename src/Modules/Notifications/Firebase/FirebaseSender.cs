using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace Talapker.Notifications.Firebase;

public interface IFirebaseSender
{
    Task SendToTokenAsync(string deviceToken, string title, string body, string? deepLink = null, Dictionary<string, string>? data = null);
    Task SendToMultipleTokensAsync(List<string> deviceTokens, string title, string body, string? deepLink = null, Dictionary<string, string>? data = null);
}

public class FirebaseSender : IFirebaseSender
{
    private readonly ILogger<FirebaseSender> _logger;

    public FirebaseSender(ILogger<FirebaseSender> logger)
    {
        _logger = logger;
    }

    public async Task SendToTokenAsync(string deviceToken, string title, string body, string? deepLink = null, Dictionary<string, string>? data = null)
    {
        _logger.LogInformation("[Firebase] Sending to single token. Token={Token}, Title={Title}, DeepLink={DeepLink}",
            deviceToken[..Math.Min(10, deviceToken.Length)] + "...", title, deepLink);

        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Data = BuildMessageData(title, body, deepLink, data)
            };

            var result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("[Firebase] Sent successfully. MessageId={MessageId}", result);
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "[Firebase] FCM error. ErrorCode={ErrorCode}, MessagingErrorCode={MessagingErrorCode}, Message={Message}",
                ex.ErrorCode, ex.MessagingErrorCode, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Firebase] Unexpected error sending to token");
            throw;
        }
    }

    public async Task SendToMultipleTokensAsync(List<string> deviceTokens, string title, string body, string? deepLink = null, Dictionary<string, string>? data = null)
    {
        if (!deviceTokens.Any())
        {
            _logger.LogWarning("[Firebase] Called with empty token list");
            return;
        }

        _logger.LogInformation("[Firebase] Sending to {Count} tokens. Title={Title}, DeepLink={DeepLink}",
            deviceTokens.Count, title, deepLink);

        try
        {
            var messageData = BuildMessageData(title, body, deepLink, data);

            _logger.LogInformation("[Firebase] Message data: {Data}",
                string.Join(", ", messageData.Select(k => $"{k.Key}={k.Value}")));

            var message = new MulticastMessage
            {
                Tokens = deviceTokens,
                Data = messageData
            };

            var result = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

            _logger.LogInformation("[Firebase] Multicast result: SuccessCount={Success}, FailureCount={Failure}",
                result.SuccessCount, result.FailureCount);

            if (result.FailureCount > 0)
            {
                foreach (var (response, index) in result.Responses.Select((r, i) => (r, i)))
                {
                    if (!response.IsSuccess)
                    {
                        _logger.LogWarning("[Firebase] Failed for token={Token}. ErrorCode={ErrorCode}, MessagingErrorCode={MessagingErrorCode}, Message={Message}",
                            deviceTokens[index][..Math.Min(20, deviceTokens[index].Length)] + "...",
                            response.Exception?.ErrorCode,
                            response.Exception?.MessagingErrorCode,
                            response.Exception?.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Firebase] Unexpected error sending to multiple tokens");
            throw;
        }
    }

    private static Dictionary<string, string> BuildMessageData(string title, string body, string? deepLink, Dictionary<string, string>? data)
    {
        var messageData = data != null
            ? new Dictionary<string, string>(data)
            : new Dictionary<string, string>();

        messageData["title"] = title;
        messageData["body"] = body;

        if (!string.IsNullOrEmpty(deepLink))
            messageData["deep_link"] = deepLink;

        return messageData;
    }
}