using System.Collections.Concurrent;
using StackExchange.Redis;
using Sunny.Framework.Cache;
using WishServer.Model;
using WishServer.Service;

namespace WishServer.Manager;

public class RoomManager : IServiceBase
{
    private readonly RedisDataSource _redisDataSource;
    private readonly IConnectionMultiplexer _redis;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _roomConsumers = new();

    public RoomManager(RedisDataSource redisDataSource)
    {
        _redisDataSource = redisDataSource;
        _redis = redisDataSource.GetDefault();
    }

    private static string GetRoomKey(PlatformEnum platform, string roomId)
    {
        return $"game:ROOM_STREAM:{platform}:{roomId}";
    }

    public void StartRoomConsumer(PlatformEnum platform, string roomId)
    {
        var key = $"{platform}:{roomId}";
        var cts = new CancellationTokenSource();
        _roomConsumers[key] = cts;

        var consumer = new RoomConsumer(_redisDataSource, platform, roomId, GetRoomKey(platform, roomId));
        Task.Run(() => consumer.StartAsync(cts.Token), cts.Token);
    }

    public void StopRoomConsumer(PlatformEnum platform, string roomId)
    {
        var key = $"{platform}:{roomId}";
        if (_roomConsumers.TryRemove(key, out var cts))
        {
            cts.Cancel();
        }
    }

    public async Task SendMessage(PlatformEnum platform, string roomId, string message)
    {
        await _redis.GetDatabase().StreamAddAsync(GetRoomKey(platform, roomId), [new NameValueEntry("message", message)]);
    }
}