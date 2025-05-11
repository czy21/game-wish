using Microsoft.AspNetCore.Mvc;
using Sunny.Framework.External.Client.DY;
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
        public async Task<DYWebCastInfoRes> GetLiveInfo([FromQuery(Name = "gameCode")] string gameCode, [FromQuery(Name = "token")] string token)
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

            bool validMsg = await ValidateMessage(rawBody);
            if (!validMsg)
            {
                HttpContext.Response.StatusCode = 403;
                return await Task.FromResult(CommonResult<object>.Fail(HttpContext.Response.StatusCode, "请求验证失败"));
            }

            var msgObj = new
            {
                Platform = PlatformEnum.DY.ToString(),
                MsgType = msgType,
                Msgs = new List<object>()
            };

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
                        var commentMsg = new LiveMessageComment()
                        {
                            MsgId = "",
                            UserId = jsonObj["open_id"]?.ToString() ?? "",
                            AvatarUrl = jsonObj["avatar_url"]?.ToString() ?? "",
                            Nickname = jsonObj["nickname"]?.ToString() ?? "",
                            Content = jsonObj["group_id"]?.ToString() ?? "",
                            Timestamp = timestamp
                        };
                        msgObj.Msgs.Add(commentMsg);
                        break;
                    default:
                        break;
                }
            }

            if (isObjMsgTypes.Contains(msgType))
            {
                JsonArray? jsonArr = JsonUtil.Deserialize<JsonArray>(rawBody);
                if (jsonArr == null)
                {
                    return await Task.FromResult(CommonResult<object>.Ok(new object()));
                }
                switch (msgType)
                {
                    case "live_comment":
                        msgObj.Msgs.AddRange(jsonArr.Select(t => LiveMessageMapper.MapFromDyComment(t)).ToList());
                        break;
                    case "live_gift":
                        msgObj.Msgs.AddRange(jsonArr.Select(t => LiveMessageMapper.MapFromDyGift(t)).ToList());
                        break;
                    case "live_like":
                        msgObj.Msgs.AddRange(jsonArr.Select(t => LiveMessageMapper.MapFromDyLike(t)).ToList());
                        break;
                    default:
                        break;
                }
            }

            if (msgObj.Msgs.Count > 0)
            {
                await _roomManager.SendMessageToRoom(PlatformEnum.DY, roomId, JsonUtil.Serialize(msgObj));
            }

            return await Task.FromResult(CommonResult<object>.Ok(new object()));
        }

        private async Task<bool> ValidateMessage(string rawBody)
        {
            string msgType = Request.Headers["x-msg-type"].ToString();

            List<string> signHeaderKeys = ["x-timestamp", "x-nonce-str", "x-roomid", "x-msg-type"];
            string fromSignature = Request.Headers["x-signature"].ToString();
            Dictionary<string, string> signHeaders = Request.Headers.Where(t => signHeaderKeys.Contains(t.Key)).ToDictionary(t => t.Key, t => t.Value.ToString());
            string selfSignature = _dyPlatformService.SignatureReceive(signHeaders, rawBody);
            _logger.LogDebug($"valid sign dyin ${msgType,-20} fromSign: ${fromSignature} selfSign: ${selfSignature}");
            return await Task.FromResult(fromSignature == selfSignature);
        }
    }
}
