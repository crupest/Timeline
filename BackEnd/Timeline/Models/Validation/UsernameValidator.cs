using System;

namespace Timeline.Models.Validation
{
    public class UsernameValidator : NameValidator
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class UsernameAttribute : ValidateWithAttribute
    {
        public UsernameAttribute()
            : base(typeof(UsernameValidator))
        {

        }
    }
}
