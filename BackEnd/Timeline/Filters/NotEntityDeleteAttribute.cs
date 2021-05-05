using System;

namespace Timeline.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NotEntityDeleteAttribute : Attribute
    {
    }
}
