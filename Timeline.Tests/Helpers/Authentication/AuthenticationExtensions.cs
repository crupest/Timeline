using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Tests.Mock.Data;

namespace Timeline.Tests.Helpers.Authentication
{
    public static class AuthenticationExtensions
    {
        private const string CreateTokenUrl = "/token/create";

        public static async Task<CreateTokenResponse> CreateUserTokenAsync(this HttpClient client, string username, string password, int? expireOffset = null)
        {
            var response = await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest { Username = username, Password = password, ExpireOffset = expireOffset });
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
            return factory.CreateClientWithCredential(MockUsers.UserUsername, MockUsers.UserPassword);
        }

        public static Task<HttpClient> CreateClientAsAdmin<T>(this WebApplicationFactory<T> factory) where T : class
        {
            return factory.CreateClientWithCredential(MockUsers.AdminUsername, MockUsers.AdminPassword);
        }
    }
}
