﻿using Microsoft.AspNetCore.TestHost;
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

namespace Timeline.Tests.IntegratedTests
{
    public abstract class IntegratedTestBase : IAsyncLifetime
    {
        protected TestApplication TestApp { get; }

        protected int TestUserCount { get; }

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

        private async Task CreateUsers()
        {
            using var scope = TestApp.Host.Services.CreateScope();

            var users = new List<(string username, string password, string nickname)>();

            for (int i = 1; i <= TestUserCount; i++)
            {
                users.Add(($"user{i}", $"user{i}pw", $"imuser{i}"));
            }

            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            await userService.ModifyUserAsync(await userService.GetUserIdByUsernameAsync("administrator"), new ModifyUserParams
            {
                Username = "admin",
                Password = "adminpw",
                Nickname = "administrator"
            });

            foreach (var user in users)
            {
                var (username, password, nickname) = user;
                var u = await userService.CreateUserAsync(new CreateUserParams(username, password));
                await userService.ModifyUserAsync(u.Id, new ModifyUserParams() { Nickname = nickname });
            }
        }

        public async Task InitializeAsync()
        {
            await TestApp.InitializeAsync();
            await CreateUsers();
            await OnInitializeAsync();
            OnInitialize();
        }

        public async Task DisposeAsync()
        {
            await OnDisposeAsync();
            OnDispose();
            await TestApp.DisposeAsync();
        }

        public Task<HttpClient> CreateDefaultClient(bool setApiBase = true)
        {
            var client = TestApp.Host.GetTestServer().CreateClient();
            if (setApiBase)
            {
                client.BaseAddress = new Uri(client.BaseAddress!, "api/");
            }
            return Task.FromResult(client);
        }

        public async Task<string> CreateTokenWithCredentialAsync(string username, string password)
        {
            var client = await CreateDefaultClient();
            var res = await client.TestPostAsync<HttpCreateTokenResponse>("token/create",
                new HttpCreateTokenRequest { Username = username, Password = password });
            return res.Token;
        }

        public async Task<HttpClient> CreateClientWithCredential(string username, string password, bool setApiBase = true)
        {
            var client = await CreateDefaultClient(setApiBase);
            var res = await client.TestPostAsync<HttpCreateTokenResponse>("token/create",
                new HttpCreateTokenRequest { Username = username, Password = password });
            var token = res.Token;
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            return client;
        }

        public Task<string> CreateTokenAsync(int userNumber)
        {
            if (userNumber == 0)
                return CreateTokenWithCredentialAsync("admin", "adminpw");
            else
                return CreateTokenWithCredentialAsync($"user{userNumber}", $"user{userNumber}pw");
        }

        public Task<HttpClient> CreateClientAs(int userNumber, bool setApiBase = true)
        {
            if (userNumber < 0)
                return CreateDefaultClient(setApiBase);
            if (userNumber == 0)
                return CreateClientWithCredential("admin", "adminpw", setApiBase);
            else
                return CreateClientWithCredential($"user{userNumber}", $"user{userNumber}pw", setApiBase);
        }

        public Task<HttpClient> CreateClientAsAdministrator(bool setApiBase = true)
        {
            return CreateClientAs(0, setApiBase);
        }

        public Task<HttpClient> CreateClientAsUser(bool setApiBase = true)
        {
            return CreateClientAs(1, setApiBase);
        }
    }
}
