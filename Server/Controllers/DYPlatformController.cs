using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Sunny.Framework.Core.Model;
using WishServer.AutoMapper;
using WishServer.Manager;
using WishServer.Model;
using WishServer.Model.DTO;
using WishServer.Service.impl;
using WishServer.Util;

namespace WishServer.Controllers;

[Route("dy")]
public class DYPlatformController : Controller
{
    private readonly DYPlatformService _dyPlatformService;
    private readonly ILogger<DYPlatformController> _logger;
    private readonly RoomManager _roomManager;

    public DYPlatformController(
        ILogger<DYPlatformController> logger,
        DYPlatformService dyPlatformService,
        RoomManager roomManager
    )
    {
        _logger = logger;
        _dyPlatformService = dyPlatformService;
        _roomManager = roomManager;
    }

    /*
     * token 直播伴侣token
     */
    [HttpGet("live/info")]
    public async Task<GameRoomDTO> GetLiveInfo([FromQuery(Name = "gameCode")] string gameCode, [FromQuery(Name = "token")] string token)
    {
        return await _dyPlatformService.GetLiveInfo(gameCode, token);
    }

    [HttpPost("test")]
    public async Task<CommonResult<object>> Test([FromQuery(Name = "gameCode")] string gameCode)
    {
        return await OnMessage(gameCode);
    }

    [HttpPost("push")]
    public async Task<CommonResult<object>> Push([FromQuery(Name = "gameCode")] string gameCode)
    {
        return await OnMessage(gameCode);
    }

    public async Task<CommonResult<object>> OnMessage([FromQuery(Name = "gameCode")] string gameCode)
    {
        var roomId = Request.Headers["x-roomid"].ToString();
        var msgType = Request.Headers["x-msg-type"].ToString();
        _ = long.TryParse(Request.Headers["x-timestamp"].ToString(), out var timestamp);

        var isObjMsgTypes = DYPlatformService.GetObjMsgTypes();
        var isArrMsgTypes = DYPlatformService.GetArrMsgTypes();
        var allowMsgTypes = DYPlatformService.GetPushMsgTypes();

        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(msgType) || !allowMsgTypes.Contains(msgType))
        {
            HttpContext.Response.StatusCode = 400;
            return await Task.FromResult(CommonResult<object>.Fail(HttpContext.Response.StatusCode, "请求参数错误"));
        }

        // 1. 启用缓冲，允许多次读取
        Request.EnableBuffering();

        // 2. 读取原始 Body（注意 leaveOpen）
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();

        // 3. 重置流位置
        Request.Body.Position = 0;

        var validMsg = await ValidateMessage(gameCode, rawBody);
        if (!validMsg)
        {
            HttpContext.Response.StatusCode = 403;
            return await Task.FromResult(CommonResult<object>.Fail(HttpContext.Response.StatusCode, "请求验证失败"));
        }

        GameMessageDTO<LiveMessageDTOBase> gameMessageDTO = new()
        {
            Platform = _dyPlatformService.GetPlatform().ToString(),
            SrcType = msgType,
            Msgs = []
        };

        if (isObjMsgTypes.Contains(msgType))
        {
            var jsonObj = JsonUtil.Deserialize<JsonObject>(rawBody);
            if (jsonObj == null) return await Task.FromResult(CommonResult<object>.Ok(new object()));
            switch (msgType)
            {
                case "user_group_push":
                    gameMessageDTO.MsgType = MessageKind.Live_Comment.ToString();
                    gameMessageDTO.Msgs.Add(LiveMessageMapper.MapFromDyGroup(jsonObj, timestamp));
                    break;
            }
        }

        if (isArrMsgTypes.Contains(msgType))
        {
            var jsonArr = JsonUtil.Deserialize<JsonArray>(rawBody);
            if (jsonArr == null) return await Task.FromResult(CommonResult<object>.Ok(new object()));
            switch (msgType)
            {
                case "live_comment":
                    gameMessageDTO.MsgType = MessageKind.Live_Comment.ToString();
                    gameMessageDTO.Msgs.AddRange(jsonArr.Select(t => LiveMessageMapper.MapFromDyComment(t)).ToList());
                    break;
                case "live_gift":
                    gameMessageDTO.MsgType = MessageKind.Live_Gift.ToString();
                    gameMessageDTO.Msgs.AddRange(jsonArr.Select(t => LiveMessageMapper.MapFromDyGift(t)).ToList());
                    break;
                case "live_like":
                    gameMessageDTO.MsgType = MessageKind.Live_Like.ToString();
                    gameMessageDTO.Msgs.AddRange(jsonArr.Select(t => LiveMessageMapper.MapFromDyLike(t)).ToList());
                    break;
            }
        }

        if (DYPlatformService.GetAckMsgTypes().Contains(msgType))
        {
            List<Dictionary<string, object>> ackData =
            [
                .. gameMessageDTO.Msgs.Select(t =>
                    new Dictionary<string, object>
                    {
                        { "msg_id", t.MsgId },
                        { "msg_type", msgType },
                        { "client_time", timestamp }
                    })
            ];
            await _dyPlatformService.Ack(gameCode, roomId, 1, ackData);
        }

        await _roomManager.SendMessageToRoom(_dyPlatformService.GetPlatform(), roomId, JsonUtil.Serialize(gameMessageDTO));
        return await Task.FromResult(CommonResult<object>.Ok(new object()));
    }

    private async Task<bool> ValidateMessage(string gameCode, string rawBody)
    {
        var msgType = Request.Headers["x-msg-type"].ToString();

        List<string> signHeaderKeys = ["x-timestamp", "x-nonce-str", "x-roomid", "x-msg-type"];
        var fromSignature = Request.Headers["x-signature"].ToString();
        var signHeaders = Request.Headers.Where(t => signHeaderKeys.Contains(t.Key)).ToDictionary(t => t.Key, t => (object)t.Value.ToString());
        var selfSignature = await _dyPlatformService.SignatureReceive(gameCode, signHeaders, rawBody);
        _logger.LogDebug($"valid sign dyin {msgType,-20} fromSign: {fromSignature} selfSign: {selfSignature}");
        return await Task.FromResult(fromSignature == selfSignature);
    }
}