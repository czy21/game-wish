using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using WishServer.Annotation;
using WishServer.Extension;
using WishServer.Model;

namespace WishServer.Service.impl
{
    public class ABPlatformService : IMessageHandler
    {

        private readonly ConcurrentDictionary<string, RoomSession> ROOM_SESSION_DICT = new();

        private readonly ILogger<DYPlatformService> _logger;
        private readonly ConfigProperties _config;
        private readonly IDatabase _redisDatabase;

        public ABPlatformService(ILogger<DYPlatformService> logger, IOptions<ConfigProperties> options, IDatabase redisDatabase)
        {
            _logger = logger;
            _config = options.Value;
            _redisDatabase = redisDatabase;
        }

        public PlatformEnum GetPlatform()
        {
            return PlatformEnum.AB;
        }

        public Task Init(Session session)
        {
            if (session.RoomId == null)
            {
                return Task.CompletedTask;
            }
            ROOM_SESSION_DICT.AddOrUpdate(session.RoomId, new RoomSession() { Session = session }, (k, v) => v);
            return Task.CompletedTask;
        }

        public Task Exit(Session session)
        {
            var removeRooms = ROOM_SESSION_DICT.Where(t => t.Value.Session.ClientId == session.ClientId).ToDictionary(k => k.Key, v => v.Value);
            foreach (var r in removeRooms)
            {
                ROOM_SESSION_DICT.TryRemove(r.Key, out _);
            }
            return Task.CompletedTask;
        }

        public string GetAccessTokenKey()
        {
            return "AB-" + _config.Platform.AppToken + "-token";
        }

        public Task<string?> GetAccessToken()
        {
            return Task.FromResult("");
        }

        public async Task SendMessages(string? roomId, string? msgType, List<Dictionary<string, object>> param)
        {
            if (roomId == null || param == null)
            {
                return;
            }

            if (ROOM_SESSION_DICT.TryGetValue(roomId, out var roomSession))
            {
                await roomSession.Session.WebSocket.SendJsonAsnyc(
                    new Dictionary<string, object?>()
                    {
                        ["msgType"] = msgType,
                        ["msg"] = param
                    });
            }
        }

        [OnMessage(MessageKind.ROOM_REPORT)]
        public async Task HandleRoomReport(Session session, MessageDTO messageDTO)
        {

        }
    }

}