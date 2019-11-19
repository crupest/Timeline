using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Tests.Mock.Data;

namespace Timeline.Tests.Helpers.Authentication
{
    public enum AuthType
    {
        None,
        User,
        Admin
    }

    public static class AuthenticationExtensions
    {
        private const string CreateTokenUrl = "/token/create";

        public static async Task<CreateTokenResponse> CreateUserTokenAsync(this HttpClient client, string username, string password, int? expireOffset = null)
        {
            var response = await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest { Username = username, Password = password, Expire = expireOffset });
            return response.Should().HaveStatusCode(200)
                .And.HaveJsonBody<CreateTokenResponse>().Which;
        }

        public static async Task<HttpClient> CreateClientWithCredential<T>(this WebApplicationFactory<T> factory, string username, string password) where T : class
        {
            var client = factory.CreateDefaultClient();
            var token = (await client.CreateUserTokenAsync(username, password)).Token;
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            return client;
        }

        public static Task<HttpClient> CreateClientAs<T>(this WebApplicationFactory<T> factory, MockUser user) where T : class
        {
            return CreateClientWithCredential(factory, user.Username, user.Password);
        }

        public static Task<HttpClient> CreateClientAsUser<T>(this WebApplicationFactory<T> factory) where T : class
        {
            return factory.CreateClientAs(MockUser.User);
        }

        public static Task<HttpClient> CreateClientAsAdmin<T>(this WebApplicationFactory<T> factory) where T : class
        {
            return factory.CreateClientAs(MockUser.Admin);
        }

        public static Task<HttpClient> CreateClientAs<T>(this WebApplicationFactory<T> factory, AuthType authType) where T : class
        {
            return authType switch
            {
                AuthType.None => Task.FromResult(factory.CreateDefaultClient()),
                AuthType.User => factory.CreateClientAsUser(),
                AuthType.Admin => factory.CreateClientAsAdmin(),
                _ => throw new InvalidOperationException("Unknown auth type.")
            };
        }

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
}
