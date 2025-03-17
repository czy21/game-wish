using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using WishServer.Client;
using WishServer.Client.DY;
using WishServer.Manager;
using WishServer.Model;

namespace WishServer.Service.impl
{
    public class DYPlatformService : IPlatformService, IHostedService, IDisposable, IMessageHandler
    {

        private readonly ConcurrentDictionary<string, RoomSession> ROOM_SESSION_DICT = new();

        private readonly ConfigProperties _config;
        private readonly IDatabase _redisDatabase;
        private readonly DYAccessTokenClient _dyAccessTokenClient;
        private readonly DYWebCastClient _dYWebCastClient;

        public DYPlatformService(
            IOptions<ConfigProperties> options,
            IDatabase redisDatabase,
            DYAccessTokenClient dYAccessTokenClient,
            DYWebCastClient dYWebCastClient
            )
        {
            _config = options.Value;
            _redisDatabase = redisDatabase;
            _dyAccessTokenClient = dYAccessTokenClient;
            _dYWebCastClient = dYWebCastClient;
        }

        public PlatformEnum GetPlatform()
        {
            return PlatformEnum.DY;
        }

        public string GetAccessTokenKey()
        {
            return "DY-" + _config.Platform.DY.AppToken + "-token";
        }

        public async Task<string?> GetAccessToken()
        {
            DYAccessTokenReq req = new()
            {
                appid = _config.Platform.DY.AppId,
                secret = _config.Platform.DY.AppSecret,
                grant_type = "client_credential"
            };

            string? accessToken = await _redisDatabase.StringGetAsync(GetAccessTokenKey());
            if (accessToken == null)
            {
                DYAccessTokenRes res = await _dyAccessTokenClient.GetAccessToken(req);
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

            DYWebCastInfoReq param = new()
            {
                token = token,
            };
            return await _dYWebCastClient.GetLiveInfo(param, accessToken);
        }

        public async Task Init(Session session, string? roomId)
        {
            string? accessToken = await GetAccessToken();
            if (roomId != null)
            {
                ROOM_SESSION_DICT.AddOrUpdate(roomId, new RoomSession()
                {
                    Session = session,
                }, (k, v) =>
                {
                    return v;
                });
                await DoRoomTask(roomId);
            }
        }

        public void Dispose()
        {

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                Thread.Sleep(5000);
                foreach (var k in ROOM_SESSION_DICT.Keys)
                {
                    await DoRoomTask(k);
                }
            }
        }

        private async Task DoRoomTask(string roomId)
        {
            foreach (var t in ROOM_SESSION_DICT[roomId].Tasks.Where(t => t.TaskStatus != "SUCCESS"))
            {
                //string? accessToken = await GetAccessToken();
                //DYLiveDataTaskRes taskRes = await _dYWebCastClient.StartPush(
                //    new()
                //    {
                //        appid = _config.Platform.DY.AppId,
                //        roomid = roomId,
                //        msg_type = t.TaskType
                //    },
                //    accessToken);
                //t.TaskId = taskRes.data.taskid;
                //t.TaskStatus = "SUCCESS";
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task Exit(Session session)
        {
            List<string> removeRoomIds = ROOM_SESSION_DICT.Where(t => t.Value.Session.ClientId == session.ClientId).Select(t => t.Key).ToList();
            foreach (var t in removeRoomIds)
            {
                ROOM_SESSION_DICT.TryRemove(t, out _);
            }
            return Task.CompletedTask;
        }
    }
}
