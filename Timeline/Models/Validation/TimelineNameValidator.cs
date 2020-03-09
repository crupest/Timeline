using System;

namespace TimelineApp.Models.Validation
{
    public class TimelineNameValidator : NameValidator
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class TimelineNameAttribute : ValidateWithAttribute
    {
        public TimelineNameAttribute()
            : base(typeof(TimelineNameValidator))
        {

        }
    }
}
