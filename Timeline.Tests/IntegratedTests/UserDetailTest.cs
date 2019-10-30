using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Threading.Tasks;
using Timeline.Tests.Helpers;
using Timeline.Tests.Helpers.Authentication;
using Timeline.Tests.Mock.Data;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class UserDetailTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly TestApplication _testApp;
        private readonly WebApplicationFactory<Startup> _factory;

        public UserDetailTest(WebApplicationFactory<Startup> factory)
        {
            _testApp = new TestApplication(factory);
            _factory = _testApp.Factory;
        }

        public void Dispose()
        {
            _testApp.Dispose();
        }

        [Fact]
        public async Task PermissionTest()
        {
            { // unauthorize
                using var client = _factory.CreateDefaultClient();
                { // GET
                    var res = await client.GetAsync($"users/{MockUser.User.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
                { // PUT
                    var res = await client.PutStringAsync($"users/{MockUser.User.Username}/nickname", "aaa");
                    res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
                }
                { // DELETE
                    var res = await client.DeleteAsync($"users/{MockUser.User.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
                }
            }
            { // user
                using var client = await _factory.CreateClientAsUser();
                { // GET
                    var res = await client.GetAsync($"users/{MockUser.User.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
                { // PUT self
                    var res = await client.PutStringAsync($"users/{MockUser.User.Username}/nickname", "aaa");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
                { // PUT other
                    var res = await client.PutStringAsync($"users/{MockUser.Admin.Username}/nickname", "aaa");
                    res.Should().HaveStatusCode(HttpStatusCode.Forbidden);
                }
                { // DELETE self
                    var res = await client.DeleteAsync($"users/{MockUser.User.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
                { // DELETE other
                    var res = await client.DeleteAsync($"users/{MockUser.Admin.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.Forbidden);
                }
            }
            { // user
                using var client = await _factory.CreateClientAsAdmin();
                { // PUT other
                    var res = await client.PutStringAsync($"users/{MockUser.User.Username}/nickname", "aaa");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
                { // DELETE other
                    var res = await client.DeleteAsync($"users/{MockUser.User.Username}/nickname");
                    res.Should().HaveStatusCode(HttpStatusCode.OK);
                }
            }
        }
    }
}
