using Livekit.Server.Sdk.Dotnet;

namespace LiveEvent;

public class EgressService
{
    private readonly EgressServiceClient _client;
    private readonly string _publicBase;
    private readonly S3Upload _s3;

    public EgressService(IConfiguration config)
    {
        _client = new EgressServiceClient(
            config["LiveKit:Host"]!,
            config["LiveKit:ApiKey"]!,
            config["LiveKit:ApiSecret"]!);

        _publicBase = config["R2:PublicURL"]!;

        _s3 = new S3Upload
        {
            AccessKey = config["R2:AccessKeyId"]!,
            Secret = config["R2:SecretAccessKey"]!,
            Region = "auto",
            Endpoint = config["R2:Endpoint"]!,
            Bucket = config["R2:BucketName"]!,
            ForcePathStyle = true,
        };
    }

    public async Task<string> StartAsync(string roomId)
    {
        var req = new RoomCompositeEgressRequest { RoomName = roomId };

        req.SegmentOutputs.Add(new SegmentedFileOutput
        {
            FilenamePrefix = $"hls/{roomId}/seg",
            PlaylistName = $"hls/{roomId}/index.m3u8",
            LivePlaylistName = $"hls/{roomId}/live.m3u8",
            SegmentDuration = 4,
            S3 = _s3,
        });

        var info = await _client.StartRoomCompositeEgress(req);
        return info.EgressId;
    }

    public async Task StopAsync(string egressId) =>
        await _client.StopEgress(new StopEgressRequest { EgressId = egressId });

    public string GetLiveUrl(string roomId) =>
        $"{_publicBase}/hls/{roomId}/live.m3u8";
}