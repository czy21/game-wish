using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using WishServer.Client;
using WishServer.Client.DY;
using WishServer.Domain;
using WishServer.Model;
using WishServer.Model.DY;
using WishServer.Repository;
using WishServer.Util;

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
        private readonly IConfigService _configService;
        private readonly IWishUserRepository _wishUserRepository;
        private readonly IWishItemRepository _wishItemRepository;

        public DYPlatformService(
            ILogger<DYPlatformService> logger,
            IOptions<ConfigProperties> options,
            IDatabase redisDatabase,
            DYOAuthClient dYOAuthClient,
            DYWebCastClient dYWebCastClient,
            IConfigService configService,
            IWishUserRepository wishUserRepository,
            IWishItemRepository wishItemRepository
            )
        {
            _logger = logger;
            _config = options.Value;
            _redisDatabase = redisDatabase;
            _dyOAuthClient = dYOAuthClient;
            _dYWebCastClient = dYWebCastClient;
            _configService = configService;
            _wishUserRepository = wishUserRepository;
            _wishItemRepository = wishItemRepository;
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

        private string GetRoomPrefix()
        {
            return "DY-RoomId-";
        }

        private string GetRoomKey(string roomId)
        {
            return GetRoomPrefix() + roomId;
        }

        public async Task<DYWebCastInfoRes> GetLiveInfo(string token)
        {
            string? accessToken = await GetAccessToken();

            DYWebCastInfoReq param = new() { token = token };

            DYWebCastInfoRes res = await _dYWebCastClient.GetLiveInfo(param, accessToken);

            if (res.data?.info?.room_id != null)
            {
                await _redisDatabase.StringSetAsync(GetRoomKey(res.data.info.room_id.ToString()), JsonUtil.Serialize(res.data.info));
            }

            return res;
        }

        private async Task<DYWebCastInfo?> GetRoomInfo(string roomId)
        {
            string? roomInfoStr = await _redisDatabase.StringGetAsync(GetRoomKey(roomId));
            DYWebCastInfo? roomInfo = null;
            if (!string.IsNullOrEmpty(roomInfoStr))
            {
                roomInfo = JsonUtil.Deserialize<DYWebCastInfo>(roomInfoStr);
            }
            return roomInfo;
        }

        public async Task Init(Session session)
        {
            if (session.RoomId == null)
            {
                return;
            }

            ROOM_SESSION_DICT.AddOrUpdate(session.RoomId, new DYRoomSession() { Session = session }, (k, v) => v);
            await DoRoomTask(session.RoomId);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("DY Push Check Task is running.");
            _ = new Timer(
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

        private string GetRoomUserPrefix(string roomId, string userId)
        {
            return "{" + GetRoomKey(roomId) + "-" + userId + "}-";
        }

        private string GetRoomUserContentKey(string roomId, string userId)
        {
            return GetRoomUserPrefix(roomId, userId) + "content";
        }

        private string GetRoomUserMoneyKey(string roomId, string userId)
        {
            return GetRoomUserPrefix(roomId, userId) + "money";
        }

        public async Task SendMessages(string? roomId, string? msgType, List<Dictionary<string, object>> param)
        {
            if (roomId == null || param == null)
            {
                return;
            }

            if (ROOM_SESSION_DICT.TryGetValue(roomId, out var roomSession))
            {
                if (roomSession.Session.WebSocket.State == WebSocketState.Open)
                {
                    string paramStr = JsonUtil.Serialize(param);
                    if (msgType == "live_comment")
                    {
                        List<DYLiveCommentMessage> comments = (JsonUtil.Deserialize<List<DYLiveCommentMessage>>(paramStr) ?? []).Where(t => t.content.EndsWith(" 寄")).ToList();
                        foreach (var t in comments)
                        {
                            await _redisDatabase.ListRightPushAsync(GetRoomUserContentKey(roomId, t.sec_openid), t.content);
                            await _redisDatabase.KeyExpireAsync(GetRoomUserContentKey(roomId, t.sec_openid), TimeSpan.FromDays(1));
                            await CaculateMoneyAndSaveContent(roomId, roomSession, t);
                        }
                    }
                    if (msgType == "live_gift")
                    {
                        List<DYLiveGiftMessage> gifts = (JsonUtil.Deserialize<List<DYLiveGiftMessage>>(paramStr) ?? []).ToList();
                        foreach (var t in gifts)
                        {
                            await _redisDatabase.StringIncrementAsync(GetRoomUserMoneyKey(roomId, t.sec_openid), t.gift_value);
                            await _redisDatabase.KeyExpireAsync(GetRoomUserMoneyKey(roomId, t.sec_openid), TimeSpan.FromDays(1));
                            await CaculateMoneyAndSaveContent(roomId, roomSession, t);
                        }
                    }
                    //await roomSession.Session.WebSocket.SendJsonAsnyc(
                    //       new Dictionary<string, object?>()
                    //       {
                    //           ["msgType"] = msgType,
                    //           ["msg"] = param
                    //       });
                }
            }
        }

        private async Task CaculateMoneyAndSaveContent(string roomId, RoomSession roomSession, DYMessageBase userInfo)
        {
            DYWebCastInfo? roomInfo = await GetRoomInfo(roomId);
            if (roomInfo == null) return;
            string? anchorId = roomInfo.anchor_open_id;
            if (anchorId == null) return;
            //string anchorId = "1";

            int cost = await _configService.GetValue<int>("WishServer", "CONTENT_COST");

            var script = @"
            local val = redis.call('GET', KEYS[2])
            local len = redis.call('LLEN',KEYS[1])
            if not val then
                val = 0
            else
                val = tonumber(val)
            end

            local tmp = val - tonumber(ARGV[1])

            if tmp >= 0 and len > 0 then
               redis.call('SET', KEYS[2], tmp)
               return redis.call('LPOP',KEYS[1])
            end
            return nil
        ";
            var content = await _redisDatabase.ScriptEvaluateAsync(script, [GetRoomUserContentKey(roomId, userInfo.sec_openid), GetRoomUserMoneyKey(roomId, userInfo.sec_openid)], [cost]);
            if (content == null)
            {
                return;
            }
            WishUserPO? wishUserPO = await _wishUserRepository.GetDbSet()
                .Where(t =>
                    t.RoomId == roomId &&
                    t.AnchorUid == anchorId &&
                    t.AudienceUid == userInfo.sec_openid
                ).FirstOrDefaultAsync();
            if (wishUserPO == null)
            {
                wishUserPO = new()
                {
                    RoomId = roomId,
                    AnchorUid = anchorId,
                    AudienceUid = userInfo.sec_openid
                };
                await _wishUserRepository.InsertAsync(wishUserPO);
            }
            await _wishItemRepository.InsertAsync(new()
            {
                UserId = wishUserPO.Id,
                Content = (string?)content,
            });
        }

        //[OnMessage(MessageKind.ROOM_REPORT)]
        //public async Task HandleRoomReport(Session session, MessageDTO messageDTO)
        //{

        //}
    }
}
