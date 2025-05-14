using Microsoft.AspNetCore.Mvc;
using WishServer.Service;
using WishServer.Service.impl;

namespace WishServer.Controllers;

[Route("ab")]
public class ABPlatformController : Controller
{
    private readonly ABPlatformService _abPlatformService;
    private readonly ILogger<DYPlatformController> _logger;
    private readonly IWishUserService _wishUserService;


    public ABPlatformController(
        ILogger<DYPlatformController> logger,
        ABPlatformService abPlatformService,
        IWishUserService wishUserService
    )
    {
        _logger = logger;
        _abPlatformService = abPlatformService;
        _wishUserService = wishUserService;
    }

    [HttpPost("receive")]
    public async Task Receive([FromBody] List<Dictionary<string, object>> param)
    {
        string roomId = Request.Headers["x-roomid"];
        string msgType = Request.Headers["x-msg-type"];
        await _abPlatformService.SendMessages(roomId, msgType, param);
    }

    [HttpGet("dbTest")]
    public async Task DBTest([FromQuery(Name = "id")] long id, [FromQuery(Name = "error")] bool error)
    {
        await _wishUserService.TestTransaction(id, error);
    }
}