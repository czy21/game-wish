using System.Collections.Concurrent;
using StackExchange.Redis;
using Sunny.Framework.External.Client;
using Sunny.Framework.External.Client.DY;
using Sunny.Framework.External.Client.KS;
using Sunny.Framework.External.Util;
using WishServer.Model;
using WishServer.Model.DTO;
using WishServer.Model.KS;
using WishServer.Repository;
using WishServer.Util;

namespace WishServer.Service.impl;

public class KSPlatformService : AbstractMessageHandler, IMessageHandler
{
    private readonly IKSClient _ksClient;

    private readonly ILogger<DYPlatformService> _logger;
    private readonly IDatabase _redisDatabase;
    private readonly ConcurrentDictionary<string, KSRoomSession> _roomSessionDict = new();

    public KSPlatformService(
        ILogger<DYPlatformService> logger,
        IGameAppRepository gameAppRepository,
        IDatabase redisDatabase,
        IKSClient ksClient
    ) : base(logger, gameAppRepository, redisDatabase)
    {
        _logger = logger;
        _redisDatabase = redisDatabase;
        _ksClient = ksClient;
    }

    public override PlatformEnum GetPlatform()
    {
        return PlatformEnum.KS;
    }

    private async Task<string> SignatureRequest(string gameCode, Dictionary<string, object> param)
    {
        var gameApp = await GetGameApp(gameCode);
        return await Task.FromResult(KSUtil.SignatureRequest(param, gameApp.GameApp.AppSecret));
    }
    
    public async Task<string> SignatureReceive(string gameCode, string rawBody)
    {
        var gameApp = await GetGameApp(gameCode);
        return KSUtil.SignatureReceive(rawBody, gameApp?.GameApp.AppSecret);
    }
    
    public override async Task<string> GetAccessToken(string gameCode)
    {
        var gameApp = await GetGameApp(gameCode);

        return await GetCacheValue(((IMessageHandler)this).GetAccessTokenKey(gameCode),
            async () =>
            {
                KSAccessTokenReq req = new()
                {
                    app_id = gameApp.GameApp.AppId,
                    app_secret = gameApp.GameApp.AppSecret,
                    grant_type = "client_credentials"
                };
                var res = await _ksClient.GetAccessToken(req);
                return res.access_token;
            }
        );
    }

    public async Task<GameRoomDTO> GetLiveInfo(string gameCode, string roomId)
    {
        return await Task.FromResult(new GameRoomDTO());
    }
    
    public override async Task Init(Session session)
    {
        _roomSessionDict.AddOrUpdate(session.RoomId, new KSRoomSession { Session = session }, (k, v) => v);
        await DoRoomTask(session.RoomId);
    }

    public async Task DoRoomTask(string roomId)
    {
        var roomSession = _roomSessionDict[roomId];
        if (roomSession.Bind.TaskStatus != "SUCCESS")
        {
            var accessToken = await GetAccessToken(roomSession.Session.GameCode);
            var gameApp = await GetGameApp(roomSession.Session.GameCode);
            var param = new Dictionary<string, object>
            {
                { "roomCode", roomId },
                { "timestamp", DateTimeOffset.Now.ToUnixTimeSeconds() },
                { "moduleType", "bind" },
                { "actionType", "start" }
            };
            param["sign"] = SignatureRequest(roomSession.Session.GameCode, param);
            var res = await _ksClient.Bind(gameApp.GameApp.AppId, accessToken, param);
            if (res.result == 1) _roomSessionDict[roomId].Bind.TaskStatus = "SUCCESS";
        }
    }

    public override async Task Exit(Session session)
    {
        var removeRooms = _roomSessionDict.Where(t => t.Value.Session.ClientId == session.ClientId).ToDictionary(k => k.Key, v => v.Value);
        foreach (var r in removeRooms)
        {
            var accessToken = await GetAccessToken(r.Value.Session.GameCode);
            var gameApp = await GetGameApp(r.Value.Session.GameCode);
            var param = new Dictionary<string, object>
            {
                { "roomCode", r.Key },
                { "timestamp", DateTimeOffset.Now.ToUnixTimeSeconds() },
                { "moduleType", "bind" },
                { "actionType", "stop" }
            };
            param["sign"] = SignatureRequest(r.Value.Session.GameCode, param);
            await _ksClient.Bind(gameApp.GameApp.AppId, accessToken, param);
            _roomSessionDict.TryRemove(r.Key, out _);
        }
    }

    public async Task Ack(string gameCode, string roomId, string ackType, Dictionary<string, object> data)
    {
        var accessToken = await GetAccessToken(gameCode);
        var gameApp = await GetGameApp(gameCode);
        var param = new Dictionary<string, object>
        {
            { "roomCode", roomId },
            { "timestamp", DateTimeOffset.Now.ToUnixTimeSeconds() },
            { "ackType", ackType },
            { "data", JsonUtil.Serialize(data) }
        };
        param["sign"] = SignatureRequest(gameCode, param);
        await _ksClient.Ack(gameApp?.GameApp?.AppId, accessToken, data);
    }

    protected override async Task DoPeriodTask()
    {
        foreach (var k in _roomSessionDict.Keys)
        {
            await DoRoomTask(k);
        }
    }

    public HashSet<string> GetPushMsgTypes()
    {
        return ["liveComment", "liveLike", "giftSend"];
    }

    public static HashSet<string> GetAckMsgTypes()
    {
        return ["giftSend"];
    }
}