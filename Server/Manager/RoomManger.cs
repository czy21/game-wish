using StackExchange.Redis;
using WishServer.Controllers;
using WishServer.Extension;
using WishServer.Model;
using WishServer.Service;

namespace WishServer.Manager
{
    public class RoomManager : IHostedService, IServiceBase
    {
        private readonly IConnectionMultiplexer _redis;

        public RoomManager(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var sub = _redis.GetSubscriber();

            sub.Subscribe(new RedisChannel("game:ROOM_CHANNEL:*", RedisChannel.PatternMode.Pattern), async (channel, value) =>
            {
                var platform = channel.ToString().Split(':')[2];
                var roomId = channel.ToString().Split(':')[3];
                var message = value.ToString();
                Session session = WebSocketController.CLIENTID_SESION_DICT.Where(t => t.Value.Platform.ToString() == platform && t.Value.RoomId == roomId).FirstOrDefault().Value;
                if (session != null)
                {
                    await session.WebSocket.SendTextAsync(message);
                }
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task SendMessageToRoom(PlatformEnum platform, string roomId, string message)
        {
            Session session = WebSocketController.CLIENTID_SESION_DICT.Where(t => t.Value.Platform == platform && t.Value.RoomId == roomId).FirstOrDefault().Value;
            if (session == null)
            {
                await _redis.GetSubscriber().PublishAsync(new RedisChannel($"game:ROOM_CHANNEL:${platform}:${roomId}", RedisChannel.PatternMode.Pattern), message);
            }
            else
            {
                await session.WebSocket.SendTextAsync(message);
            }
        }
    }
}
