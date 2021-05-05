using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Timeline.Models;
using Timeline.Models.Http;
using Xunit;

namespace Timeline.Tests.IntegratedTests
{
    public class TimelineTest : BaseTimelineTest
    {
        [Fact]
        public async Task TimelineGet_Should_Work()
        {
            using var client = await CreateDefaultClient();

            await client.TestGetAssertInvalidModelAsync("timelines/@!!!");
            await client.TestGetAssertInvalidModelAsync("timelines/!!!");

            {
                var body = await client.TestGetAsync<HttpTimeline>("timelines/@user1");
                body.Owner.Should().BeEquivalentTo(await client.GetUserAsync("user1"));
                body.Visibility.Should().Be(TimelineVisibility.Register);
                body.Description.Should().Be("");
                body.Members.Should().NotBeNull().And.BeEmpty();
                var links = body._links;
                links.Should().NotBeNull();
                links.Self.Should().EndWith("timelines/@user1");
                links.Posts.Should().EndWith("timelines/@user1/posts");
            }

            {
                var body = await client.TestGetAsync<HttpTimeline>("timelines/t1");
                body.Owner.Should().BeEquivalentTo(await client.GetUserAsync("user1"));
                body.Visibility.Should().Be(TimelineVisibility.Register);
                body.Description.Should().Be("");
                body.Members.Should().NotBeNull().And.BeEmpty();
                var links = body._links;
                links.Should().NotBeNull();
                links.Self.Should().EndWith("timelines/t1");
                links.Posts.Should().EndWith("timelines/t1/posts");
            }
        }

        [Fact]
        public async Task TimelineList_Should_Work()
        {
            using var client = await CreateDefaultClient();

            var result = new List<HttpTimeline>
            {
                await client.GetTimelineAsync("@user1")
            };

            for (int i = 0; i <= TestUserCount; i++)
            {
                result.Add(await client.GetTimelineAsync($"t{i}"));
            }


            var body = await client.TestGetAsync<List<HttpTimeline>>("timelines");
            body.Should().BeEquivalentTo(result);
        }

        [Fact]
        public async Task TimelineListWithQuery_Should_Work()
        {
            {
                using var client = await CreateDefaultClient();

                await client.TestGetAssertInvalidModelAsync("timelines?relate=us!!");
                await client.TestGetAssertInvalidModelAsync("timelines?relateType=aaa");
                await client.TestGetAssertInvalidModelAsync("timelines?visibility=aaa");
            }

            {
                using var client = await CreateClientAsUser();

                await client.PutTimelineMemberAsync("@user1", "user3");
                await client.PutTimelineMemberAsync("t1", "user3");
                await client.PatchTimelineAsync("@user1", new() { Visibility = TimelineVisibility.Public });
                await client.PatchTimelineAsync("t1", new() { Visibility = TimelineVisibility.Register });
            }

            {
                using var client = await CreateClientAs(2);

                await client.PutTimelineMemberAsync("@user2", "user3");
                await client.PutTimelineMemberAsync("t2", "user3");
                await client.PatchTimelineAsync("@user2", new() { Visibility = TimelineVisibility.Register });
                await client.PatchTimelineAsync("t2", new() { Visibility = TimelineVisibility.Private });
            }

            {
                using var client = await CreateClientAs(3);
                await client.PatchTimelineAsync("@user3", new HttpTimelinePatchRequest { Visibility = TimelineVisibility.Private });
                await client.PatchTimelineAsync("t3", new HttpTimelinePatchRequest { Visibility = TimelineVisibility.Register });
            }

            {
                var testResultRelate = new List<HttpTimeline>();
                var testResultOwn = new List<HttpTimeline>();
                var testResultJoin = new List<HttpTimeline>();
                var testResultOwnPrivate = new List<HttpTimeline>();
                var testResultRelatePublic = new List<HttpTimeline>();
                var testResultRelateRegister = new List<HttpTimeline>();
                var testResultJoinPrivate = new List<HttpTimeline>();
                var testResultPublic = new List<HttpTimeline>();

                using var client = await CreateDefaultClient();

                {
                    var timeline = await client.GetTimelineAsync("@user1");
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultRelatePublic.Add(timeline);
                    testResultPublic.Add(timeline);
                }

                {
                    var timeline = await client.GetTimelineAsync("t1");
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultRelateRegister.Add(timeline);
                }

                {
                    var timeline = await client.GetTimelineAsync("@user2");
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultRelateRegister.Add(timeline);
                }

                {
                    var timeline = await client.GetTimelineAsync("t2");
                    testResultRelate.Add(timeline);
                    testResultJoin.Add(timeline);
                    testResultJoinPrivate.Add(timeline);
                }

                {
                    var timeline = await client.GetTimelineAsync("@user3");
                    testResultRelate.Add(timeline);
                    testResultOwn.Add(timeline);
                    testResultOwnPrivate.Add(timeline);
                }

                {
                    var timeline = await client.GetTimelineAsync("t3");
                    testResultRelate.Add(timeline);
                    testResultOwn.Add(timeline);
                    testResultRelateRegister.Add(timeline);
                }

                async Task TestAgainst(string url, List<HttpTimeline> against)
                {
                    var body = await client.TestGetAsync<List<HttpTimeline>>(url);
                    body.Should().BeEquivalentTo(against);
                }

                await TestAgainst("timelines?relate=user3", testResultRelate);
                await TestAgainst("timelines?relate=user3&relateType=own", testResultOwn);
                await TestAgainst("timelines?relate=user3&visibility=public", testResultRelatePublic);
                await TestAgainst("timelines?relate=user3&visibility=register", testResultRelateRegister);
                await TestAgainst("timelines?relate=user3&relateType=join&visibility=private", testResultJoinPrivate);
                await TestAgainst("timelines?relate=user3&relateType=own&visibility=private", testResultOwnPrivate);
                await TestAgainst("timelines?visibility=public", testResultPublic);
            }
        }

