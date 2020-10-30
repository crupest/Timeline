using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq;

namespace Timeline.Routes
{
    public static class MvcOptionsExtensions
    {
        public static void UseApiRoutePrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute)
        {
            opts.Conventions.Add(new ApiRoutePrefixConvention(routeAttribute));
        }

        public static void UseApiRoutePrefix(this MvcOptions opts, string prefix)
        {
            opts.UseApiRoutePrefix(new RouteAttribute(prefix));
        }
    }

    public class ApiRoutePrefixConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _routePrefix;

        public ApiRoutePrefixConvention(IRouteTemplateProvider route)
        {
            _routePrefix = new AttributeRouteModel(route);
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var selector in application.Controllers.Where(c => c.Filters.Any(f => f is IApiBehaviorMetadata)).SelectMany(c => c.Selectors))
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_routePrefix, selector.AttributeRouteModel);
                }
                else
                {
                    selector.AttributeRouteModel = _routePrefix;
                }
            }
        }
    }
}
