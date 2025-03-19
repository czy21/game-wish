using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using WishServer.Annotation;
using WishServer.Client;
using WishServer.Client.DY;
using WishServer.Extension;
using WishServer.Model;

namespace WishServer.Service.impl
{
    public class DYPlatformService : IMessageHandler, IHostedService
    {

        private readonly ConcurrentDictionary<string, DYRoomSession> ROOM_SESSION_DICT = new();

        private readonly ILogger<DYPlatformService> _logger;
        private readonly ConfigProperties _config;
        private readonly IDatabase _redisDatabase;
        private readonly DYOAuthClient _dyOAuthClient;
        private readonly DYWebCastClient _dYWebCastClient;

        public DYPlatformService(
            ILogger<DYPlatformService> logger,
            IOptions<ConfigProperties> options,
            IDatabase redisDatabase,
            DYOAuthClient dYOAuthClient,
            DYWebCastClient dYWebCastClient
            )
        {
            _logger = logger;
            _config = options.Value;
            _redisDatabase = redisDatabase;
            _dyOAuthClient = dYOAuthClient;
            _dYWebCastClient = dYWebCastClient;
        }

        public PlatformEnum GetPlatform()
        {
            return PlatformEnum.DY;
        }

        public string GetAccessTokenKey()
        {
            return "DY-" + _config.Platform.AppToken + "-token";
        }

        public async Task<string?> GetAccessToken()
        {
            DYAccessTokenReq req = new()
            {
                appid = _config.Platform.DY.OAuth.AppId,
                secret = _config.Platform.DY.OAuth.AppSecret,
                grant_type = "client_credential"
            };

            string? accessToken = await _redisDatabase.StringGetAsync(GetAccessTokenKey());
            if (accessToken == null)
            {
                DYAccessTokenRes res = await _dyOAuthClient.GetAccessToken(req);
                if (res.data != null)
                {
                    accessToken = await _redisDatabase.StringSetAndGetAsync(GetAccessTokenKey(), res.data.access_token, TimeSpan.FromSeconds(res.data.expires_in - 30));
                }
            }
            return accessToken;
        }

        public async Task<DYWebCastInfoRes> GetLiveInfo(string token)
        {
            string? accessToken = await GetAccessToken();

            DYWebCastInfoReq param = new() { token = token };

            return await _dYWebCastClient.GetLiveInfo(param, accessToken);
        }

        public async Task Init(Session session)
        {
            if (session.RoomId == null)
            {
                return;
            }

            ROOM_SESSION_DICT.AddOrUpdate(session.RoomId, new DYRoomSession() { Session = session, }, (k, v) => v);
            await DoRoomTask(session.RoomId);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("DY Push Check Task is running.");
            new Timer(
                async (object? state) =>
                {
                    foreach (var k in ROOM_SESSION_DICT.Keys)
                    {
                        await DoRoomTask(k);
                    }
                },
                null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task DoRoomTask(string roomId)
        {
            foreach (var t in ROOM_SESSION_DICT[roomId].Tasks.Where(t => t.TaskStatus != "SUCCESS"))
            {
                string? accessToken = await GetAccessToken();
                DYLiveDataTaskRes taskRes = await _dYWebCastClient.StartTaskPush(new()
                {
                    appid = _config.Platform.DY.OAuth.AppId,
                    roomid = roomId,
                    msg_type = t.TaskType
                }, accessToken);
                t.TaskId = taskRes.data.taskid;
                if (!string.IsNullOrEmpty(t.TaskId))
                {
                    t.TaskStatus = "SUCCESS";
                }
            }
        }

        public async Task Exit(Session session)
        {
            var removeRooms = ROOM_SESSION_DICT.Where(t => t.Value.Session.ClientId == session.ClientId).ToDictionary(k => k.Key, v => v.Value);
            foreach (var r in removeRooms)
            {
                string? accessToken = await GetAccessToken();
                foreach (var t in r.Value.Tasks)
                {
                    await _dYWebCastClient.StopTaskPush(new()
                    {
                        appid = _config.Platform.DY.OAuth.AppId,
                        roomid = r.Key,
                        msg_type = t.TaskType
                    }, accessToken);
                }
                ROOM_SESSION_DICT.TryRemove(r.Key, out _);
            }
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
        public async Task HandleRoomReport(Session session, MessageDTO messageDTO,Dictionary<string,object> message)
        {

        }
    }
}
