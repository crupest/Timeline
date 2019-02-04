using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
