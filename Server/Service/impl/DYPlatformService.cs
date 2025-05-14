using System.Collections.Concurrent;
using StackExchange.Redis;
using Sunny.Framework.External.Client;
using Sunny.Framework.External.Client.DY;
using Sunny.Framework.External.Util;
using WishServer.Model;
using WishServer.Model.DTO;
using WishServer.Model.DY;
using WishServer.Repository;
using WishServer.Util;

namespace WishServer.Service.impl;

public class DYPlatformService : AbstractMessageHandler, IMessageHandler, IHostedService
{
    private readonly IDYClient _dYClient;
    private readonly IDYOAuthClient _dyOAuthClient;

    private readonly ILogger<DYPlatformService> _logger;
    private readonly IDatabase _redisDatabase;
    private readonly ConcurrentDictionary<string, DYRoomSession> _roomSessionDict = new();

    public DYPlatformService(
        ILogger<DYPlatformService> logger,
        IGameAppRepository gameAppRepository,
        IDatabase redisDatabase,
        IDYOAuthClient dYoAuthClient,
        IDYClient dyClient
    ) : base(logger, gameAppRepository, redisDatabase)
    {
        _logger = logger;
        _redisDatabase = redisDatabase;
        _dyOAuthClient = dYoAuthClient;
        _dYClient = dyClient;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DY Push Check Task is running.");
        _ = new Timer(
            async state =>
            {
                foreach (var k in _roomSessionDict.Keys) await DoRoomTask(k);
            },
            null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override PlatformEnum GetPlatform()
    {
        return PlatformEnum.DY;
    }

    public override async Task<string> GetAccessToken(string gameCode)
    {
        string accessToken = await _redisDatabase.StringGetAsync(((IMessageHandler)this).GetAccessTokenKey(gameCode));
        if (!string.IsNullOrEmpty(accessToken)) return accessToken;

        var gameApp = await GetGameApp(gameCode);

        DYAccessTokenReq req = new()
        {
            appid = gameApp.GameApp.AppId,
            secret = gameApp.GameApp.AppSecret,
            grant_type = "client_credential"
        };

        var res = await _dyOAuthClient.GetAccessToken(req);
        if (res.data != null) accessToken = await _redisDatabase.StringSetAndGetAsync(((IMessageHandler)this).GetAccessTokenKey(gameCode), res.data.access_token, TimeSpan.FromHours(1));

        return accessToken;
    }

    public override async Task Init(Session session)
    {
        _roomSessionDict.AddOrUpdate(session.RoomId, new DYRoomSession { Session = session }, (k, v) => v);
        await DoRoomTask(session.RoomId);
    }

    public override async Task DoRoomTask(string roomId)
    {
        var roomSession = _roomSessionDict[roomId];
        foreach (var t in roomSession.Tasks.Where(t => t.TaskStatus != "SUCCESS"))
        {
            var accessToken = await GetAccessToken(roomSession.Session.GameCode);
            var gameApp = await GetGameApp(roomSession.Session.GameCode);
            var taskRes = await _dYClient.StartTaskPush(
                new DYLiveDataTaskReq
                {
                    appid = gameApp.GameApp.AppId,
                    roomid = roomId,
                    msg_type = t.TaskType
                }, accessToken);
            t.TaskId = taskRes.data.taskid;
            if (!string.IsNullOrEmpty(t.TaskId)) t.TaskStatus = "SUCCESS";
        }
    }

    public override async Task Exit(Session session)
    {
        var removeRooms = _roomSessionDict
            .Where(t => t.Value.Session.ClientId == session.ClientId)
            .ToDictionary(k => k.Key, v => v.Value);
        foreach (var r in removeRooms)
        {
            var accessToken = await GetAccessToken(r.Value.Session.GameCode);
            var gameApp = await GetGameApp(r.Value.Session.GameCode);
            foreach (var t in r.Value.Tasks)
                await _dYClient.StopTaskPush(
                    new DYLiveDataTaskReq
                    {
                        appid = gameApp.GameApp.AppId,
                        roomid = r.Key,
                        msg_type = t.TaskType
                    }, accessToken);
            _roomSessionDict.TryRemove(r.Key, out _);
        }
    }

    public async Task<GameRoomDTO> GetLiveInfo(string gameCode, string token)
    {
        var accessToken = await GetAccessToken(gameCode);

        DYLiveInfoReq param = new() { token = token };

        var res = await _dYClient.GetLiveInfo(param, accessToken);
        GameRoomDTO grDto = new();
        if (res.data?.info?.room_id != null)
        {
            var gameApp = await GetGameApp(gameCode);
            grDto = new GameRoomDTO
            {
                Game = gameApp?.Game,
                GameApp = gameApp?.GameApp,
                RoomId = res.data?.info?.room_id.ToString(),
                AnchorId = res.data?.info?.anchor_open_id,
                AvatarUrl = res.data?.info?.avatar_url,
                Nickname = res.data?.info?.nick_name
            };
            await _redisDatabase.StringSetAsync(((IMessageHandler)this).GetRoomKey(grDto.RoomId), JsonUtil.Serialize(grDto), TimeSpan.FromDays(1));
        }

        grDto.Game = null;
        grDto.GameApp = null;
        return await Task.FromResult(grDto);
    }

    public async Task<string> SignatureReceive(string gameCode, Dictionary<string, object> headers, string rawBody)
    {
        var gameApp = await GetGameApp(gameCode);
        return DYUtil.SignatureReceive(headers, rawBody, gameApp?.GameApp.AppSecretPush ?? string.Empty);
    }

    public async Task Ack(string gameCode, string roomId, int ackType, List<Dictionary<string, object>> data)
    {
        var accessToken = await GetAccessToken(gameCode);
        await _dYClient.Ack(new DYLiveDataAckReq
        {
            room_id = roomId,
            ack_type = ackType,
            data = JsonUtil.Serialize(data)
        }, accessToken);
    }

    public static HashSet<string> GetObjMsgTypes()
    {
        return ["user_group_push"];
    }

    public static HashSet<string> GetArrMsgTypes()
    {
        return ["live_comment", "live_like", "live_gift"];
    }

    public static HashSet<string> GetPushMsgTypes()
    {
        return [.. GetObjMsgTypes(), .. GetArrMsgTypes()];
    }

    public static HashSet<string> GetAckMsgTypes()
    {
        return [.. GetArrMsgTypes()];
    }
}