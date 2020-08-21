using Microsoft.AspNetCore.Mvc;

[assembly: ApiConventionType(typeof(Timeline.Controllers.ApiConvention))]

namespace Timeline.Controllers
{
    // There is some bug if nullable is enable. So disable it.
#nullable disable
    /// <summary>
    /// My api convention.
    /// </summary>
    public static class ApiConvention
    {
    }
}
