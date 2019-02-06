using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Timeline.Controllers
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [HttpGet("[action]")]
        [Authorize]
        public string Action1()
        {
            return "test";
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "User,Admin")]
        public string Action2()
        {
            return "test";
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public string Action3()
        {
            return "test";
        }
    }
}
