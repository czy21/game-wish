using Microsoft.AspNetCore.Mvc;
using Sunny.Framework.Core.Model;
using System.Dynamic;
using System.Text.Json.Nodes;
using System.Text;
using WishServer.AutoMapper;
using WishServer.Manager;
using WishServer.Model.DTO;
using WishServer.Model;
using WishServer.Service.impl;
using WishServer.Util;
using Sunny.Framework.External.Client;

namespace WishServer.Controllers
{
    [Route("ks")]
    public class KSPlatformController : Controller
    {

        private readonly ILogger<DYPlatformController> _logger;
        private readonly KSPlatformService _ksPlatformService;

        public KSPlatformController(ILogger<DYPlatformController> logger, KSPlatformService ksPlatformService)
        {
            _logger = logger;
            _ksPlatformService = ksPlatformService;
        }

        [HttpGet("live/info")]
        public async Task<GameRoomDTO> GetLiveInfo([FromQuery(Name = "gameCode")] string gameCode, [FromQuery(Name = "roomId")] string roomId)
        {
            return await _ksPlatformService.GetLiveInfo(gameCode, roomId);
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
            // 1. 启用缓冲，允许多次读取
            Request.EnableBuffering();

            // 2. 读取原始 Body（注意 leaveOpen）
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            string rawBody = await reader.ReadToEndAsync();

            // 3. 重置流位置
            Request.Body.Position = 0;

            JsonObject? jsonObj = JsonUtil.Deserialize<JsonObject>(rawBody);

            string msgType = jsonObj?["data"]?.AsValue()["push_type"]?.ToString() ?? string.Empty;

            bool validMsg = await ValidateMessage(gameCode, msgType, rawBody);
            if (!validMsg)
            {
                HttpContext.Response.StatusCode = 403;
                return await Task.FromResult(CommonResult<object>.Fail(HttpContext.Response.StatusCode, "请求验证失败"));
            }

            string msgId = jsonObj?["data"]?.AsValue()["unique_message_id"]?.ToString() ?? string.Empty;

            string roomId = jsonObj?["data"]?.AsValue()["room_code"]?.ToString() ?? string.Empty;
            long timestamp = jsonObj?["timestamp"]?.GetValue<long>() ?? 0;

            JsonArray payload = jsonObj?["payload"]?.AsArray() ?? [];

            GameMessageDTO<LiveMessageDTOBase> gameMessageDTO = new()
            {
                MsgId = msgId,
                Platform = _ksPlatformService.GetPlatform().ToString(),
                SrcType = msgType,
                Msgs = []
            };

            switch (msgType)
            {
                case "liveComment":
                    gameMessageDTO.MsgType = MessageKind.Live_Comment.ToString();
                    gameMessageDTO.Msgs.AddRange(payload.Select(t => LiveMessageMapper.MapFromKsComment(t, timestamp)));
                    break;
                case "liveLike":
                    gameMessageDTO.MsgType = MessageKind.Live_Like.ToString();
                    gameMessageDTO.Msgs.AddRange(payload.Select(t => LiveMessageMapper.MapFromKsLike(t, timestamp)));
                    break;
                case "giftSend":
                    gameMessageDTO.MsgType = MessageKind.Live_Gift.ToString();
                    gameMessageDTO.Msgs.AddRange(payload.Select(t => LiveMessageMapper.MapFromKsGift(t, timestamp)));
                    break;
                default:
                    break;
            }

            if (gameMessageDTO.Msgs.Count > 0)
            {
                Dictionary<string, object> actData = new()
                {
                    {"uniqueMessageId",msgId},
                    {"pushType",msgType },
                    {"cpServerReceiveTime",DateTimeOffset.Now.ToUnixTimeSeconds()},
                    {"cpClientReceiveTime",DateTimeOffset.Now.ToUnixTimeSeconds()},
                };
                await _ksPlatformService.Ack(gameCode,roomId, "cpClientReceive", actData);
            }

            return await Task.FromResult(CommonResult<object>.Ok(new object()));
        }

        private async Task<bool> ValidateMessage(string gameCode, string msgType, string rawBody)
        {
            if (string.IsNullOrWhiteSpace(msgType))
            {
                return await Task.FromResult(false);
            }
            string fromSignature = Request.Headers["kwaisign"].ToString();
            string selfSignature = await _ksPlatformService.SignatureRecive(gameCode, rawBody);
            _logger.LogDebug($"valid sign dyin {msgType,-20} fromSign: {fromSignature} selfSign: {selfSignature}");
            return await Task.FromResult(fromSignature == selfSignature);
        }
    }
}
