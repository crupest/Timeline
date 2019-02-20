using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Timeline.Configs;

namespace Timeline.Controllers
{
    [Route("api/[controller]")]
    public class TodoPageController : Controller
    {
        private readonly IOptionsMonitor<TodoPageConfig> _config;

        public TodoPageController(IOptionsMonitor<TodoPageConfig> config)
        {
            _config = config;
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        public ActionResult<AzureDevOpsAccessInfo> AzureDevOpsAccessInfo()
        {
            return Ok(_config.CurrentValue.AzureDevOpsAccessInfo);
        }
    }
}
