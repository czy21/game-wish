using System.Collections.Concurrent;
using StackExchange.Redis;
using Sunny.Framework.Cache;
using WishServer.Model;
using WishServer.Service;

namespace WishServer.Manager;

public class RoomManager : IServiceBase
{
    private readonly IDatabase _redisDatabase;
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> RoomIdConsumerDict = new();

    public RoomManager(RedisDataSource redisDataSource)
    {
        _redisDatabase = redisDataSource.GetDefault().GetDatabase();
    }

    private static string GetRoomKey(PlatformEnum platform, string roomId)
    {
        return $"game:ROOM_STREAM:{platform}:{roomId}";
    }

    public void StartRoomConsumer(PlatformEnum platform, string roomId)
    {
        var key = $"{platform}:{roomId}";
        var cts = new CancellationTokenSource();
        RoomIdConsumerDict.AddOrUpdate(key, cts, (k, v) => v);

        var consumer = new RoomConsumer(_redisDatabase, platform, roomId, GetRoomKey(platform, roomId));
        Task.Run(() => consumer.StartAsync(cts.Token), cts.Token);
    }

    public void StopRoomConsumer(PlatformEnum platform, string roomId)
    {
        var key = $"{platform}:{roomId}";
        if (RoomIdConsumerDict.TryRemove(key, out var cts))
        {
            cts.Cancel();
        }
    }

    public async Task SendMessage(PlatformEnum platform, string roomId, string message)
    {
        await _redisDatabase.StreamAddAsync(GetRoomKey(platform, roomId), [new NameValueEntry("message", message)]);
    }
}