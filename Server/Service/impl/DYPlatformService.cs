using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Utilities.Encoders;
using StackExchange.Redis;
using Sunny.Framework.External.Client;
using Sunny.Framework.External.Client.DY;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using WishServer.Domain;
using WishServer.Model;
using WishServer.Model.BO;
using WishServer.Model.DY;
using WishServer.Repository;
using WishServer.Repository.impl;
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

        private string GetRoomPrefix()
        {
            return $"{_config.Platform.AppId}:DY:ROOM";
        }

        private string GetRoomKey(string roomId)
        {
            return $"{GetRoomPrefix()}:{roomId}";
        }

        public async Task<DYWebCastInfoRes> GetLiveInfo(string gameCode, string token)
        {
            string? accessToken = await GetAccessToken(gameCode);

            DYWebCastInfoReq param = new() { token = token };

            DYWebCastInfoRes res = await _dYClient.GetLiveInfo(param, accessToken);

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

        public string SignatureReceive(Dictionary<string, string> headers, string rawBody)
        {
            var sortedParam = headers.OrderBy(item => item.Key).ToDictionary(item => item.Key, item => item.Value);
            string paramStr = string.Join("&", sortedParam.Select(item => $"{item.Key}={item.Value}"));
            string signStr = paramStr + this._config.Platform.DY.OAuth.AppSecret;
            byte[] inputBytes = Encoding.UTF8.GetBytes(signStr);
            byte[] hashBytes = MD5.HashData(inputBytes);
            return Convert.ToBase64String(hashBytes);
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
    }
}
