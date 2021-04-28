using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Timeline.Services.Mapper
{
    public interface IMapper<TSource, TDestination>
    {
        Task<TDestination> MapAsync(TSource source, IUrlHelper urlHelper, ClaimsPrincipal? user);
    }
}
