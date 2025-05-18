using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Sunny.Framework.Core.Model;
using WishServer.AutoMapper;
using WishServer.Manager;
using WishServer.Model;
using WishServer.Model.DTO;
using WishServer.Service;
using WishServer.Service.impl;
using WishServer.Util;

namespace WishServer.Controllers;

[Route("ks")]
public class KSPlatformController : Controller
{
    private readonly KSPlatformService _ksPlatformService;

    private readonly ILogger<DYPlatformController> _logger;
    private readonly IDatabase _redisDatabase;
    private readonly RoomManager _roomManager;

    public KSPlatformController(
        ILogger<DYPlatformController> logger,
        KSPlatformService ksPlatformService,
        RoomManager roomManager,
        IDatabase redisDatabase
    )
    {
        _logger = logger;
        _ksPlatformService = ksPlatformService;
        _roomManager = roomManager;
        _redisDatabase = redisDatabase;
    }

    [HttpGet("live/info")]
    public async Task<GameRoomDTO> GetLiveInfo([FromQuery(Name = "gameCode")] string gameCode, [FromQuery(Name = "roomId")] string roomId)
    {
        return await _ksPlatformService.GetLiveInfo(gameCode, roomId);
    }

    [HttpPost("test")]
    public async Task<Dictionary<string, object>> Test([FromQuery(Name = "gameCode")] string gameCode)
    {
        return await OnMessage(gameCode);
    }

    [HttpPost("push")]
    public async Task<Dictionary<string, object>> Push([FromQuery(Name = "gameCode")] string gameCode)
    {
        return await OnMessage(gameCode);
    }

    public async Task<Dictionary<string, object>> OnMessage([FromQuery(Name = "gameCode")] string gameCode)
    {
        // 1. 启用缓冲，允许多次读取
        Request.EnableBuffering();

        // 2. 读取原始 Body（注意 leaveOpen）
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();

        // 3. 重置流位置
        Request.Body.Position = 0;

        var jsonObj = JsonUtil.Deserialize<JsonObject>(rawBody);

        var msgType = jsonObj?["data"]?.AsValue()["push_type"]?.ToString() ?? string.Empty;

        var validMsg = await ValidateMessage(gameCode, msgType, rawBody);
        if (!validMsg)
        {
            HttpContext.Response.StatusCode = 403;
            return await Task.FromResult(new Dictionary<string, object> { { "result", 403 }, { "errorMsg", "请求验证失败" } });
        }

        var msgId = jsonObj?["data"]?.AsValue()["unique_message_id"]?.ToString() ?? string.Empty;

        var roomId = jsonObj?["data"]?.AsValue()["room_code"]?.ToString() ?? string.Empty;
        var timestamp = jsonObj?["timestamp"]?.GetValue<long>() ?? 0;

        var payload = jsonObj?["payload"]?.AsArray() ?? [];

        GameMessageDTO<LiveMessageDTOBase> gameMessageDto = new()
        {
            MsgId = msgId,
            Platform = _ksPlatformService.GetPlatform().ToString(),
            SrcType = msgType,
            Msgs = []
        };

        switch (msgType)
        {
            case "liveComment":
                gameMessageDto.MsgType = nameof(MessageKind.Live_Comment);
                gameMessageDto.Msgs.AddRange(payload.Select(t => LiveMessageMapper.MapFromKsComment(t, timestamp)));
                break;
            case "liveLike":
                gameMessageDto.MsgType = nameof(MessageKind.Live_Like);
                gameMessageDto.Msgs.AddRange(payload.Select(t => LiveMessageMapper.MapFromKsLike(t, timestamp)));
                break;
            case "giftSend":
                gameMessageDto.MsgType = nameof(MessageKind.Live_Gift);
                gameMessageDto.Msgs.AddRange(payload.Select(t => LiveMessageMapper.MapFromKsGift(t, timestamp)));
                break;
        }

        var setMsgSuccess = await _redisDatabase.StringSetAsync(((IMessageHandler)_ksPlatformService).GetMsgKey(msgId), 1, TimeSpan.FromMinutes(10), When.NotExists);
        if (!setMsgSuccess) return await Task.FromResult(new Dictionary<string, object> { { "result", 1 }, { "errorMsg", "" } });
        
        if (KSPlatformService.GetAckMsgTypes().Contains(msgType))
        {
            Dictionary<string, object> actData = new()
            {
                { "uniqueMessageId", msgId },
                { "pushType", msgType },
                { "cpServerReceiveTime", DateTimeOffset.Now.ToUnixTimeSeconds() },
                { "cpClientReceiveTime", DateTimeOffset.Now.ToUnixTimeSeconds() }
            };
            await _ksPlatformService.Ack(gameCode, roomId, "cpClientReceive", actData);
        }

        await _roomManager.SendMessageToRoom(_ksPlatformService.GetPlatform(), roomId, JsonUtil.Serialize(gameMessageDto));

        return await Task.FromResult(new Dictionary<string, object> { { "result", 1 }, { "errorMsg", "" } });
    }

    private async Task<bool> ValidateMessage(string gameCode, string msgType, string rawBody)
    {
        if (string.IsNullOrWhiteSpace(msgType)) return await Task.FromResult(false);
        var fromSignature = Request.Headers["kwaisign"].ToString();
        var selfSignature = await _ksPlatformService.SignatureReceive(gameCode, rawBody);
        _logger.LogDebug($"valid sign dyin {msgType,-20} fromSign: {fromSignature} selfSign: {selfSignature}");
        return await Task.FromResult(fromSignature == selfSignature);
    }
}