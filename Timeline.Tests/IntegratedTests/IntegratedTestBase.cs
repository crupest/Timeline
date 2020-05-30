﻿using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Converters;
using Timeline.Models.Http;
using Timeline.Services;
using Timeline.Tests.Helpers;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public abstract class IntegratedTestBase : IClassFixture<WebApplicationFactory<Startup>>, IAsyncLifetime
    {
        protected TestApplication TestApp { get; }

        protected WebApplicationFactory<Startup> Factory => TestApp.Factory;

        public IReadOnlyList<UserInfo> UserInfos { get; private set; }

        private readonly int _userCount;

        public IntegratedTestBase(WebApplicationFactory<Startup> factory) : this(factory, 1)
        {

        }

        public IntegratedTestBase(WebApplicationFactory<Startup> factory, int userCount)
        {
            if (userCount < 0)
                throw new ArgumentOutOfRangeException(nameof(userCount), userCount, "User count can't be negative.");

            _userCount = userCount;

            TestApp = new TestApplication(factory);
        }

        protected virtual Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDisposeAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual void OnDispose()
        {

        }

        public async Task InitializeAsync()
        {
            await TestApp.InitializeAsync();

            using (var scope = Factory.Services.CreateScope())
            {
                var users = new List<User>()
                {
                    new User
                    {
                        Username = "admin",
                        Password = "adminpw",
                        Administrator = true,
                        Nickname = "administrator"
                    }
                };

                for (int i = 1; i <= _userCount; i++)
                {
                    users.Add(new User
                    {
                        Username = $"user{i}",
                        Password = $"user{i}pw",
                        Administrator = false,
                        Nickname = $"imuser{i}"
                    });
                }

                var userInfoList = new List<UserInfo>();

                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                foreach (var user in users)
                {
                    await userService.CreateUser(user);
                }

                using var client = await CreateDefaultClient();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                options.Converters.Add(new JsonStringEnumConverter());
                options.Converters.Add(new JsonDateTimeConverter());
                foreach (var user in users)
                {
                    var s = await client.GetStringAsync($"users/{user.Username}");
                    userInfoList.Add(JsonSerializer.Deserialize<UserInfo>(s, options));
                }

                UserInfos = userInfoList;
            }

            await OnInitializeAsync();
        }

        public async Task DisposeAsync()
        {
            await OnDisposeAsync();
            OnDispose();
            await TestApp.DisposeAsync();
        }

        public Task<HttpClient> CreateDefaultClient()
        {
            var client = Factory.CreateDefaultClient();
            client.BaseAddress = new Uri(client.BaseAddress, "api/");
            return Task.FromResult(client);
        }

        public async Task<HttpClient> CreateClientWithCredential(string username, string password)
        {
            var client = Factory.CreateDefaultClient();
            client.BaseAddress = new Uri(client.BaseAddress, "api/");
            var response = await client.PostAsJsonAsync("token/create",
                new CreateTokenRequest { Username = username, Password = password });
            var token = response.Should().HaveStatusCode(200)
                .And.HaveJsonBody<CreateTokenResponse>().Which.Token;
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            return client;
        }

        public Task<HttpClient> CreateClientAs(int userNumber)
        {
            if (userNumber < 0)
                return CreateDefaultClient();
            if (userNumber == 0)
                return CreateClientWithCredential("admin", "adminpw");
            else
                return CreateClientWithCredential($"user{userNumber}", $"user{userNumber}pw");
        }

        public Task<HttpClient> CreateClientAsAdministrator()
        {
            return CreateClientAs(0);
        }

        public Task<HttpClient> CreateClientAsUser()
        {
            return CreateClientAs(1);
        }
    }
}
