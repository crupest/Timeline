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
    public class TodoListController : Controller
    {
        private readonly IOptionsMonitor<TodoListConfig> _config;

        public TodoListController(IOptionsMonitor<TodoListConfig> config)
        {
            _config = config;
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [Produces("text/plain")]
        public ActionResult<string> AzureDevOpsPat()
        {
            return Ok(_config.CurrentValue.AzureDevOpsPat);
        }
    }
}
