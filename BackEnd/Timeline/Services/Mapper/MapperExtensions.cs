using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Timeline.Services.Mapper
{
    public static class MapperExtensions
    {
        public static async Task<List<TDestination>> MapListAsync<TSource, TDestination>(this IMapper<TSource, TDestination> mapper, IEnumerable<TSource> source, IUrlHelper urlHelper, ClaimsPrincipal? user)
        {
            var result = new List<TDestination>();
            foreach (var s in source)
            {
                result.Add(await mapper.MapAsync(s, urlHelper, user));
            }
            return result;
        }

        public static Task<TDestination> MapAsync<TDestination>(this IGenericMapper mapper, object source, IUrlHelper urlHelper, ClaimsPrincipal? user)
        {
            var method = typeof(IGenericMapper).GetMethod(nameof(IGenericMapper.MapAsync));
            var m = method!.MakeGenericMethod(source.GetType(), typeof(TDestination))!;
            return (Task<TDestination>)m.Invoke(mapper, new object?[] { source, urlHelper, user })!;
        }

        public static async Task<List<TDestination>> MapListAsync<TSource, TDestination>(this IGenericMapper mapper, IEnumerable<TSource> source, IUrlHelper urlHelper, ClaimsPrincipal? user)
        {
            var result = new List<TDestination>();
            foreach (var s in source)
            {
                result.Add(await mapper.MapAsync<TSource, TDestination>(s, urlHelper, user));
            }
            return result;
        }

        public static async Task<List<TDestination>> MapListAsync<TDestination>(this IGenericMapper mapper, IEnumerable<object> source, IUrlHelper urlHelper, ClaimsPrincipal? user)
        {
            var result = new List<TDestination>();
            foreach (var s in source)
            {
                result.Add(await mapper.MapAsync<TDestination>(s, urlHelper, user));
            }
            return result;
        }
    }
}
