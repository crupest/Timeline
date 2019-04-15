using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Entities;
using Xunit;

namespace Timeline.Tests.Helpers.Authentication
{
    public static class AuthenticationHttpClientExtensions
    {
        private const string CreateTokenUrl = "/User/CreateToken";

        public static async Task<CreateTokenResponse> CreateUserTokenAsync(this HttpClient client, string username, string password, bool assertSuccess = true)
        {
            var response = await client.PostAsJsonAsync(CreateTokenUrl, new CreateTokenRequest { Username = username, Password = password });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonConvert.DeserializeObject<CreateTokenResponse>(await response.Content.ReadAsStringAsync());
            if (assertSuccess)
                Assert.True(result.Success);

            return result;
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
