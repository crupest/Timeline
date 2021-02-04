using AutoMapper;
using Timeline.Services;

namespace Timeline.Models.Http
{

    public class HttpAutoMapperProfile : Profile
    {
        public HttpAutoMapperProfile()
        {
            CreateMap<HttpUserPatchRequest, ModifyUserParams>();
            CreateMap<HttpTimelinePatchRequest, TimelineChangePropertyParams>();
        }
    }
}
