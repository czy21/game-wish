using Microsoft.AspNetCore.Mvc;
using WishServer.Domain;
using WishServer.Model;
using WishServer.Repository;
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
        public async Task DBTest([FromQuery(Name = "id")] long id)
        {
            WishUserPO u1 = new()
            {
                Platform = PlatformEnum.DY.ToString(),
            };
            
            await _wishUserRepository.InsertAsync(u1);
            await _wishUserRepository.DeleteByIdAsync(id);
        }
    }
}
