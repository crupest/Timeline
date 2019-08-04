using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Timeline.Entities;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests
{
    public class UserUnitTest : IClassFixture<MyWebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public UserUnitTest(MyWebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
        {
            _factory = factory.WithTestLogging(outputHelper);
        }

        [Fact]
        public async Task UserTest()
        {
            using (var client = await _factory.CreateClientWithUser("admin", "admin"))
            {
                var res1 = await client.GetAsync("users");
                Assert.Equal(HttpStatusCode.OK, res1.StatusCode);
                var users = JsonConvert.DeserializeObject<UserInfo[]>(await res1.Content.ReadAsStringAsync()).ToList();
                users.Sort(UserInfoComparers.Comparer);
                Assert.Equal(TestMockUsers.MockUserInfos, users, UserInfoComparers.EqualityComparer);
            }
        }
    }
}
