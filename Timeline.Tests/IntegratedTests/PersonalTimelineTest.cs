using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class PersonalTimelineTest : IntegratedTestBase
    {
        public PersonalTimelineTest(WebApplicationFactory<Startup> factory)
            : base(factory)
        {

        }

        [Theory]
        [InlineData(AuthType.None, 200, 401, 401, 401, 401)]
        [InlineData(AuthType.User, 200, 200, 403, 200, 403)]
        [InlineData(AuthType.Admin, 200, 200, 200, 200, 200)]
        public async Task Permission_Timeline(AuthType authType, int get, int opPropertyUser, int opPropertyAdmin, int opMemberUser, int opMemberAdmin)
        {
            using var client = await Factory.CreateClientAs(authType);
            {
                var res = await client.GetAsync("users/user/timeline");
                res.Should().HaveStatusCode(get);
            }

            {
                var res = await client.PostAsJsonAsync("users/user/timeline/op/property",
                    new TimelinePropertyChangeRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPropertyUser);
            }

            {
                var res = await client.PostAsJsonAsync("users/admin/timeline/op/property",
                    new TimelinePropertyChangeRequest { Description = "hahaha" });
                res.Should().HaveStatusCode(opPropertyAdmin);
            }

            {
                var res = await client.PostAsJsonAsync("users/user/timeline/op/member",
                    new TimelineMemberChangeRequest { Add = new List<string> { "admin" } });
                res.Should().HaveStatusCode(opMemberUser);
            }

            {
                var res = await client.PostAsJsonAsync("users/admin/timeline/op/member",
                    new TimelineMemberChangeRequest { Add = new List<string> { "user" } });
                res.Should().HaveStatusCode(opMemberAdmin);
            }
        }
    }
}
