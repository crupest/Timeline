using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
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
            response.Should().HaveStatusCode(200);
            var result = JsonConvert.DeserializeObject<CreateTokenResponse>(await response.Content.ReadAsStringAsync());
            return result;
        }

        public static async Task<HttpClient> CreateClientWithCredential<T>(this WebApplicationFactory<T> factory, string username, string password) where T : class
        {
            var client = factory.CreateDefaultClient();
            var token = (await client.CreateUserTokenAsync(username, password)).Token;
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            return client;
        }

        public static Task<HttpClient> CreateClientAsUser<T>(this WebApplicationFactory<T> factory) where T : class
        {
            return factory.CreateClientWithCredential(MockUser.User.Username, MockUser.User.Password);
        }

        public static Task<HttpClient> CreateClientAsAdmin<T>(this WebApplicationFactory<T> factory) where T : class
        {
            return factory.CreateClientWithCredential(MockUser.Admin.Username, MockUser.Admin.Password);
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
    }
}
