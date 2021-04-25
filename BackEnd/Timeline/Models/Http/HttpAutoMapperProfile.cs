using AutoMapper;
using Timeline.Services.Timeline;
using Timeline.Services.User;

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
