using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using WishServer.Client;
using WishServer.Client.KS;
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
        private readonly KSOAuthClient _ksOAuthClient;

        public KSPlatformService(
            ILogger<DYPlatformService> logger,
            IOptions<ConfigProperties> options,
            IDatabase redisDatabase,
            KSOAuthClient ksOAuthClient
            )
        {
            _logger = logger;
            _config = options.Value;
            _redisDatabase = redisDatabase;
            _ksOAuthClient = ksOAuthClient;
        }

        public PlatformEnum GetPlatform()
        {
            return PlatformEnum.KS;
        }

        public string GetAccessTokenKey()
        {
            return "KS-" + _config.Platform.AppToken + "-token";
        }

        public async Task<string?> GetAccessToken()
        {
            KSAccessTokenReq req = new()
            {
                app_id = _config.Platform.KS.OAuth.AppId,
                app_secret = _config.Platform.KS.OAuth.AppSecret,
                grant_type = "client_credentials"
            };

            string? accessToken = await _redisDatabase.StringGetAsync(GetAccessTokenKey());
            if (accessToken == null)
            {
                KSAccessTokenRes res = await _ksOAuthClient.GetAccessToken(req);
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

        public Task Init(Session session)
        {
            return Task.CompletedTask;
        }

        public Task SendMessages(string? roomId, string? msgType, List<Dictionary<string, object>> param)
        {
            return Task.CompletedTask;
        }

        public Task Exit(Session session)
        {
            return Task.CompletedTask;
        }
    }
}
