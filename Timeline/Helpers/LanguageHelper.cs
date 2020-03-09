using System.Linq;

namespace TimelineApp.Helpers
{
    public static class LanguageHelper
    {
        public static bool AreSame(this bool firstBool, params bool[] otherBools)
        {
            return otherBools.All(b => b == firstBool);
        }
    }
}
