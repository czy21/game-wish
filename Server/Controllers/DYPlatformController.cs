using Microsoft.AspNetCore.Mvc;
using Sunny.Framework.External.Client.DY;
using System.Dynamic;
using System.Text;
using System.Text.Json.Nodes;
using WishServer.AutoMapper;
using WishServer.Manager;
using WishServer.Model;
using WishServer.Model.DTO;
using WishServer.Service.impl;
using WishServer.Util;

namespace WishServer.Controllers
{
    [Route("dy")]
    public class DYPlatformController : Controller
    {
        private readonly ILogger<DYPlatformController> _logger;
        private readonly DYPlatformService _dyPlatformService;
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
        public async Task<CommonResult<object>> Test()
        {
            return await OnMessage();
        }

        [HttpPost("push")]
        public async Task<CommonResult<object>> Push()
        {
            return await OnMessage();
        }

        public async Task<CommonResult<object>> OnMessage()
        {
            string gameCode = Request.Query["gameCode"].ToString();
            string roomId = Request.Headers["x-roomid"].ToString();
            string msgType = Request.Headers["x-msg-type"].ToString();
            _ = long.TryParse(Request.Headers["x-timestamp"].ToString(), out long timestamp);

            if (timestamp == 0)
            {
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }

            List<string> isObjMsgTypes = ["user_group_push"];
            List<string> isArrMsgTypes = ["live_comment", "live_gift", "live_like"];
            List<string> allowMsgTypes = [.. isObjMsgTypes, .. isArrMsgTypes];

            if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(msgType) || !allowMsgTypes.Contains(msgType))
            {
                HttpContext.Response.StatusCode = 400;
                return await Task.FromResult(CommonResult<object>.Fail(HttpContext.Response.StatusCode, "请求参数错误"));
            }

            // 1. 启用缓冲，允许多次读取
            Request.EnableBuffering();

            // 2. 读取原始 Body（注意 leaveOpen）
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            string rawBody = await reader.ReadToEndAsync();

            // 3. 重置流位置
            Request.Body.Position = 0;

            bool validMsg = await ValidateMessage(gameCode, rawBody);
            if (!validMsg)
            {
                HttpContext.Response.StatusCode = 403;
                return await Task.FromResult(CommonResult<object>.Fail(HttpContext.Response.StatusCode, "请求验证失败"));
            }

            dynamic msgObj = new ExpandoObject();

            msgObj.Platform = PlatformEnum.DY.ToString();
            msgObj.MsgType = msgType;

            var msgs = new List<LiveMessageDTOBase>();

            if (isObjMsgTypes.Contains(msgType))
            {
                JsonObject? jsonObj = JsonUtil.Deserialize<JsonObject>(rawBody);
                if (jsonObj == null)
                {
                    return await Task.FromResult(CommonResult<object>.Ok(new object()));
                }
                switch (msgType)
                {
                    case "user_group_push":
                        msgObj.MsgType = MessageKind.Live_Comment.ToString();
                        msgs.Add(LiveMessageMapper.MapFromDyGroup(jsonObj, timestamp));
                        break;
                    default:
                        break;
                }
            }

            if (isArrMsgTypes.Contains(msgType))
            {
                JsonArray? jsonArr = JsonUtil.Deserialize<JsonArray>(rawBody);
                if (jsonArr == null)
                {
                    return await Task.FromResult(CommonResult<object>.Ok(new object()));
                }
                switch (msgType)
                {
                    case "live_comment":
                        msgObj.MsgType = MessageKind.Live_Comment.ToString();
                        msgs.AddRange(jsonArr.Select(t => LiveMessageMapper.MapFromDyComment(t)).ToList());
                        break;
                    case "live_gift":
                        msgObj.MsgType = MessageKind.Live_Gift.ToString();
                        msgs.AddRange(jsonArr.Select(t => LiveMessageMapper.MapFromDyGift(t)).ToList());
                        break;
                    case "live_like":
                        msgObj.MsgType = MessageKind.Live_Like.ToString();
                        msgs.AddRange(jsonArr.Select(t => LiveMessageMapper.MapFromDyLike(t)).ToList());
                        break;
                    default:
                        break;
                }
                if (msgs.Count > 0)
                {
                    await _dyPlatformService.Ack(gameCode, roomId, 1, [.. msgs.Select(t=>
                    new Dictionary<string, object?>
                    {
                        { "msg_id", t.MsgId },
                        { "msg_type", msgType },
                        { "client_time", timestamp }
                    })]);
                }
            }

            if (msgs.Count > 0)
            {
                msgObj.Msgs = msgs;
                await _roomManager.SendMessageToRoom(PlatformEnum.DY, roomId, JsonUtil.Serialize(msgObj));
            }

            return await Task.FromResult(CommonResult<object>.Ok(new object()));
        }

        private async Task<bool> ValidateMessage(string gameCode, string rawBody)
        {
            string msgType = Request.Headers["x-msg-type"].ToString();

            List<string> signHeaderKeys = ["x-timestamp", "x-nonce-str", "x-roomid", "x-msg-type"];
            string fromSignature = Request.Headers["x-signature"].ToString();
            Dictionary<string, string> signHeaders = Request.Headers.Where(t => signHeaderKeys.Contains(t.Key)).ToDictionary(t => t.Key, t => t.Value.ToString());
            string selfSignature = await _dyPlatformService.SignatureReceive(gameCode, signHeaders, rawBody);
            _logger.LogDebug($"valid sign dyin {msgType,-20} fromSign: {fromSignature} selfSign: {selfSignature}");
            return await Task.FromResult(fromSignature == selfSignature);
        }
    }
}
