using System.Collections.Generic;
using System.Threading.Tasks;
using Timeline.Models.Http;

namespace Timeline.Tests.IntegratedTests
{
    public class BaseTimelineTest : IntegratedTestBase
    {
        public BaseTimelineTest() : base(3)
        {
        }

        protected override async Task OnInitializeAsync()
        {
            for (int i = 0; i <= 3; i++)
            {
                using var client = await CreateClientAs(i);
                await client.TestPostAsync("timelines", new HttpTimelineCreateRequest { Name = $"t{i}" });
            }
        }

        public static string CreatePersonalTimelineName(int i) => i == 0 ? "@admin" : $"@user{i}";
        public static string CreateOrdinaryTimelineName(int i) => $"t{i}";
        public delegate string TimelineNameGenerator(int i);

        public static IEnumerable<object[]> TimelineNameGeneratorTestData()
        {
            yield return new object[] { new TimelineNameGenerator(CreatePersonalTimelineName) };
            yield return new object[] { new TimelineNameGenerator(CreateOrdinaryTimelineName) };
        }
    }
}
