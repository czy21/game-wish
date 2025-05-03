using Microsoft.AspNetCore.Mvc;
using WishServer.Service.impl;

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

    }
}
