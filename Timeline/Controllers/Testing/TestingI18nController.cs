using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Timeline.Controllers.Testing
{
    [Route("testing/i18n")]
    [ApiController]
    public class TestingI18nController : Controller
    {
        private readonly IStringLocalizer<TestingI18nController> _stringLocalizer;

        public TestingI18nController(IStringLocalizer<TestingI18nController> stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        [HttpGet("direct")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public ActionResult<string> Direct()
        {
            return Resources.Controllers.Testing.TestingI18nController.TestString;
        }

        [HttpGet("localizer")]
        public ActionResult<string> Localizer()
        {
            return _stringLocalizer["TestString"].Value;
        }
    }
}
