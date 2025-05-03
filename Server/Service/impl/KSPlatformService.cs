using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Sunny.Framework.External.Client;
using Sunny.Framework.External.Client.KS;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using WishServer.Model;
using WishServer.Model.KS;

namespace WishServer.Service.impl
{
    public class KSPlatformService : IMessageHandler
    {
        private readonly ConcurrentDictionary<string, KSRoomSession> ROOM_SESSION_DICT = new();

        private readonly ILogger<DYPlatformService> _logger;
        private readonly ConfigProperties _config;
        private readonly IDatabase _redisDatabase;
        private readonly KSClient _ksClient;

        public KSPlatformService(
            ILogger<DYPlatformService> logger,
            IOptions<ConfigProperties> options,
            IDatabase redisDatabase,
            KSClient ksClient
            )
        {
            _logger = logger;
            _config = options.Value;
            _redisDatabase = redisDatabase;
            _ksClient = ksClient;
        }

        public PlatformEnum GetPlatform()
        {
            return PlatformEnum.KS;
        }

        public string GetAccessTokenKey()
        {
            return "KS-" + _config.Platform.AppToken + "-token";
        }

        public async Task<string> GetAccessToken()
        {
            KSAccessTokenReq req = new()
            {
                app_id = _config.Platform.KS.OAuth.AppId,
                app_secret = _config.Platform.KS.OAuth.AppSecret,
                grant_type = "client_credentials"
            };

            string? accessToken = await _redisDatabase.StringGetAsync(GetAccessTokenKey());
            if (string.IsNullOrEmpty(accessToken))
            {
                KSAccessTokenRes res = await _ksClient.GetAccessToken(req);
                if (res.result == 1)
                {
                    accessToken = await _redisDatabase.StringSetAndGetAsync(GetAccessTokenKey(), res.access_token, TimeSpan.FromSeconds(res.expires_in - 30));
                }
            }
            return accessToken;
        }

        private string CalculateSignature(Dictionary<string, object> param)
        {

            var trimmedParam = param.Where(item => !string.IsNullOrEmpty(item.Value.ToString())).ToDictionary(item => item.Key, item => item.Value);

            var sortedParam = trimmedParam.OrderBy(item => item.Key).ToDictionary(item => item.Key, item => item.Value);

            string paramStr = string.Join("&", sortedParam.Select(item => $"{item.Key}={item.Value}"));
            string signStr = paramStr + this._config.Platform.KS.OAuth.AppSecret;

            byte[] inputBytes = Encoding.UTF8.GetBytes(signStr);
            byte[] hashBytes = MD5.HashData(inputBytes);
            return Convert.ToHexStringLower(hashBytes);
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

        public Task SendMessages(string? roomId, string? msgType, List<Dictionary<string, object>> param)
        {
            return Task.CompletedTask;
        }

        private async Task DoRoomTask(string roomId)
        {
            if (ROOM_SESSION_DICT[roomId].Bind.TaskStatus != "SUCCESS")
            {
                string accessToken = await GetAccessToken();
                var param = new Dictionary<string, object>()
                {
                    {"roomCode",roomId},
                    {"timestamp",DateTimeOffset.Now.ToUnixTimeSeconds()},
                    {"moduleType","bind" },
                    {"actionType","start" },
                };
                param["sign"] = CalculateSignature(param);
                KSBindRes res = await _ksClient.Bind(_config.Platform.DY.OAuth.AppId, accessToken, param);
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
                string accessToken = await GetAccessToken();
                var param = new Dictionary<string, object>()
                {
                    {"roomCode",r.Key},
                    {"timestamp",DateTimeOffset.Now.ToUnixTimeSeconds()},
                    {"moduleType","bind" },
                    {"actionType","stop" },
                };
                param["sign"] = CalculateSignature(param);
                KSBindRes res = await _ksClient.Bind(_config.Platform.DY.OAuth.AppId, accessToken, param);
                ROOM_SESSION_DICT.TryRemove(r.Key, out _);
            }
        }
    }
}
