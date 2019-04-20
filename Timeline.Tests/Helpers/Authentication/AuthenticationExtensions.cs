using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Entities;
using Xunit;

namespace Timeline.Tests.Helpers.Authentication
{
    public static class AuthenticationExtensions
    {
        private const string CreateTokenUrl = "/token/create";

        public static async Task<CreateTokenResponse> CreateUserTokenAsync(this HttpClient client, string username, string password, bool assertSuccess = true)
        {
            var response = await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest { Username = username, Password = password });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonConvert.DeserializeObject<CreateTokenResponse>(await response.Content.ReadAsStringAsync());
            if (assertSuccess)
                Assert.True(result.Success);

            return result;
        }

        public static async Task<HttpClient> CreateClientWithUser<T>(this WebApplicationFactory<T> factory, string username, string password) where T : class
        {
            var client = factory.CreateDefaultClient();
            var token = (await client.CreateUserTokenAsync(username, password)).Token;
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            return client;
        }

        public static async Task<HttpResponseMessage> SendWithAuthenticationAsync(this HttpClient client, string token, string path, Action<HttpRequestMessage> requestBuilder = null)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, path),
            };
            request.Headers.Add("Authorization", "Bearer " + token);
            requestBuilder?.Invoke(request);
            return await client.SendAsync(request);
        }
    }
}
