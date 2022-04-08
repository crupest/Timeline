using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Timeline.Models.Http;
using Timeline.Services.User;
using Timeline.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests2
{
    public abstract class IntegratedTestBase : IAsyncLifetime
    {
        protected TestApplication TestApp { get; }

        protected int TestUserCount { get; }

        protected string NormalUserToken { get; set; } = default!;
        protected string AdminUserToken { get; set; } = default!;

        public IntegratedTestBase(ITestOutputHelper testOutputHelper) : this(1, testOutputHelper)
        {

        }

        public IntegratedTestBase(int userCount, ITestOutputHelper testOutputHelper)
        {
            if (userCount < 0)
                throw new ArgumentOutOfRangeException(nameof(userCount), userCount, "User count can't be negative.");

            TestUserCount = userCount;

            TestApp = new TestApplication(testOutputHelper);
        }

        protected virtual Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDisposeAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual void OnInitialize()
        {

        }

        protected virtual void OnDispose()
        {

        }

        private async Task CreateInitUsersAsync()
        {
            using var scope = TestApp.Host.Services.CreateScope();

            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var userPermissionService = scope.ServiceProvider.GetRequiredService<IUserPermissionService>();

            var admin = await userService.CreateUserAsync(new CreateUserParams("admin", "adminpw"));
            foreach (var permission in Enum.GetValues<UserPermission>())
            {
                await userPermissionService.AddPermissionToUserAsync(admin.Id, permission);
            }
            await userService.CreateUserAsync(new CreateUserParams("user", "userpw"));
        }

        public async Task CreateUserAsync(string username, string password)
        {
            using var scope = TestApp.Host.Services.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            await userService.CreateUserAsync(new CreateUserParams(username, password));
        }

        public async Task InitializeAsync()
        {
            await TestApp.InitializeAsync();
            await CreateInitUsersAsync();
            NormalUserToken = await CreateTokenWithCredentialAsync("user", "userpw");
            AdminUserToken = await CreateTokenWithCredentialAsync("admin", "adminpw");
            await OnInitializeAsync();
            OnInitialize();
        }

        public async Task DisposeAsync()
        {
            await OnDisposeAsync();
            OnDispose();
            await TestApp.DisposeAsync();
        }

        public HttpClient CreateDefaultClient(bool setApiBase = true)
        {
            var client = TestApp.Host.GetTestServer().CreateClient();
            if (setApiBase)
            {
                client.BaseAddress = new Uri(client.BaseAddress!, "api/");
            }
            return client;
        }

        public async Task<string> CreateTokenWithCredentialAsync(string username, string password)
        {
            var client = CreateDefaultClient();
            var res = await client.TestJsonSendAsync<HttpCreateTokenResponse>(HttpMethod.Post, "token/create",
                new HttpCreateTokenRequest { Username = username, Password = password });
            return res.Token;
        }

        public HttpClient CreateClientWithToken(string token, bool setApiBase = true)
        {
            var client = CreateDefaultClient(setApiBase);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            return client;
        }

        public HttpClient CreateClientAsAdmin()
        {
            return CreateClientWithToken(AdminUserToken);
        }

        public HttpClient CreateClientAsUser()
        {
            return CreateClientWithToken(NormalUserToken);
        }
    }
}
