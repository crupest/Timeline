using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Timeline.Services.Mapper
{
    class GenericMapper : IGenericMapper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AutoMapper.IMapper _autoMapper;

        public GenericMapper(IServiceProvider serviceProvider, AutoMapper.IMapper autoMapper)
        {
            _serviceProvider = serviceProvider;
            _autoMapper = autoMapper;
        }

        public TDestination AutoMapperMap<TDestination>(object source)
        {
            return _autoMapper.Map<TDestination>(source);
        }

        public async Task<TDestination> MapAsync<TSource, TDestination>(TSource source, IUrlHelper urlHelper, ClaimsPrincipal? user)
        {
            var mapper = _serviceProvider.GetService<IMapper<TSource, TDestination>>();

            if (mapper is not null)
            {
                return await mapper.MapAsync(source, urlHelper, user);
            }

            return _autoMapper.Map<TSource, TDestination>(source);
        }
    }
}
