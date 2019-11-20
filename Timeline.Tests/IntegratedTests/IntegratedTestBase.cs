using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public enum AuthType
    {
        None,
        User,
        Admin
    }

    public static class AuthTypeExtensions
    {
        public static MockUser GetMockUser(this AuthType authType)
        {
            return authType switch
            {
                AuthType.None => null,
                AuthType.User => MockUser.User,
                AuthType.Admin => MockUser.Admin,
                _ => throw new InvalidOperationException("Unknown auth type.")
            };
        }

        public static string GetUsername(this AuthType authType) => authType.GetMockUser().Username;
    }

    public abstract class IntegratedTestBase : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        protected TestApplication TestApp { get; }

        protected WebApplicationFactory<Startup> Factory => TestApp.Factory;

        public IntegratedTestBase(WebApplicationFactory<Startup> factory)
        {
            TestApp = new TestApplication(factory);
        }

        protected virtual void OnDispose()
        {

        }

        public void Dispose()
        {
            OnDispose();
            TestApp.Dispose();
        }

        protected void CreateExtraMockUsers(int count)
        {
            TestApp.Database.CreateExtraMockUsers(count);
        }

        protected IReadOnlyList<MockUser> ExtraMockUsers => TestApp.Database.ExtraMockUsers;

        public Task<HttpClient> CreateClientWithNoAuth()
        {
            return Task.FromResult(Factory.CreateDefaultClient());
        }

        public async Task<HttpClient> CreateClientWithCredential(string username, string password)
        {
            var client = Factory.CreateDefaultClient();
            var response = await client.PostAsJsonAsync("/token/create",
                new CreateTokenRequest { Username = username, Password = password });
            var token = response.Should().HaveStatusCode(200)
                .And.HaveJsonBody<CreateTokenResponse>().Which.Token;
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            return client;
        }

        public Task<HttpClient> CreateClientAs(MockUser user)
        {
            if (user == null)
                return CreateClientWithNoAuth();
            return CreateClientWithCredential(user.Username, user.Password);
        }

        public Task<HttpClient> CreateClientAs(AuthType authType) => CreateClientAs(authType.GetMockUser());


        public Task<HttpClient> CreateClientAsUser() => CreateClientAs(MockUser.User);
        public Task<HttpClient> CreateClientAsAdmin() => CreateClientAs(MockUser.Admin);

    }
}
