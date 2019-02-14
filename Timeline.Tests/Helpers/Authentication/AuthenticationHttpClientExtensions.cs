using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Controllers;
using Xunit;

namespace Timeline.Tests.Helpers.Authentication
{
    public static class AuthenticationHttpClientExtensions
    {
        private const string CreateTokenUrl = "/api/User/CreateToken";

        public static async Task<UserController.CreateTokenResult> CreateUserTokenAsync(this HttpClient client, string username, string password)
        {
            var response = await client.PostAsJsonAsync(CreateTokenUrl, new UserController.UserCredentials { Username = username, Password = password });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonConvert.DeserializeObject<UserController.CreateTokenResult>(await response.Content.ReadAsStringAsync());

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
