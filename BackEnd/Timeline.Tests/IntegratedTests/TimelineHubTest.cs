using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using System;
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
            return new HubConnectionBuilder().WithUrl($"http://localhost/api/hub/timeline{(token is null ? "" : "?token=" + token)}",
              options => options.HttpMessageHandlerFactory = _ => TestApp.Server.CreateHandler()).Build();
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task TimelinePostUpdate_Should_Work(TimelineNameGenerator generator)
        {
            var token = await CreateTokenAsync(1);

            await using var connection = CreateConnection(token);

            var changed = false;

            connection.On<string>(nameof(ITimelineClient.OnTimelinePostChanged), (timelineName) =>
            {
                timelineName.Should().Be(generator(1));
                changed = true;
            });

            await connection.StartAsync();
            connection.State.Should().Be(HubConnectionState.Connected);

            using var client = await CreateClientAsUser();

            await client.TestPostAsync($"timelines/{generator(1)}/posts", TimelinePostTest.CreateTextPostRequest("aaa"));
            await Task.Delay(1);
            changed.Should().BeFalse();

            await connection.InvokeAsync(nameof(TimelineHub.SubscribeTimelinePostChange), generator(1));

            await client.TestPostAsync($"timelines/{generator(1)}/posts", TimelinePostTest.CreateTextPostRequest("bbb"));
            await Task.Delay(1);
            changed.Should().BeTrue();

            changed = false;

            await connection.InvokeAsync(nameof(TimelineHub.UnsubscribeTimelinePostChange), generator(1));

            await client.TestPostAsync($"timelines/{generator(1)}/posts", TimelinePostTest.CreateTextPostRequest("ccc"));
            await Task.Delay(1);
            changed.Should().BeFalse();
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

