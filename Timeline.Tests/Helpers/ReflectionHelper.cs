using System.Linq;
using System.Reflection;

namespace Timeline.Tests.Helpers
{
    public static class ReflectionHelper
    {
        public static ParameterInfo GetParameter(this MethodInfo methodInfo, string name)
        {
            return methodInfo.GetParameters().Where(p => p.Name == name).Single();
        }
    }
}
