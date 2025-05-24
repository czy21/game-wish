using StackExchange.Redis;
using Sunny.Framework.Cache;
using WishServer.Controllers;
using WishServer.Extension;
using WishServer.Model;

namespace WishServer.Manager;

public class RoomConsumer : BackgroundService
{
    private readonly IDatabase _redis;
    private readonly PlatformEnum _platform;
    private readonly string _roomId;
    
    private readonly string _roomKey;
    private readonly string _group;
    private readonly string _consumer;

    public RoomConsumer(RedisDataSource redisDataSource, PlatformEnum platform, string roomId, string roomKey)
    {
        _redis = redisDataSource.GetDefault().GetDatabase();
        _platform = platform;
        _roomId = roomId;
        _roomKey = roomKey;
        _group = $"group:ROOM_STREAM:{platform}:{roomId}";
        _consumer = Guid.NewGuid().ToString();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _redis.StreamCreateConsumerGroupAsync(_roomKey, _group, "0-0");
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var entries = await _redis.StreamReadGroupAsync(_roomKey, _group, _consumer, StreamPosition.NewMessages, count: 10);

            foreach (var entry in entries)
            {
                var message = entry["message"];

                var session = WebSocketController.CLIENTID_SESION_DICT.FirstOrDefault(t => t.Value.Platform == _platform && t.Value.RoomId == _roomId).Value;

                if (session != null)
                {
                    await session.WebSocket.SendTextAsync(message);
                }

                await _redis.StreamAcknowledgeAsync(_roomKey, _group, entry.Id);
                await _redis.StreamDeleteAsync(_roomKey, [entry.Id]);
            }

            await Task.Delay(10, stoppingToken);
        }
    }
}