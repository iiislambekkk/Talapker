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
        try
        {
            var messageData = data ?? new Dictionary<string, string>();
            
            // Add deep link to data if provided
            if (!string.IsNullOrEmpty(deepLink))
            {
                messageData["click_action"] = deepLink;
                messageData["deep_link"] = deepLink;
            }
            
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = messageData,
                Apns = deepLink != null ? new ApnsConfig
                {
                    FcmOptions = new ApnsFcmOptions
                    {
                        AnalyticsLabel = "deep_link"
                    }
                } : null
            };
            
            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push to {DeviceToken}", deviceToken);
            throw;
        }
    }
    
    public async Task SendToMultipleTokensAsync(List<string> deviceTokens, string title, string body, string? deepLink = null, Dictionary<string, string>? data = null)
    {
        if (!deviceTokens.Any()) return;
        
        try
        {
            var messageData = data ?? new Dictionary<string, string>();
            
            if (!string.IsNullOrEmpty(deepLink))
            {
                messageData["click_action"] = deepLink;
                messageData["deep_link"] = deepLink;
            }
            
            var message = new MulticastMessage
            {
                Tokens = deviceTokens,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = messageData,
                Apns = deepLink != null ? new ApnsConfig
                {
                    FcmOptions = new ApnsFcmOptions
                    {
                        AnalyticsLabel = "deep_link"
                    }
                } : null
            };
            
            await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push to multiple tokens");
            throw;
        }
    }
}