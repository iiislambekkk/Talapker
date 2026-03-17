using Livekit.Server.Sdk.Dotnet;

namespace LiveEvent;

public class LiveKitService
{
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly string _host;

    public LiveKitService(IConfiguration config)
    {
        _apiKey = config["LiveKit:ApiKey"]!;
        _apiSecret = config["LiveKit:ApiSecret"]!;
        _host = config["LiveKit:Host"]!;
    }

    public string CreateHostToken(string roomId, string userId, string username)
    {
        var grants = new VideoGrants
        {
            RoomJoin = true,
            Room = roomId,
            CanPublish = true,
            CanSubscribe = false,
            CanPublishData = true,
        };
        var token = new AccessToken(_apiKey, _apiSecret)
            .WithIdentity(userId)
            .WithName(username)
            .WithGrants(grants)
            .WithTtl(TimeSpan.FromHours(4));
        return token.ToJwt();
    }

    public string CreateViewerToken(string roomId, string userId, string username)
    {
        var grants = new VideoGrants
        {
            RoomJoin = true,
            Room = roomId,
            CanPublish = false,
            CanSubscribe = true,
        };
        var token = new AccessToken(_apiKey, _apiSecret)
            .WithIdentity(userId)
            .WithName(username)
            .WithGrants(grants)
            .WithTtl(TimeSpan.FromHours(8));
        return token.ToJwt();
    }

    public RoomServiceClient GetRoomClient() =>
        new RoomServiceClient(_host, _apiKey, _apiSecret);
}