        [Fact]
        public async Task TimelineCreate_Should_Work()
        {
            {
                using var client = await CreateDefaultClient();
                await client.TestPostAssertUnauthorizedAsync("timelines", new HttpTimelineCreateRequest { Name = "aaa" });
            }

            {
                using var client = await CreateClientAsUser();

                await client.TestPostAssertInvalidModelAsync("timelines", new HttpTimelineCreateRequest { Name = "!!!" });

                {
                    var body = await client.TestPostAsync<HttpTimeline>("timelines", new HttpTimelineCreateRequest { Name = "aaa" });
                    body.Should().BeEquivalentTo(await client.GetTimelineAsync("aaa"));
                }

                await client.TestPostAssertErrorAsync("timelines", new HttpTimelineCreateRequest { Name = "aaa" }, errorCode: ErrorCodes.Conflict.Timeline);
            }
        }

        [Fact]
        public async Task TimelineDelete_Should_Work()
        {
            {
                using var client = await CreateDefaultClient();
                await client.TestDeleteAssertUnauthorizedAsync("timelines/t1");
            }

            {
                using var client = await CreateClientAs(2);
                await client.TestDeleteAssertForbiddenAsync("timelines/t1");
            }

            {
                using var client = await CreateClientAsAdministrator();

                await client.TestDeleteAssertInvalidModelAsync("timelines/!!!");
                await client.TestDeleteAsync("timelines/t2", true);
                await client.TestDeleteAsync("timelines/t2", false);
            }

            {
                using var client = await CreateClientAs(1);

                await client.TestDeleteAssertInvalidModelAsync("timelines/!!!");
                await client.TestDeleteAsync("timelines/t1", true);
                await client.TestDeleteAssertForbiddenAsync("timelines/t1");
            }
        }


        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task TimelineDescription_Should_Work(TimelineNameGenerator generator)
        {
            // TODO! Permission tests.

            using var client = await CreateClientAsUser();
            var timelineName = generator(1);

            {
                var timeline = await client.GetTimelineAsync(timelineName);
                timeline.Description.Should().BeEmpty();
            }

            const string mockDescription = "haha";

            {
                var timeline = await client.PatchTimelineAsync(timelineName, new() { Description = mockDescription });
                timeline.Description.Should().Be(mockDescription);
            }

            {
                var timeline = await client.GetTimelineAsync(timelineName);
                timeline.Description.Should().Be(mockDescription);
            }

            {
                var timeline = await client.PatchTimelineAsync(timelineName, new() { Description = null });
                timeline.Description.Should().Be(mockDescription);
            }

            {
                var timeline = await client.GetTimelineAsync(timelineName);
                timeline.Description.Should().Be(mockDescription);
            }

            {
                var timeline = await client.PatchTimelineAsync(timelineName, new() { Description = "" });
                timeline.Description.Should().BeEmpty();
            }

            {
                var timeline = await client.GetTimelineAsync(timelineName);
                timeline.Description.Should().BeEmpty();
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Member_Should_Work(TimelineNameGenerator generator)
        {
            // TODO! Invalid model tests.
            // TODO! Permission tests.

            using var client = await CreateClientAsUser();

            var timelineName = generator(1);

            async Task AssertMembers(List<HttpUser> members)
            {
                var body = await client.GetTimelineAsync(timelineName);
                body.Members.Should().NotBeNull().And.BeEquivalentTo(members);
            }

            async Task AssertEmptyMembers()
            {
                var body = await client.GetTimelineAsync(timelineName);
                body.Members.Should().NotBeNull().And.BeEmpty();
            }

            await AssertEmptyMembers();
            await client.TestPutAssertErrorAsync($"timelines/{timelineName}/members/usernotexist", errorCode: ErrorCodes.NotExist.User);
            await AssertEmptyMembers();
            await client.PutTimelineMemberAsync(timelineName, "user2");
            await AssertMembers(new List<HttpUser> { await client.GetUserAsync("user2") });
            await client.DeleteTimelineMemberAsync(timelineName, "user2");
            await AssertEmptyMembers();
            await client.TestDeleteAssertErrorAsync($"timelines/{timelineName}/members/usernotexist");
            await AssertEmptyMembers();
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Timeline_LastModified(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            DateTime lastModified;

            {
                var body = await client.GetTimelineAsync(generator(1));
                lastModified = body.LastModified;
            }

            await Task.Delay(1000);

            {
                var body = await client.PatchTimelineAsync(generator(1), new() { Description = "123" });
                lastModified = body.LastModified.Should().BeAfter(lastModified).And.Subject!.Value;
            }

            {
                var body = await client.GetTimelineAsync(generator(1));
                body.LastModified.Should().Be(lastModified);
            }

            await Task.Delay(1000);

            await client.PutTimelineMemberAsync(generator(1), "user2");

            {
                var body = await client.GetTimelineAsync(generator(1));
                body.LastModified.Should().BeAfter(lastModified);
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Title(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            {
                var body = await client.GetTimelineAsync(generator(1));
                body.Title.Should().Be(body.Name);
            }

            {
                var body = await client.PatchTimelineAsync(generator(1), new HttpTimelinePatchRequest { Title = "atitle" });
                body.Title.Should().Be("atitle");
            }

            {
                var body = await client.GetTimelineAsync(generator(1));
                body.Title.Should().Be("atitle");
            }
        }

        [Fact]
        public async Task ChangeName()
        {
            {
                using var client = await CreateDefaultClient();
                await client.TestPatchAssertUnauthorizedAsync("timelines/t1", new HttpTimelinePatchRequest { Name = "tttttttt" });
            }

            {
                using var client = await CreateClientAs(2);
                await client.TestPatchAssertForbiddenAsync("timelines/t1", new HttpTimelinePatchRequest { Name = "tttttttt" });
            }

            using (var client = await CreateClientAsUser())
            {
                await client.TestPatchAssertInvalidModelAsync("timelines/t1", new HttpTimelinePatchRequest { Name = "!!!" });
                await client.TestPatchAssertErrorAsync("timelines/t1", new HttpTimelinePatchRequest { Name = "t2" }, errorCode: ErrorCodes.Conflict.Timeline);

                await client.TestPatchAsync("timelines/t1", new HttpTimelinePatchRequest { Name = "newt" });

                await client.TestGetAsync("timelines/t1", expectedStatusCode: HttpStatusCode.NotFound);

                {
                    var body = await client.TestGetAsync<HttpTimeline>("timelines/newt");
                    body.Name.Should().Be("newt");
                }
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Color(TimelineNameGenerator generator)
        {
            using var client = await CreateClientAsUser();

            {
                var timeline = await client.TestGetAsync<HttpTimeline>($"timelines/{generator(1)}");
                timeline.Color.Should().Be(null);
            }

            await client.TestPatchAssertInvalidModelAsync($"timelines/{generator(1)}", new HttpTimelinePatchRequest { Color = "!!!" });

            {
                var timeline = await client.TestPatchAsync<HttpTimeline>($"timelines/{generator(1)}", new HttpTimelinePatchRequest { Color = "#112233" });
                timeline.Color.Should().Be("#112233");
            }

            {
                var timeline = await client.TestGetAsync<HttpTimeline>($"timelines/{generator(1)}");
                timeline.Color.Should().Be("#112233");
            }
        }

        [Theory]
        [MemberData(nameof(TimelineNameGeneratorTestData))]
        public async Task Get_Manageable_Postable(TimelineNameGenerator generator)
        {
            {
                using var client = await CreateClientAsUser();
                var timeline = await client.TestGetAsync<HttpTimeline>($"timelines/{generator(1)}");
                timeline.Manageable.Should().Be(true);
                timeline.Postable.Should().Be(true);
            }

            {
                using var client = await CreateClientAs(2);
                var timeline = await client.TestGetAsync<HttpTimeline>($"timelines/{generator(1)}");
                timeline.Manageable.Should().Be(false);
                timeline.Postable.Should().Be(false);
            }
        }
    }
}
