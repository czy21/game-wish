using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Sunny.Framework.External.Client;
using Sunny.Framework.External.Client.DY;
using Sunny.Framework.External.Util;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using WishServer.Model;
using WishServer.Model.BO;
using WishServer.Model.DTO;
using WishServer.Model.DY;
using WishServer.Repository;
using WishServer.Util;

namespace WishServer.Service.impl
{
    public class DYPlatformService : IMessageHandler, IHostedService
    {

        private readonly ConcurrentDictionary<string, DYRoomSession> ROOM_SESSION_DICT = new();

        private readonly ILogger<DYPlatformService> _logger;
        private readonly AppSetting _config;
        private readonly IDatabase _redisDatabase;
        private readonly IDYOAuthClient _dyOAuthClient;
        private readonly IDYClient _dYClient;
        private readonly IConfigService _configService;
        private readonly IGameAppRepository _gameAppRepository;

        public DYPlatformService(
            ILogger<DYPlatformService> logger,
            IOptions<AppSetting> options,
            IDatabase redisDatabase,
            IDYOAuthClient dYOAuthClient,
            IDYClient dyClient,
            IConfigService configService,
            IGameAppRepository gameAppRepository
            )
        {
            _logger = logger;
            _config = options.Value;
            _redisDatabase = redisDatabase;
            _dyOAuthClient = dYOAuthClient;
            _dYClient = dyClient;
            _configService = configService;
            _gameAppRepository = gameAppRepository;
        }

        public PlatformEnum GetPlatform()
        {
            return PlatformEnum.DY;
        }

        public async Task<string> GetAccessToken(string gameCode)
        {
            string? accessToken = await _redisDatabase.StringGetAsync(((IMessageHandler)this).GetAccessTokenKey(gameCode));
            if (string.IsNullOrEmpty(accessToken))
            {
                GameAppBO? gameApp = await _gameAppRepository.SelectOneByPlatformAndGameCode(GetPlatform().ToString(), gameCode);
                if (gameApp == null)
                {
                    throw new Exception($"GameApp {gameCode} ${GetPlatform()} not exist");
                }
                DYAccessTokenReq req = new()
                {
                    appid = gameApp.GameApp.AppId,
                    secret = gameApp.GameApp.AppSecret,
                    grant_type = "client_credential"
                };

                DYAccessTokenRes res = await _dyOAuthClient.GetAccessToken(req);
                if (res.data != null)
                {
                    accessToken = await _redisDatabase.StringSetAndGetAsync(((IMessageHandler)this).GetAccessTokenKey(gameCode), res.data.access_token, TimeSpan.FromHours(1));
                }
            }
            return accessToken ?? string.Empty;
        }

        public async Task<GameRoomDTO> GetLiveInfo(string gameCode, string token)
        {
            string? accessToken = await GetAccessToken(gameCode);

            DYLiveInfoReq param = new() { token = token };

            DYWebCastResult<DYLiveInfoResData> res = await _dYClient.GetLiveInfo(param, accessToken);
            GameRoomDTO grDTO = new();
            if (res.data?.info?.room_id != null)
            {
                GameAppBO? gameApp = await _gameAppRepository.SelectOneByPlatformAndGameCode(GetPlatform().ToString(), gameCode);
                grDTO = new GameRoomDTO()
                {
                    Game = gameApp?.Game,
                    GameApp = gameApp?.GameApp,
                    RoomId = res.data?.info?.room_id.ToString(),
                    AnchorId = res.data?.info.anchor_open_id,
                    AvatarUrl = res.data?.info.avatar_url,
                    Nickname = res.data?.info.nick_name,
                };
                await _redisDatabase.StringSetAsync(((IMessageHandler)this).GetRoomKey(grDTO.RoomId), JsonUtil.Serialize(grDTO), TimeSpan.FromDays(1));
            }
            grDTO.Game = null;
            grDTO.GameApp = null;
            return await Task.FromResult(grDTO);
        }

        public async Task Init(Session session)
        {
            //if (session.RoomId == null)
            //{
            //    return;
            //}

            //ROOM_SESSION_DICT.AddOrUpdate(session.RoomId, new DYRoomSession() { Session = session }, (k, v) => v);
            //await DoRoomTask(session.RoomId);
        }

        public async Task<string> SignatureReceive(string gameCode, Dictionary<string, object> headers, string rawBody)
        {
            GameAppBO? gameApp = await _gameAppRepository.SelectOneByPlatformAndGameCode(GetPlatform().ToString(), gameCode);
            return DYUtil.SignatureReceive(headers, rawBody, gameApp?.GameApp.AppSecretPush ?? string.Empty);
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
                null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task DoRoomTask(string roomId)
        {
            DYRoomSession roomSession = ROOM_SESSION_DICT[roomId];
            foreach (var t in roomSession.Tasks.Where(t => t.TaskStatus != "SUCCESS"))
            {
                string? accessToken = await GetAccessToken(roomSession.Session.GameCode);
                DYLiveDataTaskRes taskRes = await _dYClient.StartTaskPush(new()
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
                string? accessToken = await GetAccessToken(r.Value.Session.GameCode);
                foreach (var t in r.Value.Tasks)
                {
                    await _dYClient.StopTaskPush(new()
                    {
                        appid = _config.Platform.DY.OAuth.AppId,
                        roomid = r.Key,
                        msg_type = t.TaskType
                    }, accessToken);
                }
                ROOM_SESSION_DICT.TryRemove(r.Key, out _);
            }
        }

        public async Task Ack(string gameCode, string roomId, int ackType, List<Dictionary<string, object?>> data)
        {
            string accessToken = await GetAccessToken(gameCode);
            await _dYClient.Ack(new DYLiveDataAckReq()
            {
                room_id = roomId,
                ack_type = ackType,
                data = JsonUtil.Serialize(data)
            }, accessToken);
        }

        public HashSet<string> GetObjMsgTypes()
        {
            return ["user_group_push"];
        }

        public HashSet<string> GetArrMsgTypes()
        {
            return ["live_comment", "live_like", "live_gift"];
        }

        public HashSet<string> GetPushMsgTypes()
        {
            return [.. GetObjMsgTypes(), .. GetArrMsgTypes()];
        }

        public HashSet<string> GetAckMsgTypes()
        {
            return [.. GetArrMsgTypes()];
        }
    }
}
