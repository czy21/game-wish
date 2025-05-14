using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Sunny.Framework.External.Client;
using Sunny.Framework.External.Client.KS;
using Sunny.Framework.External.Util;
using System.Collections.Concurrent;
using WishServer.Model;
using WishServer.Model.BO;
using WishServer.Model.DTO;
using WishServer.Model.KS;
using WishServer.Repository;
using WishServer.Util;

namespace WishServer.Service.impl
{
    public class KSPlatformService : IMessageHandler
    {
        private readonly ConcurrentDictionary<string, KSRoomSession> ROOM_SESSION_DICT = new();

        private readonly ILogger<DYPlatformService> _logger;
        private readonly AppSetting _config;
        private readonly IDatabase _redisDatabase;
        private readonly IKSClient _ksClient;

        private readonly IGameAppRepository _gameAppRepository;

        public KSPlatformService(
            ILogger<DYPlatformService> logger,
            IOptions<AppSetting> options,
            IDatabase redisDatabase,
            IKSClient ksClient,
            IGameAppRepository gameAppRepository
            )
        {
            _logger = logger;
            _config = options.Value;
            _redisDatabase = redisDatabase;
            _ksClient = ksClient;
            _gameAppRepository = gameAppRepository;
        }

        public PlatformEnum GetPlatform()
        {
            return PlatformEnum.KS;
        }

        public async Task<string> GetAccessToken(string gameCode)
        {
            string accessToken = await _redisDatabase.StringGetAsync(((IMessageHandler)this).GetAccessTokenKey(gameCode));
            if (string.IsNullOrEmpty(accessToken))
            {

                GameAppBO gameApp = await _gameAppRepository.SelectOneByPlatformAndGameCode(GetPlatform().ToString(), gameCode);
                if (gameApp == null)
                {
                    throw new Exception($"GameApp {gameCode} ${GetPlatform()} not exist");
                }

                KSAccessTokenReq req = new()
                {
                    app_id = gameApp.GameApp.AppId,
                    app_secret = gameApp.GameApp.AppSecret,
                    grant_type = "client_credentials"
                };

                KSAccessTokenRes res = await _ksClient.GetAccessToken(req);
                if (res.result == 1)
                {
                    accessToken = await _redisDatabase.StringSetAndGetAsync(((IMessageHandler)this).GetAccessTokenKey(gameCode), res.access_token, TimeSpan.FromHours(1));
                }
            }
            return accessToken ?? string.Empty;
        }

        public string SignatureRequest(Dictionary<string, object> param)
        {
            return KSUtil.SignatureRequest(param, _config.Platform.KS.OAuth.AppSecret);
        }

        public async Task<GameRoomDTO> GetLiveInfo(string gameCode, string roomId)
        {
            return await Task.FromResult(new GameRoomDTO());
        }

        public async Task<string> SignatureRecive(string gameCode, string rawBody)
        {
            GameAppBO gameApp = await _gameAppRepository.SelectOneByPlatformAndGameCode(GetPlatform().ToString(), gameCode);
            return KSUtil.SignatureReceive(rawBody, gameApp?.GameApp.AppSecret ?? string.Empty);
        }


        public async Task Ack(string gameCode, string roomId, string ackType, Dictionary<string, object> data)
        {
            GameAppBO gameApp = await _gameAppRepository.SelectOneByPlatformAndGameCode(GetPlatform().ToString(), gameCode);
            string accessToken = await this.GetAccessToken(gameCode);
            var param = new Dictionary<string, object>()
                {
                    {"roomCode",roomId},
                    {"timestamp",DateTimeOffset.Now.ToUnixTimeSeconds()},
                    {"ackType",ackType },
                    {"data",JsonUtil.Serialize(data) },
                };
            param["sign"] = SignatureRequest(param);
            await _ksClient.Ack(gameApp?.GameApp?.AppId ?? string.Empty, accessToken, data);
        }

        public async Task Init(Session session)
        {
            if (session.RoomId == null)
            {
                return;
            }

            ROOM_SESSION_DICT.AddOrUpdate(session.RoomId, new KSRoomSession() { Session = session }, (k, v) => v);
            await DoRoomTask(session.RoomId);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("KS Bind Check Task is running.");
            _ = new Timer(
                async (object state) =>
                {
                    foreach (var k in ROOM_SESSION_DICT.Keys)
                    {
                        await DoRoomTask(k);
                    }
                },
                null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        public Task SendMessages(string roomId, string msgType, List<Dictionary<string, object>> param)
        {
            return Task.CompletedTask;
        }

        private async Task DoRoomTask(string roomId)
        {
            KSRoomSession roomSession = ROOM_SESSION_DICT[roomId];
            if (roomSession.Bind.TaskStatus != "SUCCESS")
            {
                string accessToken = await GetAccessToken(roomSession.Session.GameCode);
                var param = new Dictionary<string, object>()
                {
                    {"roomCode",roomId},
                    {"timestamp",DateTimeOffset.Now.ToUnixTimeSeconds()},
                    {"moduleType","bind" },
                    {"actionType","start" },
                };
                param["sign"] = SignatureRequest(param);
                KSResult res = await _ksClient.Bind(_config.Platform.DY.OAuth.AppId, accessToken, param);
                if (res.result == 1)
                {
                    ROOM_SESSION_DICT[roomId].Bind.TaskStatus = "SUCCESS";
                }
            }
        }

        public async Task Exit(Session session)
        {
            var removeRooms = ROOM_SESSION_DICT.Where(t => t.Value.Session.ClientId == session.ClientId).ToDictionary(k => k.Key, v => v.Value);
            foreach (var r in removeRooms)
            {
                string accessToken = await GetAccessToken(r.Value.Session.GameCode);
                var param = new Dictionary<string, object>()
                {
                    {"roomCode",r.Key},
                    {"timestamp",DateTimeOffset.Now.ToUnixTimeSeconds()},
                    {"moduleType","bind" },
                    {"actionType","stop" },
                };
                param["sign"] = SignatureRequest(param);
                await _ksClient.Bind(_config.Platform.DY.OAuth.AppId, accessToken, param);
                ROOM_SESSION_DICT.TryRemove(r.Key, out _);
            }
        }

        public HashSet<string> GetPushMsgTypes()
        {
            return ["liveComment", "liveLike", "giftSend"];
        }

        public HashSet<string> GetAckMsgTypes()
        {
            return ["giftSend"];
        }
    }
}
