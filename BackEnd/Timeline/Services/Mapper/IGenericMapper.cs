using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Timeline.Services.Mapper
{
    public interface IGenericMapper
    {
        TDestination AutoMapperMap<TDestination>(object source);
        Task<TDestination> MapAsync<TSource, TDestination>(TSource source, IUrlHelper urlHelper, ClaimsPrincipal? user);
    }
}
