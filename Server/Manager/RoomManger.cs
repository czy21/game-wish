using StackExchange.Redis;
using WishServer.Controllers;
using WishServer.Extension;
using WishServer.Model;
using WishServer.Service;

namespace WishServer.Manager;

public class RoomManager : BackgroundService, IServiceBase
{
    private readonly IConnectionMultiplexer _redis;

    public RoomManager(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sub = _redis.GetSubscriber();

        sub.Subscribe(new RedisChannel("game:ROOM_CHANNEL:*", RedisChannel.PatternMode.Pattern), (channel, value) =>
        {
            Task.Run(async () =>
            {
                var platform = channel.ToString().Split(':')[2];
                var roomId = channel.ToString().Split(':')[3];
                var session = WebSocketController.CLIENTID_SESION_DICT.FirstOrDefault(t => t.Value.Platform.ToString() == platform && t.Value.RoomId == roomId).Value;
                if (session != null) await session.WebSocket.SendTextAsync(value);
            }, stoppingToken);
        });
        return Task.CompletedTask;
    }

    public async Task SendMessageToRoom(PlatformEnum platform, string roomId, string message)
    {
        var session = WebSocketController.CLIENTID_SESION_DICT.FirstOrDefault(t => t.Value.Platform == platform && t.Value.RoomId == roomId).Value;
        if (session == null)
        {
            await _redis.GetSubscriber().PublishAsync(new RedisChannel($"game:ROOM_CHANNEL:{platform}:{roomId}", RedisChannel.PatternMode.Pattern), message);
        }
        else
        {
            await session.WebSocket.SendTextAsync(message);
        }
    }
}