using Microsoft.AspNetCore.Hosting;

namespace Timeline
{
    public static class EnvironmentConstants
    {
        public const string TestEnvironmentName = "Test";

        public static bool IsTest(this IHostingEnvironment environment)
        {
            return environment.EnvironmentName == TestEnvironmentName;
        }
    }
}
