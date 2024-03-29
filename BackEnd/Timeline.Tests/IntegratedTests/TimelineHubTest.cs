﻿using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Timeline.SignalRHub;
using Xunit;
using Xunit.Abstractions;

namespace Timeline.Tests.IntegratedTests
{
    public class TimelineHubTest : BaseTimelineTest
    {
        public TimelineHubTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        private HubConnection CreateConnection(string? token)
        {
            return new HubConnectionBuilder().WithUrl("ws://localhost/api/hub/timeline",
              options =>
              {
                  options.HttpMessageHandlerFactory = _ => TestApp.Server.CreateHandler();
                  options.AccessTokenProvider = token is null ? null : () => Task.FromResult<string?>(token);
              }).Build();
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task TimelinePostUpdate_Should_Work(TimelineNameGenerator generator)
        {
            var token = await CreateTokenAsync(1);

            await using var connection = CreateConnection(token);

            await connection.StartAsync();
            connection.State.Should().Be(HubConnectionState.Connected);

            using SemaphoreSlim semaphore = new SemaphoreSlim(0);

            var changed = false;

            connection.On<string>(nameof(ITimelineClient.OnTimelinePostChanged), (timelineName) =>
            {
                timelineName.Should().Be(generator(1));
                changed = true;
                semaphore.Release();
            });

            await Task.Run(async () =>
            {
                using var client = await CreateClientAsUser();

                await client.TestPostAsync($"timelines/{generator(1)}/posts", TimelinePostTest.CreateTextPostRequest("aaa"));

                changed.Should().BeFalse();

                await connection.InvokeAsync(nameof(TimelineHub.SubscribeTimelinePostChange), generator(1));

                await client.TestPostAsync($"timelines/{generator(1)}/posts", TimelinePostTest.CreateTextPostRequest("bbb"));
                await semaphore.WaitAsync();
                changed.Should().BeTrue();

                changed = false;

                await connection.InvokeAsync(nameof(TimelineHub.UnsubscribeTimelinePostChange), generator(1));

                await client.TestPostAsync($"timelines/{generator(1)}/posts", TimelinePostTest.CreateTextPostRequest("ccc"));
                changed.Should().BeFalse();

            });
        }

        [Fact]
        public async Task TimelinePostUpdate_InvalidName()
        {
            await using var connection = CreateConnection(null);
            await connection.StartAsync();
            await connection.Awaiting(c => c.InvokeAsync(nameof(TimelineHub.SubscribeTimelinePostChange), "!!!")).Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task TimelinePostUpdate_NotExist()
        {
            await using var connection = CreateConnection(null);
            await connection.StartAsync();
            await connection.Awaiting(c => c.InvokeAsync(nameof(TimelineHub.SubscribeTimelinePostChange), "timelinenotexist")).Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task TimelinePostUpdate_Forbid()
        {
            await using var connection = CreateConnection(null);
            await connection.StartAsync();
            await connection.Awaiting(c => c.InvokeAsync(nameof(TimelineHub.SubscribeTimelinePostChange), "t1")).Should().ThrowAsync<Exception>();
        }
    }
}

