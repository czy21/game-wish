using Demo.Repository;
using Microsoft.AspNetCore.Mvc;
using WishServer.Domain;
using WishServer.Service.impl;

namespace WishServer.Controllers
{
    [Route("ab")]
    public class ABPlatformController : Controller
    {
        private readonly ILogger<DYPlatformController> _logger;
        private readonly ABPlatformService _abPlatformService;
        private readonly IWishUserRepository _wishUserRepository;

        public ABPlatformController(
            ILogger<DYPlatformController> logger,
            ABPlatformService abPlatformService,
            IWishUserRepository wishUserRepository
        )
        {
            _logger = logger;
            _abPlatformService = abPlatformService;
            _wishUserRepository = wishUserRepository;
        }

        [HttpPost("receive")]
        public async Task Receive([FromBody] List<Dictionary<string, object>> param)
        {
            string? roomId = Request.Headers["x-roomid"];
            string? msgType = Request.Headers["x-msg-type"];
            await _abPlatformService.SendMessages(roomId, msgType, param);
        }

        [HttpGet("dbTest")]
        public async Task<WishUserPO?> DBTest([FromQuery(Name = "id")] long id)
        {
            return await _wishUserRepository.SelectByIdAsync(id);
        }
    }
}
